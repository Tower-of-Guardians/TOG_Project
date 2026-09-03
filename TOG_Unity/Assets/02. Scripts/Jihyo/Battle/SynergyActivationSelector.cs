using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// 필드 시너지 중 실제로 발동할 상위 항목을 고릅니다.
/// UI 슬롯과 동일한 우선순위(발동 여부 → 카드 수 → 낮은 Tier)를 사용합니다.
/// </summary>
public static class SynergyActivationSelector
{
    public const int MaxActivations = 3;

    public static List<SynergyTotalData> Select(IEnumerable<SynergyTotalData> entries, int maxCount = MaxActivations)
    {
        int limit = Mathf.Max(0, maxCount);
        if (entries == null || limit == 0)
        {
            return new List<SynergyTotalData>();
        }

        return entries
            .Where(IsActivated)
            .OrderByDescending(entry => entry.count)
            .ThenBy(entry => entry.synergyData != null ? entry.synergyData.Tier : int.MaxValue)
            .Take(limit)
            .ToList();
    }

    public static int GetLoopPlayCount(int activationCount)
    {
        return Mathf.Clamp(activationCount, 0, MaxActivations);
    }

    public static bool IsActivated(SynergyTotalData entry)
    {
        if (entry?.synergyData == null || entry.count <= 0)
        {
            return false;
        }

        int count = entry.count;
        return GetEffectValueAtCount(entry.synergyData.Effect1Synergys, count) > 0
               || GetEffectValueAtCount(entry.synergyData.Effect2Synergys, count) > 0
               || GetEffectValueAtCount(entry.synergyData.Effect3Synergys, count) > 0;
    }

    public static int GetEffectValueAtCount(IList<int> effectValues, int count)
    {
        if (effectValues == null || effectValues.Count == 0 || count <= 0)
        {
            return 0;
        }

        int index = Mathf.Clamp(count - 1, 0, effectValues.Count - 1);
        int effectValue = effectValues[index];
        return effectValue > 0 ? effectValue : 0;
    }

    public static int GetMinimumActivationCount(SynergyData synergyData)
    {
        if (synergyData == null)
        {
            return 0;
        }

        int maxCount = 0;
        maxCount = Mathf.Max(maxCount, synergyData.Effect1Synergys != null ? synergyData.Effect1Synergys.Count : 0);
        maxCount = Mathf.Max(maxCount, synergyData.Effect2Synergys != null ? synergyData.Effect2Synergys.Count : 0);
        maxCount = Mathf.Max(maxCount, synergyData.Effect3Synergys != null ? synergyData.Effect3Synergys.Count : 0);

        for (int count = 1; count <= maxCount; count++)
        {
            if (IsActivated(new SynergyTotalData { synergyData = synergyData, count = count }))
            {
                return count;
            }
        }

        return 0;
    }
}
