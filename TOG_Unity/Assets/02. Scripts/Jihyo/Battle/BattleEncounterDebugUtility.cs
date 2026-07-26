using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 인게임 밸런싱/디버그용 Encounter 적용 유틸리티입니다.
/// </summary>
public static class BattleEncounterDebugUtility
{
    public static List<Monster> ApplyEncounter(
        MonsterEncounterData encounterData,
        Transform globalRoot,
        MonsterPrefabRegistry registry,
        BattleManager battleManager)
    {
        var spawnedMonsters = new List<Monster>();
        if (encounterData == null || globalRoot == null || registry == null)
        {
            Debug.LogError("BattleEncounterDebugUtility: Encounter, Global Root, Registry 중 null 참조가 있습니다.");
            return spawnedMonsters;
        }

        BattleSetupController setupController = battleManager != null
            ? battleManager.GetSetupController()
            : null;

        if (setupController != null)
        {
            List<Monster> existingMonsters = setupController.GetPrimaryMonsters();
            for (int i = existingMonsters.Count - 1; i >= 0; i--)
            {
                Monster monster = existingMonsters[i];
                if (monster == null)
                {
                    continue;
                }

                setupController.UnregisterMonster(monster);
            }
        }

        spawnedMonsters = BattleEncounterSpawner.SpawnEncounter(encounterData, globalRoot, registry);
        if (spawnedMonsters.Count == 0)
        {
            Debug.LogWarning($"BattleEncounterDebugUtility: Encounter {encounterData.Id}에서 스폰된 몬스터가 없습니다.");
            return spawnedMonsters;
        }

        if (setupController != null)
        {
            for (int i = 0; i < spawnedMonsters.Count; i++)
            {
                Monster monster = spawnedMonsters[i];
                if (monster == null)
                {
                    continue;
                }

                setupController.RegisterMonster(monster);
                monster.PrepareActionForTurn();
            }
        }

        Debug.Log($"BattleEncounterDebugUtility: Encounter {encounterData.Id} ({encounterData.Name}) 적용 완료. 몬스터 {spawnedMonsters.Count}마리.");
        return spawnedMonsters;
    }
}
