using UnityEngine;

// CSV의 한 행에 대응하는 데이터 구조
[CreateAssetMenu(fileName = "MonsterEncounterData", menuName = "Data/MonsterEncounterData")]
public class MonsterEncounterData : ScriptableObject
{
    public string Id;
    public string Name;
    public int Section;
    public int Gold;
    public int Exp;
    public string Mon1ID;
    public string Mon2ID;
    public string Mon3ID;
    public string Mon4ID;
    public float Mon1Position;
    public float Mon2Position;
    public float Mon3Position;
    public float Mon4Position;
    public float Mon1BarLength;
    public float Mon2BarLength;
    public float Mon3BarLength;
    public float Mon4BarLength;
}