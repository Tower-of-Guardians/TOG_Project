using UnityEngine;

// CSV의 한 행에 대응하는 데이터 구조
[CreateAssetMenu(fileName = "AreaEventData", menuName = "Data/AreaEventData")]
public class AreaEventData : ScriptableObject
{
    //ID	Name	Stage	Area	Section	BossEvent	MerchantEvent	SmithyEvent	BlessingEvent	BattleEvent	RandomEvent
    public string Id;
    public string Name;
    public int Stage;
    public int Area;
    public int Section;
    public int BossEvent;
    public int MerchantEvent;
    public int SmithyEvent;
    public int BlessingEvent;
    public int BattleEvent;
    public int RandomEvent;
}