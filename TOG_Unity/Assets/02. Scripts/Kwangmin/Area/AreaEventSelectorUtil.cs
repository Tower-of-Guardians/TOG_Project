using System.Collections.Generic;
using UnityEngine;

public static class AreaEventSelectorUtil
{
    private const int DrawCount = 3;
    private const int MaxPassCount = 1000;
    private const int MaxChance = 100;

    public static List<AreaEventType> GetNextRegionChoices(AreaEventData data, PlayerEventStatus status)
    {
        var choices = new List<AreaEventType>();
        if (data == null)
        {
            Debug.LogError("[AreaEventSelectorUtil] AreaEventData가 null입니다.");
            return choices;
        }

        List<(AreaEventType type, int chance)> chances = CalculateRuntimeChances(data, status);
        if (!chances.Exists(entry => entry.chance > 0))
        {
            return choices;
        }

        for (int i = 0; i < DrawCount; i++)
        {
            if (TryDrawEvent(chances, out AreaEventType picked) && !choices.Contains(picked))
            {
                choices.Add(picked);
            }
        }

        return choices;
    }

    private static bool TryDrawEvent(List<(AreaEventType type, int chance)> chances, out AreaEventType picked)
    {
        for (int pass = 0; pass < MaxPassCount; pass++)
        {
            foreach (var (type, chance) in chances)
            {
                if (chance <= 0) continue;
                if (Jongmin.RunRandom.Range(Jongmin.RunRandom.Area, 0, MaxChance) < chance)
                {
                    picked = type;
                    return true;
                }
            }
        }

        picked = default;
        return false;
    }

    private static List<(AreaEventType type, int chance)> CalculateRuntimeChances(AreaEventData data, PlayerEventStatus status)
    {
        int configuredEvents = 0;
        foreach (int chance in new[] { data.BossEvent, data.MerchantEvent, data.SmithyEvent,
            data.BlessingEvent, data.RandomEvent, data.BattleEvent })
        {
            if (chance > 0) configuredEvents++;
        }

        bool forcedEvent = configuredEvents == 1;
        int shopChance = !forcedEvent && status.ShopCountInStage >= 1 ? 0 : data.MerchantEvent;
        int smithyChance = !forcedEvent && status.SmithyCountInStage >= 2 ? 0 : data.SmithyEvent;
        int blessingChance = !forcedEvent && status.BlessingCooldownTurns > 0 ? 0 : data.BlessingEvent;
        int randomChance = RandomAreaEventSelector.HasCandidate(data.Section) ? data.RandomEvent : 0;

        return new List<(AreaEventType type, int chance)>
        {
            (AreaEventType.Boss, Mathf.Max(0, data.BossEvent)),
            (AreaEventType.Shop, Mathf.Max(0, shopChance)),
            (AreaEventType.Blacksmith, Mathf.Max(0, smithyChance)),
            (AreaEventType.Blessing, Mathf.Max(0, blessingChance)),
            (AreaEventType.Random, Mathf.Max(0, randomChance)),
            (AreaEventType.Battle, Mathf.Max(0, data.BattleEvent))
        };
    }
}
