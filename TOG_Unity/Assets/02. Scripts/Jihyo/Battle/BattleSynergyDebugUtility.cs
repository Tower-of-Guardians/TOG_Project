using System.Collections.Generic;
using UnityEngine;

public static class BattleSynergyDebugUtility
{
    public const string CardIdPrefix = "__DEBUG_SYNERGY__";

    public static void ApplyCounts(IReadOnlyDictionary<string, int> counts)
    {
        RemoveDebugCards(refresh: false);

        if (GameData.Instance == null || counts == null)
        {
            RefreshSynergyState();
            return;
        }

        foreach (KeyValuePair<string, int> pair in counts)
        {
            if (string.IsNullOrEmpty(pair.Key) || pair.Value <= 0)
            {
                continue;
            }

            for (int i = 0; i < pair.Value; i++)
            {
                CardData card = ScriptableObject.CreateInstance<CardData>();
                card.id = $"{CardIdPrefix}{pair.Key}_{i}";
                card.itemName = $"Debug {pair.Key}";
                card.synergy1ID = pair.Key;
                card.synergy2ID = string.Empty;
                card.synergy3ID = string.Empty;
                card.star = 1;
                GameData.Instance.attackField.Add(card);
            }
        }

        RefreshSynergyState();
    }

    public static void RemoveDebugCards(bool refresh = true)
    {
        if (GameData.Instance == null)
        {
            return;
        }

        DestroyAndRemove(GameData.Instance.attackField);
        DestroyAndRemove(GameData.Instance.defenseField);

        if (refresh)
        {
            RefreshSynergyState();
        }
    }

    public static bool IsDebugCard(CardData card)
    {
        return card != null
               && !string.IsNullOrEmpty(card.id)
               && card.id.StartsWith(CardIdPrefix);
    }

    private static void DestroyAndRemove(List<CardData> field)
    {
        if (field == null || field.Count == 0)
        {
            return;
        }

        for (int i = field.Count - 1; i >= 0; i--)
        {
            CardData card = field[i];
            if (!IsDebugCard(card))
            {
                continue;
            }

            field.RemoveAt(i);
            Object.Destroy(card);
        }
    }

    private static void RefreshSynergyState()
    {
        if (GameData.Instance == null)
        {
            return;
        }

        GameData.Instance.GetSynergyData();
    }
}
