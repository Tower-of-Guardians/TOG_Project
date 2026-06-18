using System.Collections.Generic;
using UnityEngine;

public static class BattleEncounterSpawner
{
    private const float DefaultSpawnY = 0.8f;

    public static List<Monster> SpawnEncounter(
        MonsterEncounterData encounterData,
        Transform globalRoot,
        MonsterPrefabRegistry prefabRegistry)
    {
        var spawnedMonsters = new List<Monster>();

        if (encounterData == null)
        {
            Debug.LogError("BattleEncounterSpawner: encounterData가 null입니다.");
            return spawnedMonsters;
        }

        if (globalRoot == null)
        {
            Debug.LogError("BattleEncounterSpawner: [Global] Transform이 null입니다.");
            return spawnedMonsters;
        }

        if (prefabRegistry == null)
        {
            Debug.LogError("BattleEncounterSpawner: MonsterPrefabRegistry가 할당되지 않았습니다.");
            return spawnedMonsters;
        }

        ClearEncounterMonsters(globalRoot);

        TrySpawnMonster(encounterData.Mon1ID, encounterData.Mon1Position, globalRoot, prefabRegistry, spawnedMonsters);
        TrySpawnMonster(encounterData.Mon2ID, encounterData.Mon2Position, globalRoot, prefabRegistry, spawnedMonsters);
        TrySpawnMonster(encounterData.Mon3ID, encounterData.Mon3Position, globalRoot, prefabRegistry, spawnedMonsters);
        TrySpawnMonster(encounterData.Mon4ID, encounterData.Mon4Position, globalRoot, prefabRegistry, spawnedMonsters);

        return spawnedMonsters;
    }

    private static void TrySpawnMonster(
        string monsterDataId,
        float positionX,
        Transform globalRoot,
        MonsterPrefabRegistry prefabRegistry,
        List<Monster> spawnedMonsters)
    {
        if (string.IsNullOrEmpty(monsterDataId))
        {
            return;
        }

        if (!prefabRegistry.TryGetPrefab(monsterDataId, out Monster prefab) || prefab == null)
        {
            Debug.LogWarning($"BattleEncounterSpawner: {monsterDataId}에 매핑된 몬스터 프리팹이 없습니다.");
            return;
        }

        Monster monster = Object.Instantiate(prefab, globalRoot);
        monster.name = prefab.name;

        monster.transform.localPosition = new Vector3(positionX, DefaultSpawnY, 0f);
        spawnedMonsters.Add(monster);
    }

    private static void ClearEncounterMonsters(Transform globalRoot)
    {
        Monster[] existingMonsters = globalRoot.GetComponentsInChildren<Monster>(true);
        for (int i = 0; i < existingMonsters.Length; i++)
        {
            Monster monster = existingMonsters[i];
            if (monster != null)
            {
                Object.Destroy(monster.gameObject);
            }
        }
    }
}
