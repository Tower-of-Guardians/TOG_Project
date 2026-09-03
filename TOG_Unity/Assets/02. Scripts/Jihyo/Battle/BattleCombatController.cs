using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Jongmin;
using UnityEngine;
using Random = UnityEngine.Random;

public class BattleCombatController : MonoBehaviour, IBattleController
{
    private const string SynergyHonestyId = "210001";
    private const string SynergyShieldAttackId = "210002";
    private const string SynergyOverwhelmingId = "210003";
    private const string SynergyBloodSuckingId = "210004";
    private const string SynergyPlunderId = "210005";
    private const string SynergyMysteryId = "210006";
    private const string SynergyBasicId = "210007";
    private const string SynergyDarknessId = "210008";

    [SerializeField] private bool playerAttackHitsAll;
    [SerializeField] private float statAnimationWaitTime = 1.0f;

    private BattleManager battleManager;
    private bool isInitialized;
    private int battlePermanentAttackBonus;
    private int pendingOverwhelmingDamage;
    private int pendingBloodSuckingPercent;
    private int preparedAttackValue;
    private int currentTurnAttackBonus;
    private EventDomain eventDomain;
    private bool playedSynergyMotion;

    public float GetStatAnimationWaitTime() => statAnimationWaitTime;
    public bool GetPlayerAttackHitsAll() => playerAttackHitsAll;
    public bool PlayedSynergyMotion => playedSynergyMotion;
    public bool IsInitialized => isInitialized;

    public void Initialize(BattleManager manager)
    {
        if (isInitialized)
        {
            Debug.LogWarning("BattleCombatController has already been initialized.");
            return;
        }

        battleManager = manager;
        eventDomain = DIContainer.IsRegistered<EventDomain>() ? DIContainer.Resolve<EventDomain>() : null;
        isInitialized = true;
    }

    public void Cleanup()
    {
        if (battleManager != null)
        {
            BattleSetupController setupController = battleManager.GetSetupController();
            if (setupController != null)
            {
                Player player = setupController.GetPlayer();
                if (player != null)
                {
                    player.SetBattleSynergyAttackBonus(0);
                    player.SetTurnSynergyAttackBonus(0);
                }
            }
        }

        battleManager = null;
        eventDomain = null;
        isInitialized = false;
        battlePermanentAttackBonus = 0;
        preparedAttackValue = 0;
        currentTurnAttackBonus = 0;
        ResetTurnScopedSynergyState();
    }

    public int GetPreparedAttackValue()
    {
        return preparedAttackValue;
    }

    /// <summary>
    /// 공격 시작 전 시너지 연출 및 선행 효과를 모두 처리합니다.
    /// Intro는 1회, Loop는 발동 시너지 수(최대 3)만큼 재생하며 각 Loop 히트 시점에 효과를 적용합니다.
    /// </summary>
    public IEnumerator ExecutePreAttackSynergyPhase(Player player, PlayerAnimation playerAnimation, SynergyUI synergyUI)
    {
        playedSynergyMotion = false;

        if (player == null)
        {
            preparedAttackValue = 0;
            yield break;
        }

        if (GameData.Instance != null)
        {
            GameData.Instance.GetSynergyData();
        }

        pendingOverwhelmingDamage = 0;
        pendingBloodSuckingPercent = 0;
        currentTurnAttackBonus = 0;
        player.SetTurnSynergyAttackBonus(0);
        player.ApplyAttackStats();

        List<SynergyTotalData> activatedSynergies = SynergyActivationSelector.Select(GameData.Instance?.synergyIDList?.Values);
        int loopCount = SynergyActivationSelector.GetLoopPlayCount(activatedSynergies.Count);
        if (loopCount == 0)
        {
            preparedAttackValue = player.AttackValue;
            yield break;
        }

        if (playerAnimation != null)
        {
            playedSynergyMotion = true;
            PlayerSynergyVisual synergyVisual = player.GetComponent<PlayerSynergyVisual>();
            synergyVisual?.PrepareIntro();

            yield return playerAnimation.PlaySynergyIntro();
            playerAnimation.StartSynergyLoop();

            for (int i = 0; i < loopCount; i++)
            {
                synergyVisual?.Prepare(activatedSynergies[i].synergyData.ID);
                synergyUI?.BeginSynergyHighlight(activatedSynergies[i]);
                yield return playerAnimation.WaitForSynergyLoopHit(i);
                synergyVisual?.PlayHit(activatedSynergies[i].synergyData.ID);
                ApplyActivatedSynergy(player, activatedSynergies[i]);
                yield return playerAnimation.WaitForSynergyLoopCycleEnd(i);
            }

            synergyVisual?.Clear();
        }
        else
        {
            for (int i = 0; i < loopCount; i++)
            {
                ApplyActivatedSynergy(player, activatedSynergies[i]);
                if (synergyUI != null)
                {
                    yield return synergyUI.HighlightSynergyEntry(activatedSynergies[i]);
                }
            }
        }

        preparedAttackValue = player.AttackValue;
    }

    /// <summary>
    /// 턴 단위 일시 시너지 상태를 초기화합니다.
    /// </summary>
    public void ResetTurnScopedSynergyState()
    {
        pendingOverwhelmingDamage = 0;
        pendingBloodSuckingPercent = 0;
    }

    /// <summary>
    /// 전투 초기화 및 타겟 선택
    /// </summary>
    public CombatInitializationResult InitializeCombat(BattleSetupController setupController)
    {
        if (setupController == null)
        {
            Debug.LogError("BattleCombatController: setupController가 null입니다.");
            return null;
        }

        var player = setupController.GetPlayer();
        var primaryMonsters = setupController.GetPrimaryMonsters();

        if (player == null)
        {
            Debug.LogWarning("BattleCombatController: Player가 null입니다.");
            return null;
        }

        List<Monster> aliveMonsters = primaryMonsters.Where(m => m != null && m.IsAlive).ToList();
        if (aliveMonsters.Count == 0)
        {
            Debug.Log("BattleCombatController: 공격할 몬스터가 없습니다.");
            return null;
        }

        // 타겟 선택: 록온한 몬스터를 공격하고, 없으면 랜덤 공격합니다.
        // 카드/유물 효과로 강제 랜덤 공격하는 경우는 아직 미구현입니다.
        List<IDamageable> playerTargets = new();
        Monster primaryMonsterTarget = null;
        Monster selectedTarget = setupController.GetSelectedTarget();

        if (playerAttackHitsAll)
        {
            playerTargets.AddRange(aliveMonsters);
            if (aliveMonsters.Count > 0)
            {
                primaryMonsterTarget = aliveMonsters[0];
            }
        }
        else
        {
            Monster target = selectedTarget != null && selectedTarget.IsAlive 
                ? selectedTarget 
                : aliveMonsters[Random.Range(0, aliveMonsters.Count)];
            primaryMonsterTarget = target;
            playerTargets.Add(target);
        }

        Vector3? attackAnchorPosition = primaryMonsterTarget != null 
            ? primaryMonsterTarget.AttackAnchor.position 
            : null;

        // 애니메이션 리셋
        var playerAnimation = player.GetComponent<PlayerAnimation>();
        if (playerAnimation != null)
        {
            playerAnimation.ResetAnimationState();
        }

        return new CombatInitializationResult
        {
            player = player,
            playerTargets = playerTargets,
            primaryMonsterTarget = primaryMonsterTarget,
            attackAnchorPosition = attackAnchorPosition,
            playerAnimation = playerAnimation
        };
    }

    /// <summary>
    /// 플레이어 공격력 계산 및 적용
    /// </summary>
    public int CalculatePlayerAttack(Player player)
    {
        if (player == null)
        {
            return 0;
        }

        pendingOverwhelmingDamage = 0;
        pendingBloodSuckingPercent = 0;
        currentTurnAttackBonus = 0;
        player.SetTurnSynergyAttackBonus(0);
        player.ApplyAttackStats();

        List<SynergyTotalData> activatedSynergies = SynergyActivationSelector.Select(GameData.Instance?.synergyIDList?.Values);
        for (int i = 0; i < activatedSynergies.Count; i++)
        {
            ApplyActivatedSynergy(player, activatedSynergies[i]);
        }

        return player.AttackValue;
    }

    /// <summary>
    /// 방어 카드가 있으면 방어도 적용 및 방어 모션 후, Enforce 애니메이션을 재생합니다.
    /// </summary>
    public IEnumerator PlayPreAttackSetupPhase(
        Player player,
        PlayerAnimation playerAnimation,
        int attackValue)
    {
        if (player != null && player.HasDefenseCardsOnField())
        {
            yield return player.ApplyDefenseStatsWithEffect();
        }

        if (playerAnimation != null)
        {
            yield return PlayEnforceAnimation(playerAnimation, attackValue);
        }
    }

    /// <summary>
    /// 플레이어 강화 애니메이션 재생
    /// </summary>
    public IEnumerator PlayEnforceAnimation(PlayerAnimation playerAnimation, int attackValue)
    {
        if (playerAnimation == null) yield break;

        playerAnimation.PlayEnforce(attackValue);
        yield return playerAnimation.WaitForEnforceAnimationComplete(attackValue);
    }

    /// <summary>
    /// 플레이어 방어력 이펙트 적용
    /// </summary>
    public IEnumerator ApplyDefenseEffect(Player player)
    {
        if (player == null) yield break;
        yield return player.ApplyDefenseStatsWithEffect();
    }

    /// <summary>
    /// Move 애니메이션 상태와 함께 공격 위치로 이동
    /// </summary>
    public IEnumerator MovePlayerToAttackPosition(
        Player player,
        PlayerAnimation playerAnimation,
        Vector3? attackAnchorPosition,
        bool isAreaAttack,
        int attackValue)
    {
        if (player == null)
        {
            yield break;
        }

        if (playerAnimation != null)
        {
            yield return playerAnimation.WaitUntilMoveState(attackValue);
        }

        yield return player.MoveToAttackPosition(attackAnchorPosition, isAreaAttack);
    }

    /// <summary>
    /// 플레이어 Attack 애니메이션 재생 및 데미지 적용
    /// </summary>
    public IEnumerator ExecutePlayerAttack(Player player, PlayerAnimation playerAnimation, 
        int currentAttack, List<IDamageable> targets)
    {
        if (player == null || targets == null) yield break;

        if (playerAnimation != null)
        {
            playerAnimation.TriggerAttack();
            yield return playerAnimation.WaitUntilAttackHitFrame(currentAttack);
            AttackEffectSpawner.SpawnOnTargets(currentAttack, targets);
        }
        else
        {
            AttackEffectSpawner.SpawnOnTargets(currentAttack, targets);
        }

        int totalDealtDamage = 0;
        List<IDamageable> damagedTargets = new List<IDamageable>();
        foreach (IDamageable target in targets)
        {
            if (target != null && target.IsAlive)
            {
                BaseUnit targetUnit = target as BaseUnit;
                int finalDamage = player.ApplyOutgoingStatusEffects(currentAttack, targetUnit);
                if (targetUnit != null)
                {
                    targetUnit.TakeDamage(finalDamage, player);
                }
                else
                {
                    target.TakeDamage(finalDamage);
                }
                totalDealtDamage += finalDamage;
                damagedTargets.Add(target);
            }
        }

        eventDomain?.RecordSingleAttackDamage(totalDealtDamage);

        ApplyOnHitSynergies(player, currentAttack, damagedTargets);

        if (playerAnimation != null)
        {
            yield return playerAnimation.WaitForAttackAnimationComplete(currentAttack);
        }

        yield return player.ReturnToOriginalPosition();

        if (playerAnimation != null)
        {
            yield return null;
            playerAnimation.ResetAnimationState();
        }
    }

    private void ApplyActivatedSynergy(Player player, SynergyTotalData entry)
    {
        if (player == null || entry?.synergyData == null || string.IsNullOrEmpty(entry.synergyData.ID))
        {
            return;
        }

        string synergyId = entry.synergyData.ID;
        int effectValue = GetActiveEffectValue(GetEffect1Values(entry), entry.count);
        if (effectValue <= 0)
        {
            return;
        }

        switch (synergyId)
        {
            case SynergyHonestyId:
                if (effectValue > battlePermanentAttackBonus)
                {
                    battlePermanentAttackBonus = effectValue;
                    player.SetBattleSynergyAttackBonus(battlePermanentAttackBonus);
                }
                break;

            case SynergyShieldAttackId:
                int shieldBonus = Mathf.RoundToInt(player.ProtectionValue * (effectValue / 100f));
                AddTurnAttackBonus(player, shieldBonus);
                break;

            case SynergyBasicId:
                int totalStars = GetTotalBattleCardStars();
                AddTurnAttackBonus(player, 5 + totalStars * 5);
                break;

            case SynergyOverwhelmingId:
                pendingOverwhelmingDamage = effectValue;
                ApplyOverwhelmingDamage(player);
                break;

            case SynergyBloodSuckingId:
                pendingBloodSuckingPercent = effectValue;
                break;

            case SynergyPlunderId:
                int plunderCardCount = CountAttackFieldSynergyCards(SynergyPlunderId);
                int plunderGold = plunderCardCount * effectValue;
                if (plunderGold > 0 && DataCenter.Instance != null)
                {
                    DataCenter.Instance.SetMoney(plunderGold);
                }
                break;

            case SynergyMysteryId:
                // TODO: 마법 시스템 구현 후 Mystery(무작위 마법 생성) 연동
                break;

            case SynergyDarknessId:
                ApplyDarknessCurse(effectValue);
                break;
        }
    }

    private void AddTurnAttackBonus(Player player, int amount)
    {
        if (player == null || amount <= 0)
        {
            return;
        }

        currentTurnAttackBonus += amount;
        player.SetTurnSynergyAttackBonus(currentTurnAttackBonus);
    }

    private void ApplyOverwhelmingDamage(Player player)
    {
        if (pendingOverwhelmingDamage <= 0 || battleManager == null)
        {
            return;
        }

        BattleSetupController setupController = battleManager.GetSetupController();
        if (setupController == null)
        {
            return;
        }

        IEnumerable<Monster> monsters = setupController.GetPrimaryMonsters();
        foreach (Monster monster in monsters)
        {
            if (monster != null && monster.IsAlive)
            {
                monster.TakeDamage(pendingOverwhelmingDamage, player);
            }
        }
    }

    private void ApplyDarknessCurse(int curseStack)
    {
        if (curseStack <= 0 || battleManager == null)
        {
            return;
        }

        BattleSetupController setupController = battleManager.GetSetupController();
        if (setupController == null)
        {
            return;
        }

        IEnumerable<Monster> monsters = setupController.GetPrimaryMonsters();
        foreach (Monster monster in monsters)
        {
            if (monster == null || !monster.IsAlive)
            {
                continue;
            }

            StatusEffectController statusEffectController = StatusEffectController.Resolve(monster);
            if (statusEffectController != null)
            {
                statusEffectController.TryApplyStatus(StatusEffectController.CurseStatusId, curseStack);
            }
        }
    }

    private void ApplyOnHitSynergies(Player player, int attackPower, List<IDamageable> hitTargets)
    {
        if (player == null || pendingBloodSuckingPercent <= 0)
        {
            return;
        }

        int healAmount = BloodSuckingEffectSpawner.CalculateHeal(attackPower, pendingBloodSuckingPercent);
        if (BloodSuckingEffectSpawner.CanApplyHeal(player.CurrentHealth, player.MaxHealth, healAmount))
        {
            player.Heal(healAmount);
        }

        BloodSuckingEffectSpawner.SpawnFromTargets(player.transform, hitTargets);
    }

    private int GetActiveEffectValue(List<int> effectValues, int synergyCount)
    {
        if (effectValues == null || effectValues.Count == 0 || synergyCount <= 0)
        {
            return 0;
        }

        int index = Mathf.Clamp(synergyCount - 1, 0, effectValues.Count - 1);
        int effectValue = effectValues[index];
        return effectValue > 0 ? effectValue : 0;
    }

    private List<int> GetEffect1Values(SynergyTotalData synergyData)
    {
        return synergyData?.synergyData != null ? synergyData.synergyData.Effect1Synergys : null;
    }

    private int GetTotalBattleCardStars()
    {
        if (GameData.Instance == null)
        {
            return 0;
        }

        int totalStars = 0;
        foreach (CardData cardData in GameData.Instance.attackField)
        {
            if (cardData != null)
            {
                totalStars += cardData.star;
            }
        }

        foreach (CardData cardData in GameData.Instance.defenseField)
        {
            if (cardData != null)
            {
                totalStars += cardData.star;
            }
        }

        return totalStars;
    }

    private int CountAttackFieldSynergyCards(string synergyId)
    {
        if (GameData.Instance == null || string.IsNullOrEmpty(synergyId))
        {
            return 0;
        }

        int count = 0;
        foreach (CardData cardData in GameData.Instance.attackField)
        {
            if (cardData == null)
            {
                continue;
            }

            bool hasSynergy = cardData.synergy1ID == synergyId
                              || cardData.synergy2ID == synergyId
                              || cardData.synergy3ID == synergyId;
            if (hasSynergy)
            {
                count++;
            }
        }

        return count;
    }

    /// <summary>
    /// 몬스터 공격 시퀀스 실행
    /// </summary>
    public IEnumerator ExecuteMonsterAttackSequence(BattleSetupController setupController)
    {
        if (setupController == null) yield break;

        var player = setupController.GetPlayer();
        var primaryMonsters = setupController.GetPrimaryMonsters();

        if (player == null) yield break;

        // 몬스터 공격 대기
        yield return new WaitForSeconds(0.5f);

        setupController.RefreshSelectedTargetLock();

        List<Monster> aliveMonsters = primaryMonsters.Where(m => m != null && m.IsAlive).ToList();

        foreach (Monster monster in aliveMonsters)
        {
            if (monster == null || !monster.IsAlive)
            {
                continue;
            }

            yield return monster.PerformAttack(player);

            if (!player.IsAlive)
            {
                Debug.Log("Player defeated.");
                if (battleManager != null)
                {
                    yield return battleManager.HandleDefeat();
                }
                yield break;
            }
        }
    }

    /// <summary>
    /// 승리 체크
    /// </summary>
    public bool CheckVictory(BattleSetupController setupController)
    {
        if (setupController == null) return false;

        var primaryMonsters = setupController.GetPrimaryMonsters();
        List<Monster> aliveMonsters = primaryMonsters.Where(m => m != null && m.IsAlive).ToList();
        return aliveMonsters.Count == 0;
    }
}

/// <summary>
/// 전투 초기화 결과
/// </summary>
public class CombatInitializationResult
{
    public Player player { get; set; }
    public List<IDamageable> playerTargets { get; set; }
    public Monster primaryMonsterTarget { get; set; }
    public Vector3? attackAnchorPosition { get; set; }
    public PlayerAnimation playerAnimation { get; set; }
}

