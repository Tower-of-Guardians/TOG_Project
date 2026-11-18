using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class CardMergeUtil
{
    private const int MergeCount = 3;

    public static bool WillMergeOnGain(CardData card)
    {
        if (card == null || DataCenter.Instance == null || FindNextStar(card) == null)
        {
            return false;
        }

        return DataCenter.Instance.userDeck.Count(owned => owned != null && owned.id == card.id) >= MergeCount - 1;
    }

    public static void MergeDeck()
    {
        List<CardData> deck = DataCenter.Instance.userDeck;
        List<CardData> materials = FindMaterials(deck);
        while (materials != null)
        {
            Merge(deck, materials);
            materials = FindMaterials(deck);
        }
    }

    private static List<CardData> FindMaterials(List<CardData> deck)
    {
        return deck
            .Where(card => card != null && FindNextStar(card) != null)
            .GroupBy(card => card.id)
            .Where(group => group.Count() >= MergeCount)
            .Select(group => group.Take(MergeCount).ToList())
            .FirstOrDefault();
    }

    private static void Merge(List<CardData> deck, List<CardData> materials)
    {
        CardData merged = Object.Instantiate(FindNextStar(materials[0]));
        merged.ATK += materials.Sum(GetAtkBonus);
        merged.DEF += materials.Sum(GetDefBonus);
        merged.time = materials.Max(card => card.time);

        int index = deck.IndexOf(materials[0]);
        foreach (CardData material in materials)
        {
            deck.Remove(material);
        }

        deck.Insert(index, merged);
    }

    private static float GetAtkBonus(CardData card)
    {
        return DataCenter.card_datas.TryGetValue(card.id, out CardData original) ? card.ATK - original.ATK : 0f;
    }

    private static float GetDefBonus(CardData card)
    {
        return DataCenter.card_datas.TryGetValue(card.id, out CardData original) ? card.DEF - original.DEF : 0f;
    }

    private static CardData FindNextStar(CardData card)
    {
        return DataCenter.Instance.GetSeriesCards(card.id).Find(series => series.star == card.star + 1);
    }
}
