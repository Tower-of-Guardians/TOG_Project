using UnityEngine;

// CSV의 한 행에 대응하는 데이터 구조
[CreateAssetMenu(fileName = "EffectData", menuName = "Data/EffectData")]
public class EffectData : ScriptableObject
{
    public string Id;
    public string Name;
    public string Effect;
    public int Target;
    public int Choice;
    public string CreateMagic;
    public string StatusEffect;
    public string NumType;

    public bool TryGetStatusEffectId(out string statusEffectId)
    {
        if (IsStatusEffectId(StatusEffect))
        {
            statusEffectId = StatusEffect;
            return true;
        }

        // CSV importer currently stores StatusEffect1 in NumType on existing assets.
        if (IsStatusEffectId(NumType))
        {
            statusEffectId = NumType;
            return true;
        }

        statusEffectId = null;
        return false;
    }

    private static bool IsStatusEffectId(string value)
    {
        return !string.IsNullOrEmpty(value) && value.StartsWith("51") && value.Length >= 8;
    }
}