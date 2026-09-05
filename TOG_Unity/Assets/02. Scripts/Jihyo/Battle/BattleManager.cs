using System;
using System.Collections;
using System.Collections.Generic;
using Jongmin;
using UnityEngine;
using UnityEngine.UI;

public class BattleManager : MonoBehaviour
{
    [Header("Controllers")]
    [SerializeField] private BattleSetupController setupController;
    [SerializeField] private BattleActionController actionController;
    [SerializeField] private BattleTurnEndController turnEndController;
    [SerializeField] private BattleCombatController combatController;

    [Space(30f), Header("Effectors")]
    [SerializeField] private EffectDomain effectDomain;
    [SerializeField] private SynergyUI synergyUI;

    private bool isInitialized;
    private bool isProcessingAttack;
    private bool isBattleFinished;
    private MonsterEncounterData currentEncounterData;

    public bool CanInitializeBattle => setupController != null && actionController != null
        && turnEndController != null && combatController != null && effectDomain != null;

    private void Awake()
    {
        InitializeControllers();
    }

    private void OnDestroy()
    {
        CleanupControllers();
    }

    private void InitializeControllers()
    {
        if (setupController != null)
        {
            setupController.Initialize(this);
        }
        if (actionController != null)
        {
            actionController.Initialize(this);
        }
        if (turnEndController != null)
        {
            turnEndController.Initialize(this);
        }
        if (combatController != null)
        {
            combatController.Initialize(this);
        }
    }

    private void CleanupControllers()
    {
        if (combatController != null)
        {
            combatController.StopAllCoroutines();
            combatController.Cleanup();
        }
        if (setupController != null)
        {
            setupController.StopAllCoroutines();
            setupController.Cleanup();
        }
        if (actionController != null)
        {
            actionController.StopAllCoroutines();
            actionController.Cleanup();
        }
        if (turnEndController != null)
        {
            turnEndController.StopAllCoroutines();
            turnEndController.Cleanup();
        }
    }

    public void PrepareForNextBattle()
    {
        StopAllCoroutines();
        Player player = setupController != null ? setupController.GetPlayer() : null;
        CleanupControllers();
        ClearBattleCards();
        isInitialized = false;
        isProcessingAttack = false;
        isBattleFinished = false;
        currentEncounterData = null;

        if (player != null)
        {
            player.ResetAttackToBase();
            player.GetComponent<StatusEffectController>()?.ClearStatuses();
            player.GetComponent<PlayerAnimation>()?.ResetAnimationState();
        }

        InitializeControllers();
        effectDomain?.EnableBattleView();
    }

    private static void ClearBattleCards()
    {
        if (DIContainer.IsRegistered<HandDomain>())
        {
            HandDomain hand = DIContainer.Resolve<HandDomain>();
            if (hand != null && hand.Container != null)
            {
                foreach (Jongmin.Card card in new List<Jongmin.Card>(hand.Container.Cards))
                    hand.System.RemoveCard(card, false);
                hand.System.HoverCard = null;
            }
        }

        if (DIContainer.IsRegistered<FieldDomain>())
        {
            FieldDomain field = DIContainer.Resolve<FieldDomain>();
            if (field != null && field.AtkContainer != null && field.DefContainer != null)
            {
                foreach (Jongmin.Card card in new List<Jongmin.Card>(field.AtkContainer.Cards))
                    field.AtkSystem.RemoveCard(card, false);
                foreach (Jongmin.Card card in new List<Jongmin.Card>(field.DefContainer.Cards))
                    field.DefSystem.RemoveCard(card, false);
                field.AtkSystem.HoverCard = null;
                field.DefSystem.HoverCard = null;
            }
        }

        if (DIContainer.IsRegistered<DiscardDomain>())
        {
            DiscardDomain discard = DIContainer.Resolve<DiscardDomain>();
            if (discard != null && discard.Container != null)
            {
                foreach (Jongmin.Card card in new List<Jongmin.Card>(discard.Container.Cards))
                    discard.System.RemoveCard(card);
                discard.System.HoverCard = null;
                discard.System.CloseView();
            }
        }

        if (GameData.Instance == null || DataCenter.Instance == null) return;

        GameData gameData = GameData.Instance;
        gameData.handDeck.Clear();
        gameData.garbageDeck.Clear();
        gameData.notuseDeck.Clear();
        gameData.attackField.Clear();
        gameData.defenseField.Clear();
        foreach (CardData card in DataCenter.Instance.userDeck)
        {
            if (card != null) gameData.notuseDeck.Add(card.id);
        }
        gameData.Shuffle();
        gameData.InvokeDeckCountChange(DeckType.Draw);
        gameData.InvokeDeckCountChange(DeckType.Throw);
        gameData.GetSynergyData();
    }

    public void Initialize(Player playerUnit, IEnumerable<Monster> monsters, Button attackBtn)
    {
        Initialize(playerUnit, monsters, attackBtn, null);
    }

    public void Initialize(
        Player playerUnit,
        IEnumerable<Monster> monsters,
        Button attackBtn,
        MonsterEncounterData encounterData,
        Action<bool> onComplete = null)
    {
        if (isInitialized)
        {
            Debug.LogWarning("BattleManager has already been initialized.");
            onComplete?.Invoke(false);
            return;
        }

        if (setupController == null)
        {
            Debug.LogError("BattleSetupController is not assigned.");
            onComplete?.Invoke(false);
            return;
        }

        currentEncounterData = encounterData;
        setupController.SetupBattle(playerUnit, monsters, attackBtn);
        isInitialized = true;
        isBattleFinished = false;

        StartCoroutine(StartFirstTurnDelayed(onComplete));
    }

    private IEnumerator StartFirstTurnDelayed(Action<bool> onComplete)
    {
        yield return null;
        float deadline = Time.realtimeSinceStartup + 30f;
        while (!DIContainer.IsRegistered<TurnManager>() && Time.realtimeSinceStartup < deadline)
        {
            yield return null;
        }

        if (!DIContainer.IsRegistered<TurnManager>())
        {
            Debug.LogError("BattleManager: 첫 턴 초기화에 필요한 TurnManager가 준비되지 않았습니다.", this);
            isInitialized = false;
            onComplete?.Invoke(false);
            yield break;
        }

        var turnManager = DIContainer.Resolve<TurnManager>();
        if (turnManager != null)
        {
            while ((GameData.Instance == null ||
                GameData.Instance.notuseDeck.Count + GameData.Instance.garbageDeck.Count < turnManager.MaxHandCount)
                && Time.realtimeSinceStartup < deadline)
            {
                yield return null;
            }
            if (GameData.Instance == null ||
                GameData.Instance.notuseDeck.Count + GameData.Instance.garbageDeck.Count < turnManager.MaxHandCount)
            {
                Debug.LogError("BattleManager: 첫 턴을 시작할 카드 덱이 준비되지 않았습니다.", this);
                isInitialized = false;
                onComplete?.Invoke(false);
                yield break;
            }
            turnManager.Initialize();
            turnManager.ResetTurnNumber();
            turnManager.StartTurn();
            InvokeStatusEffectTurnStart();
            ShowSynergyUIForTurnStart();
            onComplete?.Invoke(true);
            yield break;
        }

        isInitialized = false;
        onComplete?.Invoke(false);
    }

    public void OnAttackButtonClicked()
    {
        if (!isInitialized || isProcessingAttack || isBattleFinished) return;

        // 턴 시작 처리
        if (actionController != null)
        {
            actionController.OnTurnStart();
        }

        isProcessingAttack = true;
        StartCoroutine(ProcessAttackSequence());
    }

    private IEnumerator CloseSynergyOverlayUI()
    {
        if (synergyUI != null)
        {
            yield return synergyUI.HideWithFade();
        }

        CloseSynergyOverlayExtras();
    }

    private void CloseSynergyOverlayExtras()
    {
        if (DIContainer.IsRegistered<CardInfoDomain>())
        {
            var cardInfoDomain = DIContainer.Resolve<CardInfoDomain>();
            cardInfoDomain.System.CloseView();
        }
    }

    private void ShowSynergyUIForTurnStart()
    {
        RefreshFieldSynergyState();
        synergyUI?.SetVisible(true);
    }

    private static void RefreshFieldSynergyState()
    {
        if (GameData.Instance == null)
        {
            return;
        }

        GameData.Instance.attackField.Clear();
        GameData.Instance.defenseField.Clear();
        GameData.Instance.GetSynergyData();
    }

    private IEnumerator ProcessAttackSequence()
    {
        if (setupController == null || combatController == null)
        {
            isProcessingAttack = false;
            yield break;
        }
        
        // 카드 버리기 및 전투 UI 비활성화
        yield return effectDomain.DiscardHandCards();

        // 전투 초기화 및 타겟 선택
        var initResult = combatController.InitializeCombat(setupController);
        if (initResult == null)
        {
            isProcessingAttack = false;
            yield break;
        }

        yield return combatController.ExecutePreAttackSynergyPhase(initResult.player, initResult.playerAnimation, synergyUI);

        if (combatController.PlayedSynergyMotion)
        {
            StartCoroutine(CloseSynergyOverlayUI());
        }
        else
        {
            yield return CloseSynergyOverlayUI();

            float statAnimationWaitTime = combatController.GetStatAnimationWaitTime();
            if (statAnimationWaitTime > 0f)
            {
                yield return new WaitForSeconds(statAnimationWaitTime);
            }
        }

        int currentAttack = combatController.GetPreparedAttackValue();

        yield return combatController.PlayPreAttackSetupPhase(
            initResult.player,
            initResult.playerAnimation,
            currentAttack
        );

        // 플레이어를 공격 위치로 이동
        bool playerAttackHitsAll = combatController.GetPlayerAttackHitsAll();
        yield return combatController.MovePlayerToAttackPosition(
            initResult.player,
            initResult.playerAnimation,
            initResult.attackAnchorPosition,
            playerAttackHitsAll,
            currentAttack
        );

        // 플레이어 공격 트리거 및 데미지 적용
        yield return combatController.ExecutePlayerAttack(
            initResult.player,
            initResult.playerAnimation,
            currentAttack,
            initResult.playerTargets
        );

        // 플레이어 공격 후 죽은 몬스터 제거
        setupController.RemoveDeadMonsters();

        // 승리 체크
        if (combatController.CheckVictory(setupController))
        {
            // 필드 카드 버리기
            yield return new WaitForSeconds(0.5f);
            yield return effectDomain.DiscardFieldCards(FieldType.Attack);
            yield return effectDomain.DiscardFieldCards(FieldType.Defense);
            RefreshFieldSynergyState();
            yield return new WaitForSeconds(1f);
            effectDomain.EnableBattleView();
            
            yield return HandleVictory();
            isProcessingAttack = false;
            yield break;
        }

        // 몬스터 공격 시퀀스
        yield return combatController.ExecuteMonsterAttackSequence(setupController);

        // 플레이어가 죽었는지 확인
        if (initResult.player != null && !initResult.player.IsAlive)
        {
            isProcessingAttack = false;
            yield break;
        }

        // 몬스터 공격 후 죽은 몬스터들 제거
        setupController.RemoveDeadMonsters();

        // 필드 카드 버리기
        yield return new WaitForSeconds(0.5f);
        yield return effectDomain.DiscardFieldCards(FieldType.Attack);
        yield return effectDomain.DiscardFieldCards(FieldType.Defense);
        RefreshFieldSynergyState();
        yield return new WaitForSeconds(1f);

        // 최종 승리 체크
        if (combatController.CheckVictory(setupController))
        {
            // 필드 카드 버리기
            yield return new WaitForSeconds(0.5f);
            yield return effectDomain.DiscardFieldCards(FieldType.Attack);
            yield return effectDomain.DiscardFieldCards(FieldType.Defense);
            RefreshFieldSynergyState();
            yield return new WaitForSeconds(1f);
            effectDomain.EnableBattleView();
            
            yield return HandleVictory();
            isProcessingAttack = false;
            yield break;
        }

        // 턴 종료 요청
        effectDomain.EnableBattleView();
        RequestTurnEnd();
        isProcessingAttack = false;
    }

    public void RequestDrawCards(int count = -1)
    {
        if (turnEndController == null)
        {
            Debug.LogWarning("BattleTurnEndController is not assigned.");
            return;
        }

        turnEndController.DrawCards(count);
    }

    public void RequestTurnEnd()
    {
        if (turnEndController == null)
        {
            Debug.LogWarning("BattleTurnEndController is not assigned.");
            return;
        }

        turnEndController.ProcessTurnEnd();

        if (combatController != null)
        {
            combatController.ResetTurnScopedSynergyState();
        }

        // 공격 종료 시 턴 증가
        if (DIContainer.IsRegistered<TurnManager>())
        {
            var turnManager = DIContainer.Resolve<TurnManager>();
            if (turnManager != null)
            {
                InvokeStatusEffectTurnEnd();
                turnManager.EndTurn();
                turnManager.StartTurn();
                InvokeStatusEffectTurnStart();
                ShowSynergyUIForTurnStart();
            }
        }
    }

    private void InvokeStatusEffectTurnStart()
    {
        if (setupController == null)
        {
            return;
        }

        Player player = setupController.GetPlayer();
        player?.NotifyTurnStartStatusEffects();

        List<Monster> monsters = setupController.GetPrimaryMonsters();
        for (int i = 0; i < monsters.Count; i++)
        {
            Monster monster = monsters[i];
            if (monster == null)
            {
                continue;
            }

            monster.NotifyTurnStartStatusEffects();
            monster.PrepareActionForTurn();
        }
    }

    private void InvokeStatusEffectTurnEnd()
    {
        if (setupController == null)
        {
            return;
        }

        Player player = setupController.GetPlayer();
        player?.NotifyTurnEndStatusEffects();

        List<Monster> monsters = setupController.GetPrimaryMonsters();
        for (int i = 0; i < monsters.Count; i++)
        {
            monsters[i]?.NotifyTurnEndStatusEffects();
        }
    }

    public IEnumerator HandleVictory()
    {
        if (isBattleFinished) yield break;
        isBattleFinished = true;
        int totalGold = CalculateTotalGold();
        int totalExp = CalculateTotalExp();
        bool isLevelUp = WillLevelUp(totalExp);

        ApplyEncounterRewards(totalGold, totalExp);

        yield return new WaitUntil(() => DIContainer.IsRegistered<ResultDomain>());

        var resultDomain = DIContainer.Resolve<ResultDomain>();
        var resultData = new ResultData(totalGold, totalExp, isLevelUp);
        resultDomain.Show(resultData);
    }

    public IEnumerator HandleDefeat()
    {
        if (isBattleFinished) yield break;
        isBattleFinished = true;
        // ResultPresenter가 등록될 때까지 대기
        yield return new WaitUntil(() => DIContainer.IsRegistered<ResultDomain>());

        // Result 창 열기
        var resultDomain = DIContainer.Resolve<ResultDomain>();
        var resultData = new ResultData(0, 0, isVictory: false);
        resultDomain.Show(resultData);
    }

    private int CalculateTotalGold()
    {
        return currentEncounterData != null ? currentEncounterData.Gold : 0;
    }

    private int CalculateTotalExp()
    {
        return currentEncounterData != null ? currentEncounterData.Exp : 0;
    }

    private void ApplyEncounterRewards(int gold, int exp)
    {
        if (DataCenter.Instance == null)
        {
            return;
        }

        if (gold > 0)
        {
            DataCenter.Instance.SetMoney(gold);
        }

        if (exp > 0)
        {
            DataCenter.Instance.SetPlayerLevel(exp);
        }
    }

    private static bool WillLevelUp(int exp)
    {
        if (DataCenter.Instance == null || exp <= 0)
        {
            return false;
        }

        PlayerState playerState = DataCenter.Instance.playerstate;
        return playerState.level < 9 && playerState.experience + exp >= playerState.maxexperience;
    }

    public void RegisterMonster(Monster monster)
    {
        if (setupController != null)
        {
            setupController.RegisterMonster(monster);
        }
    }

    public void UnregisterMonster(Monster monster)
    {
        if (setupController != null)
        {
            setupController.UnregisterMonster(monster);
        }
    }

    public void ConfigureAttackButton(Button button)
    {
        if (setupController != null)
        {
            setupController.ConfigureAttackButton(button);
        }
    }

    public void SetPlayer(Player playerUnit)
    {
        if (setupController != null)
        {
            setupController.SetPlayer(playerUnit);
        }
    }

    public BattleSetupController GetSetupController()
    {
        return setupController;
    }

    public bool IsProcessingAttack()
    {
        return isProcessingAttack;
    }

    public void ForceVictoryForDebug()
    {
        if (!isInitialized || isProcessingAttack || isBattleFinished)
        {
            return;
        }

        StartCoroutine(ForceVictoryRoutine());
    }

    public void PlaySynergyActivationForDebug()
    {
        if (!isInitialized || isProcessingAttack)
        {
            return;
        }

        StartCoroutine(PlaySynergyActivationForDebugRoutine());
    }

    private IEnumerator PlaySynergyActivationForDebugRoutine()
    {
        if (setupController == null || combatController == null)
        {
            yield break;
        }

        Player player = setupController.GetPlayer();
        if (player == null)
        {
            yield break;
        }

        isProcessingAttack = true;

        if (GameData.Instance != null)
        {
            GameData.Instance.GetSynergyData();
        }

        synergyUI?.SetVisible(true);

        PlayerAnimation playerAnimation = player.GetComponent<PlayerAnimation>();
        playerAnimation?.ResetAnimationState();

        yield return combatController.ExecutePreAttackSynergyPhase(player, playerAnimation, synergyUI);
        playerAnimation?.StopSynergyMotion();

        isProcessingAttack = false;
    }

    private IEnumerator ForceVictoryRoutine()
    {
        isProcessingAttack = true;
        KillAllPrimaryMonsters();
        yield return new WaitForSeconds(0.5f);

        if (setupController != null)
        {
            setupController.RemoveDeadMonsters();
        }

        yield return HandleVictory();
        isProcessingAttack = false;
    }

    private void KillAllPrimaryMonsters()
    {
        if (setupController == null)
        {
            return;
        }

        List<Monster> monsters = setupController.GetPrimaryMonsters();
        for (int i = 0; i < monsters.Count; i++)
        {
            Monster monster = monsters[i];
            if (monster == null || !monster.IsAlive)
            {
                continue;
            }

            int lethalDamage = monster.MaxHealth + Mathf.CeilToInt(monster.ProtectionValue) + 1;
            monster.TakeDamage(lethalDamage);
        }
    }
}
