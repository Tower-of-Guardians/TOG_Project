using UnityEngine;
using System;

[Serializable]
public class ForgeData
{
    [SerializeField] private int stage;
    public int Stage => stage;
    
    [SerializeField] private float growthATK;
    public float ATK => growthATK;
    
    [SerializeField] private float growthDEF;
    public float DEF => growthDEF;
}