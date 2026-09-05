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

        if (runtimeWeights[AreaEventType.Battle] > 0)
        {
            selectedEvents.Add(AreaEventType.Battle);
            runtimeWeights[AreaEventType.Battle] = 0;
        }
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
        int configuredEvents = 0;
        foreach (int weight in new[] { data.BossEvent, data.MerchantEvent, data.BattleEvent,
            data.SmithyEvent, data.BlessingEvent, data.RandomEvent })
        {
            if (weight > 0) configuredEvents++;
        }

        bool forcedEvent = configuredEvents == 1;
        int shopWeight = !forcedEvent && status.ShopCountInStage >= 1 ? 0 : data.MerchantEvent;
        int smithyWeight = !forcedEvent && status.SmithyCountInStage >= 2 ? 0 : data.SmithyEvent;
        int blessingWeight = !forcedEvent && status.BlessingCooldownTurns > 0 ? 0 : data.BlessingEvent;

        return new Dictionary<AreaEventType, int>
        {
            { AreaEventType.Boss, Mathf.Max(0, data.BossEvent) },
            { AreaEventType.Shop, Mathf.Max(0, shopWeight) },
            { AreaEventType.Battle, Mathf.Max(0, data.BattleEvent) },
            { AreaEventType.Blacksmith, Mathf.Max(0, smithyWeight) },
            { AreaEventType.Blessing, Mathf.Max(0, blessingWeight) },
            { AreaEventType.Random, Mathf.Max(0, data.RandomEvent) }
        };
    }
}
