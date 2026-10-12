using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Object = UnityEngine.Object;

public static class RandomEventCardUtil
{
    private const int MinResultLevel = 1;
    private const int MaxResultLevel = 9;

    public static List<CardData> GetDeckCards(Func<CardData, bool> filter)
    {
        if (DataCenter.Instance == null)
        {
            return new List<CardData>();
        }

        return DataCenter.Instance.userDeck.Where(card => card != null && filter(card)).ToList();
    }

    public static bool IsBasic(CardData card)
    {
        return card.grade == 0;
    }

    public static bool CanUpgradeStar(CardData card)
    {
        return card.star == 0 && FindStarCard(card, card.star + 1) != null;
    }

    public static CardData UpgradeStar(CardData card)
    {
        List<CardData> deck = DataCenter.Instance.userDeck;
        int index = deck.IndexOf(card);
        CardData master = FindStarCard(card, card.star + 1);
        if (index < 0 || master == null)
        {
            return null;
        }

        CardData upgraded = Object.Instantiate(master);
        upgraded.time = card.time;
        deck[index] = upgraded;
        return upgraded;
    }

    public static void RemoveFromDeck(CardData card)
    {
        DataCenter.Instance.userDeck.Remove(card);
    }

    public static void AddToDeck(CardData master)
    {
        CardData card = Object.Instantiate(master);
        card.time = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        DataCenter.Instance.userDeck.Add(card);

        if (DIContainer.IsRegistered<Jongmin.EventDomain>())
        {
            DIContainer.Resolve<Jongmin.EventDomain>().RecordGainedCard(card);
        }
    }

    public static List<CardData> CreateRewardCards(int count)
    {
        var picked = new List<CardData>();
        int level = Mathf.Clamp(DataCenter.Instance.playerstate.level, MinResultLevel, MaxResultLevel);
        if (!DataCenter.result_datas.TryGetValue(level, out ResultPercentData percentData))
        {
            return picked;
        }

        List<CardData> pool = DataCenter.card_datas.Values
            .Where(card => card != null && card.star == 0 && card.grade > 0)
            .OrderBy(card => card.id, StringComparer.Ordinal)
            .ToList();

        for (int i = 0; i < count; i++)
        {
            int grade = RollGrade(percentData.percent);
            List<CardData> candidates = pool.Where(card => card.grade == grade && !picked.Contains(card)).ToList();
            if (candidates.Count == 0)
            {
                candidates = pool.Where(card => !picked.Contains(card)).ToList();
            }

            if (candidates.Count == 0)
            {
                break;
            }

            picked.Add(candidates[Jongmin.RunRandom.Range(Jongmin.RunRandom.Reward, 0, candidates.Count)]);
        }

        return picked;
    }

    private static int RollGrade(List<float> percents)
    {
        float roll = Jongmin.RunRandom.Range(Jongmin.RunRandom.Reward, 0f, 100f);
        float accumulated = 0f;
        for (int i = 0; i < percents.Count; i++)
        {
            accumulated += percents[i];
            if (roll <= accumulated)
            {
                return i + 1;
            }
        }

        return percents.Count;
    }

    private static CardData FindStarCard(CardData card, int star)
    {
        return DataCenter.Instance.GetSeriesCards(card.id).Find(series => series.star == star);
    }
}
