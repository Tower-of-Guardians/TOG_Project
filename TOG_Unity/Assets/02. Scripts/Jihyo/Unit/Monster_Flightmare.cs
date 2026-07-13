using UnityEngine;

/// <summary>
/// 플라이트메어. 공격 후 자신에게 힘:적(51003029) +1을 부여합니다.
/// 힘:적은 영구(누적)이므로 첫 공격은 기본 8, 이후 매 공격마다 +1씩 누적됩니다.
/// </summary>
public class Monster_Flightmare : Monster
{
    private const string StrengthBuffStatusId = StatusEffectController.EnemyStrengthStatusId;

    [Header("플라이트메어 데이터 ID")]
    [SerializeField] private string monsterId = "41001002";

    [Header("공격 후 자가 버프")]
    [SerializeField] private int selfBuffStackPerAttack = 1;

    protected override void Awake()
    {
        SetMonsterDataId(monsterId);
        base.Awake();
    }

    protected override void ConfigureMonsterTraits()
    {
        string attackId = "2410001";
        int attackMin = 8;
        int attackMax = 8;

        if (TryGetLoadedMonsterData(out MonsterData data) && !string.IsNullOrEmpty(data.Action1ID))
        {
            attackId = data.Action1ID;
            attackMin = data.Action1Min;
            attackMax = data.Action1Max;
        }

        OverrideBehavior(
            MonsterActionPatternType.Cycle,
            CreateFlightmareAction(attackId, attackMin, attackMax)
        );
    }

    protected override void OnAfterExecuteSelectedAction(
        MonsterActionDefinition action,
        IDamageable defaultTarget,
        int actionValue)
    {
        if (action == null || action.ActionType != MonsterActionType.Attack)
        {
            return;
        }

        ApplySelfStrengthBuff();
    }

    protected override int GetPreparedAttackDisplayValue(int baseValue)
    {
        return baseValue + GetStrengthStackCount();
    }

    private int GetStrengthStackCount()
    {
        StatusEffectController statusEffectController = GetComponent<StatusEffectController>();
        if (statusEffectController == null)
        {
            return 0;
        }

        return statusEffectController.TryGetStatusStack(StrengthBuffStatusId, out int stack) ? stack : 0;
    }

    private void ApplySelfStrengthBuff()
    {
        StatusEffectController statusEffectController = GetComponent<StatusEffectController>();
        if (statusEffectController == null)
        {
            statusEffectController = gameObject.AddComponent<StatusEffectController>();
        }

        if (statusEffectController.TryApplyStatus(StrengthBuffStatusId, selfBuffStackPerAttack))
        {
            RefreshUI();
        }
    }

    private static MonsterActionDefinition CreateFlightmareAction(string actionId, int min, int max)
    {
        MonsterActionDefinition definition = new MonsterActionDefinition
        {
            ActionId = actionId,
            MinValue = min,
            MaxValue = max,
            ActionType = MonsterActionType.Attack,
            TargetType = MonsterActionTargetType.Player
        };

        return definition;
    }
}
