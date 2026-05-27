using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class GameData : Singleton<GameData>
{
    public List<string> notuseDeck = new List<string>(); // 미사용덱
    public List<string> handDeck = new List<string>(); // 핸드덱
    public List<string> garbageDeck = new List<string>(); // 사용덱

    public event Action<DeckType, int> DeckChange;
    public event Action<Dictionary<string, SynergyTotalData>> SynergyChange;

    public List<CardData> attackField = new List<CardData>();
    public List<CardData> defenseField = new List<CardData>();

    public Dictionary<string, SynergyTotalData> synergyIDList = new Dictionary<string, SynergyTotalData>();
    public Dictionary<string, int> effectIDList = new Dictionary<string, int>();
    private void Start()
    {
        StartCoroutine(FirstDeckSet());
        DeckChange?.Invoke(DeckType.Draw, notuseDeck.Count);
    }

    /// <summary>
    /// 덱 정보 변경시 카운트 정보 이벤트
    /// </summary>
    /// <param name="deck_type"></param>
    public void InvokeDeckCountChange(DeckType deck_type)
        => DeckChange?.Invoke(deck_type, deck_type == DeckType.Draw ? notuseDeck.Count
                                                                    : garbageDeck.Count);

    public void InvokeSynergys()
        => SynergyChange?.Invoke(synergyIDList);

    /// <summary>
    /// 처음 시작시 덱 정보 불러오고 섞기
    /// </summary>
    IEnumerator FirstDeckSet()
    {
        Debug.Log("GameData 카드 세팅 대기");
        yield return new WaitUntil(() => DataCenter.Instance.userDeck.Count > 0);
        Debug.Log("GameData 카드 세팅 완료");
        foreach (CardData id in DataCenter.Instance.userDeck)
        {
            notuseDeck.Add(id.id);
        }

        Shuffle();
        InvokeDeckCountChange(DeckType.Draw);
        InvokeDeckCountChange(DeckType.Throw);
    }

    /// <summary>
    /// 카드 뽑을때 데이터 받아올수 있게
    /// </summary>
    /// <param name="count"></param>
    /// <returns></returns>
    public List<BattleCardData> NextDeckSet(int count)
    {
        List<BattleCardData> returnDatas = new List<BattleCardData>();

        for (int i = 0; i < count; i++)
        {
            if (notuseDeck.Count <= 0)
            {
                notuseDeck = new List<string>(garbageDeck);
                Shuffle();
                garbageDeck.Clear();
            }

            BattleCardData getdata = new BattleCardData();

            DataCenter.Instance.GetCardData(notuseDeck[0], (data) =>
            {
                getdata.data = Instantiate(data);
            });
            handDeck.Add(notuseDeck[0]);
            notuseDeck.RemoveAt(0);
            getdata.index = handDeck.Count - 1;
            returnDatas.Add(getdata);
        }

        InvokeDeckCountChange(DeckType.Draw);
        InvokeDeckCountChange(DeckType.Throw);
        return returnDatas;
    }

    /// <summary>
    /// 핸드에서 필드에 넣을때
    /// </summary>
    /// <param name="bc_data"></param>
    public void HandToFieldMove(BattleCardData bc_data)
    {
        handDeck.Remove(bc_data.data.id);
        GetSynergyData();
    }
    /// <summary>
    /// 필드에서 핸드에 넣을때
    /// </summary>
    /// <param name="bc_data"></param>
    public void FieldToHandMove(BattleCardData bc_data)
    {
        handDeck.Add(bc_data.data.id);
        GetSynergyData();
    }

    /// <summary>
    /// 사용하지 않은 카드 섞기
    /// </summary>
    public void Shuffle()
    {
        for (int i = notuseDeck.Count - 1; i > 0; i--)
        {
            int j = UnityEngine.Random.Range(0, i + 1);

            string temp = notuseDeck[i];
            notuseDeck[i] = notuseDeck[j];
            notuseDeck[j] = temp;
        }
    }

    public float AttackField()
    {
        float damege = 0f;
        foreach (CardData data in attackField)
        {
            damege += data.ATK;
        }
        return damege;
    }

    public float DefenseField()
    {
        float shield = 0f;
        foreach (CardData data in defenseField)
        {
            shield += data.DEF;
        }
        return shield;
    }

    /// <summary>
    /// 사용한 카드 리스트에 추가
    /// </summary>
    /// <param name="index"></param>
    public void UseCard(string index)
    {
        garbageDeck.Add(index);
    }

    /// <summary>
    /// 덱 가지고 있는거 표기해주기 위한 리스트 요청
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public List<BattleCardData> GetDeckDatas(DeckType type)
    {
        List<BattleCardData> deck_data = new List<BattleCardData>();

        List<string> cards = new List<string>();

        switch (type)
        {
            case DeckType.Draw:
                cards = notuseDeck;
                break;
            case DeckType.Throw:
                cards = garbageDeck;
                break;
        }

        int index = 0;
        foreach (string card in cards)
        {
            DataCenter.Instance.GetCardData(card, (data) =>
            {
                BattleCardData battleCardData = new BattleCardData();
                battleCardData.index = index;
                battleCardData.data = Instantiate(data);
                deck_data.Add(battleCardData);
            });
        }

        int curIndex;
        int preIndex;
        BattleCardData value;

        for (curIndex = 1; curIndex < deck_data.Count; curIndex++)
        {
            value = deck_data[curIndex];
            preIndex = curIndex - 1;

            while (preIndex >= 0 && int.Parse(value.data.id) < int.Parse(deck_data[preIndex].data.id))
            {
                deck_data[preIndex + 1] = deck_data[preIndex];
                preIndex--;
            }
            deck_data[preIndex + 1] = value;
        }

        return deck_data;
    }

    /// <summary>
    /// 렌덤 아이템 추출용 함수
    /// </summary>
    /// <param name="count"> 추출 갯수 </param>
    /// <param name="level"> 레벨 보정값 </param>
    /// <returns></returns>
    public List<BattleCardData> GetResultItems(int count = 3, int level = 0)
    {
        int t_level = Mathf.Clamp(DataCenter.Instance.playerstate.level + level, 1, 9);
        ResultPercentData resultPercent = ScriptableObject.CreateInstance<ResultPercentData>();
        DataCenter.Instance.GetResultPercentData(t_level, (data) =>
        {
            resultPercent = Instantiate(data);
        });
        List<BattleCardData> results = new List<BattleCardData>();

        for (int i = 0; i < count; i++)
        {
            float roll = UnityEngine.Random.Range(0, 100);
            float accumulatedChance = 0;

            for (int n = 0; n < resultPercent.percent.Count; n++)
            {
                accumulatedChance += resultPercent.percent[n];
                // 추첨 값이 누적 확률 범위 내에 있으면 해당 등급을 반환
                if (roll <= accumulatedChance)
                {
                    results.Add(GetRandomCardData(n + 1));
                    break;
                }
            }
        }

        return results;
    }

    /// <summary>
    /// 확률(등급)에 따른 아이템 뽑기
    /// </summary>
    /// <param name="cut"></param>
    /// <returns></returns>
    public BattleCardData GetRandomCardData(int cut)
    {
        BattleCardData cardData = new BattleCardData();
        cardData.data = null;
        while (cardData.data == null)
        {
            string radom_id = DataCenter.random_card_datas[UnityEngine.Random.Range(0, DataCenter.random_card_datas.Count - 1)].ToString();
            DataCenter.Instance.GetCardData(radom_id, (data) =>
            {
                cardData.data = Instantiate(data);
            });
            if (cardData.data.grade != cut)
            {
                cardData.data = null;
            }
        }
        //Debug.Log($"이름: {cardData.data.itemName}, ID: {cardData.data.id}, 공격력: {cardData.data.ATK}, 방어력: {cardData.data.DEF}, cut : {cut} , grade : {cardData.data.grade}");
        Debug.Log($"grade : {cardData.data.grade}");
        return cardData;
    }

    /// <summary>
    /// 플레이어 레벨에 따른 퍼센트값 리턴(Normal, Rare, Unique, Epic)
    /// </summary>
    /// <returns></returns>
    public List<float> GetResultPercent()
    {
        ResultPercentData resultPercent = ScriptableObject.CreateInstance<ResultPercentData>();
        DataCenter.Instance.GetResultPercentData(DataCenter.Instance.playerstate.level, (data) =>
        {
            resultPercent = Instantiate(data);
        });
        return resultPercent.percent;
    }

    public void GetSynergyData()
    {
        List<CardData> t_onfieldcard = new List<CardData>();
        List<string> t_onfieldsynergy = new List<string>();
        synergyIDList = new Dictionary<string, SynergyTotalData>();

        foreach (CardData data in attackField)
        {
            t_onfieldcard.Add(data);
        }
        foreach (CardData data in defenseField)
        {
            t_onfieldcard.Add(data);
        }

        var uniqueList = t_onfieldcard.GroupBy(card => card.id)
                             .Select(group => group.First())
                             .ToList();

        // 기존 리스트를 비우고 중복 제거된 리스트로 채웁니다.
        t_onfieldcard.Clear();
        t_onfieldcard.AddRange(uniqueList);

        for (int i = 0; i < t_onfieldcard.Count; i++)
        {
            if (synergyIDList.TryGetValue(t_onfieldcard[i].synergy1ID, out SynergyTotalData synergy1count))
            {
                synergy1count.count += 1;
                synergyIDList[t_onfieldcard[i].synergy1ID] = synergy1count;
                Debug.Log("Synergy keep : " + t_onfieldcard[i].synergy1ID);
            }
            else if (t_onfieldcard[i].synergy1ID.Length > 0)
            {
                SynergyTotalData t = DataCenter.Instance.GetSynergyTotalData(t_onfieldcard[i].synergy1ID);
                t.count = 1;
                synergyIDList[t_onfieldcard[i].synergy1ID] = t;
            }
            if (synergyIDList.TryGetValue(t_onfieldcard[i].synergy2ID, out SynergyTotalData synergy2count))
            {
                synergy2count.count += 1;
                synergyIDList[t_onfieldcard[i].synergy2ID] = synergy2count;
                Debug.Log("Synergy keep : " + t_onfieldcard[i].synergy2ID);
            }
            else if (t_onfieldcard[i].synergy2ID.Length > 0)
            {
                SynergyTotalData t = DataCenter.Instance.GetSynergyTotalData(t_onfieldcard[i].synergy2ID);
                t.count = 1;
                synergyIDList[t_onfieldcard[i].synergy2ID] = t;
            }
            if (synergyIDList.TryGetValue(t_onfieldcard[i].synergy3ID, out SynergyTotalData synergy3count))
            {
                synergy3count.count += 1;
                synergyIDList[t_onfieldcard[i].synergy3ID] = synergy3count;
                Debug.Log("Synergy keep : " + t_onfieldcard[i].synergy3ID);
            }
            else if (t_onfieldcard[i].synergy3ID.Length > 0)
            {
                SynergyTotalData t = DataCenter.Instance.GetSynergyTotalData(t_onfieldcard[i].synergy3ID);
                t.count = 1;
                synergyIDList[t_onfieldcard[i].synergy3ID] = t;
            }
        }
        synergyIDList.OrderByDescending(x => x.Value.count)
                                     .Select(x => x.Key)
                                     .ToList();

        List<string> keysToProcess = synergyIDList
            .OrderByDescending(x => IsSynergyActivated(x.Value))
            .ThenByDescending(x => x.Value.count)
            .ThenByDescending(x => x.Value.synergyData != null ? x.Value.synergyData.Tier : 0)
            .Select(x => x.Key)
            .ToList();
        foreach (string key in keysToProcess)
        {
            SynergyTotalData totalData = synergyIDList[key];
            SynergyData sd = totalData.synergyData;
            IList<int> effectTiers = sd != null ? sd.Effect1Synergys : null;
            bool inRange = effectTiers != null
                           && totalData.count >= 0
                           && totalData.count < effectTiers.Count;
            bool effectReady = inRange && effectTiers[totalData.count] > 0;

            if (effectReady)
            {
                Debug.Log("Synergy use : " + key);
            }
            else
            {
                // UI(SynergyUI 등)에서는 미충족 시너지도 0칸 게이지로 표시하기 위해 목록에서 제거하지 않음
                Debug.Log("Synergy not use (표시만) : " + key);
            }
        }
        synergyIDList.OrderByDescending(x => x.Value.synergyData.Tier)
                                     .Select(x => x.Key)
                                     .ToList();
        InvokeSynergys();
    }

    private bool IsSynergyActivated(SynergyTotalData totalData)
    {
        if (totalData?.synergyData == null || totalData.count <= 0)
        {
            return false;
        }

        int count = totalData.count;
        bool effect1Active = GetEffectValueAtCount(totalData.synergyData.Effect1Synergys, count) > 0;
        bool effect2Active = GetEffectValueAtCount(totalData.synergyData.Effect2Synergys, count) > 0;
        bool effect3Active = GetEffectValueAtCount(totalData.synergyData.Effect3Synergys, count) > 0;
        return effect1Active || effect2Active || effect3Active;
    }

    private int GetEffectValueAtCount(List<int> effectValues, int count)
    {
        if (effectValues == null || effectValues.Count == 0 || count <= 0)
        {
            return 0;
        }

        int index = Mathf.Clamp(count - 1, 0, effectValues.Count - 1);
        return effectValues[index];
    }

    public bool IsCheckInventory(string cardid)
    {
        bool check = false;
        foreach (CardData data in DataCenter.Instance.userDeck)
        {
            Debug.Log("userDeck : " + data.id);
            if (data.id == cardid)
            {
                check = true;
                break;
            }
        }
        Debug.Log("card id : "+cardid+" / inventory check = "+check);
        return check;
    }
}
