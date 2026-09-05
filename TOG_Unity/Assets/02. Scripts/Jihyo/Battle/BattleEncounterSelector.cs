using System.Collections.Generic;

public static class BattleEncounterSelector
{
    private const int BossMonsterKind = 2;

    public static List<MonsterEncounterData> GetCandidates(
        IEnumerable<MonsterEncounterData> encounters,
        IReadOnlyDictionary<string, MonsterData> monsters,
        int section,
        AreaEventType type)
    {
        var candidates = new List<MonsterEncounterData>();
        if (encounters == null || monsters == null || (type != AreaEventType.Battle && type != AreaEventType.Boss))
        {
            return candidates;
        }

        foreach (MonsterEncounterData encounter in encounters)
        {
            if (encounter == null || encounter.Section != section) continue;

            bool hasMonster = false;
            bool hasBoss = false;
            bool missingMonster = false;
            foreach (string id in new[] { encounter.Mon1ID, encounter.Mon2ID, encounter.Mon3ID, encounter.Mon4ID })
            {
                if (string.IsNullOrWhiteSpace(id)) continue;
                hasMonster = true;
                if (!monsters.TryGetValue(id, out MonsterData monster) || monster == null)
                {
                    missingMonster = true;
                    break;
                }
                hasBoss |= monster.Kind == BossMonsterKind;
            }

            if (hasMonster && !missingMonster && hasBoss == (type == AreaEventType.Boss))
            {
                candidates.Add(encounter);
            }
        }

        return candidates;
    }
}
