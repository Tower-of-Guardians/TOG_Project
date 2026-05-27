using System; // async/await 사용을 위해 필요
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceLocations;

public class DataCenter : Singleton<DataCenter>
{
    ////// 플레이어 관련 /////
    public PlayerState playerstate = new PlayerState();
    public event Action<PlayerState> playerStateEvent;
    public event Action<int> playerMoneyEvent;
    public event Action<int> playerHpEvent;
    public event Action<int,int> playerLevelEvent;
    public event Action<bool> playerStateLoadEvent;
    //////////////////////////

    ////// 카드 관련 //////
    public static Dictionary<string, CardData> card_datas = new Dictionary<string, CardData>(); // 카드데이터
    private static AsyncOperationHandle<IList<CardData>> carddata_loadHandle; // 메모리 관리를 위한 핸들
    public List<CardData> userDeck = new List<CardData>();// 유저 소지 카드 데이터
    public static List<string> random_card_datas = new List<string>(); // 랜덤하게 카드 뽑기위한 데이터
    SortType sortType;
    bool sortType_oder = true; // true = 오름차순, false = 내림차순
    public static bool IsCardDataLoaded { get; private set; } = false;
    public event Action<bool> cardDataLoadEvent;
    //////////////////////

    ////// 리절트 관련 //////
    public static Dictionary<int, ResultPercentData> result_datas = new Dictionary<int, ResultPercentData>(); // 카드데이터
    private static AsyncOperationHandle<IList<ResultPercentData>> resultdata_loadHandle; // 메모리 관리를 위한 핸들
    public static bool IsResultDataLoaded { get; private set; } = false;
    /////////////////////////


    ////// 몬스터 관련 ///////
    public static Dictionary<string, MonsterData> monster_datas = new Dictionary<string, MonsterData>(); // 몬스터 데이터
    private static AsyncOperationHandle<IList<MonsterData>> monsterdata_loadHandle; // 메모리 관리를 위한 핸들

    public static Dictionary<string, MonsterEncounterData> monster_encounter_datas = new Dictionary<string, MonsterEncounterData>(); // 몬스터 데이터
    private static AsyncOperationHandle<IList<MonsterEncounterData>> monster_encounter_datas_loadHandle; // 메모리 관리를 위한 핸들

    public static bool IsMonsterDataLoaded { get; private set; } = false;
    public static bool IsMonsterActionDataLoaded { get; private set; } = false;
    public static bool IsMonsterEncounterDataLoaded { get; private set; } = false;
    //////////////////////////

    ////// 스테이 터스 이팩트 관련 ///////
    public static Dictionary<string, StatusEffectData> status_effect_datas = new Dictionary<string, StatusEffectData>(); // 스테이 터스 이팩트 데이터
    private static AsyncOperationHandle<IList<StatusEffectData>> status_effect_datas_loadHandle; // 메모리 관리를 위한 핸들

    public static bool IsStatusEffectDataLoaded { get; private set; } = false;
    //////////////////////////

    ////// 시너지 관련 ///////
    public static Dictionary<string, SynergyData> synergy_datas = new Dictionary<string, SynergyData>(); // 시너지 데이터
    private static AsyncOperationHandle<IList<SynergyData>> synergy_datas_loadHandle; // 메모리 관리를 위한 핸들

    public static bool IsSynergyDataLoaded { get; private set; } = false;
    //////////////////////////

    ////// 이팩트 관련 ///////
    public static Dictionary<string, EffectData> effect_datas = new Dictionary<string, EffectData>(); // 이팩트 데이터
    private static AsyncOperationHandle<IList<EffectData>> effect_datas_loadHandle; // 메모리 관리를 위한 핸들

    public static bool IsEffectDataLoaded { get; private set; } = false;
    //////////////////////////

    protected override void Awake()
    {
        base.Awake();
        DataLoad();
    }

    private async void DataLoad()
    {
        LoadPlayerData();

        await AllCardData();
        StartCoroutine(SetStartDeck());

        await AllResultPercentData();
        await AllMonsterData();
        await AllMonsterEncounterData();
        await AllStatusEffectData();
        await AllSynergyData();
        await AllEffectData();
    }

    public void LoadPlayerData()
    {
        playerstate.level = 1;
        playerstate.experience = 40;
        playerstate.hp = 70;
        playerstate.lhp = 5;
        playerstate.atk = 4;
        playerstate.latk = 0;
        playerstate.maxmagic = 2;
        playerstate.money = 100;
        playerStateLoadEvent?.Invoke(true);
    }
    IEnumerator SetStartDeck()
    {
        yield return new WaitUntil(() => IsCardDataLoaded == true);
        string[] startdecks = { "11000001", "11000002", "11010003", "11010004", "11010005", "11010006", "11010007", "11010008", "11010009", "11010025" };
        foreach (string id in startdecks)
        {
            GetCardData(id, (data) => userDeck.Add(Instantiate(data)));
        }
    }
    public async Task AllCardData()
    {
        // 1. 키를 명확하게 리스트로 선언 (InvalidKeyException 방지 핵심)
        var keys = new List<string> { "CardData" };

        // 2. LoadAssetsAsync 호출
        // 인자: 키 리스트, 개별 완료 콜백, 머지 모드
        carddata_loadHandle = Addressables.LoadAssetsAsync<CardData>(
            keys,
            (item) =>
            {
                if (item != null)
                {
                    // 중복 방지 로직 추가
                    if (!card_datas.ContainsKey(item.id))
                    {
                        card_datas[item.id] = item;
                        random_card_datas.Add(item.id);
                    }
                }
            },
            Addressables.MergeMode.Union
        );

        // 3. 비동기 대기
        await carddata_loadHandle.Task;

        // 4. 상태 확인
        if (carddata_loadHandle.Status == AsyncOperationStatus.Succeeded)
        {
            IsCardDataLoaded = true;
            cardDataLoadEvent?.Invoke(true);
            UnityEngine.Debug.Log($"CardData 로드 완료: {card_datas.Count}개");
        }
        else
        {
            IsCardDataLoaded = false;
            cardDataLoadEvent?.Invoke(false);
            UnityEngine.Debug.LogError($"CardData 로드 실패: {carddata_loadHandle.OperationException}");

            // 로드 실패 시 핸들 해제 (메모리 관리)
            if (carddata_loadHandle.IsValid())
                Addressables.Release(carddata_loadHandle);
        }
    }
    public async Task AllResultPercentData()
    {
        // Addressables.LoadAssetsAsync<TObject>(key, callback)
        // key는 주소 또는 레이블을 사용할 수 있습니다. 여기서는 레이블을 사용합니다.
        string myLabel = "ResultPercentData";
        resultdata_loadHandle = Addressables.LoadAssetsAsync<ResultPercentData>(
            myLabel,
            // 로드된 각 Asset에 대한 콜백 (선택 사항)
            (item) =>
            {
                if (item != null)
                {
                    result_datas[item.level] = item;
                }
            }
        );

        // 비동기 작업이 완료될 때까지 대기
        await resultdata_loadHandle.Task;

        if (resultdata_loadHandle.Status == AsyncOperationStatus.Succeeded)
        {
            IsResultDataLoaded = true;
            UnityEngine.Debug.Log($"ResultPercentData 로드 완료: {result_datas.Count}");
        }
        else
        {
            UnityEngine.Debug.LogError($"ResultPercentData 로드 실패: {resultdata_loadHandle.OperationException}");
        }
    }
    public async Task AllMonsterData()
    {
        monsterdata_loadHandle = Addressables.LoadAssetsAsync<MonsterData>(
            "MonsterData",
            (item) =>
            {
                if (item != null)
                {
                    monster_datas[item.Id] = item;
                }
            }
        );

        await monsterdata_loadHandle.Task;

        if (monsterdata_loadHandle.Status == AsyncOperationStatus.Succeeded)
        {
            IsMonsterDataLoaded = true;
            UnityEngine.Debug.Log($"MonsterData 로드 완료: {monster_datas.Count}");
        }
        else
        {
            UnityEngine.Debug.LogError($"MonsterData 로드 실패: {monsterdata_loadHandle.OperationException}");
        }
    }   
    public async Task AllMonsterEncounterData()
    {
        monster_encounter_datas_loadHandle = Addressables.LoadAssetsAsync<MonsterEncounterData>(
            "MonsterEncounterData",
            (item) =>
            {
                if (item != null)
                {
                    monster_encounter_datas[item.Id] = item;
                }
            }
        );

        await monster_encounter_datas_loadHandle.Task;

        if (monster_encounter_datas_loadHandle.Status == AsyncOperationStatus.Succeeded)
        {
            IsMonsterEncounterDataLoaded = true;
            UnityEngine.Debug.Log($"MonsterEncounterData 로드 완료: {monster_encounter_datas.Count}");
        }
        else
        {
            UnityEngine.Debug.LogError($"MonsterEncounterData 로드 실패: {monster_encounter_datas_loadHandle.OperationException}");
        }
    }
    public async Task AllStatusEffectData()
    {
        status_effect_datas_loadHandle = Addressables.LoadAssetsAsync<StatusEffectData>(
            "StatusEffectData",
            (item) =>
            {
                if (item != null)
                {
                    status_effect_datas[item.Id] = item;
                }
            }
        );

        await status_effect_datas_loadHandle.Task;

        if (status_effect_datas_loadHandle.Status == AsyncOperationStatus.Succeeded)
        {
            IsStatusEffectDataLoaded = true;
            UnityEngine.Debug.Log($"StatusEffectData 로드 완료: {status_effect_datas.Count}");
        }
        else
        {
            UnityEngine.Debug.LogError($"StatusEffectData 로드 실패: {status_effect_datas_loadHandle.OperationException}");
        }
    }
    public async Task AllSynergyData()
    {
        synergy_datas_loadHandle = Addressables.LoadAssetsAsync<SynergyData>(
            "SynergyData",
            (item) =>
            {
                if (item != null)
                {
                    synergy_datas[item.ID] = item;
                }
            }
        );

        await synergy_datas_loadHandle.Task;

        if (synergy_datas_loadHandle.Status == AsyncOperationStatus.Succeeded)
        {
            IsSynergyDataLoaded = true;
            UnityEngine.Debug.Log($"StatusEffectData 로드 완료: {synergy_datas.Count}");
        }
        else
        {
            UnityEngine.Debug.LogError($"StatusEffectData 로드 실패: {synergy_datas_loadHandle.OperationException}");
        }
    }
    public async Task AllEffectData()
    {
        effect_datas_loadHandle = Addressables.LoadAssetsAsync<EffectData>(
            "EffectData",
            (item) =>
            {
                if (item != null)
                {
                    effect_datas[item.Id] = item;
                }
            }
        );

        await effect_datas_loadHandle.Task;

        if (effect_datas_loadHandle.Status == AsyncOperationStatus.Succeeded)
        {
            IsEffectDataLoaded = true;
            UnityEngine.Debug.Log($"EffectData 로드 완료: {effect_datas.Count}");
        }
        else
        {
            UnityEngine.Debug.LogError($"EffectData 로드 실패: {effect_datas_loadHandle.OperationException}");
        }
    }
    public void ReleaseDataHandle()
    {
        Addressables.Release(carddata_loadHandle);
        Addressables.Release(resultdata_loadHandle);
        Addressables.Release(monsterdata_loadHandle);
        Addressables.Release(monster_encounter_datas_loadHandle);
        Addressables.Release(status_effect_datas_loadHandle);
        Addressables.Release(synergy_datas_loadHandle);
        UnityEngine.Debug.Log("Addressables 핸들 해제 완료.");
    }

    #region 데이터 받는 함수
    /// <summary>
    /// 아이템 id 를 사용한 데이터 받기
    /// </summary>
    /// <param name="id"></param>
    /// <param name="data"></param>
    public void GetCardData(string id, Action<CardData> data)
    {
        if (IsCardDataLoaded && card_datas.TryGetValue(id, out CardData itemData))
        {
            data?.Invoke(itemData);
        }
        else
        {
            UnityEngine.Debug.Log($"ID {id}에 해당하는 아이템 데이터가 로드되지 않았습니다. IsCardDataLoaded = {IsCardDataLoaded}.");
            data?.Invoke(null);
        }
    }


    /// <summary>
    /// 레벨당 리절트 확률 데이터 받기
    /// </summary>
    /// <param name="level"></param>
    /// <param name="data"></param>
    public void GetResultPercentData(int level, Action<ResultPercentData> data)
    {
        if (IsResultDataLoaded && result_datas.TryGetValue(level, out ResultPercentData itemData))
        {
            data?.Invoke(itemData);
        }
        else
        {
            UnityEngine.Debug.Log($"ID {level}에 해당하는 아이템 데이터가 로드되지 않았습니다.");
            data?.Invoke(null);
        }
    }

    /// <summary>
    /// 정렬 타입에 따른 userDeck 리스트 정렬
    /// </summary>
    /// <param name="type"></param>
    public void SortUserCards(SortType type)
    {
        List<CardData> sortedDescending = new List<CardData>();
        if (type != sortType)
        {
            sortType_oder = true;
            if (type == SortType.Grade)
            {
                sortType_oder = false;
            }
        }

        switch (type)
        {
            case SortType.Grade:
                if (!sortType_oder)
                {
                    sortedDescending = userDeck.OrderByDescending(data => data.star).ToList();
                    sortedDescending = userDeck.OrderByDescending(data => data.grade).ToList();
                }
                else
                {
                    sortedDescending = userDeck.OrderBy(data => data.star).ToList();
                    sortedDescending = userDeck.OrderBy(data => data.grade).ToList();
                }
                break;
            case SortType.Time:
                if (sortType_oder)
                {
                    sortedDescending = userDeck.OrderBy(data => data.time).ToList();
                }
                else
                {
                    sortedDescending = userDeck.OrderByDescending(data => data.time).ToList();
                }
                break;
            case SortType.Attack:
                if (sortType_oder)
                {
                    sortedDescending = userDeck.OrderBy(data => data.ATK).ToList();
                }
                else
                {
                    sortedDescending = userDeck.OrderByDescending(data => data.ATK).ToList();
                }
                break;
            case SortType.Defense:
                if (sortType_oder)
                {
                    sortedDescending = userDeck.OrderBy(data => data.DEF).ToList();
                }
                else
                {
                    sortedDescending = userDeck.OrderByDescending(data => data.DEF).ToList();
                }
                break;
        }

        sortType = type;
        sortType_oder = !sortType_oder;
        userDeck = sortedDescending;
        foreach (CardData data in userDeck)
        {
            UnityEngine.Debug.Log(data.itemName);
        }
    }

    public List<CardData> GetFront4_End3_CardList(string front, string back)
    {
        // LINQ의 Where를 사용하면 한 줄로 필터링이 가능합니다.
        return userDeck.Where(user =>
            user.id != null &&              // ID가 비어있지 않은지 확인
            user.id.Length == 8 &&          // 8자리인지 확인
            user.id.StartsWith(front) &&    // 앞 4자리 체크
            user.id.EndsWith(back)          // 뒤 3자리 체크
        ).ToList();
    }

    /// <summary>
    /// 몬스터 데이터 받기
    /// </summary>
    /// <param name="id"></param>
    /// <param name="data"></param>
    public void GetMonsterData(string id, Action<MonsterData> data)
    {
        if (IsMonsterDataLoaded && monster_datas.TryGetValue(id, out MonsterData itemData))
        {
            data?.Invoke(itemData);
        }
        else
        {
            UnityEngine.Debug.Log($"ID {id}에 해당하는 아이템 데이터가 로드되지 않았습니다. IsMonsterDataLoaded = {IsMonsterDataLoaded}.");
            data?.Invoke(null);
        }
    }

   /// <summary>
   /// 몬스터 인카운터 데이터 받기
   /// </summary>
   /// <param name="id"></param>
   /// <param name="data"></param>
    public void GetMonsterEncounterData(string id, Action<MonsterEncounterData> data)
    {
        if (IsMonsterEncounterDataLoaded && monster_encounter_datas.TryGetValue(id, out MonsterEncounterData itemData))
        {
            data?.Invoke(itemData);
        }
        else
        {
            UnityEngine.Debug.Log($"ID {id}에 해당하는 아이템 데이터가 로드되지 않았습니다. IsMonsterEncounterDataLoaded = {IsMonsterEncounterDataLoaded}.");
            data?.Invoke(null);
        }
    }

    /// <summary>
    /// 상태 데이터 받기
    /// </summary>
    /// <param name="id"></param>
    /// <param name="data"></param>
    public void GetStatusEffectData(string id, Action<StatusEffectData> data)
    {
        if (IsStatusEffectDataLoaded && status_effect_datas.TryGetValue(id, out StatusEffectData itemData))
        {
            data?.Invoke(itemData);
        }
        else
        {
            UnityEngine.Debug.Log($"ID {id}에 해당하는 아이템 데이터가 로드되지 않았습니다. IsStatusEffectDataLoaded = {IsStatusEffectDataLoaded}.");
            data?.Invoke(null);
        }
    }

    /// <summary>
    /// 시너지 데이터 받기
    /// </summary>
    /// <param name="id"></param>
    /// <param name="data"></param>
    public void GetSynergyData(string id, Action<SynergyData> data)
    {
        if (IsSynergyDataLoaded && synergy_datas.TryGetValue(id, out SynergyData itemData))
        {
            data?.Invoke(itemData);
        }
        else
        {
            UnityEngine.Debug.Log($"ID {id}에 해당하는 아이템 데이터가 로드되지 않았습니다. IsSynergyDataLoaded = {IsSynergyDataLoaded}.");
            data?.Invoke(null);
        }
    }

    /// <summary>
    /// 이팩트 데이터 받기
    /// </summary>
    /// <param name="id"></param>
    /// <param name="data"></param>
    public void GetEffectData(string id, Action<EffectData> data)
    {
        if (IsEffectDataLoaded && effect_datas.TryGetValue(id, out EffectData itemData))
        {
            data?.Invoke(itemData);
        }
        else
        {
            UnityEngine.Debug.Log($"ID {id}에 해당하는 아이템 데이터가 로드되지 않았습니다. IsEffectDataLoaded = {IsEffectDataLoaded}.");
            data?.Invoke(null);
        }
    }

    /// <summary>
    /// 카드ID를 통한 해당 아이디의 모든 성급의 카드 데이터 리스트 리턴
    /// </summary>
    /// <param name="card_id"></param>
    /// <returns></returns>
    public List<CardData> GetSeriesCards(string card_id)
    {
        var cards = new List<CardData>();

        string front_id = card_id.Substring(0, 4);
        string back_id = card_id.Substring(5, 3);

        for (int s = 0; s < 3; s++)
        {
            string find_id = front_id + s + back_id;
            if (card_datas.TryGetValue(find_id, out CardData itemData))
            {
                cards.Add(itemData);
            }
        }

        return cards;
    }

    public SynergyTotalData GetSynergyTotalData(string synergyId)
    {
        var data = new SynergyTotalData();
        data.synergyData = ScriptableObject.CreateInstance<SynergyData>();

        GetSynergyData(synergyId , (load_data) =>
        {
            data.synergyData = load_data;
        });

        return data;
    }
    #endregion

    #region 플레이어 데이터 이벤트
    public void SetPlayerState()
    {
        playerStateEvent?.Invoke(playerstate);
    }
    public void SetMoney(int money)
    {
        playerstate.money += money;
        playerMoneyEvent?.Invoke(playerstate.money);
    }
    public void SetPlayerHP(int hp)
    {
        playerstate.hp += hp;
        playerstate.hp = Mathf.Clamp(playerstate.hp, 0, playerstate.maxhp);
        playerHpEvent?.Invoke(hp);
    }
    public void SetPlayerLevel(int experience)
    {
        if (playerstate.level >= 9)
        {
            playerstate.level = 9;
            playerLevelEvent?.Invoke(playerstate.level, 0);
        }
        else
        {
            playerstate.experience += experience;

            while (playerstate.maxexperience <= playerstate.experience) // 혹시 모를 과다 경험치 획득시 연속 레벨업 대비
            {
                int ex = playerstate.experience - playerstate.maxexperience;
                playerstate.level++;
                playerstate.maxexperience = 25 + 6 * (playerstate.level - 1); // 경험치 수식
                playerstate.experience = ex;
            }
            playerLevelEvent?.Invoke(playerstate.level, playerstate.experience);
        }
    }
    #endregion

    private void OnDisable()
    {
        ReleaseDataHandle();
    }
}
