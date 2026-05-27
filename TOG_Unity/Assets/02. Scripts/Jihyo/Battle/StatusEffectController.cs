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
    int ResolveInitialDuration(StatusEffectData data);
    int ResolveInitialValue(StatusEffectData data, int requestedValue);
    void OnApplied(StatusEffectController controller, StatusEffectRuntime runtime, bool isNew);
    void OnTurnStart(StatusEffectController controller, StatusEffectRuntime runtime);
    void OnTurnEnd(StatusEffectController controller, StatusEffectRuntime runtime);
    void OnBeforeDealDamage(StatusEffectController controller, StatusEffectRuntime runtime, DamageContext context);
    void OnBeforeTakeDamage(StatusEffectController controller, StatusEffectRuntime runtime, DamageContext context);
    void OnAfterTakeDamage(StatusEffectController controller, StatusEffectRuntime runtime, DamageContext context);
}

/// <summary>
/// 약점노출(51001001) 핸들러.
/// 다음 피격 1회 피해를 50% 증가시키고 소모됩니다.
/// </summary>
public sealed class WeaknessExposureHandler : IStatusEffectHandler
{
    public string StatusEffectId => StatusEffectController.WeaknessExposureStatusId;
    public StatusEffectStackPolicy StackPolicy => StatusEffectStackPolicy.RefreshDuration;

    public int ResolveInitialStack(StatusEffectData data, int requestedStack)
    {
        return Mathf.Max(1, requestedStack);
    }

    public int ResolveInitialDuration(StatusEffectData data)
    {
        // 약점노출은 최대 3턴 유지되며, 그 전에 피격 1회가 발생하면 소모됩니다.
        return 3;
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
        runtime.ConsumeStack(1);
        controller.NotifyStatusChanged(runtime);

        if (runtime.IsExpired)
        {
            controller.RemoveStatus(runtime.StatusEffectId);
        }
    }

    public void OnAfterTakeDamage(StatusEffectController controller, StatusEffectRuntime runtime, DamageContext context) { }
}

/// <summary>
/// 상태효과의 부여/해제/훅 실행을 담당하는 컨트롤러입니다.
/// 상태 정의는 DataCenter(StatusEffectData)에서 조회합니다.
/// </summary>
public class StatusEffectController : MonoBehaviour
{
    public const string WeaknessExposureStatusId = "51001001";

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

    public bool TryApplyStatus(string statusEffectId, int requestedStack = 1, int requestedValue = 0)
    {
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
            Log($"핸들러가 등록되지 않은 상태효과입니다. id={statusEffectId}");
            return false;
        }

        int incomingStack = handler.ResolveInitialStack(data, requestedStack);
        int incomingDuration = handler.ResolveInitialDuration(data);
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
            OnStatusAdded?.Invoke(runtime);
        }
        else
        {
            ApplyStackPolicy(handler.StackPolicy, runtime, incomingStack, incomingDuration, incomingValue);
            OnStatusChanged?.Invoke(runtime);
        }

        handler.OnApplied(this, runtime, created);
        CleanupExpiredStatuses();
        return true;
    }

    public bool TryApplyFromStatusEffectId(string statusEffectId, int requestedStack = 1, int requestedValue = 0)
    {
        return TryApplyStatus(statusEffectId, requestedStack, requestedValue);
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

            if (ShouldDecreaseTurn(runtime))
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

    private static bool ShouldDecreaseTurn(StatusEffectRuntime runtime)
    {
        if (runtime == null)
        {
            return false;
        }

        // 턴 기반으로 관리되는 효과는 남은 턴이 유한한 값일 때 감소시킵니다.
        return runtime.RemainingTurns != int.MaxValue;
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
