using UnityEngine;

/// <summary>
/// 헥터 전용 몬스터. 현재 WhiteDog 외형/애니메이션을 공유하며 MonsterData(41001004) 스탯·패턴만 적용합니다.
/// 행동 패턴: 공격 -> 보호 -> 상태부여 순환
/// </summary>
public class Monster_Hector : Monster
{
    [Header("헥터 데이터 ID")]
    [SerializeField] private string monsterId = "41001004";

    protected override void Awake()
    {
        SetMonsterDataId(monsterId);
        base.Awake();
    }

    protected override void ConfigureMonsterTraits()
    {
        string action1Id = "2410001";
        int action1Min = 7;
        int action1Max = 8;
        string action2Id = "2410002";
        int action2Min = 10;
        int action2Max = 11;
        string action3Id = "2410003";
        int action3Min = 5;
        int action3Max = 5;

        if (TryGetLoadedMonsterData(out MonsterData data))
        {
            if (!string.IsNullOrEmpty(data.Action1ID))
            {
                action1Id = data.Action1ID;
                action1Min = data.Action1Min;
                action1Max = data.Action1Max;
            }

            if (!string.IsNullOrEmpty(data.Action2ID))
            {
                action2Id = data.Action2ID;
                action2Min = data.Action2Min;
                action2Max = data.Action2Max;
            }

            if (!string.IsNullOrEmpty(data.Action3ID))
            {
                action3Id = data.Action3ID;
                action3Min = data.Action3Min;
                action3Max = data.Action3Max;
            }
        }

        OverrideBehavior(
            MonsterActionPatternType.Cycle,
            CreateHectorAction(action1Id, action1Min, action1Max),
            CreateHectorAction(action2Id, action2Min, action2Max),
            CreateHectorAction(action3Id, action3Min, action3Max)
        );
    }

    private static MonsterActionDefinition CreateHectorAction(string actionId, int min, int max)
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
                definition.TargetType = MonsterActionTargetType.Player;
                definition.StatusEffectId = StatusEffectController.CurseStatusId;
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
