using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// DataCenter에 로드된 상태효과만 아이콘을 찾습니다.
/// 아이콘 파일이 없는 상태(당황)는 비워 둡니다.
/// 같은 계열(응징 1~3 등)은 아이콘 하나를 공유합니다.
/// </summary>
public static class StatusEffectIconCatalog
{
    public const string IconFolder = "Assets/04. Images/UI/StatusEffect/";
    public const string DataFolder = "Assets/Datas/StatusEffectData/";
    public const string LibraryResourceName = "StatusEffectIconLibrary";

    private static Dictionary<string, Sprite> spritesById;

    public static bool TryGetIcon(string statusEffectId, out Sprite sprite)
    {
        sprite = null;
        if (string.IsNullOrEmpty(statusEffectId) || string.IsNullOrEmpty(ResolveIconFileName(statusEffectId)))
        {
            return false;
        }

        if (!IsKnownToDataCenter(statusEffectId))
        {
            return false;
        }

        EnsureCache();
        return spritesById.TryGetValue(statusEffectId, out sprite) && sprite != null;
    }

    public static void ClearCache()
    {
        spritesById = null;
    }

    public static string ResolveIconFileName(string statusEffectId)
    {
        switch (statusEffectId)
        {
            case "51001001":
                return "StatusEffect_Weakness.png";
            case "51001002":
                return "StatusEffect_HolyShield.png";
            case "51001003":
                return "StatusEffect_Frenzy.png";
            case "51001004":
                return "StatusEffect_CurseThorn.png";
            case "51001005":
                return "StatusEffect_Cursed.png";
            case "51001006":
                return "StatusEffect_Undying.png";
            case "51001007":
                return "StatusEffect_Evasion.png";
            case "51001033":
                return "StatusEffect_SuperFrenzy.png";
            case "51002008":
                return "StatusEffect_AoE.png";
            case "51002009":
            case "51002010":
            case "51002011":
                return "StatusEffect_Chastisement.png";
            case "51002012":
            case "51002013":
            case "51002014":
                return "StatusEffect_Slayer.png";
            case "51002015":
                return "StatusEffect_Apprentice.png";
            case "51002016":
                return "StatusEffect_Twins.png";
            case "51002017":
                return "StatusEffect_Beggar.png";
            case "51002018":
                return "StatusEffect_NoHeal.png";
            case "51002019":
            case "51003029":
                return "StatusEffect_Strength.png";
            case "51002020":
            case "51002021":
            case "51002022":
                return "StatusEffect_ShieldAttack.png";
            case "51002023":
            case "51002024":
            case "51002025":
                return "StatusEffect_BloodSucking.png";
            case "51002026":
                return "StatusEffect_BonusDamage.png";
            case "51002027":
                return "StatusEffect_BonusDraw.png";
            case "51003028":
                return "StatusEffect_Stun.png";
            case "51003030":
                return "StatusEffect_NextTurnWeakness.png";
            case "51003031":
                return "StatusEffect_KinRevenge.png";
            case "51003032":
                return "StatusEffect_HitStrength.png";
            default:
                return null;
        }
    }

    private static void EnsureCache()
    {
        if (spritesById != null)
        {
            return;
        }

        spritesById = new Dictionary<string, Sprite>();
#if UNITY_EDITOR
        LoadFromEditorAssets();
#endif
        LoadMissingFromLibrary();
    }

    private static void LoadMissingFromLibrary()
    {
        StatusEffectIconLibrary library = Resources.Load<StatusEffectIconLibrary>(LibraryResourceName);
        if (library == null || library.Entries == null)
        {
            return;
        }

        for (int i = 0; i < library.Entries.Length; i++)
        {
            StatusEffectIconLibrary.Entry entry = library.Entries[i];
            if (string.IsNullOrEmpty(entry.StatusEffectId)
                || entry.Icon == null
                || string.IsNullOrEmpty(ResolveIconFileName(entry.StatusEffectId)))
            {
                continue;
            }

            if (!spritesById.ContainsKey(entry.StatusEffectId))
            {
                spritesById[entry.StatusEffectId] = entry.Icon;
            }
        }
    }

    private static bool IsKnownToDataCenter(string statusEffectId)
    {
        if (DataCenter.IsStatusEffectDataLoaded)
        {
            return DataCenter.status_effect_datas != null
                && DataCenter.status_effect_datas.ContainsKey(statusEffectId);
        }

#if UNITY_EDITOR
        return UnityEditor.AssetDatabase.LoadAssetAtPath<StatusEffectData>(DataFolder + statusEffectId + ".asset") != null;
#else
        return false;
#endif
    }

#if UNITY_EDITOR
    private static void LoadFromEditorAssets()
    {
        string[] guids = UnityEditor.AssetDatabase.FindAssets("t:StatusEffectData", new[] { "Assets/Datas/StatusEffectData" });
        for (int i = 0; i < guids.Length; i++)
        {
            string assetPath = UnityEditor.AssetDatabase.GUIDToAssetPath(guids[i]);
            StatusEffectData data = UnityEditor.AssetDatabase.LoadAssetAtPath<StatusEffectData>(assetPath);
            if (data == null || string.IsNullOrEmpty(data.Id))
            {
                continue;
            }

            string fileName = ResolveIconFileName(data.Id);
            if (string.IsNullOrEmpty(fileName))
            {
                continue;
            }

            Sprite sprite = LoadFirstFrame(IconFolder + fileName);
            if (sprite != null)
            {
                spritesById[data.Id] = sprite;
            }
        }
    }

    private static Sprite LoadFirstFrame(string assetPath)
    {
        UnityEngine.Object[] assets = UnityEditor.AssetDatabase.LoadAllAssetsAtPath(assetPath);
        Sprite fallback = null;
        for (int i = 0; i < assets.Length; i++)
        {
            if (!(assets[i] is Sprite sprite))
            {
                continue;
            }

            if (sprite.name.EndsWith("_0"))
            {
                return sprite;
            }

            if (fallback == null)
            {
                fallback = sprite;
            }
        }

        return fallback;
    }
#endif
}
