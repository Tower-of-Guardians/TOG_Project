using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Forge Database", menuName = "SO/DB/Forge Database")]
public class ForgeDatabase : ScriptableObject, IForgeDatabase
{
    [SerializeField] private List<ForgeData> forgeDataList;
    private Dictionary<int, ForgeData> _forgeDataDict;

#if UNITY_EDITOR
    private void OnEnable()
        => Initialize();
#endif

    private void Initialize()
    {
        if (forgeDataList == null || forgeDataList.Count == 0)
        {
            return;
        }

        _forgeDataDict = new();
        foreach (ForgeData data in forgeDataList)
        {
            _forgeDataDict[data.Stage] = data;
        }
    }

    public ForgeData GetForgeData(int stage)
    {
        if (_forgeDataDict == null)
        {
            Initialize();
        }

        return _forgeDataDict.GetValueOrDefault(stage);
    }
}
