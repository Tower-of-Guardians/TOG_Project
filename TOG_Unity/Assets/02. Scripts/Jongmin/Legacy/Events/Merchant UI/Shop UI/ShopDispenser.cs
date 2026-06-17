using System;
using System.Collections.Generic;
using UnityEngine;

public class ShopDispenser : MonoBehaviour
{
    private List<ShopCardPresenter> _shopCardPresenterList = new();
    private PotionCardPresenter _potionCardPresenter;

    public event Action OnPurchasedAnyItem;

    public void Inject(List<ShopCardPresenter> shopCardPresenterList,
                       PotionCardPresenter potionCardPresenter)
    {
        _shopCardPresenterList = shopCardPresenterList;
        _potionCardPresenter = potionCardPresenter;
    }

    public void Initialize()
    {
        List<BattleCardData> battleCardDataList = GetRandomCards();

        for (int i = 0; i < _shopCardPresenterList.Count; i++)
        {
            _shopCardPresenterList[i].Inject(battleCardDataList[i]);
        }

        _potionCardPresenter?.Initialize();
    }

    public void Alert()
        => OnPurchasedAnyItem?.Invoke();

    private List<BattleCardData> GetRandomCards()
    {
        ResultPercentData resultPercent = ScriptableObject.CreateInstance<ResultPercentData>();
        DataCenter.Instance.GetResultPercentData(DataCenter.Instance.playerstate.level + 2, (data) =>
        {
            resultPercent = Instantiate(data);
        });
        
        List<BattleCardData> results = new List<BattleCardData>();

        for (int i = 0; i < 5; i++)
        {
            float roll = UnityEngine.Random.Range(0, 100);
            float accumulatedChance = 0;

            for (int n = 0; n < resultPercent.percent.Count; n++)
            {
                accumulatedChance += resultPercent.percent[n];

                if (roll <= accumulatedChance)
                {
                    results.Add(GetRandomCardData(n + 1));
                    break;
                }
            }
        }

        return results;
    }

    private BattleCardData GetRandomCardData(int cut)
    {
        BattleCardData cardData = new BattleCardData();
        cardData.data = null;
        while (cardData.data == null)
        {
            string randomID = DataCenter.random_card_datas[UnityEngine.Random.Range(0, DataCenter.random_card_datas.Count - 1)].ToString();
            DataCenter.Instance.GetCardData(randomID, (data) =>
            {
                cardData.data = Instantiate(data);
            });
            
            if (cardData.data.grade != cut)
            {
                cardData.data = null;
            }
        }

        return cardData;
    }
}
