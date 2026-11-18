using System;
using UnityEngine;

[CreateAssetMenu(fileName = "StatusEffectIconLibrary", menuName = "Data/StatusEffectIconLibrary")]
public class StatusEffectIconLibrary : ScriptableObject
{
    [Serializable]
    public struct Entry
    {
        public string StatusEffectId;
        public Sprite Icon;
    }

    public Entry[] Entries;
}
