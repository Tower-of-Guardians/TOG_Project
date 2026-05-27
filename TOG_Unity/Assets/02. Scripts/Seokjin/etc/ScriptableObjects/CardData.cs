using System;
using UnityEngine;

// CSV의 한 행에 대응하는 데이터 구조
[CreateAssetMenu(fileName = "CardData", menuName = "Data/CardData")]
public class CardData : ScriptableObject
{
    public string id;
    public string itemName;
    public Sprite iconimage;
    public Sprite synergyFrameImage;
    public int grade;
    public int star;
    public int price;
    public Sprite cardimage;
    public Sprite synergy1Icon, synergy2Icon, synergy3Icon;
    public string synergy1ID, synergy2ID, synergy3ID;
    public string effectDescription;
    public float ATK;
    public float DEF;
    public int when;
    public int trigger;
    public string effect1ID, effect2ID;
    public string effect1Value, effect2Value;

    public DateTime time;
}