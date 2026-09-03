using UnityEngine;

public static class SynergyVisualCatalog
{
    public const string HonestyId = "210001";
    public const string ShieldAttackId = "210002";
    public const string OverwhelmingId = "210003";
    public const string BloodSuckingId = "210004";
    public const string PlunderId = "210005";
    public const string MysteryId = "210006";
    public const string BasicId = "210007";
    public const string DarknessId = "210008";

    public const string BasicEffectStateName = "Effect_Synergy_Basic";
    public const string BloodSuckingEffectStateName = "Effect_Synergy_BloodSucking";
    public const string IdleEffectStateName = "Effect_Synergy_Idle";

    public static bool TryGetEffectStateName(string synergyId, out string stateName)
    {
        switch (synergyId)
        {
            case BasicId:
                stateName = BasicEffectStateName;
                return true;
            case BloodSuckingId:
                stateName = BloodSuckingEffectStateName;
                return true;
            default:
                stateName = null;
                return false;
        }
    }

    public static Color GetAuraColor(string synergyId)
    {
        switch (synergyId)
        {
            case HonestyId:
                return Hex("FFD24A");
            case ShieldAttackId:
                return Hex("B8A0D8");
            case OverwhelmingId:
                return Hex("9FD84A");
            case BloodSuckingId:
                return Hex("BF0000");
            case PlunderId:
                return Hex("FF9A2E");
            case MysteryId:
                return Hex("4AA6FF");
            case BasicId:
                return Hex("FFFBE6");
            case DarknessId:
                return Hex("7B1FA2");
            default:
                return Color.white;
        }
    }

    public static Color Hex(string hex)
    {
        if (ColorUtility.TryParseHtmlString("#" + hex.TrimStart('#'), out Color color))
        {
            return color;
        }

        return Color.white;
    }
}
