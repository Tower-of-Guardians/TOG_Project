#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BattleWindowEditor : EditorWindow
{
    private const string GameScenePath = "Assets/01. Scenes/Game.unity";
    private const string GameSceneName = "Game";
    private const string EncounterDataFolder = "Assets/Datas/MonsterEncounterData";
    private const string MonsterDataFolder = "Assets/Datas/MonsterData";
    private const string SynergyDataFolder = "Assets/Datas/SynergyData";
    private const string DefaultRegistryPath = "Assets/02. Scripts/Jihyo/Battle/MonsterPrefabRegistry.asset";
    private const int DebugSynergyMaxCount = 5;

    private readonly List<MonsterEncounterData> encounterDatas = new List<MonsterEncounterData>();
    private readonly Dictionary<string, MonsterData> monsterDataCache = new Dictionary<string, MonsterData>();
    private readonly List<SynergyData> synergyDatas = new List<SynergyData>();
    private readonly Dictionary<string, int> debugSynergyCounts = new Dictionary<string, int>();

    private Vector2 scrollPosition;
    private int selectedEncounterIndex;
    private string statusMessage = string.Empty;
    private MessageType statusType = MessageType.Info;

    [MenuItem("\u2764 IngameSetTools/Battle")]
    public static void OpenWindow()
    {
        BattleWindowEditor window = GetWindow<BattleWindowEditor>();
        window.titleContent = new GUIContent("IngameSetTools - Battle");
        window.minSize = new Vector2(420f, 720f);
        window.Show();
    }

    private static bool IsGameSceneActive()
    {
        Scene activeScene = EditorSceneManager.GetActiveScene();
        return activeScene.path == GameScenePath || activeScene.name == GameSceneName;
    }

    private void OnEnable()
    {
        ReloadEncounterDatas();
        ReloadSynergyDatas();
        EditorApplication.update += OnEditorUpdate;
        EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
    }

    private void OnDisable()
    {
        EditorApplication.update -= OnEditorUpdate;
        EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
    }

    private void OnPlayModeStateChanged(PlayModeStateChange state)
    {
        if (state == PlayModeStateChange.EnteredPlayMode || state == PlayModeStateChange.EnteredEditMode)
        {
            Repaint();
        }
    }

    private void OnEditorUpdate()
    {
        if (!Application.isPlaying)
        {
            return;
        }

        if (focusedWindow == this)
        {
            Repaint();
        }
    }

    private static bool CanUseBattleTools()
    {
        return IsGameSceneActive() && Application.isPlaying;
    }

    private void OnGUI()
    {
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

        DrawHeader();

        if (!IsGameSceneActive())
        {
            EditorGUILayout.HelpBox(
                "Encounter 미리보기는 어디서든 볼 수 있습니다.\n몬스터 세팅/체력 조절은 Game 씬(Assets/01. Scenes/Game.unity) 플레이 모드에서만 사용할 수 있습니다.",
                MessageType.Info);
            EditorGUILayout.Space(8f);
        }

        if (IsGameSceneActive())
        {
            DrawSceneStatusSection();
            EditorGUILayout.Space(8f);
        }

        DrawEncounterSection();
        EditorGUILayout.Space(8f);
        DrawSynergyDebugSection();
        EditorGUILayout.Space(8f);
        DrawPlayerHealthSection();
        EditorGUILayout.Space(8f);
        DrawMonsterHealthSection();
        EditorGUILayout.Space(8f);
        DrawDebugVictorySection();

        if (!string.IsNullOrEmpty(statusMessage))
        {
            EditorGUILayout.Space(8f);
            EditorGUILayout.HelpBox(statusMessage, statusType);
        }

        EditorGUILayout.EndScrollView();
    }

    private void DrawHeader()
    {
        EditorGUILayout.LabelField("IngameSetTools - Battle", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox(
            "Game 씬 플레이 모드에서 Encounter 세팅, 시너지 발동 연출 확인, 플레이어/몬스터 체력 조절을 합니다.",
            MessageType.Info);
    }

    private void DrawSceneStatusSection()
    {
        EditorGUILayout.LabelField("Scene Status", EditorStyles.boldLabel);

        BattleManager battleManager = FindSceneObject<BattleManager>();
        Player player = FindSceneObject<Player>();
        bool spawnReady = TryResolveSpawnReferences(out Transform monsterSpawnRoot, out MonsterPrefabRegistry prefabRegistry);

        EditorGUILayout.LabelField("BattleManager", battleManager != null ? battleManager.name : "(not found)");
        EditorGUILayout.LabelField("Player", player != null ? player.name : "(not found)");
        EditorGUILayout.LabelField("Monster Spawn Root", monsterSpawnRoot != null ? monsterSpawnRoot.name : "(not found)");
        EditorGUILayout.LabelField("Monster Prefab Map", prefabRegistry != null ? prefabRegistry.name : "(not found)");

        if (!spawnReady)
        {
            EditorGUILayout.HelpBox(
                "몬스터 스폰 준비가 되지 않았습니다. Game 씬의 BattleManagerInjector와 [Global] 오브젝트를 확인해주세요.",
                MessageType.Warning);
        }
    }

    private void DrawEncounterSection()
    {
        EditorGUILayout.LabelField("Encounter Setup", EditorStyles.boldLabel);

        using (new EditorGUILayout.HorizontalScope())
        {
            if (GUILayout.Button("Reload Encounters", GUILayout.Width(140f)))
            {
                ReloadEncounterDatas();
            }

            EditorGUI.BeginDisabledGroup(!CanUseBattleTools() || !TryResolveSpawnReferences(out _, out _));
            if (GUILayout.Button("Apply Encounter", GUILayout.Height(24f)))
            {
                ApplySelectedEncounter();
            }
            EditorGUI.EndDisabledGroup();
        }

        if (encounterDatas.Count == 0)
        {
            EditorGUILayout.HelpBox($"Encounter 데이터를 찾을 수 없습니다.\n경로: {EncounterDataFolder}", MessageType.Warning);
            return;
        }

        selectedEncounterIndex = Mathf.Clamp(selectedEncounterIndex, 0, encounterDatas.Count - 1);
        string[] labels = encounterDatas
            .Select(data => $"{data.Id} - {data.Name} (Section {data.Section})")
            .ToArray();
        selectedEncounterIndex = EditorGUILayout.Popup("Encounter ID", selectedEncounterIndex, labels);

        MonsterEncounterData selectedEncounter = encounterDatas[selectedEncounterIndex];
        DrawEncounterPreview(selectedEncounter);
    }

    private void DrawEncounterPreview(MonsterEncounterData encounterData)
    {
        if (encounterData == null)
        {
            return;
        }

        EditorGUILayout.LabelField("Preview", EditorStyles.miniBoldLabel);
        EditorGUI.indentLevel++;
        DrawMonsterSlotPreview("Mon1", encounterData.Mon1ID);
        DrawMonsterSlotPreview("Mon2", encounterData.Mon2ID);
        DrawMonsterSlotPreview("Mon3", encounterData.Mon3ID);
        DrawMonsterSlotPreview("Mon4", encounterData.Mon4ID);
        EditorGUILayout.LabelField($"Gold: {encounterData.Gold}  |  Exp: {encounterData.Exp}");
        EditorGUI.indentLevel--;
    }

    private void DrawMonsterSlotPreview(string slotLabel, string monsterId)
    {
        if (string.IsNullOrEmpty(monsterId))
        {
            EditorGUILayout.LabelField(slotLabel, "(empty)");
            return;
        }

        MonsterData monsterData = LoadMonsterData(monsterId);
        if (monsterData != null)
        {
            EditorGUILayout.LabelField(slotLabel, $"{monsterId} - {monsterData.Name} (HP {monsterData.HP})");
            return;
        }

        EditorGUILayout.LabelField(slotLabel, monsterId);
    }

    private void DrawSynergyDebugSection()
    {
        EditorGUILayout.LabelField("Synergy Debug", EditorStyles.boldLabel);

        using (new EditorGUILayout.HorizontalScope())
        {
            if (GUILayout.Button("Reload Synergies", GUILayout.Width(140f)))
            {
                ReloadSynergyDatas();
            }

            if (GUILayout.Button("기본기 3"))
            {
                SetDebugPreset(("210007", 3));
            }

            if (GUILayout.Button("흡혈 2"))
            {
                SetDebugPreset(("210004", 2));
            }

            if (GUILayout.Button("기본기+흡혈"))
            {
                SetDebugPreset(("210007", 3), ("210004", 2));
            }
        }

        if (GUILayout.Button("정직+흡혈+기본기 (Loop 3회)"))
        {
            SetDebugPreset(("210001", 2), ("210004", 2), ("210007", 3));
        }

        if (synergyDatas.Count == 0)
        {
            EditorGUILayout.HelpBox($"시너지 데이터를 찾을 수 없습니다.\n경로: {SynergyDataFolder}", MessageType.Warning);
            return;
        }

        EditorGUILayout.HelpBox(
            "장수를 맞춘 뒤 Apply하면 디버그 카드가 필드에 들어갑니다. Play는 Intro 1회 + Loop N회로 시너지 연출을 재생합니다.",
            MessageType.Info);

        for (int i = 0; i < synergyDatas.Count; i++)
        {
            DrawSynergyCountRow(synergyDatas[i]);
        }

        DrawSynergyActivationPreview();

        bool canApply = CanUseBattleTools() && GameData.Instance != null;
        BattleManager battleManager = CanUseBattleTools() ? FindSceneObject<BattleManager>() : null;
        bool canPlay = canApply && battleManager != null && !battleManager.IsProcessingAttack();

        EditorGUI.BeginDisabledGroup(!canApply);
        using (new EditorGUILayout.HorizontalScope())
        {
            if (GUILayout.Button("Apply Synergies", GUILayout.Height(24f)))
            {
                ApplyDebugSynergies();
            }

            if (GUILayout.Button("Clear Synergy", GUILayout.Height(24f)))
            {
                ClearDebugSynergies();
            }
        }
        EditorGUI.EndDisabledGroup();

        EditorGUI.BeginDisabledGroup(!canPlay);
        if (GUILayout.Button("Apply & Play Synergy Motion", GUILayout.Height(28f)))
        {
            ApplyDebugSynergies();
            PlayDebugSynergyMotion(battleManager);
        }
        EditorGUI.EndDisabledGroup();

        if (!CanUseBattleTools())
        {
            EditorGUILayout.HelpBox("Game 씬 플레이 모드에서만 시너지를 적용/재생할 수 있습니다.", MessageType.None);
        }
    }

    private void DrawSynergyCountRow(SynergyData synergyData)
    {
        if (synergyData == null || string.IsNullOrEmpty(synergyData.ID))
        {
            return;
        }

        if (!debugSynergyCounts.TryGetValue(synergyData.ID, out int count))
        {
            count = 0;
        }

        int requiredCount = SynergyActivationSelector.GetMinimumActivationCount(synergyData);
        bool willActivate = SynergyActivationSelector.IsActivated(new SynergyTotalData
        {
            synergyData = synergyData,
            count = count
        });

        using (new EditorGUILayout.HorizontalScope())
        {
            string label = string.IsNullOrEmpty(synergyData.Name)
                ? synergyData.ID
                : $"{synergyData.Name} ({synergyData.ID})";
            count = EditorGUILayout.IntSlider(label, count, 0, DebugSynergyMaxCount);
            string stateLabel = willActivate ? "발동" : (requiredCount > 0 ? $"필요 {requiredCount}" : "불가");
            GUILayout.Label(stateLabel, GUILayout.Width(64f));
        }

        debugSynergyCounts[synergyData.ID] = count;
    }

    private void DrawSynergyActivationPreview()
    {
        List<SynergyTotalData> previewEntries = new List<SynergyTotalData>();
        for (int i = 0; i < synergyDatas.Count; i++)
        {
            SynergyData synergyData = synergyDatas[i];
            if (synergyData == null || !debugSynergyCounts.TryGetValue(synergyData.ID, out int count) || count <= 0)
            {
                continue;
            }

            previewEntries.Add(new SynergyTotalData
            {
                synergyData = synergyData,
                count = count
            });
        }

        List<SynergyTotalData> selected = SynergyActivationSelector.Select(previewEntries);
        if (selected.Count == 0)
        {
            EditorGUILayout.LabelField("발동 예정", "없음");
            return;
        }

        string names = string.Join(" → ", selected.Select(entry => entry.synergyData.Name));
        EditorGUILayout.LabelField(
            "발동 예정",
            $"{names}  /  Loop {SynergyActivationSelector.GetLoopPlayCount(selected.Count)}회");
    }

    private void SetDebugPreset(params (string id, int count)[] presets)
    {
        List<string> keys = new List<string>(debugSynergyCounts.Keys);
        for (int i = 0; i < keys.Count; i++)
        {
            debugSynergyCounts[keys[i]] = 0;
        }

        if (presets != null)
        {
            for (int i = 0; i < presets.Length; i++)
            {
                debugSynergyCounts[presets[i].id] = presets[i].count;
            }
        }

        Repaint();
    }

    private void ApplyDebugSynergies()
    {
        if (!CanUseBattleTools() || GameData.Instance == null)
        {
            SetStatus("Game 씬 플레이 모드에서만 시너지를 적용할 수 있습니다.", MessageType.Warning);
            return;
        }

        BattleSynergyDebugUtility.ApplyCounts(debugSynergyCounts);

        List<SynergyTotalData> selected = SynergyActivationSelector.Select(GameData.Instance.synergyIDList?.Values);
        if (selected.Count == 0)
        {
            SetStatus("디버그 시너지를 적용했습니다. 발동 조건을 충족한 시너지가 없습니다.", MessageType.Warning);
            return;
        }

        string names = string.Join(", ", selected.Select(entry => entry.synergyData.Name));
        SetStatus($"디버그 시너지 적용: {names} (Loop {selected.Count}회)", MessageType.Info);
    }

    private void ClearDebugSynergies()
    {
        if (!CanUseBattleTools() || GameData.Instance == null)
        {
            SetStatus("Game 씬 플레이 모드에서만 디버그 시너지를 지울 수 있습니다.", MessageType.Warning);
            return;
        }

        BattleSynergyDebugUtility.RemoveDebugCards();
        SetStatus("디버그 시너지 카드를 제거했습니다.", MessageType.Info);
    }

    private void PlayDebugSynergyMotion(BattleManager battleManager)
    {
        if (battleManager == null)
        {
            SetStatus("BattleManager가 없습니다.", MessageType.Error);
            return;
        }

        if (battleManager.IsProcessingAttack())
        {
            SetStatus("전투 처리 중에는 시너지 연출을 재생할 수 없습니다.", MessageType.Warning);
            return;
        }

        battleManager.PlaySynergyActivationForDebug();
        SetStatus("시너지 발동 연출을 재생합니다.", MessageType.Info);
    }

    private void DrawPlayerHealthSection()
    {
        EditorGUILayout.LabelField("Player Health", EditorStyles.boldLabel);

        if (!CanUseBattleTools())
        {
            EditorGUILayout.HelpBox("Game 씬 플레이 모드에서 체력을 조절할 수 있습니다.", MessageType.None);
            return;
        }

        Player player = FindSceneObject<Player>();
        if (player == null)
        {
            EditorGUILayout.HelpBox("씬에서 Player를 찾을 수 없습니다.", MessageType.Warning);
            return;
        }

        DrawUnitHealthControls(player, "Player");
    }

    private void DrawMonsterHealthSection()
    {
        EditorGUILayout.LabelField("Monster Health", EditorStyles.boldLabel);

        if (!CanUseBattleTools())
        {
            EditorGUILayout.HelpBox("Game 씬 플레이 모드에서 Encounter 적용 후 몬스터 체력을 조절할 수 있습니다.", MessageType.None);
            return;
        }

        List<Monster> monsters = GetActiveMonsters();
        if (monsters.Count == 0)
        {
            EditorGUILayout.HelpBox("활성 몬스터가 없습니다. Encounter를 Apply 해주세요.", MessageType.Info);
            return;
        }

        for (int i = 0; i < monsters.Count; i++)
        {
            Monster monster = monsters[i];
            if (monster == null)
            {
                continue;
            }

            EditorGUILayout.Space(4f);
            DrawUnitHealthControls(monster, $"{i + 1}. {monster.name}");
        }
    }

    private void DrawDebugVictorySection()
    {
        EditorGUILayout.LabelField("Debug Victory", EditorStyles.boldLabel);

        if (!CanUseBattleTools())
        {
            EditorGUILayout.HelpBox("Game 씬 플레이 모드에서만 디버그 승리를 실행할 수 있습니다.", MessageType.None);
            return;
        }

        BattleManager battleManager = FindSceneObject<BattleManager>();
        if (battleManager == null)
        {
            EditorGUILayout.HelpBox("씬에서 BattleManager를 찾을 수 없습니다.", MessageType.Warning);
            return;
        }

        EditorGUI.BeginDisabledGroup(battleManager.IsProcessingAttack());
        if (GUILayout.Button("Kill All & Show Result", GUILayout.Height(28f)))
        {
            ForceDebugVictory(battleManager);
        }
        EditorGUI.EndDisabledGroup();

        EditorGUILayout.HelpBox(
            "필드 몬스터를 전부 죽인 뒤 기존 승리 판정(HandleVictory)으로 결과창을 엽니다. 인카운터 Gold/Exp가 지급됩니다.",
            MessageType.Info);
    }

    private void ForceDebugVictory(BattleManager battleManager)
    {
        if (battleManager == null)
        {
            SetStatus("BattleManager가 없습니다.", MessageType.Error);
            return;
        }

        if (battleManager.IsProcessingAttack())
        {
            SetStatus("전투 처리 중에는 디버그 승리를 실행할 수 없습니다.", MessageType.Warning);
            return;
        }

        battleManager.ForceVictoryForDebug();
        SetStatus("디버그 승리: 몬스터 처치 후 결과창을 엽니다.", MessageType.Info);
    }

    private static void DrawUnitHealthControls(BaseUnit unit, string label)
    {
        EditorGUILayout.LabelField(label, EditorStyles.miniBoldLabel);
        EditorGUI.indentLevel++;

        int maxHealth = unit.MaxHealth;
        int currentHealth = unit.CurrentHealth;

        EditorGUI.BeginChangeCheck();
        maxHealth = EditorGUILayout.IntSlider("Max HP", maxHealth, 1, 9999);
        currentHealth = EditorGUILayout.IntSlider("Current HP", currentHealth, 0, maxHealth);
        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(unit, "Adjust Unit Health");
            unit.SetMaxHealth(maxHealth);
            unit.SetCurrentHealth(currentHealth);
            EditorUtility.SetDirty(unit);
        }

        using (new EditorGUILayout.HorizontalScope())
        {
            if (GUILayout.Button("Refill HP"))
            {
                Undo.RecordObject(unit, "Refill Unit Health");
                unit.SetCurrentHealth(unit.MaxHealth);
                EditorUtility.SetDirty(unit);
            }

            if (GUILayout.Button("Set Max = Current"))
            {
                Undo.RecordObject(unit, "Set Max Health To Current");
                unit.SetMaxHealth(unit.CurrentHealth, adjustCurrentToMax: true);
                EditorUtility.SetDirty(unit);
            }
        }

        EditorGUI.indentLevel--;
    }

    private void ApplySelectedEncounter()
    {
        if (!IsGameSceneActive())
        {
            SetStatus("Game 씬에서만 Encounter를 적용할 수 있습니다.", MessageType.Warning);
            return;
        }

        if (!Application.isPlaying)
        {
            SetStatus("플레이 모드에서만 Encounter를 적용할 수 있습니다.", MessageType.Warning);
            return;
        }

        if (encounterDatas.Count == 0)
        {
            SetStatus("Encounter 데이터가 없습니다.", MessageType.Error);
            return;
        }

        MonsterEncounterData encounterData = encounterDatas[Mathf.Clamp(selectedEncounterIndex, 0, encounterDatas.Count - 1)];
        if (!TryResolveSpawnReferences(out Transform monsterSpawnRoot, out MonsterPrefabRegistry prefabRegistry))
        {
            SetStatus("몬스터 스폰 참조를 찾을 수 없습니다. BattleManagerInjector와 [Global]을 확인해주세요.", MessageType.Error);
            return;
        }

        BattleManager battleManager = FindSceneObject<BattleManager>();
        List<Monster> spawnedMonsters = BattleEncounterDebugUtility.ApplyEncounter(
            encounterData,
            monsterSpawnRoot,
            prefabRegistry,
            battleManager);

        if (spawnedMonsters.Count == 0)
        {
            SetStatus($"Encounter {encounterData.Id} 적용 실패.", MessageType.Error);
            return;
        }

        SetStatus($"Encounter {encounterData.Id} ({encounterData.Name}) 적용 완료.", MessageType.Info);
    }

    private void ReloadEncounterDatas()
    {
        encounterDatas.Clear();
        monsterDataCache.Clear();

        string[] guids = AssetDatabase.FindAssets("t:MonsterEncounterData", new[] { EncounterDataFolder });
        for (int i = 0; i < guids.Length; i++)
        {
            string path = AssetDatabase.GUIDToAssetPath(guids[i]);
            MonsterEncounterData data = AssetDatabase.LoadAssetAtPath<MonsterEncounterData>(path);
            if (data != null)
            {
                encounterDatas.Add(data);
            }
        }

        encounterDatas.Sort((left, right) => string.CompareOrdinal(left.Id, right.Id));
        selectedEncounterIndex = Mathf.Clamp(selectedEncounterIndex, 0, Mathf.Max(0, encounterDatas.Count - 1));
        Repaint();
    }

    private void ReloadSynergyDatas()
    {
        synergyDatas.Clear();

        string[] guids = AssetDatabase.FindAssets("t:SynergyData", new[] { SynergyDataFolder });
        for (int i = 0; i < guids.Length; i++)
        {
            string path = AssetDatabase.GUIDToAssetPath(guids[i]);
            SynergyData data = AssetDatabase.LoadAssetAtPath<SynergyData>(path);
            if (data == null || string.IsNullOrEmpty(data.ID))
            {
                continue;
            }

            synergyDatas.Add(data);
            if (!debugSynergyCounts.ContainsKey(data.ID))
            {
                debugSynergyCounts[data.ID] = 0;
            }
        }

        synergyDatas.Sort((left, right) => string.CompareOrdinal(left.ID, right.ID));
        Repaint();
    }

    private MonsterData LoadMonsterData(string monsterId)
    {
        if (string.IsNullOrEmpty(monsterId))
        {
            return null;
        }

        if (monsterDataCache.TryGetValue(monsterId, out MonsterData cachedData))
        {
            return cachedData;
        }

        string path = $"{MonsterDataFolder}/{monsterId}.asset";
        MonsterData data = AssetDatabase.LoadAssetAtPath<MonsterData>(path);
        monsterDataCache[monsterId] = data;
        return data;
    }

    private bool TryResolveSpawnReferences(out Transform monsterSpawnRoot, out MonsterPrefabRegistry prefabRegistry)
    {
        monsterSpawnRoot = null;
        prefabRegistry = null;

        BattleManagerInjector injector = FindSceneObject<BattleManagerInjector>();
        if (injector != null)
        {
            SerializedObject serializedInjector = new SerializedObject(injector);
            monsterSpawnRoot = serializedInjector.FindProperty("globalRoot").objectReferenceValue as Transform;
            prefabRegistry = serializedInjector.FindProperty("monsterPrefabRegistry").objectReferenceValue as MonsterPrefabRegistry;
        }

        if (monsterSpawnRoot == null)
        {
            GameObject globalObject = GameObject.Find("[Global]");
            if (globalObject != null)
            {
                monsterSpawnRoot = globalObject.transform;
            }
        }

        if (prefabRegistry == null)
        {
            prefabRegistry = AssetDatabase.LoadAssetAtPath<MonsterPrefabRegistry>(DefaultRegistryPath);
        }

        return monsterSpawnRoot != null && prefabRegistry != null;
    }

    private static List<Monster> GetActiveMonsters()
    {
        BattleManager battleManager = FindSceneObject<BattleManager>();
        if (battleManager != null)
        {
            BattleSetupController setupController = battleManager.GetSetupController();
            if (setupController != null)
            {
                List<Monster> registeredMonsters = setupController.GetPrimaryMonsters()
                    .Where(monster => monster != null)
                    .ToList();
                if (registeredMonsters.Count > 0)
                {
                    return registeredMonsters;
                }
            }
        }

        return FindObjectsByType<Monster>(FindObjectsInactive.Exclude, FindObjectsSortMode.None)
            .Where(monster => monster != null && monster.IsAlive)
            .ToList();
    }

    private static T FindSceneObject<T>() where T : Object
    {
        return Object.FindAnyObjectByType<T>(FindObjectsInactive.Exclude);
    }

    private void SetStatus(string message, MessageType messageType)
    {
        statusMessage = message;
        statusType = messageType;
        Repaint();
    }
}
#endif
