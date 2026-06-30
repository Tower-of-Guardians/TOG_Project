using System.Collections.Generic;
using UnityEngine;

public class AreaEventSelectorUtil
{
    public static List<AreaEventType> GetNextRegionChoices(AreaEventData data, PlayerEventStatus status)
    {
        if (data == null)
        {
            Debug.LogError("Null");
            return new List<AreaEventType>();
        }

        Dictionary<AreaEventType, int> runtimeWeights = CalculateRuntimeWeights(data, status);

        List<AreaEventType> selectedEvents = new List<AreaEventType>();
        int safetyNet = 0;

        while (selectedEvents.Count < 3 && safetyNet < 100)
        {
            safetyNet++;

            AreaEventType picked = GetWeightedRandomEvent(runtimeWeights);

            if (picked != (AreaEventType)( -1 ))
            {
                selectedEvents.Add(picked);

                runtimeWeights[picked] = 0;
            }
            else
            {
                break;
            }
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
            return (AreaEventType)( -1 );

        int roll = UnityEngine.Random.Range(1, totalWeight + 1);
        int processedWeight = 0;

        foreach (var kvp in weights)
        {
            processedWeight += kvp.Value;
            if (roll <= processedWeight)
            {
                return kvp.Key;
            }
        }

        return (AreaEventType)( -1 );
    }

    private static Dictionary<AreaEventType, int> CalculateRuntimeWeights(AreaEventData data, PlayerEventStatus status)
    {
        Dictionary<AreaEventType, int> weightsDict = new Dictionary<AreaEventType, int>();

        int boss = data.BossEvent;
        int shop = data.MerchantEvent;
        int battle = data.BattleEvent;
        int smithy = data.SmithyEvent;
        int blessing = data.BlessingEvent;
        int random = data.RandomEvent;

        if (status.ShopCountInStage >= 1)
            shop = 0;
        if (status.SmithyCountInStage >= 2)
            smithy = 0;
        if (status.BlessingCooldownTurns > 0)
            blessing = 0;

        weightsDict.Add(AreaEventType.Boss, boss);
        weightsDict.Add(AreaEventType.Shop, shop);
        weightsDict.Add(AreaEventType.Battle, battle);
        weightsDict.Add(AreaEventType.Blacksmith, smithy);
        weightsDict.Add(AreaEventType.Blessing, blessing);
        weightsDict.Add(AreaEventType.Random, random);

        return weightsDict;
    }
}