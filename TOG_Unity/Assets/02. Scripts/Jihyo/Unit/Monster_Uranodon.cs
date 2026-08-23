using UnityEngine;

/// <summary>
/// 우라노돈. 방어 -> 공격 -> 버프(저주의 가시) 순환.
/// </summary>
public class Monster_Uranodon : Monster
{
    [Header("우라노돈 데이터 ID")]
    [SerializeField] private string monsterId = "41001003";

    protected override void Awake()
    {
        SetMonsterDataId(monsterId);
        base.Awake();
    }

    protected override void ConfigureMonsterTraits()
    {
        string guardId = "2410002";
        int guardMin = 60;
        int guardMax = 66;
        string attackId = "2410001";
        int attackMin = 15;
        int attackMax = 15;
        string buffId = "2410003";
        int buffMin = 0;
        int buffMax = 0;
        string statusEffectId = StatusEffectController.CurseThornsStatusId;
        int statusValue = 0;

        if (TryGetLoadedMonsterData(out MonsterData data))
        {
            if (!string.IsNullOrEmpty(data.Action2ID))
            {
                guardId = data.Action2ID;
                guardMin = data.Action2Min;
                guardMax = data.Action2Max;
            }

            if (!string.IsNullOrEmpty(data.Action1ID))
            {
                attackId = data.Action1ID;
                attackMin = data.Action1Min;
                attackMax = data.Action1Max;
            }

            if (!string.IsNullOrEmpty(data.Action3ID))
            {
                buffId = data.Action3ID;
                buffMin = data.Action3Min;
                buffMax = data.Action3Max;
            }

            statusEffectId = ResolvePrimaryStatusEffectId(statusEffectId);
            statusValue = ResolvePrimaryStatusValue(Mathf.Max(buffMin, buffMax));
        }

        OverrideBehavior(
            MonsterActionPatternType.Cycle,
            CreateUranodonAction(guardId, guardMin, guardMax, statusEffectId, statusValue),
            CreateUranodonAction(attackId, attackMin, attackMax, statusEffectId, statusValue),
            CreateUranodonAction(buffId, buffMin, buffMax, statusEffectId, statusValue)
        );
    }

    private MonsterActionDefinition CreateUranodonAction(string actionId, int min, int max, string statusEffectId, int statusValue)
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
            case "2410002":
                definition.ActionType = MonsterActionType.Guard;
                definition.TargetType = MonsterActionTargetType.Self;
                break;
            case "2410003":
                definition.ActionType = MonsterActionType.ApplyStatus;
                definition.TargetType = MonsterActionTargetType.Self;
                definition.StatusEffectId = statusEffectId;
                definition.StatusStack = 1;
                definition.StatusValue = Mathf.Max(0, statusValue);
                break;
            default:
                definition.ActionType = MonsterActionType.Attack;
                definition.TargetType = MonsterActionTargetType.Player;
                break;
        }

        return definition;
    }
}
