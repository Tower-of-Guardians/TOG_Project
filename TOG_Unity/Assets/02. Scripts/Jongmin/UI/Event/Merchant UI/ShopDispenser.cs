using System;
using System.Collections.Generic;
using JxModule;
using UnityEngine;

namespace Jongmin
{
    public class ShopDispenser : MonoBehaviour
    {
        private ShopSlot[] _shopSlots;
        private PotionSlot _potionSlot;

        public event Action OnPurchasedAnyItem;

        private void Awake()
        {
            _shopSlots = GetComponentsInChildren<ShopSlot>();
            _potionSlot = GetComponentInChildren<PotionSlot>();
        }

        public bool Initialize()
        {
            if (!TryGetRandomCards(out var cardDataList))
            {
                return false;
            }

            var currentGold = DataCenter.Instance.playerstate.money;
            
            for (var i = 0; i < _shopSlots.Length; i++)
            {
                _shopSlots[i].Initialize(cardDataList[i], currentGold);
            }
            
            _potionSlot.Initialize(currentGold);

            return true;
        }

        private bool TryGetRandomCards(out List<CardData> results)
        {
            results = new List<CardData>(_shopSlots.Length);

            ResultPercentData resultPercent = null;
            DataCenter.Instance.GetResultPercentData(DataCenter.Instance.playerstate.level + 2, data => resultPercent = data);
            
            if (resultPercent?.percent == null || _shopSlots.Length == 0)
            {
                Debug.LogError("상점의 카드 확률 데이터 또는 판매 슬롯이 준비되어 있지 않습니다.", this);
                return false;
            }

            var cardsByGrade = new List<CardData>[resultPercent.percent.Count];

            foreach (var cardId in DataCenter.random_card_datas)
            {
                if (!DataCenter.card_datas.TryGetValue(cardId, out CardData cardData) || cardData == null)
                {
                    continue;
                }

                var gradeIndex = cardData.grade - 1;
                if (gradeIndex < 0 || gradeIndex >= cardsByGrade.Length || resultPercent.percent[gradeIndex] <= 0f)
                {
                    continue;
                }

                cardsByGrade[gradeIndex] ??= new List<CardData>();
                cardsByGrade[gradeIndex].Add(cardData);
            }

            var availableGrades = new List<int>();
            for (var i = 0; i < cardsByGrade.Length; i++)
            {
                if (cardsByGrade[i] is { Count: > 0 })
                {
                    availableGrades.Add(i);
                }
            }

            if (availableGrades.Count == 0)
            {
                Debug.LogError("상점 확률에 맞는 판매 가능 카드가 없습니다.", this);
                return false;
            }

            for (var i = 0; i < _shopSlots.Length; i++)
            {
                if (!RandomUtility.TryGetWeightedRandom(availableGrades, grade => resultPercent.percent[grade], out var selectedGrade))
                {
                    Debug.LogError("카드 등급 추첨에 실패했습니다.", this);
                    return false;
                }

                var selectedCard = RandomUtility.GetRandom(cardsByGrade[selectedGrade]);

                results.Add(Instantiate(selectedCard));
            }

            return true;
        }
    }
}