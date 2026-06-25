using System.Collections;
using System.Collections.Generic;
using System.Linq;
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

    [SerializeField] private bool playerAttackHitsAll;
    [SerializeField] private float statAnimationWaitTime = 1.0f;

    private BattleManager battleManager;
    private bool isInitialized;
    private int battlePermanentAttackBonus;
    private int pendingOverwhelmingDamage;
    private int pendingBloodSuckingPercent;
    private int preparedAttackValue;

    public float GetStatAnimationWaitTime() => statAnimationWaitTime;
    public bool GetPlayerAttackHitsAll() => playerAttackHitsAll;
    public bool IsInitialized => isInitialized;

    public void Initialize(BattleManager manager)
    {
        if (isInitialized)
        {
            Debug.LogWarning("BattleCombatController has already been initialized.");
            return;
        }

        battleManager = manager;
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
        isInitialized = false;
        battlePermanentAttackBonus = 0;
        preparedAttackValue = 0;
        ResetTurnScopedSynergyState();
    }

    public int GetPreparedAttackValue()
    {
        return preparedAttackValue;
    }

    /// <summary>
    /// 공격 시작 전 시너지 연출 및 선행 효과를 모두 처리합니다.
    /// </summary>
    public IEnumerator ExecutePreAttackSynergyPhase(Player player, SynergyUI synergyUI)
    {
        if (player == null)
        {
            preparedAttackValue = 0;
            yield break;
        }

        if (GameData.Instance != null)
        {
            GameData.Instance.GetSynergyData();
        }

        if (synergyUI != null)
        {
            yield return synergyUI.PlaySynergyActivationSequence();
        }

        preparedAttackValue = CalculatePlayerAttack(player);
        ApplyPreHitSynergies();
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

        // 타겟 선택
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
        if (player == null) return 0;

        player.SetTurnSynergyAttackBonus(0);
        ApplyBattleWideSynergies(player);
        player.ApplyAttackStats();
        int attackValue = player.AttackValue;
        attackValue = ApplyAttackCalculationSynergies(player, attackValue);
        int extraTurnBonus = Mathf.Max(0, attackValue - player.AttackValue);
        player.SetTurnSynergyAttackBonus(extraTurnBonus);
        return attackValue;
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

        playerAnimation.TriggerAttackByValue(attackValue);
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

            float effectDelay = playerAnimation.GetAttackEffectDelay();
            if (effectDelay > 0f)
            {
                yield return new WaitForSeconds(effectDelay);
            }

            AttackEffectSpawner.SpawnOnTargets(currentAttack, targets);

            yield return playerAnimation.WaitUntilAttackState(currentAttack);
            yield return new WaitForSeconds(playerAnimation.GetAttackDamageDelay());
        }
        else
        {
            AttackEffectSpawner.SpawnOnTargets(currentAttack, targets);
        }

        int totalDealtDamage = 0;
        foreach (IDamageable target in targets)
        {
            if (target != null && target.IsAlive)
            {
                BaseUnit targetUnit = target as BaseUnit;
                int finalDamage = player.ApplyOutgoingStatusEffects(currentAttack, targetUnit);
                target.TakeDamage(finalDamage);
                totalDealtDamage += finalDamage;
            }
        }
        ApplyOnHitSynergies(player, totalDealtDamage);

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

    private void ApplyBattleWideSynergies(Player player)
    {
        if (player == null)
        {
            return;
        }

        if (TryGetSynergyData(SynergyHonestyId, out SynergyTotalData honestySynergy))
        {
            int honestyBonus = GetActiveEffectValue(GetEffect1Values(honestySynergy), honestySynergy.count);
            if (honestyBonus > battlePermanentAttackBonus)
            {
                battlePermanentAttackBonus = honestyBonus;
                player.SetBattleSynergyAttackBonus(battlePermanentAttackBonus);
            }
        }
    }

    private int ApplyAttackCalculationSynergies(Player player, int baseAttack)
    {
        int attackValue = baseAttack + battlePermanentAttackBonus;
        pendingOverwhelmingDamage = 0;
        pendingBloodSuckingPercent = 0;

        if (player != null && TryGetSynergyData(SynergyShieldAttackId, out SynergyTotalData shieldAttackSynergy))
        {
            int shieldPercent = GetActiveEffectValue(GetEffect1Values(shieldAttackSynergy), shieldAttackSynergy.count);
            int shieldBonus = Mathf.RoundToInt(player.ProtectionValue * (shieldPercent / 100f));
            attackValue += shieldBonus;
        }

        if (TryGetSynergyData(SynergyBasicId, out SynergyTotalData basicSynergy))
        {
            int activeBasicValue = GetActiveEffectValue(GetEffect1Values(basicSynergy), basicSynergy.count);
            if (activeBasicValue > 0)
            {
                int totalStars = GetTotalBattleCardStars();
                attackValue += 5 + totalStars * 5;
            }
        }

        if (TryGetSynergyData(SynergyOverwhelmingId, out SynergyTotalData overwhelmingSynergy))
        {
            pendingOverwhelmingDamage = GetActiveEffectValue(GetEffect1Values(overwhelmingSynergy), overwhelmingSynergy.count);
        }

        if (TryGetSynergyData(SynergyBloodSuckingId, out SynergyTotalData bloodSuckingSynergy))
        {
            pendingBloodSuckingPercent = GetActiveEffectValue(GetEffect1Values(bloodSuckingSynergy), bloodSuckingSynergy.count);
        }

        if (TryGetSynergyData(SynergyPlunderId, out SynergyTotalData plunderSynergy))
        {
            int plunderPerCard = GetActiveEffectValue(GetEffect1Values(plunderSynergy), plunderSynergy.count);
            if (plunderPerCard > 0)
            {
                int plunderCardCount = CountAttackFieldSynergyCards(SynergyPlunderId);
                int plunderGold = plunderCardCount * plunderPerCard;
                if (plunderGold > 0 && DataCenter.Instance != null)
                {
                    DataCenter.Instance.SetMoney(plunderGold);
                }
            }
        }

        if (TryGetSynergyData(SynergyMysteryId, out SynergyTotalData mysterySynergy))
        {
            int mysteryGrade = GetActiveEffectValue(GetEffect1Values(mysterySynergy), mysterySynergy.count);
            // TODO: 마법 시스템 구현 후 Mystery(무작위 마법 생성) 연동
            _ = mysteryGrade;
        }

        return attackValue;
    }

    private void ApplyPreHitSynergies()
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
                monster.TakeDamage(pendingOverwhelmingDamage);
            }
        }
    }

    private void ApplyOnHitSynergies(Player player, int totalDealtDamage)
    {
        if (player == null || totalDealtDamage <= 0 || pendingBloodSuckingPercent <= 0)
        {
            return;
        }

        int healAmount = Mathf.RoundToInt(totalDealtDamage * (pendingBloodSuckingPercent / 100f));
        if (healAmount > 0)
        {
            player.Heal(healAmount);
        }
    }

    private bool TryGetSynergyData(string synergyId, out SynergyTotalData synergyData)
    {
        synergyData = null;
        if (GameData.Instance == null || GameData.Instance.synergyIDList == null || string.IsNullOrEmpty(synergyId))
        {
            return false;
        }

        if (!GameData.Instance.synergyIDList.TryGetValue(synergyId, out synergyData) || synergyData == null)
        {
            return false;
        }

        return synergyData.synergyData != null;
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

        // 타겟 선택 해제
        foreach (Monster monster in primaryMonsters)
        {
            if (monster != null)
            {
                monster.SetTargeted(false);
            }
        }
        setupController.ClearSelectedTarget();

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

