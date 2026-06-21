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
    public float Mon1XPosition;
    public float Mon2XPosition;
    public float Mon3XPosition;
    public float Mon4XPosition;
    public float Mon1YPosition;
    public float Mon2YPosition;
    public float Mon3YPosition;
    public float Mon4YPosition;
    public float Mon1BarLength;
    public float Mon2BarLength;
    public float Mon3BarLength;
    public float Mon4BarLength;
}
