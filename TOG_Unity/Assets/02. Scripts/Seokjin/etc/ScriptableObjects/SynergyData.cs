using UnityEngine;
using System.Collections.Generic;

// CSV의 한 행에 대응하는 데이터 구조
[CreateAssetMenu(fileName = "SynergyData", menuName = "Data/SynergyData")]
public class SynergyData : ScriptableObject
{
    public string ID;
    public string Name;
    public string Description;
    public int Tier;
    public string Effect1ID;
    public List<int> Effect1Synergys;
    public string Effect2ID;
    public List<int> Effect2Synergys;
    public string Effect3ID;
    public List<int> Effect3Synergys;
    public Dictionary<string, List<int>> EffectSynergys = new Dictionary<string, List<int>>();
}