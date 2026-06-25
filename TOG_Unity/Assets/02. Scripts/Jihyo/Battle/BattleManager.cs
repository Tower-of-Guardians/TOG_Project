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
    private MonsterEncounterData currentEncounterData;

    private void Awake()
    {
        InitializeControllers();
        ResolveSynergyUIIfNeeded();
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
        if (setupController != null)
        {
            setupController.Cleanup();
        }
        if (actionController != null)
        {
            actionController.Cleanup();
        }
        if (turnEndController != null)
        {
            turnEndController.Cleanup();
        }
        if (combatController != null)
        {
            combatController.Cleanup();
        }
    }

    public void Initialize(Player playerUnit, IEnumerable<Monster> monsters, Button attackBtn)
    {
        Initialize(playerUnit, monsters, attackBtn, null);
    }

    public void Initialize(
        Player playerUnit,
        IEnumerable<Monster> monsters,
        Button attackBtn,
        MonsterEncounterData encounterData)
    {
        if (isInitialized)
        {
            Debug.LogWarning("BattleManager has already been initialized.");
            return;
        }

        if (setupController == null)
        {
            Debug.LogError("BattleSetupController is not assigned.");
            return;
        }

        currentEncounterData = encounterData;
        setupController.SetupBattle(playerUnit, monsters, attackBtn);
        isInitialized = true;

        StartCoroutine(StartFirstTurnDelayed());
    }

    private IEnumerator StartFirstTurnDelayed()
    {
        yield return new WaitUntil(() => DIContainer.IsRegistered<TurnManager>());

        var turnManager = DIContainer.Resolve<TurnManager>();
        if (turnManager != null)
        {
            turnManager.Initialize();
            turnManager.ResetTurnNumber();
            turnManager.StartTurn();
            InvokeStatusEffectTurnStart();
            ShowSynergyUIForTurnStart();
        }
    }

    public void OnAttackButtonClicked()
    {
        if (isProcessingAttack) return;

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
        ResolveSynergyUIIfNeeded();
        if (synergyUI != null)
        {
            yield return synergyUI.HideWithFade();
        }

        CloseSynergyOverlayExtras();
    }

    private void CloseSynergyOverlayExtras()
    {
        if (DIContainer.IsRegistered<TooltipPresenter>())
        {
            TooltipPresenter tooltipPresenter = DIContainer.Resolve<TooltipPresenter>();
            tooltipPresenter?.CloseUI();
        }

        if (DIContainer.IsRegistered<CardInfoDomain>())
        {
            var cardInfoDomain = DIContainer.Resolve<CardInfoDomain>();
            cardInfoDomain.System.CloseView();
        }
    }

    private void ShowSynergyUIForTurnStart()
    {
        ResolveSynergyUIIfNeeded();
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

    private void ResolveSynergyUIIfNeeded()
    {
        if (synergyUI == null)
        {
            synergyUI = FindFirstObjectByType<SynergyUI>();
        }
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

        ResolveSynergyUIIfNeeded();
        yield return combatController.ExecutePreAttackSynergyPhase(initResult.player, synergyUI);
        yield return CloseSynergyOverlayUI();

        // 공격력 애니메이션 대기
        float statAnimationWaitTime = combatController.GetStatAnimationWaitTime();
        if (statAnimationWaitTime > 0f)
        {
            yield return new WaitForSeconds(statAnimationWaitTime);
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
        // ResultPresenter가 등록될 때까지 대기
        yield return new WaitUntil(() => DIContainer.IsRegistered<ResultDomain>());

        // Result 창 열기
        var resultDomain = DIContainer.Resolve<ResultDomain>();
        var resultData = new ResultData(0, 0);
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
}
