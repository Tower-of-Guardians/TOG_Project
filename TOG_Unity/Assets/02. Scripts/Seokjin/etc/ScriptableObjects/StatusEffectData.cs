using UnityEngine;

// CSV의 한 행에 대응하는 데이터 구조
[CreateAssetMenu(fileName = "StatusEffectData", menuName = "Data/StatusEffectData")]
public class StatusEffectData : ScriptableObject
{
    public string Id;
    public string Name;
    public int Apply;
    public int BuffType;
    public int NumType;
    public int DurationType;
    public int ReleaseCondition;
    public string Description;
}