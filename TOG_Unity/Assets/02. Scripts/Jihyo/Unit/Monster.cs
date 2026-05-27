using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public enum MonsterActionType
{
    Attack,
    Guard,
    ApplyStatus,
    Heal,
    Summon,
    Ready,
    Stun
}

public enum MonsterActionTargetType
{
    None,
    Self,
    Player,
    AllyRandom,
    AllyRandomExceptSelf,
    AlliesAll
}

public enum MonsterActionPatternType
{
    Random = 1,
    Cycle = 2
}

[Serializable]
public class MonsterActionDefinition
{
    public string ActionId;
    public MonsterActionType ActionType = MonsterActionType.Attack;
    public MonsterActionTargetType TargetType = MonsterActionTargetType.Player;
    public int MinValue = 0;
    public int MaxValue = 0;
    public string StatusEffectId;
    public int StatusStack = 1;
}

public class Monster : BaseUnit, IPointerClickHandler
{
    [Header("Data")]
    [SerializeField] private int defaultAttack = 5;
    [SerializeField] private string monsterDataId;
    [SerializeField] private bool useMonsterDataActions = true;
    [SerializeField] private MonsterActionPatternType actionPatternType = MonsterActionPatternType.Random;
    [SerializeField] private List<MonsterActionDefinition> actionDefinitions = new List<MonsterActionDefinition>();

    [Header("Status UI")]
    [SerializeField] private Transform attackAnchor;
    [SerializeField] private GameObject targetIndicator;
    [SerializeField] private SpriteRenderer actionIndicatorRenderer;
    [SerializeField] private Sprite actionAttackSprite;
    [SerializeField] private Sprite actionShieldSprite;
    [SerializeField] private Sprite actionBuffSprite;
    [SerializeField] private Sprite actionDebuffSprite;

    [Header("Animation")]
    private MonsterAnimation monsterAnimation;
    [SerializeField] private SpriteRenderer spriteRenderer;

    [Header("Movement")]
    [SerializeField] private float attackMoveOffset = 0f;
    [SerializeField] private float attackMoveDuration = 0.2f;
    
    [Header("Attack Timing")]
    [SerializeField] private float damageApplyDelay = 0.5f;

    private Vector3 initialSpriteLocalPosition;
    private bool hasSavedInitialPosition;
    private const int NormalSortingOrder = 5;
    private const int AttackSortingOrder = 8;

    public event Action<Monster> Clicked;
    private BattleManager battleManager;
    private Coroutine registrationRoutine;
    private int actionCursor;
    private MonsterData loadedMonsterData;
    private MonsterActionDefinition preparedAction;
    private int preparedActionValue;
    private bool hasPreparedAction;
    private bool hasGuardShieldPendingExpire;
    private int guardShieldAppliedTurnNumber = -1;
    private GameObject attackStatusRoot;

    protected override void Awake()
    {
        ApplyMonsterDataIfConfigured();
        base.Awake();
        ResolveStatusUIReferences();
        currentHealth = maxHealth;
        BuildActionsFromMonsterDataIfNeeded();
        ConfigureMonsterTraits();
        InitializeAnimation();
        SaveInitialPosition();
        SetTargeted(false);
        RegisterBattleManager();
    }

    private void InitializeAnimation()
    {
        if (monsterAnimation == null)
        {
            monsterAnimation = GetComponent<MonsterAnimation>();
        }
    }

    private void OnEnable()
    {
        RegisterBattleManager();
    }

    private void OnDisable()
    {
        if (battleManager != null)
        {
            battleManager.UnregisterMonster(this);
            battleManager = null;
        }

        if (registrationRoutine != null)
        {
            StopCoroutine(registrationRoutine);
            registrationRoutine = null;
        }
    }

    public void SetTargeted(bool isTargeted)
    {
        if (targetIndicator != null)
        {
            targetIndicator.SetActive(isTargeted);
        }
    }

    public Transform AttackAnchor => attackAnchor != null ? attackAnchor : transform;

    public int GetAttackValue()
    {
        return defaultAttack;
    }

    protected virtual void ConfigureMonsterTraits() { }

    protected void SetMonsterDataId(string id)
    {
        monsterDataId = id;
    }

    protected bool TryGetLoadedMonsterData(out MonsterData data)
    {
        data = loadedMonsterData;
        return data != null;
    }

    protected void OverrideBehavior(MonsterActionPatternType patternType, params MonsterActionDefinition[] actions)
    {
        actionPatternType = patternType;
        actionCursor = 0;
        actionDefinitions.Clear();
        if (actions != null && actions.Length > 0)
        {
            actionDefinitions.AddRange(actions);
        }
    }

    public IEnumerator PerformAttack(IDamageable target)
    {
        if (target == null || !target.IsAlive)
        {
            yield break;
        }

        MonsterActionDefinition selectedAction;
        int actionValue;
        ResolveActionForExecution(out selectedAction, out actionValue);

        bool isAttackAction = selectedAction == null || selectedAction.ActionType == MonsterActionType.Attack;
        SaveInitialPosition();

        if (isAttackAction)
        {
            SetSortingOrder(AttackSortingOrder);
        }

        if (isAttackAction && attackMoveOffset > 0f)
        {
            Vector3 targetPosition = GetTargetAttackPosition(target);
            yield return StartCoroutine(MoveSpriteToPosition(targetPosition, attackMoveDuration));
        }

        PlayActionAnimation(selectedAction);

        // 공격 중 체력이 감소하는 타이밍
        yield return new WaitForSeconds(damageApplyDelay);

        ExecuteSelectedAction(selectedAction, target, actionValue);

        if (monsterAnimation != null)
        {
            MonsterActionType actionType = selectedAction != null ? selectedAction.ActionType : MonsterActionType.Attack;
            yield return StartCoroutine(monsterAnimation.WaitForActionAnimationComplete(actionType));
        }

        if (isAttackAction && attackMoveOffset > 0f)
        {
            yield return StartCoroutine(MoveSpriteToPosition(initialSpriteLocalPosition, attackMoveDuration));
        }

        if (isAttackAction)
        {
            SetSortingOrder(NormalSortingOrder);
        }
    }

    private void ResolveActionForExecution(out MonsterActionDefinition selectedAction, out int actionValue)
    {
        if (!TryConsumePreparedAction(out selectedAction, out actionValue))
        {
            selectedAction = SelectNextAction();
            actionValue = selectedAction != null ? ResolveActionValue(selectedAction.MinValue, selectedAction.MaxValue) : defaultAttack;
        }
    }

    private void PlayActionAnimation(MonsterActionDefinition selectedAction)
    {
        if (monsterAnimation == null)
        {
            return;
        }

        MonsterActionType actionType = selectedAction != null ? selectedAction.ActionType : MonsterActionType.Attack;
        switch (actionType)
        {
            case MonsterActionType.Guard:
                monsterAnimation.PlayDefenseAnimation();
                break;
            case MonsterActionType.ApplyStatus:
                monsterAnimation.PlayCurseAnimation();
                break;
            default:
                monsterAnimation.PlayAttackAnimation();
                break;
        }
    }

    public void PrepareActionForTurn()
    {
        preparedAction = SelectNextAction();
        preparedActionValue = preparedAction != null ? ResolveActionValue(preparedAction.MinValue, preparedAction.MaxValue) : defaultAttack;
        hasPreparedAction = true;
        RefreshUI();
    }

    private void ApplyMonsterDataIfConfigured()
    {
        if (string.IsNullOrEmpty(monsterDataId) || DataCenter.Instance == null)
        {
            return;
        }

        DataCenter.Instance.GetMonsterData(monsterDataId, data => loadedMonsterData = data);
        if (loadedMonsterData == null)
        {
            return;
        }

        if (loadedMonsterData.HP > 0)
        {
            maxHealth = loadedMonsterData.HP;
            currentHealth = maxHealth;
        }

        if (loadedMonsterData.PatternType == (int)MonsterActionPatternType.Random
            || loadedMonsterData.PatternType == (int)MonsterActionPatternType.Cycle)
        {
            actionPatternType = (MonsterActionPatternType)loadedMonsterData.PatternType;
        }
    }

    private void BuildActionsFromMonsterDataIfNeeded()
    {
        if (!useMonsterDataActions || loadedMonsterData == null || actionDefinitions.Count > 0)
        {
            return;
        }

        AppendAction(loadedMonsterData.Action1ID, loadedMonsterData.Action1Min, loadedMonsterData.Action1Max);
        AppendAction(loadedMonsterData.Action2ID, loadedMonsterData.Action2Min, loadedMonsterData.Action2Max);
        AppendAction(loadedMonsterData.Action3ID, loadedMonsterData.Action3Min, loadedMonsterData.Action3Max);
        AppendAction(loadedMonsterData.Action4ID, loadedMonsterData.Action4Min, loadedMonsterData.Action4Max);
        AppendAction(loadedMonsterData.Action5ID, loadedMonsterData.Action5Min, loadedMonsterData.Action5Max);
        AppendAction(loadedMonsterData.Action6ID, loadedMonsterData.Action6Min, loadedMonsterData.Action6Max);
        AppendAction(loadedMonsterData.Action7ID, loadedMonsterData.Action7Min, loadedMonsterData.Action7Max);

        // 데이터가 비어있는 몬스터는 기존 기본 공격으로 동작하도록 1개 액션을 보장합니다.
        if (actionDefinitions.Count == 0)
        {
            actionDefinitions.Add(new MonsterActionDefinition
            {
                ActionId = "DEFAULT_ATTACK",
                ActionType = MonsterActionType.Attack,
                TargetType = MonsterActionTargetType.Player,
                MinValue = defaultAttack,
                MaxValue = defaultAttack
            });
        }
    }

    private void AppendAction(string actionId, int min, int max)
    {
        if (string.IsNullOrEmpty(actionId))
        {
            return;
        }

        actionDefinitions.Add(CreateActionFromData(actionId, min, max));
    }

    private MonsterActionDefinition CreateActionFromData(string actionId, int min, int max)
    {
        var definition = new MonsterActionDefinition
        {
            ActionId = actionId,
            MinValue = min,
            MaxValue = max
        };

        switch (actionId)
        {
            case "2410001":
                definition.ActionType = MonsterActionType.Attack;
                definition.TargetType = MonsterActionTargetType.Player;
                break;
            case "2410002":
                definition.ActionType = MonsterActionType.Guard;
                definition.TargetType = MonsterActionTargetType.Self;
                break;
            case "2410003":
                definition.ActionType = MonsterActionType.ApplyStatus;
                definition.TargetType = MonsterActionTargetType.Player;
                definition.StatusEffectId = StatusEffectController.WeaknessExposureStatusId;
                definition.StatusStack = Mathf.Max(1, min);
                break;
            default:
                // 기본값은 유저 대상 공격으로 해석
                definition.ActionType = MonsterActionType.Attack;
                definition.TargetType = MonsterActionTargetType.Player;
                break;
        }

        return definition;
    }

    private MonsterActionDefinition SelectNextAction()
    {
        if (actionDefinitions == null || actionDefinitions.Count == 0)
        {
            return null;
        }

        if (actionPatternType == MonsterActionPatternType.Cycle)
        {
            MonsterActionDefinition cycleAction = actionDefinitions[actionCursor];
            actionCursor = (actionCursor + 1) % actionDefinitions.Count;
            return cycleAction;
        }

        int randomIndex = UnityEngine.Random.Range(0, actionDefinitions.Count);
        return actionDefinitions[randomIndex];
    }

    private void ExecuteSelectedAction(MonsterActionDefinition action, IDamageable defaultTarget, int actionValue)
    {
        if (action == null)
        {
            ExecuteFallbackAttack(defaultTarget);
            return;
        }

        List<IDamageable> targets = ResolveTargets(action.TargetType, defaultTarget);

        switch (action.ActionType)
        {
            case MonsterActionType.Attack:
                ExecuteAttackAction(targets, actionValue);
                break;
            case MonsterActionType.Guard:
                AddProtection(actionValue);
                MarkGuardShieldAppliedThisTurn();
                break;
            case MonsterActionType.ApplyStatus:
                ExecuteApplyStatusAction(targets, action.StatusEffectId, action.StatusStack);
                break;
            case MonsterActionType.Heal:
                SetCurrentHealth(CurrentHealth + actionValue);
                break;
            case MonsterActionType.Summon:
                Debug.Log($"{name}: 소환 행동은 아직 구현 전입니다.");
                break;
            case MonsterActionType.Ready:
                // 준비 행동은 의도적으로 아무것도 하지 않습니다.
                break;
            case MonsterActionType.Stun:
                // 기절 행동은 행동 불가를 표현하므로 아무것도 하지 않습니다.
                break;
            default:
                ExecuteFallbackAttack(defaultTarget);
                break;
        }
    }

    private bool TryConsumePreparedAction(out MonsterActionDefinition action, out int actionValue)
    {
        if (!hasPreparedAction)
        {
            action = null;
            actionValue = 0;
            return false;
        }

        action = preparedAction;
        actionValue = preparedActionValue;
        hasPreparedAction = false;
        preparedAction = null;
        preparedActionValue = 0;
        return true;
    }

    private void ExecuteFallbackAttack(IDamageable target)
    {
        if (target == null || !target.IsAlive)
        {
            return;
        }

        int damage = GetAttackValue();
        BaseUnit targetUnit = target as BaseUnit;
        int finalDamage = ApplyOutgoingStatusEffects(damage, targetUnit);
        target.TakeDamage(finalDamage);
    }

    private void ExecuteAttackAction(List<IDamageable> targets, int damage)
    {
        if (targets == null || targets.Count == 0 || damage <= 0)
        {
            return;
        }

        for (int i = 0; i < targets.Count; i++)
        {
            IDamageable target = targets[i];
            if (target == null || !target.IsAlive)
            {
                continue;
            }

            BaseUnit targetUnit = target as BaseUnit;
            int finalDamage = ApplyOutgoingStatusEffects(damage, targetUnit);
            target.TakeDamage(finalDamage);
        }
    }

    private void ExecuteApplyStatusAction(List<IDamageable> targets, string statusEffectId, int stack)
    {
        if (targets == null || targets.Count == 0 || string.IsNullOrEmpty(statusEffectId))
        {
            return;
        }

        for (int i = 0; i < targets.Count; i++)
        {
            BaseUnit targetUnit = targets[i] as BaseUnit;
            if (targetUnit == null || !targetUnit.IsAlive)
            {
                continue;
            }

            StatusEffectController statusEffectController = targetUnit.GetComponent<StatusEffectController>();
            if (statusEffectController == null)
            {
                statusEffectController = targetUnit.gameObject.AddComponent<StatusEffectController>();
            }

            statusEffectController.TryApplyStatus(statusEffectId, stack);
        }
    }

    private List<IDamageable> ResolveTargets(MonsterActionTargetType targetType, IDamageable defaultPlayerTarget)
    {
        List<IDamageable> resolvedTargets = new List<IDamageable>();
        List<Monster> aliveAllies = GetAliveAllies();

        switch (targetType)
        {
            case MonsterActionTargetType.None:
                break;
            case MonsterActionTargetType.Self:
                resolvedTargets.Add(this);
                break;
            case MonsterActionTargetType.Player:
                if (defaultPlayerTarget != null && defaultPlayerTarget.IsAlive)
                {
                    resolvedTargets.Add(defaultPlayerTarget);
                }
                break;
            case MonsterActionTargetType.AllyRandom:
                AddRandomAlly(resolvedTargets, aliveAllies);
                break;
            case MonsterActionTargetType.AllyRandomExceptSelf:
                aliveAllies.Remove(this);
                AddRandomAlly(resolvedTargets, aliveAllies);
                break;
            case MonsterActionTargetType.AlliesAll:
                for (int i = 0; i < aliveAllies.Count; i++)
                {
                    resolvedTargets.Add(aliveAllies[i]);
                }
                break;
        }

        return resolvedTargets;
    }

    private List<Monster> GetAliveAllies()
    {
        var allies = new List<Monster>();
        if (battleManager == null)
        {
            if (IsAlive)
            {
                allies.Add(this);
            }

            return allies;
        }

        BattleSetupController setup = battleManager.GetSetupController();
        if (setup == null)
        {
            if (IsAlive)
            {
                allies.Add(this);
            }

            return allies;
        }

        List<Monster> monsters = setup.GetPrimaryMonsters();
        for (int i = 0; i < monsters.Count; i++)
        {
            Monster monster = monsters[i];
            if (monster != null && monster.IsAlive)
            {
                allies.Add(monster);
            }
        }

        return allies;
    }

    private static void AddRandomAlly(List<IDamageable> output, List<Monster> candidates)
    {
        if (output == null || candidates == null || candidates.Count == 0)
        {
            return;
        }

        int index = UnityEngine.Random.Range(0, candidates.Count);
        output.Add(candidates[index]);
    }

    private static int ResolveActionValue(int min, int max)
    {
        int normalizedMin = Mathf.Min(min, max);
        int normalizedMax = Mathf.Max(min, max);
        if (normalizedMin == normalizedMax)
        {
            return normalizedMin;
        }

        return UnityEngine.Random.Range(normalizedMin, normalizedMax + 1);
    }

    private Vector3 GetTargetAttackPosition(IDamageable target)
    {
        if (target == null || spriteRenderer == null)
        {
            return initialSpriteLocalPosition;
        }

        MonoBehaviour targetMono = target as MonoBehaviour;
        if (targetMono == null)
        {
            return initialSpriteLocalPosition;
        }

        Vector3 targetWorldPosition = targetMono.transform.position;
        
        Transform spriteParent = spriteRenderer.transform.parent;
        Vector3 targetLocal = spriteParent != null
            ? spriteParent.InverseTransformPoint(targetWorldPosition)
            : targetWorldPosition;

        Vector3 direction = (targetLocal - initialSpriteLocalPosition).normalized;
        Vector3 attackPosition = targetLocal - direction * attackMoveOffset;
        
        attackPosition.y = initialSpriteLocalPosition.y;

        return attackPosition;
    }

    private void SaveInitialPosition()
    {
        if (!hasSavedInitialPosition && spriteRenderer != null)
        {
            initialSpriteLocalPosition = spriteRenderer.transform.localPosition;
            hasSavedInitialPosition = true;
        }
    }

    private IEnumerator MoveSpriteToPosition(Vector3 targetPosition, float duration)
    {
        if (spriteRenderer == null)
        {
            yield break;
        }

        targetPosition.y = initialSpriteLocalPosition.y;
        
        if (duration <= 0f)
        {
            Vector3 finalPosition = spriteRenderer.transform.localPosition;
            finalPosition.x = targetPosition.x;
            finalPosition.y = initialSpriteLocalPosition.y;
            spriteRenderer.transform.localPosition = finalPosition;
            yield break;
        }

        Vector3 startPosition = spriteRenderer.transform.localPosition;
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(elapsedTime / duration);
            Vector3 currentPosition = Vector3.Lerp(startPosition, targetPosition, t);
            currentPosition.y = initialSpriteLocalPosition.y;
            spriteRenderer.transform.localPosition = currentPosition;
            yield return null;
        }

        Vector3 finalPos = targetPosition;
        finalPos.y = initialSpriteLocalPosition.y;
        spriteRenderer.transform.localPosition = finalPos;
    }

    private void SetSortingOrder(int sortingOrder)
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.sortingOrder = sortingOrder;
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        // UI 위에 포인터가 있으면 몬스터 클릭 무시
        if (IsPointerOverUI(eventData))
        {
            return;
        }

        if (!IsAlive)
        {
            return;
        }

        Clicked?.Invoke(this);
    }

    private void OnMouseDown()
    {
        // UI 위에 포인터가 있으면 몬스터 클릭 무시
        if (IsPointerOverUI())
        {
            return;
        }

        if (!IsAlive)
        {
            return;
        }

        Clicked?.Invoke(this);
    }

    /// <summary>
    /// 포인터가 UI 위에 있는지 확인 (PointerEventData 사용)
    /// </summary>
    private bool IsPointerOverUI(PointerEventData eventData)
    {
        if (EventSystem.current == null)
        {
            return false;
        }

        var results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, results);

        // UI 요소가 있으면 true 반환
        return results.Count > 0;
    }

    /// <summary>
    /// 포인터가 UI 위에 있는지 확인
    /// </summary>
    private bool IsPointerOverUI()
    {
        if (EventSystem.current == null)
        {
            return false;
        }

        if (Input.touchCount > 0)
        {
            // 터치 입력
            return EventSystem.current.IsPointerOverGameObject(Input.GetTouch(0).fingerId);
        }
        else
        {
            // 마우스 입력
            return EventSystem.current.IsPointerOverGameObject();
        }
    }

    private void RegisterBattleManager()
    {
        if (battleManager != null || registrationRoutine != null)
        {
            return;
        }

        if (DIContainer.IsRegistered<BattleManager>())
        {
            battleManager = DIContainer.Resolve<BattleManager>();
            battleManager.RegisterMonster(this);
        }
        else
        {
            registrationRoutine = StartCoroutine(WaitForBattleManager());
        }
    }

    private IEnumerator WaitForBattleManager()
    {
        while (!DIContainer.IsRegistered<BattleManager>())
        {
            yield return null;
        }

        registrationRoutine = null;
        battleManager = DIContainer.Resolve<BattleManager>();
        battleManager.RegisterMonster(this);
    }

    private bool isMarkedForDeath = false;

    private void MarkGuardShieldAppliedThisTurn()
    {
        hasGuardShieldPendingExpire = true;
        guardShieldAppliedTurnNumber = ResolveCurrentTurnNumber();
    }

    private static int ResolveCurrentTurnNumber()
    {
        if (!DIContainer.IsRegistered<TurnManager>())
        {
            return -1;
        }

        TurnManager turnManager = DIContainer.Resolve<TurnManager>();
        return turnManager != null ? turnManager.CurrentTurnNumber : -1;
    }

    public void ExpireGuardShieldIfNeeded(int currentTurnNumber)
    {
        if (!hasGuardShieldPendingExpire)
        {
            return;
        }

        // 가드가 적용된 같은 턴에는 유지하고, 그 다음 턴 종료 시 만료합니다.
        if (currentTurnNumber <= guardShieldAppliedTurnNumber)
        {
            return;
        }

        SetProtection(0f);
        hasGuardShieldPendingExpire = false;
        guardShieldAppliedTurnNumber = -1;
    }

    public override void TakeDamage(int amount)
    {
        base.TakeDamage(amount);

        if (ProtectionValue <= 0f)
        {
            hasGuardShieldPendingExpire = false;
            guardShieldAppliedTurnNumber = -1;
        }
        
        if (monsterAnimation != null && IsAlive)
        {
            monsterAnimation.PlayHitAnimation();
        }
        
        if (!IsAlive && !isMarkedForDeath)
        {
            HandleDeath();
        }
    }

    private void HandleDeath()
    {
        isMarkedForDeath = true;
        if (monsterAnimation != null)
        {
            monsterAnimation.PlayDeadAnimation();
        }
    }

    public void DestroyMonster()
    {
        if (battleManager != null)
        {
            battleManager.UnregisterMonster(this);
        }
        
        Destroy(gameObject);
    }

    protected override void RefreshUI()
    {
        base.RefreshUI();

        bool showAttackStatus = true;
        if (attackText != null)
        {
            int displayValue = defaultAttack;
            if (hasPreparedAction && preparedAction != null)
            {
                bool isAttackAction = preparedAction.ActionType == MonsterActionType.Attack;
                displayValue = isAttackAction ? preparedActionValue : 0;
                showAttackStatus = isAttackAction;
            }

            attackText.text = displayValue.ToString();
        }

        if (attackStatusRoot != null)
        {
            attackStatusRoot.SetActive(showAttackStatus);
        }

        RefreshActionIndicator();
    }

    private void ResolveStatusUIReferences()
    {
        if (attackText != null && attackText.transform.parent != null)
        {
            attackStatusRoot = attackText.transform.parent.gameObject;
        }

        if (actionIndicatorRenderer == null)
        {
            Transform actionTransform = transform.Find("Action");
            if (actionTransform != null)
            {
                actionIndicatorRenderer = actionTransform.GetComponent<SpriteRenderer>();
            }
        }
    }

    private void RefreshActionIndicator()
    {
        if (actionIndicatorRenderer == null)
        {
            return;
        }

        if (!hasPreparedAction || preparedAction == null)
        {
            actionIndicatorRenderer.gameObject.SetActive(false);
            return;
        }

        Sprite actionSprite = ResolveActionIndicatorSprite(preparedAction);
        if (actionSprite == null)
        {
            actionIndicatorRenderer.gameObject.SetActive(false);
            return;
        }

        actionIndicatorRenderer.sprite = actionSprite;
        actionIndicatorRenderer.gameObject.SetActive(true);
    }

    private Sprite ResolveActionIndicatorSprite(MonsterActionDefinition action)
    {
        if (action == null)
        {
            return actionAttackSprite;
        }

        switch (action.ActionType)
        {
            case MonsterActionType.Attack:
                return actionAttackSprite;
            case MonsterActionType.Guard:
                return actionShieldSprite;
            case MonsterActionType.ApplyStatus:
                return IsDebuffTarget(action.TargetType) ? actionDebuffSprite : actionBuffSprite;
            case MonsterActionType.Heal:
            case MonsterActionType.Summon:
                return actionBuffSprite;
            default:
                return actionAttackSprite;
        }
    }

    private static bool IsDebuffTarget(MonsterActionTargetType targetType)
    {
        return targetType == MonsterActionTargetType.Player;
    }

}
