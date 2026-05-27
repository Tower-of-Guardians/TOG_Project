using System;
using System.Collections.Generic;
using UnityEngine;


[Serializable]
public class SaveData
{
    public List<string> userDeckData = new List<string>();
}

[Serializable]
public class BattleCardData
{
    public int index;
    public CardData data = ScriptableObject.CreateInstance<CardData>();
}

[Serializable]
public class PlayerState
{
    public int level;
    public int maxexperience;
    public int experience;
    public int hp;
    public int maxhp;
    public int lhp;
    public int atk;
    public int latk;
    public int maxmagic;
    public int money;
}
[Serializable]
public class SynergyTotalData
{
    public SynergyData synergyData;
    public int count;
}