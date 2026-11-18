using System.Collections.Generic;
using Jongmin;
using UnityEngine;

/// <summary>
/// 시트에 적힌 상태효과 규칙입니다.
/// 지속시간, 해제, 부여 수치는 CSV에서 만든 StatusEffectData와 적용 값({0}, N)을 사용합니다.
/// </summary>
public static class StatusEffectQueries
{
    public static int ResolvePayload(StatusEffectRuntime runtime)
    {
        if (runtime == null)
        {
            return 0;
        }

        if (runtime.Value > 0)
        {
            return runtime.Value;
        }

        return Mathf.Max(0, runtime.Stack);
    }

    public static bool Has(BaseUnit unit, string statusEffectId)
    {
        StatusEffectController controller = unit != null ? unit.GetComponent<StatusEffectController>() : null;
        return controller != null && controller.TryGetStatusStack(statusEffectId, out int stack) && stack > 0;
    }

    public static int GetPayload(BaseUnit unit, string statusEffectId)
    {
        StatusEffectController controller = unit != null ? unit.GetComponent<StatusEffectController>() : null;
        if (controller == null || !controller.TryGetRuntime(statusEffectId, out StatusEffectRuntime runtime))
        {
            return 0;
        }

        return ResolvePayload(runtime);
    }

    public static int GetUserStrengthBonus(BaseUnit unit)
    {
        return GetPayload(unit, StatusEffectController.UserStrengthStatusId);
    }

    /// <summary>
    /// 기본공격력 증가분. 100% 증가는 기본공격력만큼을 더합니다.
    /// </summary>
    public static int GetChastisementBonus(BaseUnit unit, int baseAttack)
    {
        float rate = 0f;
        if (Has(unit, StatusEffectController.Chastisement1StatusId))
        {
            rate += 1f;
        }

        if (Has(unit, StatusEffectController.Chastisement2StatusId))
        {
            rate += 1.5f;
        }

        if (Has(unit, StatusEffectController.Chastisement3StatusId))
        {
            rate += 2f;
        }

        if (rate <= 0f)
        {
            return 0;
        }

        int strength = GetUserStrengthBonus(unit);
        return Mathf.CeilToInt(Mathf.Max(0, baseAttack + strength) * rate);
    }

    public static int GetExtraCardPlays()
    {
        Player player = ResolvePlayer();
        return player != null ? GetPayload(player, StatusEffectController.ApprenticeStatusId) : 0;
    }

    public static bool BlocksHealing(BaseUnit unit)
    {
        return Has(unit, StatusEffectController.NoHealStatusId);
    }

    public static bool AttacksAllEnemies(BaseUnit unit)
    {
        return Has(unit, StatusEffectController.AreaAttackStatusId);
    }

    public static bool KeepsHandAfterAttack(BaseUnit unit)
    {
        return Has(unit, StatusEffectController.BeggarStatusId);
    }

    public static int GetLifeStealPercent(BaseUnit unit)
    {
        int percent = 0;
        if (Has(unit, StatusEffectController.LifeSteal1StatusId))
        {
            percent += 4;
        }

        if (Has(unit, StatusEffectController.LifeSteal2StatusId))
        {
            percent += 10;
        }

        if (Has(unit, StatusEffectController.LifeSteal3StatusId))
        {
            percent += 30;
        }

        return percent;
    }

    public static void RefreshRuleBindings(BaseUnit unit)
    {
        if (unit is Player player)
        {
            player.ApplyAttackStats();
        }

        if (DIContainer.IsRegistered<TurnManager>())
        {
            DIContainer.Resolve<TurnManager>()?.RefreshActionLimit();
        }
    }

    public static void NotifyMonsterKilled(Monster dead)
    {
        if (dead == null || !DIContainer.IsRegistered<BattleManager>())
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
            Monster monster = monsters[i];
            if (monster == null || monster == dead || !monster.IsAlive)
            {
                continue;
            }

            int bonus = GetPayload(monster, StatusEffectController.KinRevengeStatusId);
            if (bonus <= 0)
            {
                continue;
            }

            StatusEffectController.Resolve(monster)?.TryApplyStatus(StatusEffectController.EnemyStrengthStatusId, bonus, bonus);
        }
    }

    private static Player ResolvePlayer()
    {
        if (!DIContainer.IsRegistered<BattleManager>())
        {
            return null;
        }

        return DIContainer.Resolve<BattleManager>()?.GetSetupController()?.GetPlayer();
    }
}

public sealed class StunHandler : IStatusEffectHandler
{
    public string StatusEffectId => StatusEffectController.StunStatusId;
    public StatusEffectStackPolicy StackPolicy => StatusEffectStackPolicy.RefreshDuration;

    public int ResolveInitialStack(StatusEffectData data, int requestedStack) => Mathf.Max(1, requestedStack);
    public int ResolveInitialValue(StatusEffectData data, int requestedValue) => Mathf.Max(0, requestedValue);

    public void OnApplied(StatusEffectController controller, StatusEffectRuntime runtime, bool isNew)
    {
        controller?.GetComponent<Monster>()?.ApplyStunToPreparedAction();
    }

    public void OnTurnStart(StatusEffectController controller, StatusEffectRuntime runtime) { }
    public void OnTurnEnd(StatusEffectController controller, StatusEffectRuntime runtime) { }
    public void OnBeforeDealDamage(StatusEffectController controller, StatusEffectRuntime runtime, DamageContext context) { }
    public void OnBeforeTakeDamage(StatusEffectController controller, StatusEffectRuntime runtime, DamageContext context) { }
    public void OnAfterTakeDamage(StatusEffectController controller, StatusEffectRuntime runtime, DamageContext context) { }
}

public sealed class SlayerHandler : IStatusEffectHandler
{
    private readonly float damageIncreaseRate;

    public SlayerHandler(string statusEffectId, float damageIncreaseRate)
    {
        StatusEffectId = statusEffectId;
        this.damageIncreaseRate = damageIncreaseRate;
    }

    public string StatusEffectId { get; }
    public StatusEffectStackPolicy StackPolicy => StatusEffectStackPolicy.RefreshDuration;

    public int ResolveInitialStack(StatusEffectData data, int requestedStack) => Mathf.Max(1, requestedStack);
    public int ResolveInitialValue(StatusEffectData data, int requestedValue) => Mathf.Max(0, requestedValue);

    public void OnApplied(StatusEffectController controller, StatusEffectRuntime runtime, bool isNew) { }
    public void OnTurnStart(StatusEffectController controller, StatusEffectRuntime runtime) { }
    public void OnTurnEnd(StatusEffectController controller, StatusEffectRuntime runtime) { }

    public void OnBeforeDealDamage(StatusEffectController controller, StatusEffectRuntime runtime, DamageContext context)
    {
        if (context?.Target is not Monster monster || !monster.IsBoss || damageIncreaseRate <= 0f)
        {
            return;
        }

        context.MultiplyFinalDamage(1f + damageIncreaseRate);
    }

    public void OnBeforeTakeDamage(StatusEffectController controller, StatusEffectRuntime runtime, DamageContext context) { }
    public void OnAfterTakeDamage(StatusEffectController controller, StatusEffectRuntime runtime, DamageContext context) { }
}

public sealed class ShieldAttackStatusHandler : IStatusEffectHandler
{
    private readonly float protectionRate;

    public ShieldAttackStatusHandler(string statusEffectId, float protectionRate)
    {
        StatusEffectId = statusEffectId;
        this.protectionRate = protectionRate;
    }

    public string StatusEffectId { get; }
    public StatusEffectStackPolicy StackPolicy => StatusEffectStackPolicy.RefreshDuration;

    public int ResolveInitialStack(StatusEffectData data, int requestedStack) => Mathf.Max(1, requestedStack);
    public int ResolveInitialValue(StatusEffectData data, int requestedValue) => Mathf.Max(0, requestedValue);

    public void OnApplied(StatusEffectController controller, StatusEffectRuntime runtime, bool isNew) { }
    public void OnTurnStart(StatusEffectController controller, StatusEffectRuntime runtime) { }
    public void OnTurnEnd(StatusEffectController controller, StatusEffectRuntime runtime) { }

    public void OnBeforeDealDamage(StatusEffectController controller, StatusEffectRuntime runtime, DamageContext context)
    {
        BaseUnit source = context?.Source;
        if (source == null || protectionRate <= 0f)
        {
            return;
        }

        int bonus = Mathf.CeilToInt(source.ProtectionValue * protectionRate);
        if (bonus > 0)
        {
            context.AddFinalDamage(bonus);
        }
    }

    public void OnBeforeTakeDamage(StatusEffectController controller, StatusEffectRuntime runtime, DamageContext context) { }
    public void OnAfterTakeDamage(StatusEffectController controller, StatusEffectRuntime runtime, DamageContext context) { }
}

public sealed class BonusDamageHandler : IStatusEffectHandler
{
    public string StatusEffectId => StatusEffectController.BonusDamageStatusId;
    public StatusEffectStackPolicy StackPolicy => StatusEffectStackPolicy.AddStack;

    public int ResolveInitialStack(StatusEffectData data, int requestedStack) => Mathf.Max(1, requestedStack);
    public int ResolveInitialValue(StatusEffectData data, int requestedValue) => Mathf.Max(0, requestedValue);

    public void OnApplied(StatusEffectController controller, StatusEffectRuntime runtime, bool isNew) { }
    public void OnTurnStart(StatusEffectController controller, StatusEffectRuntime runtime) { }
    public void OnTurnEnd(StatusEffectController controller, StatusEffectRuntime runtime) { }

    public void OnBeforeDealDamage(StatusEffectController controller, StatusEffectRuntime runtime, DamageContext context)
    {
        int bonus = StatusEffectQueries.ResolvePayload(runtime);
        if (bonus > 0)
        {
            context?.AddFinalDamage(bonus);
        }
    }

    public void OnBeforeTakeDamage(StatusEffectController controller, StatusEffectRuntime runtime, DamageContext context) { }
    public void OnAfterTakeDamage(StatusEffectController controller, StatusEffectRuntime runtime, DamageContext context) { }
}

public sealed class BonusDrawHandler : IStatusEffectHandler
{
    public string StatusEffectId => StatusEffectController.BonusDrawStatusId;
    public StatusEffectStackPolicy StackPolicy => StatusEffectStackPolicy.AddStack;

    public int ResolveInitialStack(StatusEffectData data, int requestedStack) => Mathf.Max(1, requestedStack);
    public int ResolveInitialValue(StatusEffectData data, int requestedValue) => Mathf.Max(0, requestedValue);

    public void OnApplied(StatusEffectController controller, StatusEffectRuntime runtime, bool isNew)
    {
        if (!isNew)
        {
            return;
        }

        int count = StatusEffectQueries.ResolvePayload(runtime);
        if (count <= 0 || !DIContainer.IsRegistered<BattleManager>())
        {
            return;
        }

        DIContainer.Resolve<BattleManager>()?.AddPendingExtraDraw(count);
    }

    public void OnTurnStart(StatusEffectController controller, StatusEffectRuntime runtime) { }
    public void OnTurnEnd(StatusEffectController controller, StatusEffectRuntime runtime) { }
    public void OnBeforeDealDamage(StatusEffectController controller, StatusEffectRuntime runtime, DamageContext context) { }
    public void OnBeforeTakeDamage(StatusEffectController controller, StatusEffectRuntime runtime, DamageContext context) { }
    public void OnAfterTakeDamage(StatusEffectController controller, StatusEffectRuntime runtime, DamageContext context) { }
}

public sealed class KinRevengeHandler : IStatusEffectHandler
{
    public string StatusEffectId => StatusEffectController.KinRevengeStatusId;
    public StatusEffectStackPolicy StackPolicy => StatusEffectStackPolicy.RefreshDuration;

    public int ResolveInitialStack(StatusEffectData data, int requestedStack) => Mathf.Max(1, requestedStack);
    public int ResolveInitialValue(StatusEffectData data, int requestedValue) => Mathf.Max(0, requestedValue);

    public void OnApplied(StatusEffectController controller, StatusEffectRuntime runtime, bool isNew) { }
    public void OnTurnStart(StatusEffectController controller, StatusEffectRuntime runtime) { }
    public void OnTurnEnd(StatusEffectController controller, StatusEffectRuntime runtime) { }
    public void OnBeforeDealDamage(StatusEffectController controller, StatusEffectRuntime runtime, DamageContext context) { }
    public void OnBeforeTakeDamage(StatusEffectController controller, StatusEffectRuntime runtime, DamageContext context) { }
    public void OnAfterTakeDamage(StatusEffectController controller, StatusEffectRuntime runtime, DamageContext context) { }
}

public sealed class HitStrengthHandler : IStatusEffectHandler
{
    public string StatusEffectId => StatusEffectController.HitStrengthStatusId;
    public StatusEffectStackPolicy StackPolicy => StatusEffectStackPolicy.RefreshDuration;

    public int ResolveInitialStack(StatusEffectData data, int requestedStack) => Mathf.Max(1, requestedStack);
    public int ResolveInitialValue(StatusEffectData data, int requestedValue) => Mathf.Max(0, requestedValue);

    public void OnApplied(StatusEffectController controller, StatusEffectRuntime runtime, bool isNew) { }
    public void OnTurnStart(StatusEffectController controller, StatusEffectRuntime runtime) { }
    public void OnTurnEnd(StatusEffectController controller, StatusEffectRuntime runtime) { }
    public void OnBeforeDealDamage(StatusEffectController controller, StatusEffectRuntime runtime, DamageContext context) { }
    public void OnBeforeTakeDamage(StatusEffectController controller, StatusEffectRuntime runtime, DamageContext context) { }

    public void OnAfterTakeDamage(StatusEffectController controller, StatusEffectRuntime runtime, DamageContext context)
    {
        if (controller == null || context == null || context.Source == null || context.Source == context.Target)
        {
            return;
        }

        int bonus = StatusEffectQueries.ResolvePayload(runtime);
        if (bonus <= 0)
        {
            return;
        }

        controller.TryApplyStatus(StatusEffectController.EnemyStrengthStatusId, bonus, bonus);
    }
}

public sealed class TwinsHandler : IStatusEffectHandler
{
    public string StatusEffectId => StatusEffectController.TwinsStatusId;
    public StatusEffectStackPolicy StackPolicy => StatusEffectStackPolicy.RefreshDuration;

    public int ResolveInitialStack(StatusEffectData data, int requestedStack) => Mathf.Max(1, requestedStack);
    public int ResolveInitialValue(StatusEffectData data, int requestedValue) => Mathf.Max(0, requestedValue);

    public void OnApplied(StatusEffectController controller, StatusEffectRuntime runtime, bool isNew) { }
    public void OnTurnStart(StatusEffectController controller, StatusEffectRuntime runtime) { }
    public void OnTurnEnd(StatusEffectController controller, StatusEffectRuntime runtime) { }

    public void OnBeforeDealDamage(StatusEffectController controller, StatusEffectRuntime runtime, DamageContext context)
    {
        if (context == null || !DIContainer.IsRegistered<TurnManager>())
        {
            return;
        }

        TurnManager turnManager = DIContainer.Resolve<TurnManager>();
        if (turnManager != null && turnManager.CurrentActionCount == 2)
        {
            context.MultiplyFinalDamage(3f);
        }
    }

    public void OnBeforeTakeDamage(StatusEffectController controller, StatusEffectRuntime runtime, DamageContext context) { }
    public void OnAfterTakeDamage(StatusEffectController controller, StatusEffectRuntime runtime, DamageContext context) { }
}
