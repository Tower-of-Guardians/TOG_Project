using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
#endif
using System.IO;
using System;


#if UNITY_EDITOR
public class AddressableAutoConfigurator : Editor
{

    // ScriptableObject 에셋이 저장된 폴더 경로 (Addressable로 만들 대상)
    private static string GROUP_NAME = "CardData";
    private static Type class_name;
    [MenuItem("Tools/Addressables/CardData for Addressables")]
    public static void SetCardData()
    {
        GROUP_NAME = "CardData";
        class_name = typeof(CardData);
        ConfigureItemSOsAsAddressable();
    }

    [MenuItem("Tools/Addressables/ResultPercentData for Addressables")]
    public static void SetResultPercentData()
    {
        GROUP_NAME = "ResultPercentData";
        class_name = typeof(ResultPercentData);
        ConfigureItemSOsAsAddressable();
    }
    [MenuItem("Tools/Addressables/MonsterData for Addressables")]
    public static void SetMonsterData()
    {
        GROUP_NAME = "MonsterData";
        class_name = typeof(MonsterData);
        ConfigureItemSOsAsAddressable();
    }
    [MenuItem("Tools/Addressables/MonsterEncounterData for Addressables")]
    public static void SetMonsterEncounterData()
    {
        GROUP_NAME = "MonsterEncounterData";
        class_name = typeof(MonsterEncounterData);
        ConfigureItemSOsAsAddressable();
    }
    [MenuItem("Tools/Addressables/StatusEffectData for Addressables")]
    public static void SetStatusEffectData()
    {
        GROUP_NAME = "StatusEffectData";
        class_name = typeof(StatusEffectData);
        ConfigureItemSOsAsAddressable();
    }
    [MenuItem("Tools/Addressables/SynergyData for Addressables")]
    public static void SetSynergyData()
    {
        GROUP_NAME = "SynergyData";
        class_name = typeof(SynergyData);
        ConfigureItemSOsAsAddressable();
    }
    [MenuItem("Tools/Addressables/EffectData for Addressables")]
    public static void SetEffectData()
    {
        GROUP_NAME = "EffectData";
        class_name = typeof(EffectData);
        ConfigureItemSOsAsAddressable();
    }

    public static void ConfigureItemSOsAsAddressable()
    {
        string SO_FOLDER_PATH = "Assets/Datas"+ "/" + GROUP_NAME;
        // 1. Addressable Settings 시스템 로드
        AddressableAssetSettings settings = AddressableAssetSettingsDefaultObject.Settings;
        if (settings == null)
        {
            Debug.LogError("Addressable Settings를 찾을 수 없습니다. Addressable 시스템을 초기화했는지 확인하세요.");
            return;
        }

        // 2. 사용할 Addressable 그룹을 찾거나 생성합니다.
        AddressableAssetGroup group = settings.FindGroup(GROUP_NAME);
        if (group == null)
        {
            group = settings.CreateGroup(GROUP_NAME, false, false, true, null);
            Debug.Log($"Addressable 그룹 '{GROUP_NAME}'을(를) 새로 생성했습니다.");
        }

        // 3. ScriptableObject 파일 경로 목록 가져오기
        string[] assetGuids = AssetDatabase.FindAssets("", new[] { SO_FOLDER_PATH });

        int configuredCount = 0;

        // 4. 모든 에셋에 대해 Addressable 등록 및 주소 설정
        foreach (string guid in assetGuids)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(guid);

            // ItemData 타입의 ScriptableObject인지 확인
            if (assetPath.EndsWith(".asset"))
            {
                UnityEngine.Object assetObject = AssetDatabase.LoadAssetAtPath(assetPath, class_name);
                if (assetObject == null) continue;

                // 주소로 사용할 이름 결정 (예: 파일 이름에서 확장자 제거)
                string assetName = Path.GetFileNameWithoutExtension(assetPath);

                // 5. Addressable 에셋 엔트리 생성 또는 업데이트
                AddressableAssetEntry entry = settings.CreateOrMoveEntry(guid, group);

                // 주소 할당 (코드에서 사용할 키)
                entry.SetAddress(assetName);
                entry.SetLabel(GROUP_NAME,true);

                configuredCount++;
            }
        }

        EditorUtility.SetDirty(settings); // 변경 사항 저장
        AssetDatabase.SaveAssets();

        Debug.Log($"성공적으로 {configuredCount}개의 Item ScriptableObject를 Addressable 그룹 '{GROUP_NAME}'에 등록하고 주소를 설정했습니다.");
    }
}
#endif