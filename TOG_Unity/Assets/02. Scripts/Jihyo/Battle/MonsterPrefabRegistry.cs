using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "MonsterPrefabRegistry", menuName = "Data/MonsterPrefabRegistry")]
public class MonsterPrefabRegistry : ScriptableObject
{
    [Serializable]
    public class Entry
    {
        public string MonsterDataId;
        public Monster Prefab;
    }

    [SerializeField] private List<Entry> entries = new List<Entry>();

    public bool TryGetPrefab(string monsterDataId, out Monster prefab)
    {
        if (string.IsNullOrEmpty(monsterDataId))
        {
            prefab = null;
            return false;
        }

        for (int i = 0; i < entries.Count; i++)
        {
            Entry entry = entries[i];
            if (entry != null
                && entry.Prefab != null
                && entry.MonsterDataId == monsterDataId)
            {
                prefab = entry.Prefab;
                return true;
            }
        }

        prefab = null;
        return false;
    }
}
