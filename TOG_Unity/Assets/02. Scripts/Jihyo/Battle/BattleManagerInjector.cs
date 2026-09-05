using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BattleManagerInjector : MonoBehaviour, IInjector
{
    private const float DataLoadTimeoutSeconds = 30f;
    private const int InitialBattleSection = 11;

    [SerializeField] private BattleManager battleManager;
    [SerializeField] private Player player;
    [SerializeField] private Button attackButton;
    [SerializeField] private MonsterPrefabRegistry monsterPrefabRegistry;
    [SerializeField] private Transform globalRoot;

    private bool isStartingBattle;
    private Action<bool> pendingCompletion;

    public void Inject()
    {
        if (!DIContainer.IsRegistered<BattleManagerInjector>())
        {
            DIContainer.Register<BattleManagerInjector>(this);
        }
        if (!isStartingBattle)
        {
            isStartingBattle = true;
            StartCoroutine(InitializeBattleRoutine());
        }
    }

    public bool TryStartAreaBattle(AreaEventData areaData, AreaEventType type, Action<bool> onComplete = null)
    {
        if (isStartingBattle || areaData == null || !HasBattleData() || !ValidateReferences())
        {
            return false;
        }

        if (!player.IsAlive)
        {
            Debug.LogWarning("BattleManagerInjector: 쓰러진 플레이어는 다음 전투에 진입할 수 없습니다.", this);
            return false;
        }

        MonsterEncounterData encounter = SelectEncounter(areaData.Section, type);
        if (encounter == null)
        {
            return false;
        }

        isStartingBattle = true;
        pendingCompletion = onComplete;
        StartCoroutine(StartEncounterRoutine(encounter, true));
        return true;
    }

    private IEnumerator InitializeBattleRoutine()
    {
        float deadline = Time.realtimeSinceStartup + DataLoadTimeoutSeconds;
        while (!HasBattleData() && Time.realtimeSinceStartup < deadline)
        {
            yield return null;
        }

        if (!HasBattleData())
        {
            Debug.LogError("BattleManagerInjector: 전투 데이터 로드 제한 시간을 초과했습니다.", this);
            Complete(false);
            yield break;
        }

        if (!ValidateReferences())
        {
            Complete(false);
            yield break;
        }

        MonsterEncounterData encounter = SelectEncounter(InitialBattleSection, AreaEventType.Battle);
        if (encounter == null)
        {
            Complete(false);
            yield break;
        }

        yield return StartEncounterRoutine(encounter, false);
    }

    private IEnumerator StartEncounterRoutine(MonsterEncounterData encounter, bool restart)
    {
        yield return null;

        if (!ValidateReferences())
        {
            Complete(false);
            yield break;
        }

        if (restart)
        {
            battleManager.PrepareForNextBattle();
        }

        List<Monster> monsters = BattleEncounterSpawner.SpawnEncounter(encounter, globalRoot, monsterPrefabRegistry);
        if (monsters.Count == 0)
        {
            Debug.LogError($"BattleManagerInjector: Encounter {encounter.Id}에서 스폰된 몬스터가 없습니다.", this);
            Complete(false);
            yield break;
        }

        if (!DIContainer.IsRegistered<BattleManager>())
        {
            DIContainer.Register<BattleManager>(battleManager);
        }

        battleManager.Initialize(player, monsters, attackButton, encounter, Complete);
    }

    private MonsterEncounterData SelectEncounter(int section, AreaEventType type)
    {
        List<MonsterEncounterData> candidates = BattleEncounterSelector.GetCandidates(
            DataCenter.monster_encounter_datas.Values, DataCenter.monster_datas, section, type);
        candidates.RemoveAll(encounter => !BattleEncounterSpawner.CanSpawnEncounter(encounter, monsterPrefabRegistry));
        if (candidates.Count == 0)
        {
            Debug.LogWarning($"BattleManagerInjector: Section {section}의 {type} 전투 데이터 또는 몬스터 프리팹이 없습니다.", this);
            return null;
        }

        return candidates[UnityEngine.Random.Range(0, candidates.Count)];
    }

    private bool ValidateReferences()
    {
        if (battleManager == null)
        {
            battleManager = GetComponent<BattleManager>();
        }

        if (battleManager != null && player != null && attackButton != null
            && globalRoot != null && monsterPrefabRegistry != null && battleManager.CanInitializeBattle)
        {
            return true;
        }

        Debug.LogError("BattleManagerInjector: 전투 매니저, 컨트롤러, 플레이어, 버튼, 스폰 위치 또는 몬스터 레지스트리 참조가 없습니다.", this);
        return false;
    }

    private static bool HasBattleData()
    {
        return DataCenter.Instance != null && DataCenter.IsMonsterEncounterDataLoaded && DataCenter.IsMonsterDataLoaded;
    }

    private void Complete(bool success)
    {
        isStartingBattle = false;
        Action<bool> callback = pendingCompletion;
        pendingCompletion = null;
        callback?.Invoke(success);
    }

    private void OnDisable()
    {
        StopAllCoroutines();
        if (isStartingBattle)
        {
            Complete(false);
        }
    }
}
