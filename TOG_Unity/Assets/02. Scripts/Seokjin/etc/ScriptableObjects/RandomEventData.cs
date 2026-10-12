using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "RandomEventData", menuName = "Data/RandomEventData")]
public class RandomEventData : ScriptableObject
{
    public string Id;
    public string Name;
    public List<int> Sections = new List<int>();
}
