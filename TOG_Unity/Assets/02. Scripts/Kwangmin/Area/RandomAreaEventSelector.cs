using System;
using System.Collections.Generic;
using System.Linq;

public static class RandomAreaEventSelector
{
    public static RandomEventData Select(int section)
    {
        List<RandomEventData> candidates = GetCandidates(section);
        if (candidates.Count == 0)
        {
            return null;
        }

        return candidates[Jongmin.RunRandom.Range(Jongmin.RunRandom.Event, 0, candidates.Count)];
    }

    public static bool HasCandidate(int section)
    {
        return GetCandidates(section).Count > 0;
    }

    private static List<RandomEventData> GetCandidates(int section)
    {
        if (DataCenter.randomevent_datas == null)
        {
            return new List<RandomEventData>();
        }

        return DataCenter.randomevent_datas.Values
            .Where(data => data != null && data.Sections != null && data.Sections.Contains(section))
            .Where(data => !Jongmin.RunSaveService.HasExperiencedRandomEvent(data.Id))
            .OrderBy(data => data.Id, StringComparer.Ordinal)
            .ToList();
    }
}
