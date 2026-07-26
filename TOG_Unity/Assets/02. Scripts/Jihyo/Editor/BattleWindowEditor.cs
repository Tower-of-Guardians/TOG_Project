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
    private const string DefaultRegistryPath = "Assets/02. Scripts/Jihyo/Battle/MonsterPrefabRegistry.asset";

    private readonly List<MonsterEncounterData> encounterDatas = new List<MonsterEncounterData>();
    private readonly Dictionary<string, MonsterData> monsterDataCache = new Dictionary<string, MonsterData>();

    private Vector2 scrollPosition;
    private int selectedEncounterIndex;
    private string statusMessage = string.Empty;
    private MessageType statusType = MessageType.Info;

    [MenuItem("\u2764 IngameSetTools/Battle")]
    public static void OpenWindow()
    {
        BattleWindowEditor window = GetWindow<BattleWindowEditor>();
        window.titleContent = new GUIContent("IngameSetTools - Battle");
        window.minSize = new Vector2(420f, 560f);
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
        DrawPlayerHealthSection();
        EditorGUILayout.Space(8f);
        DrawMonsterHealthSection();

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
            "Game 씬 플레이 모드에서 Encounter ID로 몬스터를 세팅하고, 플레이어/몬스터 체력을 실시간으로 조절합니다.",
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
