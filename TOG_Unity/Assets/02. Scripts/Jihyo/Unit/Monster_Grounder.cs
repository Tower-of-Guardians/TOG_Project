using UnityEngine;

/// <summary>
/// 그라운더 전용 행동 패턴: 공격 -> 디버프(약점노출) 순환
/// </summary>
public class Monster_Grounder : Monster
{
    [Header("그라운더 데이터 ID")]
    [SerializeField] private string monsterId = "41001001";

    protected override void Awake()
    {
        SetMonsterDataId(monsterId);
        base.Awake();
    }

    protected override void ConfigureMonsterTraits()
    {
        string attackId = "2410001";
        int attackMin = 7;
        int attackMax = 8;
        string debuffId = "2410003";
        int debuffMin = 1;
        int debuffMax = 1;

        if (TryGetLoadedMonsterData(out MonsterData data))
        {
            if (!string.IsNullOrEmpty(data.Action1ID))
            {
                attackId = data.Action1ID;
                attackMin = data.Action1Min;
                attackMax = data.Action1Max;
            }

            if (!string.IsNullOrEmpty(data.Action3ID))
            {
                debuffId = data.Action3ID;
                debuffMin = data.Action3Min;
                debuffMax = data.Action3Max;
            }
        }

        OverrideBehavior(
            MonsterActionPatternType.Cycle,
            CreateGrounderAction(attackId, attackMin, attackMax),
            CreateGrounderAction(debuffId, debuffMin, debuffMax)
        );
    }

    private static MonsterActionDefinition CreateGrounderAction(string actionId, int min, int max)
    {
        MonsterActionDefinition definition = new MonsterActionDefinition
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
            case "2410003":
                definition.ActionType = MonsterActionType.ApplyStatus;
                definition.TargetType = MonsterActionTargetType.Player;
                definition.StatusEffectId = StatusEffectController.WeaknessExposureStatusId;
                definition.StatusStack = Mathf.Max(1, min);
                break;
            default:
                definition.ActionType = MonsterActionType.Attack;
                definition.TargetType = MonsterActionTargetType.Player;
                break;
        }

        return definition;
    }
}
