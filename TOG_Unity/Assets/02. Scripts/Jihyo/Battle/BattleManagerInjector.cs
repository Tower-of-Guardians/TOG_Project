using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BattleManagerInjector : MonoBehaviour, IInjector
{
    [SerializeField] private BattleManager battleManager;
    [SerializeField] private Player player;
    [SerializeField] private Button attackButton;

    // TODO: 현재는 테스트용 ID를 직접 나열합니다.
    // 추후 현재 등장 구간(Section)에 맞는 조합만 MonsterEncounterData(몬스터 조합 테이블)에서
    // 필터링해 randomEncounterIds를 구성한 뒤, 그 목록에서 랜덤 선택하도록 변경합니다.
    // (MonsterEncounterData.Section, Mon1~4ID, Mon*Position, Gold, Exp 참조)
    [Header("Encounter Spawn")]
    [SerializeField] private string[] randomEncounterIds = { "1410000", "1410001" };
    [SerializeField] private MonsterPrefabRegistry monsterPrefabRegistry;
    [SerializeField] private Transform globalRoot;

    public void Inject()
    {
        StartCoroutine(InitializeBattleRoutine());
    }

    private IEnumerator InitializeBattleRoutine()
    {
        if (battleManager == null)
        {
            battleManager = GetComponent<BattleManager>();
        }

        if (battleManager == null)
        {
            Debug.LogError("BattleManagerInjector requires a BattleManager reference.");
            yield break;
        }

        if (player == null)
        {
            player = FindAnyObjectByType<Player>();
        }

        if (globalRoot == null)
        {
            GameObject globalObject = GameObject.Find("[Global]");
            if (globalObject != null)
            {
                globalRoot = globalObject.transform;
            }
        }

        yield return new WaitUntil(() =>
            DataCenter.Instance != null
            && DataCenter.IsMonsterEncounterDataLoaded
            && DataCenter.IsMonsterDataLoaded);

        string selectedEncounterId = SelectRandomEncounterId();
        if (string.IsNullOrEmpty(selectedEncounterId))
        {
            Debug.LogError("BattleManagerInjector: 랜덤 선택 가능한 Encounter ID가 없습니다.");
            yield break;
        }

        MonsterEncounterData encounterData = null;
        DataCenter.Instance.GetMonsterEncounterData(selectedEncounterId, data => encounterData = data);
        yield return new WaitUntil(() =>
        {
            if (encounterData != null)
            {
                return true;
            }

            DataCenter.Instance.GetMonsterEncounterData(selectedEncounterId, data => encounterData = data);
            return encounterData != null;
        });

        if (encounterData == null)
        {
            Debug.LogError($"BattleManagerInjector: Encounter ID {selectedEncounterId} 데이터를 찾을 수 없습니다.");
            yield break;
        }

        List<Monster> spawnedMonsters = BattleEncounterSpawner.SpawnEncounter(
            encounterData,
            globalRoot,
            monsterPrefabRegistry);

        if (spawnedMonsters.Count == 0)
        {
            Debug.LogError($"BattleManagerInjector: Encounter {selectedEncounterId}에서 스폰된 몬스터가 없습니다.");
            yield break;
        }

        battleManager.Initialize(player, spawnedMonsters, attackButton, encounterData);

        if (!DIContainer.IsRegistered<BattleManager>())
        {
            DIContainer.Register<BattleManager>(battleManager);
        }
    }

    private string SelectRandomEncounterId()
    {
        if (DataCenter.monster_encounter_datas == null || DataCenter.monster_encounter_datas.Count == 0)
        {
            return null;
        }

        int index = Random.Range(0, DataCenter.monster_encounter_datas.Count);
        int current = 0;
        foreach (KeyValuePair<string, MonsterEncounterData> pair in DataCenter.monster_encounter_datas)
        {
            if (current == index)
            {
                return pair.Key;
            }

            current++;
        }

        return null;
    }
}
