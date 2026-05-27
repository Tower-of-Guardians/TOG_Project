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
}