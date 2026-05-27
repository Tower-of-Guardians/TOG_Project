using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ResultPercentData", menuName = "Data/ResultPercentData")]
public class ResultPercentData : ScriptableObject
{
    public int level;
    public List<float> percent = new List<float>();
}