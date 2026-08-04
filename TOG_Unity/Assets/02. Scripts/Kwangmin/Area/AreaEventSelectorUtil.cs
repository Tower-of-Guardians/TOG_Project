using System.Collections.Generic;
using UnityEngine;

public static class AreaEventSelectorUtil
{
    private const int MaxSafetyLoopCount = 100;
    private const int TargetChoiceCount = 3;

    public static List<AreaEventType> GetNextRegionChoices(AreaEventData data, PlayerEventStatus status)
    {
        if (data == null)
        {
            Debug.LogError("[AreaEventSelectorUtil] AreaEventData가 null입니다.");
            return new List<AreaEventType>();
        }

        var runtimeWeights = CalculateRuntimeWeights(data, status);
        var selectedEvents = new List<AreaEventType>();
        int safetyNet = 0;

        while (selectedEvents.Count < TargetChoiceCount && safetyNet < MaxSafetyLoopCount)
        {
            safetyNet++;

            AreaEventType picked = GetWeightedRandomEvent(runtimeWeights);
            if (picked == (AreaEventType)(-1))
            {
                break;
            }

            selectedEvents.Add(picked);
            runtimeWeights[picked] = 0;
        }

        return selectedEvents;
    }

    private static AreaEventType GetWeightedRandomEvent(Dictionary<AreaEventType, int> weights)
    {
        int totalWeight = 0;
        foreach (var weight in weights.Values)
        {
            totalWeight += weight;
        }

        if (totalWeight <= 0)
        {
            return (AreaEventType)(-1);
        }

        int roll = Random.Range(1, totalWeight + 1);
        int processedWeight = 0;

        foreach (var kvp in weights)
        {
            processedWeight += kvp.Value;
            if (roll <= processedWeight)
            {
                return kvp.Key;
            }
        }

        return (AreaEventType)(-1);
    }

    private static Dictionary<AreaEventType, int> CalculateRuntimeWeights(AreaEventData data, PlayerEventStatus status)
    {
        int shopWeight = status.ShopCountInStage >= 1 ? 0 : data.MerchantEvent;
        int smithyWeight = status.SmithyCountInStage >= 2 ? 0 : data.SmithyEvent;
        int blessingWeight = status.BlessingCooldownTurns > 0 ? 0 : data.BlessingEvent;

        return new Dictionary<AreaEventType, int>
        {
            { AreaEventType.Boss, data.BossEvent },
            { AreaEventType.Shop, shopWeight },
            { AreaEventType.Battle, data.BattleEvent },
            { AreaEventType.Blacksmith, smithyWeight },
            { AreaEventType.Blessing, blessingWeight },
            { AreaEventType.Random, data.RandomEvent }
        };
    }
}