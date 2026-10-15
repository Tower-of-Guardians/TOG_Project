using System;
using System.Collections.Generic;
using UnityEngine;

public enum StatusEffectStackPolicy
{
    NoDuplicate,
    AddStack,
    RefreshDuration,
    StrongestValue
}

public enum StatusEffectDurationType
{
    OneTurn = 1,
    ValueTurns = 2,
    Permanent = 3
}

public enum StatusEffectReleaseCondition
{
    None = 0,
    OnHit = 1,
    OnHpLoss = 2
}

/// <summary>
/// 피해 계산 시 상태효과가 수정할 수 있는 컨텍스트입니다.
/// </summary>
public sealed class DamageContext
{
    public int BaseDamage { get; }
    public int FinalDamage { get; private set; }
    public bool IsCritical { get; set; }
    public BaseUnit Source { get; }
    public BaseUnit Target { get; }

    public DamageContext(int baseDamage, BaseUnit source, BaseUnit target, bool isCritical = false)
    {
        BaseDamage = Mathf.Max(0, baseDamage);
        FinalDamage = BaseDamage;
        Source = source;
        Target = target;
        IsCritical = isCritical;
    }

    public void SetFinalDamage(int damage)
    {
        FinalDamage = Mathf.Max(0, damage);
    }

    public void AddFinalDamage(int damage)
    {
        SetFinalDamage(FinalDamage + damage);
    }

    public void MultiplyFinalDamage(float multiplier)
    {
        SetFinalDamage(Mathf.CeilToInt(FinalDamage * Mathf.Max(0f, multiplier)));
    }
}

/// <summary>
/// 유닛에 부여된 개별 상태효과의 런타임 상태입니다.
/// </summary>
[Serializable]
public sealed class StatusEffectRuntime
{
    public string StatusEffectId;
    public string Name;
    public int Stack;
    public int RemainingTurns;
    public int Value;
    public int DurationType;
    public int ReleaseCondition;
    public bool SuppressNextDurationTick;

    public bool IsExpired => Stack <= 0 || RemainingTurns == 0;

    public void DecreaseTurn()
    {
        if (RemainingTurns == int.MaxValue)
        {
            return;
        }

        RemainingTurns = Mathf.Max(0, RemainingTurns - 1);
    }

    public void AddStack(int amount)
    {
        Stack = Mathf.Max(0, Stack + amount);
    }

    public void ConsumeStack(int amount)
    {
        Stack = Mathf.Max(0, Stack - Mathf.Max(0, amount));
    }
}

public interface IStatusEffectHandler
{
    string StatusEffectId { get; }
    StatusEffectStackPolicy StackPolicy { get; }
    int ResolveInitialStack(StatusEffectData data, int requestedStack);
    int ResolveInitialValue(StatusEffectData data, int requestedValue);
    void OnApplied(StatusEffectController controller, StatusEffectRuntime runtime, bool isNew);
    void OnTurnStart(StatusEffectController controller, StatusEffectRuntime runtime);
    void OnTurnEnd(StatusEffectController controller, StatusEffectRuntime runtime);
    void OnBeforeDealDamage(StatusEffectController controller, StatusEffectRuntime runtime, DamageContext context);
    void OnBeforeTakeDamage(StatusEffectController controller, StatusEffectRuntime runtime, DamageContext context);
    void OnAfterTakeDamage(StatusEffectController controller, StatusEffectRuntime runtime, DamageContext context);
}

public static class StatusEffectDurationUtility
{
    /// <summary>
    /// DurationType 분류(1=1턴, 2=수치, 3=영구)에 따라 남은 턴 수를 반환합니다.
    /// </summary>
    public static int ResolveDurationFromData(StatusEffectData data, int requestedValue)
    {
        int durationType = data != null ? data.DurationType : (int)StatusEffectDurationType.Permanent;
        switch ((StatusEffectDurationType)durationType)
        {
            case StatusEffectDurationType.OneTurn:
                return 1;
            case StatusEffectDurationType.ValueTurns:
                return Mathf.Max(1, requestedValue);
            case StatusEffectDurationType.Permanent:
                return int.MaxValue;
            default:
                return Mathf.Max(1, requestedValue);
        }
    }

    public static bool ShouldDecreaseTurn(StatusEffectRuntime runtime)
    {
        if (runtime == null || runtime.RemainingTurns == int.MaxValue)
        {
            return false;
        }

        return runtime.DurationType == (int)StatusEffectDurationType.OneTurn
            || runtime.DurationType == (int)StatusEffectDurationType.ValueTurns;
    }

    public static string ResolveDisplayAmount(StatusEffectRuntime runtime)
    {
        if (runtime == null)
        {
            return string.Empty;
        }

        if (runtime.DurationType == (int)StatusEffectDurationType.Permanent
            || runtime.RemainingTurns == int.MaxValue)
        {
            return Mathf.Max(0, runtime.Stack).ToString();
        }

        return Mathf.Max(0, runtime.RemainingTurns).ToString();
    }
}

/// <summary>
/// 약점노출(51001001) 핸들러.
/// DurationType=영구이며, 피격 시 피해 50% 증가 후 ReleaseCondition으로 해제됩니다.
/// </summary>
public sealed class WeaknessExposureHandler : IStatusEffectHandler
{
    public string StatusEffectId => StatusEffectController.WeaknessExposureStatusId;
    public StatusEffectStackPolicy StackPolicy => StatusEffectStackPolicy.RefreshDuration;

    public int ResolveInitialStack(StatusEffectData data, int requestedStack)
    {
        return Mathf.Max(1, requestedStack);
    }

    public int ResolveInitialValue(StatusEffectData data, int requestedValue)
    {
        return Mathf.Max(0, requestedValue);
    }

    public void OnApplied(StatusEffectController controller, StatusEffectRuntime runtime, bool isNew) { }
    public void OnTurnStart(StatusEffectController controller, StatusEffectRuntime runtime) { }
    public void OnTurnEnd(StatusEffectController controller, StatusEffectRuntime runtime) { }
    public void OnBeforeDealDamage(StatusEffectController controller, StatusEffectRuntime runtime, DamageContext context) { }

    public void OnBeforeTakeDamage(StatusEffectController controller, StatusEffectRuntime runtime, DamageContext context)
    {
        if (runtime == null || context == null || runtime.Stack <= 0)
        {
            return;
        }

        context.MultiplyFinalDamage(1.5f);
    }

    public void OnAfterTakeDamage(StatusEffectController controller, StatusEffectRuntime runtime, DamageContext context) { }
}

/// <summary>
/// 저주(51001005) 핸들러.
/// 매 턴 시작 시 스택 수치만큼 피해를 입습니다.
/// </summary>
public sealed class CurseHandler : IStatusEffectHandler
{
    public string StatusEffectId => StatusEffectController.CurseStatusId;
    public StatusEffectStackPolicy StackPolicy => StatusEffectStackPolicy.AddStack;

    public int ResolveInitialStack(StatusEffectData data, int requestedStack)
    {
        return Mathf.Max(1, requestedStack);
    }

    public int ResolveInitialValue(StatusEffectData data, int requestedValue)
    {
        return Mathf.Max(0, requestedValue);
    }

    public void OnApplied(StatusEffectController controller, StatusEffectRuntime runtime, bool isNew) { }

    public void OnTurnStart(StatusEffectController controller, StatusEffectRuntime runtime)
    {
        if (runtime == null || runtime.Stack <= 0 || controller == null)
        {
            return;
        }

        BaseUnit owner = controller.GetComponent<BaseUnit>();
        if (owner == null || !owner.IsAlive)
        {
            return;
        }

        owner.TakeDamage(runtime.Stack);
        controller.NotifyStatusChanged(runtime);
    }

    public void OnTurnEnd(StatusEffectController controller, StatusEffectRuntime runtime) { }
    public void OnBeforeDealDamage(StatusEffectController controller, StatusEffectRuntime runtime, DamageContext context) { }
    public void OnBeforeTakeDamage(StatusEffectController controller, StatusEffectRuntime runtime, DamageContext context) { }
    public void OnAfterTakeDamage(StatusEffectController controller, StatusEffectRuntime runtime, DamageContext context) { }
}

/// <summary>
/// 저주의 가시(51001004) 핸들러.
/// DurationType=1은 상태효과 테이블의 1턴 지속입니다.
/// 피격 시 공격자에게 부여하는 저주 수치는 몬스터 테이블 Value에서 전달된 runtime.Value를 사용합니다.
/// </summary>
public sealed class CurseThornsHandler : IStatusEffectHandler
{
    public string StatusEffectId => StatusEffectController.CurseThornsStatusId;
    public StatusEffectStackPolicy StackPolicy => StatusEffectStackPolicy.RefreshDuration;

    public int ResolveInitialStack(StatusEffectData data, int requestedStack)
    {
        return Mathf.Max(1, requestedStack);
    }

    public int ResolveInitialValue(StatusEffectData data, int requestedValue)
    {
        return Mathf.Max(0, requestedValue);
    }

    public void OnApplied(StatusEffectController controller, StatusEffectRuntime runtime, bool isNew) { }
    public void OnTurnStart(StatusEffectController controller, StatusEffectRuntime runtime) { }
    public void OnTurnEnd(StatusEffectController controller, StatusEffectRuntime runtime) { }
    public void OnBeforeDealDamage(StatusEffectController controller, StatusEffectRuntime runtime, DamageContext context) { }
    public void OnBeforeTakeDamage(StatusEffectController controller, StatusEffectRuntime runtime, DamageContext context) { }

    public void OnAfterTakeDamage(StatusEffectController controller, StatusEffectRuntime runtime, DamageContext context)
    {
        if (runtime == null || context == null || context.Source == null || context.Source == context.Target)
        {
            return;
        }

        int curseStack = runtime.Value > 0 ? runtime.Value : runtime.Stack;
        if (curseStack <= 0)
        {
            return;
        }

        StatusEffectController attackerController = StatusEffectController.Resolve(context.Source);
        if (attackerController == null)
        {
            return;
        }

        attackerController.TryApplyStatus(StatusEffectController.CurseStatusId, curseStack);
    }
}

/// <summary>
/// 힘:적(51003029) 핸들러.
/// 공격 행동 피해량을 스택 수치만큼 증가시킵니다. (영구, 스택 누적)
/// </summary>
public sealed class EnemyStrengthBuffHandler : IStatusEffectHandler
{
    public string StatusEffectId => StatusEffectController.EnemyStrengthStatusId;
    public StatusEffectStackPolicy StackPolicy => StatusEffectStackPolicy.AddStack;

    public int ResolveInitialStack(StatusEffectData data, int requestedStack)
    {
        return Mathf.Max(1, requestedStack);
    }

    public int ResolveInitialValue(StatusEffectData data, int requestedValue)
    {
        return Mathf.Max(0, requestedValue);
    }

    public void OnApplied(StatusEffectController controller, StatusEffectRuntime runtime, bool isNew) { }
    public void OnTurnStart(StatusEffectController controller, StatusEffectRuntime runtime) { }
    public void OnTurnEnd(StatusEffectController controller, StatusEffectRuntime runtime) { }

    public void OnBeforeDealDamage(StatusEffectController controller, StatusEffectRuntime runtime, DamageContext context)
    {
        if (runtime == null || context == null || runtime.Stack <= 0)
        {
            return;
        }

        context.AddFinalDamage(runtime.Stack);
    }

    public void OnBeforeTakeDamage(StatusEffectController controller, StatusEffectRuntime runtime, DamageContext context) { }
    public void OnAfterTakeDamage(StatusEffectController controller, StatusEffectRuntime runtime, DamageContext context) { }
}

public sealed class TableDrivenStatusEffectHandler : IStatusEffectHandler
{
    public static readonly TableDrivenStatusEffectHandler Instance = new TableDrivenStatusEffectHandler();

    public string StatusEffectId => string.Empty;
    public StatusEffectStackPolicy StackPolicy => StatusEffectStackPolicy.RefreshDuration;

    public int ResolveInitialStack(StatusEffectData data, int requestedStack)
    {
        return Mathf.Max(1, requestedStack);
    }

    public int ResolveInitialValue(StatusEffectData data, int requestedValue)
    {
        return Mathf.Max(0, requestedValue);
    }

    public void OnApplied(StatusEffectController controller, StatusEffectRuntime runtime, bool isNew) { }
    public void OnTurnStart(StatusEffectController controller, StatusEffectRuntime runtime) { }
    public void OnTurnEnd(StatusEffectController controller, StatusEffectRuntime runtime) { }
    public void OnBeforeDealDamage(StatusEffectController controller, StatusEffectRuntime runtime, DamageContext context) { }
    public void OnBeforeTakeDamage(StatusEffectController controller, StatusEffectRuntime runtime, DamageContext context) { }
    public void OnAfterTakeDamage(StatusEffectController controller, StatusEffectRuntime runtime, DamageContext context) { }
}

public sealed class SacredShieldHandler : IStatusEffectHandler
{
    public string StatusEffectId => StatusEffectController.SacredShieldStatusId;
    public StatusEffectStackPolicy StackPolicy => StatusEffectStackPolicy.RefreshDuration;

    public int ResolveInitialStack(StatusEffectData data, int requestedStack) => Mathf.Max(1, requestedStack);
    public int ResolveInitialValue(StatusEffectData data, int requestedValue) => Mathf.Max(0, requestedValue);

    public void OnApplied(StatusEffectController controller, StatusEffectRuntime runtime, bool isNew) { }

    public void OnTurnStart(StatusEffectController controller, StatusEffectRuntime runtime)
    {
        int damage = runtime != null && runtime.Value > 0 ? runtime.Value : runtime != null ? runtime.Stack : 0;
        if (damage <= 0)
        {
            return;
        }

        BaseUnit owner = controller != null ? controller.GetComponent<BaseUnit>() : null;
        if (!DIContainer.IsRegistered<BattleManager>())
        {
            return;
        }

        List<Monster> monsters = DIContainer.Resolve<BattleManager>()?.GetSetupController()?.GetPrimaryMonsters();
        if (monsters == null)
        {
            return;
        }

        for (int i = 0; i < monsters.Count; i++)
        {
            if (monsters[i] != null && monsters[i].IsAlive)
            {
                monsters[i].TakeDamage(damage, owner);
            }
        }
    }

    public void OnTurnEnd(StatusEffectController controller, StatusEffectRuntime runtime) { }
    public void OnBeforeDealDamage(StatusEffectController controller, StatusEffectRuntime runtime, DamageContext context) { }
    public void OnBeforeTakeDamage(StatusEffectController controller, StatusEffectRuntime runtime, DamageContext context) { }
    public void OnAfterTakeDamage(StatusEffectController controller, StatusEffectRuntime runtime, DamageContext context) { }
}

public sealed class FrenzyHandler : IStatusEffectHandler
{
    public string StatusEffectId => StatusEffectController.FrenzyStatusId;
    public StatusEffectStackPolicy StackPolicy => StatusEffectStackPolicy.RefreshDuration;

    public int ResolveInitialStack(StatusEffectData data, int requestedStack) => Mathf.Max(1, requestedStack);
    public int ResolveInitialValue(StatusEffectData data, int requestedValue) => Mathf.Max(0, requestedValue);

    public void OnApplied(StatusEffectController controller, StatusEffectRuntime runtime, bool isNew) { }
    public void OnTurnStart(StatusEffectController controller, StatusEffectRuntime runtime) { }
    public void OnTurnEnd(StatusEffectController controller, StatusEffectRuntime runtime) { }

    public void OnBeforeDealDamage(StatusEffectController controller, StatusEffectRuntime runtime, DamageContext context)
    {
        context?.MultiplyFinalDamage(2f);
    }

    public void OnBeforeTakeDamage(StatusEffectController controller, StatusEffectRuntime runtime, DamageContext context)
    {
        context?.MultiplyFinalDamage(2f);
    }

    public void OnAfterTakeDamage(StatusEffectController controller, StatusEffectRuntime runtime, DamageContext context) { }
}

public sealed class SuperFrenzyHandler : IStatusEffectHandler
{
    public string StatusEffectId => StatusEffectController.SuperFrenzyStatusId;
    public StatusEffectStackPolicy StackPolicy => StatusEffectStackPolicy.RefreshDuration;

    public int ResolveInitialStack(StatusEffectData data, int requestedStack) => Mathf.Max(1, requestedStack);
    public int ResolveInitialValue(StatusEffectData data, int requestedValue) => Mathf.Max(0, requestedValue);

    public void OnApplied(StatusEffectController controller, StatusEffectRuntime runtime, bool isNew) { }
    public void OnTurnStart(StatusEffectController controller, StatusEffectRuntime runtime) { }
    public void OnTurnEnd(StatusEffectController controller, StatusEffectRuntime runtime) { }

    public void OnBeforeDealDamage(StatusEffectController controller, StatusEffectRuntime runtime, DamageContext context)
    {
        context?.MultiplyFinalDamage(3f);
    }

    public void OnBeforeTakeDamage(StatusEffectController controller, StatusEffectRuntime runtime, DamageContext context)
    {
        context?.MultiplyFinalDamage(2f);
    }

    public void OnAfterTakeDamage(StatusEffectController controller, StatusEffectRuntime runtime, DamageContext context) { }
}

public sealed class ImmortalityHandler : IStatusEffectHandler
{
    public string StatusEffectId => StatusEffectController.ImmortalityStatusId;
    public StatusEffectStackPolicy StackPolicy => StatusEffectStackPolicy.RefreshDuration;

    public int ResolveInitialStack(StatusEffectData data, int requestedStack) => Mathf.Max(1, requestedStack);
    public int ResolveInitialValue(StatusEffectData data, int requestedValue) => Mathf.Max(0, requestedValue);

    public void OnApplied(StatusEffectController controller, StatusEffectRuntime runtime, bool isNew) { }
    public void OnTurnStart(StatusEffectController controller, StatusEffectRuntime runtime) { }
    public void OnTurnEnd(StatusEffectController controller, StatusEffectRuntime runtime) { }
    public void OnBeforeDealDamage(StatusEffectController controller, StatusEffectRuntime runtime, DamageContext context) { }

    public void OnBeforeTakeDamage(StatusEffectController controller, StatusEffectRuntime runtime, DamageContext context)
    {
        BaseUnit owner = controller != null ? controller.GetComponent<BaseUnit>() : null;
        if (owner == null || context == null)
        {
            return;
        }

        int maxLoss = Mathf.Max(0, owner.CurrentHealth - 1);
        if (context.FinalDamage > maxLoss)
        {
            context.SetFinalDamage(maxLoss);
        }
    }

    public void OnAfterTakeDamage(StatusEffectController controller, StatusEffectRuntime runtime, DamageContext context) { }
}

public sealed class EvasionHandler : IStatusEffectHandler
{
    public string StatusEffectId => StatusEffectController.EvasionStatusId;
    public StatusEffectStackPolicy StackPolicy => StatusEffectStackPolicy.RefreshDuration;

    public int ResolveInitialStack(StatusEffectData data, int requestedStack) => Mathf.Max(1, requestedStack);
    public int ResolveInitialValue(StatusEffectData data, int requestedValue) => Mathf.Max(0, requestedValue);

    public void OnApplied(StatusEffectController controller, StatusEffectRuntime runtime, bool isNew) { }
    public void OnTurnStart(StatusEffectController controller, StatusEffectRuntime runtime) { }
    public void OnTurnEnd(StatusEffectController controller, StatusEffectRuntime runtime) { }
    public void OnBeforeDealDamage(StatusEffectController controller, StatusEffectRuntime runtime, DamageContext context) { }

    public void OnBeforeTakeDamage(StatusEffectController controller, StatusEffectRuntime runtime, DamageContext context)
    {
        context?.SetFinalDamage(0);
    }

    public void OnAfterTakeDamage(StatusEffectController controller, StatusEffectRuntime runtime, DamageContext context) { }
}

public sealed class NextTurnWeaknessApplyHandler : IStatusEffectHandler
{
    public NextTurnWeaknessApplyHandler(string statusEffectId)
    {
        StatusEffectId = statusEffectId;
    }

    public string StatusEffectId { get; }
    public StatusEffectStackPolicy StackPolicy => StatusEffectStackPolicy.RefreshDuration;

    public int ResolveInitialStack(StatusEffectData data, int requestedStack) => Mathf.Max(1, requestedStack);
    public int ResolveInitialValue(StatusEffectData data, int requestedValue) => Mathf.Max(0, requestedValue);

    public void OnApplied(StatusEffectController controller, StatusEffectRuntime runtime, bool isNew) { }

    public void OnTurnStart(StatusEffectController controller, StatusEffectRuntime runtime)
    {
        if (controller == null)
        {
            return;
        }

        controller.TryApplyStatus(StatusEffectController.WeaknessExposureStatusId, 1);
        controller.RemoveStatus(StatusEffectId);
    }

    public void OnTurnEnd(StatusEffectController controller, StatusEffectRuntime runtime) { }
    public void OnBeforeDealDamage(StatusEffectController controller, StatusEffectRuntime runtime, DamageContext context) { }
    public void OnBeforeTakeDamage(StatusEffectController controller, StatusEffectRuntime runtime, DamageContext context) { }
    public void OnAfterTakeDamage(StatusEffectController controller, StatusEffectRuntime runtime, DamageContext context) { }
}

/// <summary>
/// 상태효과의 부여/해제/훅 실행을 담당하는 컨트롤러입니다.
/// 상태 정의는 DataCenter(StatusEffectData)에서 조회합니다.
/// </summary>
public class StatusEffectController : MonoBehaviour
{
    public const string WeaknessExposureStatusId = "51001001";
    public const string SacredShieldStatusId = "51001002";
    public const string FrenzyStatusId = "51001003";
    public const string CurseThornsStatusId = "51001004";
    public const string CurseStatusId = "51001005";
    public const string ImmortalityStatusId = "51001006";
    public const string EvasionStatusId = "51001007";
    public const string SuperFrenzyStatusId = "51001033";
    public const string ConfusionStatusId = "51001034";
    public const string StunStatusId = "51003028";
    public const string NextTurnWeaknessStatusId = "51003030";
    public const string EnemyStrengthStatusId = "51003029";

    [SerializeField] private bool enableDebugLog;

    private static readonly Dictionary<string, IStatusEffectHandler> HandlerMap = new Dictionary<string, IStatusEffectHandler>();
    private static bool isDefaultHandlerRegistered;

    private readonly Dictionary<string, StatusEffectRuntime> activeStatusMap = new Dictionary<string, StatusEffectRuntime>();

    public event Action<StatusEffectRuntime> OnStatusAdded;
    public event Action<StatusEffectRuntime> OnStatusRemoved;
    public event Action<StatusEffectRuntime> OnStatusChanged;

    public IReadOnlyCollection<StatusEffectRuntime> ActiveStatuses => activeStatusMap.Values;

    private void Awake()
    {
        RegisterDefaultHandlers();
    }

    public static void RegisterHandler(IStatusEffectHandler handler)
    {
        if (handler == null || string.IsNullOrEmpty(handler.StatusEffectId))
        {
            return;
        }

        HandlerMap[handler.StatusEffectId] = handler;
    }

    public static StatusEffectController Resolve(BaseUnit unit)
    {
        if (unit == null)
        {
            return null;
        }

        StatusEffectController controller = unit.GetComponent<StatusEffectController>();
        if (controller == null)
        {
            controller = unit.gameObject.AddComponent<StatusEffectController>();
        }

        return controller;
    }

    public bool TryApplyStatus(string statusEffectId, int requestedStack = 1, int requestedValue = 0)
    {
        RegisterDefaultHandlers();

        if (string.IsNullOrEmpty(statusEffectId))
        {
            return false;
        }

        if (!TryGetStatusEffectData(statusEffectId, out StatusEffectData data))
        {
            return false;
        }

        if (!HandlerMap.TryGetValue(statusEffectId, out IStatusEffectHandler handler))
        {
            handler = TableDrivenStatusEffectHandler.Instance;
        }

        int incomingStack = handler.ResolveInitialStack(data, requestedStack);
        int durationRequest = requestedValue > 0 ? requestedValue : requestedStack;
        int incomingDuration = StatusEffectDurationUtility.ResolveDurationFromData(data, durationRequest);
        int incomingValue = handler.ResolveInitialValue(data, requestedValue);

        bool created = !activeStatusMap.TryGetValue(statusEffectId, out StatusEffectRuntime runtime);
        if (created)
        {
            runtime = new StatusEffectRuntime
            {
                StatusEffectId = data.Id,
                Name = data.Name,
                Stack = incomingStack,
                RemainingTurns = incomingDuration,
                Value = incomingValue,
                DurationType = data.DurationType,
                ReleaseCondition = data.ReleaseCondition
            };
            activeStatusMap[statusEffectId] = runtime;
            MarkTurnDurationSkip(runtime, data);
            OnStatusAdded?.Invoke(runtime);
        }
        else
        {
            ApplyStackPolicy(handler.StackPolicy, runtime, incomingStack, incomingDuration, incomingValue);
            MarkTurnDurationSkip(runtime, data);
            OnStatusChanged?.Invoke(runtime);
        }

        handler.OnApplied(this, runtime, created);
        CleanupExpiredStatuses();
        return true;
    }

    public bool TryGetStatusStack(string statusEffectId, out int stack)
    {
        stack = 0;
        if (string.IsNullOrEmpty(statusEffectId))
        {
            return false;
        }

        if (!activeStatusMap.TryGetValue(statusEffectId, out StatusEffectRuntime runtime) || runtime == null || runtime.IsExpired)
        {
            return false;
        }

        stack = runtime.Stack;
        return stack > 0;
    }

    public void RemoveStatus(string statusEffectId)
    {
        if (string.IsNullOrEmpty(statusEffectId))
        {
            return;
        }

        if (activeStatusMap.TryGetValue(statusEffectId, out StatusEffectRuntime runtime))
        {
            activeStatusMap.Remove(statusEffectId);
            OnStatusRemoved?.Invoke(runtime);
        }
    }

    public void ClearStatuses()
    {
        if (activeStatusMap.Count == 0)
        {
            return;
        }

        List<string> keys = new List<string>(activeStatusMap.Keys);
        for (int i = 0; i < keys.Count; i++)
        {
            RemoveStatus(keys[i]);
        }
    }

    public void OnTurnStart()
    {
        List<string> keys = new List<string>(activeStatusMap.Keys);
        for (int i = 0; i < keys.Count; i++)
        {
            if (!activeStatusMap.TryGetValue(keys[i], out StatusEffectRuntime runtime))
            {
                continue;
            }

            if (HandlerMap.TryGetValue(runtime.StatusEffectId, out IStatusEffectHandler handler))
            {
                handler.OnTurnStart(this, runtime);
            }
        }

        CleanupExpiredStatuses();
    }

    public void OnTurnEnd()
    {
        List<string> keys = new List<string>(activeStatusMap.Keys);
        for (int i = 0; i < keys.Count; i++)
        {
            if (!activeStatusMap.TryGetValue(keys[i], out StatusEffectRuntime runtime))
            {
                continue;
            }

            if (HandlerMap.TryGetValue(runtime.StatusEffectId, out IStatusEffectHandler handler))
            {
                handler.OnTurnEnd(this, runtime);
            }

            if (runtime.SuppressNextDurationTick)
            {
                runtime.SuppressNextDurationTick = false;
            }
            else if (StatusEffectDurationUtility.ShouldDecreaseTurn(runtime))
            {
                runtime.DecreaseTurn();
                OnStatusChanged?.Invoke(runtime);
            }
        }

        CleanupExpiredStatuses();
    }

    public void OnBeforeDealDamage(DamageContext context)
    {
        if (context == null)
        {
            return;
        }

        List<string> keys = new List<string>(activeStatusMap.Keys);
        for (int i = 0; i < keys.Count; i++)
        {
            if (!activeStatusMap.TryGetValue(keys[i], out StatusEffectRuntime runtime))
            {
                continue;
            }

            if (HandlerMap.TryGetValue(runtime.StatusEffectId, out IStatusEffectHandler handler))
            {
                handler.OnBeforeDealDamage(this, runtime, context);
            }
        }

        CleanupExpiredStatuses();
    }

    public void OnBeforeTakeDamage(DamageContext context)
    {
        if (context == null)
        {
            return;
        }

        List<string> keys = new List<string>(activeStatusMap.Keys);
        for (int i = 0; i < keys.Count; i++)
        {
            if (!activeStatusMap.TryGetValue(keys[i], out StatusEffectRuntime runtime))
            {
                continue;
            }

            if (HandlerMap.TryGetValue(runtime.StatusEffectId, out IStatusEffectHandler handler))
            {
                handler.OnBeforeTakeDamage(this, runtime, context);
            }
        }

        CleanupExpiredStatuses();
    }

    public void OnAfterTakeDamage(DamageContext context)
    {
        if (context == null)
        {
            return;
        }

        List<string> keys = new List<string>(activeStatusMap.Keys);
        for (int i = 0; i < keys.Count; i++)
        {
            if (!activeStatusMap.TryGetValue(keys[i], out StatusEffectRuntime runtime))
            {
                continue;
            }

            if (HandlerMap.TryGetValue(runtime.StatusEffectId, out IStatusEffectHandler handler))
            {
                handler.OnAfterTakeDamage(this, runtime, context);
            }

            TryReleaseByCondition(runtime, context);
        }

        CleanupExpiredStatuses();
    }

    public void NotifyStatusChanged(StatusEffectRuntime runtime)
    {
        if (runtime == null)
        {
            return;
        }

        OnStatusChanged?.Invoke(runtime);
    }

    private static void RegisterDefaultHandlers()
    {
        if (isDefaultHandlerRegistered)
        {
            return;
        }

        RegisterHandler(new WeaknessExposureHandler());
        RegisterHandler(new SacredShieldHandler());
        RegisterHandler(new FrenzyHandler());
        RegisterHandler(new SuperFrenzyHandler());
        RegisterHandler(new CurseThornsHandler());
        RegisterHandler(new CurseHandler());
        RegisterHandler(new ImmortalityHandler());
        RegisterHandler(new EvasionHandler());
        RegisterHandler(new NextTurnWeaknessApplyHandler(ConfusionStatusId));
        RegisterHandler(new NextTurnWeaknessApplyHandler(NextTurnWeaknessStatusId));
        RegisterHandler(new EnemyStrengthBuffHandler());
        isDefaultHandlerRegistered = true;
    }

    private bool TryGetStatusEffectData(string statusEffectId, out StatusEffectData loadedData)
    {
        loadedData = null;
        if (DataCenter.Instance == null)
        {
            Log("DataCenter.Instance가 없어 상태효과 데이터를 조회할 수 없습니다.");
            return false;
        }

        StatusEffectData resolvedData = null;
        DataCenter.Instance.GetStatusEffectData(statusEffectId, data => resolvedData = data);
        loadedData = resolvedData;
        if (loadedData == null)
        {
            Log($"상태효과 데이터를 찾을 수 없습니다. id={statusEffectId}");
            return false;
        }

        return true;
    }

    private void ApplyStackPolicy(StatusEffectStackPolicy policy, StatusEffectRuntime runtime, int incomingStack, int incomingDuration, int incomingValue)
    {
        switch (policy)
        {
            case StatusEffectStackPolicy.NoDuplicate:
                break;
            case StatusEffectStackPolicy.AddStack:
                runtime.AddStack(incomingStack);
                runtime.RemainingTurns = Mathf.Max(runtime.RemainingTurns, incomingDuration);
                runtime.Value += incomingValue;
                break;
            case StatusEffectStackPolicy.RefreshDuration:
                runtime.Stack = Mathf.Max(runtime.Stack, incomingStack);
                runtime.RemainingTurns = Mathf.Max(runtime.RemainingTurns, incomingDuration);
                runtime.Value = Mathf.Max(runtime.Value, incomingValue);
                break;
            case StatusEffectStackPolicy.StrongestValue:
                runtime.Stack = Mathf.Max(runtime.Stack, incomingStack);
                runtime.RemainingTurns = Mathf.Max(runtime.RemainingTurns, incomingDuration);
                runtime.Value = Mathf.Max(runtime.Value, incomingValue);
                break;
        }
    }

    private static void MarkTurnDurationSkip(StatusEffectRuntime runtime, StatusEffectData data)
    {
        if (runtime == null || data == null)
        {
            return;
        }

        if (data.DurationType != (int)StatusEffectDurationType.OneTurn
            && data.DurationType != (int)StatusEffectDurationType.ValueTurns)
        {
            return;
        }

        runtime.SuppressNextDurationTick = true;
    }

    private void TryReleaseByCondition(StatusEffectRuntime runtime, DamageContext context)
    {
        if (runtime == null || context == null || !activeStatusMap.ContainsKey(runtime.StatusEffectId))
        {
            return;
        }

        bool wasHitByOther = context.Source != null && context.Source != context.Target;
        bool hpDecreased = context.FinalDamage > 0;
        if (runtime.ReleaseCondition == (int)StatusEffectReleaseCondition.OnHit && wasHitByOther)
        {
            RemoveStatus(runtime.StatusEffectId);
            return;
        }

        if (runtime.ReleaseCondition == (int)StatusEffectReleaseCondition.OnHpLoss && hpDecreased)
        {
            RemoveStatus(runtime.StatusEffectId);
        }
    }

    private void CleanupExpiredStatuses()
    {
        if (activeStatusMap.Count == 0)
        {
            return;
        }

        List<string> keys = new List<string>(activeStatusMap.Keys);
        for (int i = 0; i < keys.Count; i++)
        {
            if (activeStatusMap.TryGetValue(keys[i], out StatusEffectRuntime runtime) && runtime.IsExpired)
            {
                RemoveStatus(keys[i]);
            }
        }
    }

    private void Log(string message)
    {
        if (!enableDebugLog)
        {
            return;
        }

        Debug.Log($"[StatusEffectController] {message}", this);
    }
}
