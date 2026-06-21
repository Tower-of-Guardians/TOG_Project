using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 클로니어. 공격 → 소환(2.5 → 7.5)을 순환하며,
/// 소환 슬롯이 없으면 회복합니다.
/// </summary>
public class Monster_Clonier : Monster
{
    private const string SummonActionId = "2410004";
    private const string AttackActionId = "2410001";
    private const string HealFallbackActionId = "CLONIER_HEAL_FALLBACK";
    private const int MaxCloneCount = 2;

    private static readonly float[] CloneSummonSlotPositions = { 2.5f, 7.5f };

    [Header("클로니어 데이터 ID")]
    [SerializeField] private string monsterId = "41002004";

    [Header("소환")]
    [SerializeField] private Monster_ClonierClone clonePrefab;

    [Header("회복 (소환 불가 시)")]
    [SerializeField] private int healMin = 15;
    [SerializeField] private int healMax = 20;

    protected override void Awake()
    {
        SetMonsterDataId(monsterId);
        base.Awake();
    }

    protected override void ConfigureMonsterTraits()
    {
        int attackMin = 17;
        int attackMax = 20;

        if (TryGetLoadedMonsterData(out MonsterData data))
        {
            if (!string.IsNullOrEmpty(data.Action1ID) && data.Action1ID == AttackActionId)
            {
                attackMin = data.Action1Min;
                attackMax = data.Action1Max;
            }
        }

        OverrideBehavior(
            MonsterActionPatternType.Cycle,
            CreateClonierAction(AttackActionId, attackMin, attackMax),
            CreateClonierAction(SummonActionId, 0, 0)
        );
    }

    protected override MonsterActionDefinition AdjustPreparedAction(MonsterActionDefinition action)
    {
        if (action != null
            && action.ActionType == MonsterActionType.Summon
            && !CanSummonClone())
        {
            return CreateHealFallbackAction();
        }

        return action;
    }

    protected override void ExecuteSummonAction(int actionValue)
    {
        if (clonePrefab == null)
        {
            Debug.LogWarning($"{name}: clonePrefab이 할당되지 않아 소환할 수 없습니다.");
            return;
        }

        if (!TryGetNextSummonSlot(out float slotX))
        {
            return;
        }

        Transform spawnParent = transform.parent != null ? transform.parent : transform;
        Monster_ClonierClone clone = Instantiate(clonePrefab, spawnParent);

        Vector3 spawnPosition = transform.position;
        spawnPosition.x = slotX;
        clone.transform.position = spawnPosition;
        clone.BindSummonSlot(slotX);
        clone.RefreshMonsterDataStats();
        clone.PrepareActionForTurn();
    }

    private bool CanSummonClone()
    {
        return GetAliveCloneCount() < MaxCloneCount && TryGetNextSummonSlot(out _);
    }

    private bool TryGetNextSummonSlot(out float slotX)
    {
        for (int i = 0; i < CloneSummonSlotPositions.Length; i++)
        {
            float position = CloneSummonSlotPositions[i];
            if (!IsCloneSlotOccupied(position))
            {
                slotX = position;
                return true;
            }
        }

        slotX = 0f;
        return false;
    }

    private bool IsCloneSlotOccupied(float slotX)
    {
        List<Monster> allies = GetAliveAllies();
        for (int i = 0; i < allies.Count; i++)
        {
            if (allies[i] is Monster_ClonierClone clone
                && clone != this
                && clone.HasSummonSlot
                && Mathf.Approximately(clone.SummonSlotX, slotX))
            {
                return true;
            }
        }

        return false;
    }

    private int GetAliveCloneCount()
    {
        int count = 0;
        List<Monster> allies = GetAliveAllies();
        for (int i = 0; i < allies.Count; i++)
        {
            if (allies[i] is Monster_ClonierClone clone && clone != this && clone.IsAlive)
            {
                count++;
            }
        }

        return count;
    }

    private static MonsterActionDefinition CreateClonierAction(string actionId, int min, int max)
    {
        MonsterActionDefinition definition = new MonsterActionDefinition
        {
            ActionId = actionId,
            MinValue = min,
            MaxValue = max
        };

        switch (actionId)
        {
            case SummonActionId:
                definition.ActionType = MonsterActionType.Summon;
                definition.TargetType = MonsterActionTargetType.None;
                break;
            case AttackActionId:
                definition.ActionType = MonsterActionType.Attack;
                definition.TargetType = MonsterActionTargetType.Player;
                break;
            default:
                definition.ActionType = MonsterActionType.Attack;
                definition.TargetType = MonsterActionTargetType.Player;
                break;
        }

        return definition;
    }

    private MonsterActionDefinition CreateHealFallbackAction()
    {
        return new MonsterActionDefinition
        {
            ActionId = HealFallbackActionId,
            ActionType = MonsterActionType.Heal,
            TargetType = MonsterActionTargetType.Self,
            MinValue = healMin,
            MaxValue = healMax
        };
    }
}
