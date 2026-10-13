using System;
using System.Collections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

[DefaultExecutionOrder(-1000)]
[RequireComponent(typeof(CanvasGroup))]
public class MainFlow : MonoBehaviour
{
    private enum EMainState
    {
        Logo,
        Loading,
        Title,
        SlotSelect,
        Settings,
        Entering
    }

    private const float LoadingMinDuration = 0.3f;
    private const float LoadingTimeout = 30f;

    private static readonly Func<bool>[] LoadChecks =
    {
        () => DataCenter.IsCardDataLoaded,
        () => DataCenter.IsResultDataLoaded,
        () => DataCenter.IsMonsterDataLoaded,
        () => DataCenter.IsMonsterEncounterDataLoaded,
        () => DataCenter.IsStatusEffectDataLoaded,
        () => DataCenter.IsSynergyDataLoaded,
        () => DataCenter.IsEffectDataLoaded,
        () => DataCenter.IsAreaEventLoaded,
        () => DataCenter.IsRandomEventLoaded
    };

    [SerializeField] private CanvasGroup _bootBackdrop;
    [SerializeField] private LogoView _logoView;
    [SerializeField] private BootLoadingView _loadingView;
    [SerializeField] private TitleView _titleView;
    [SerializeField] private SlotSelectView _slotSelectView;
    [SerializeField] private SettingsView _settingsView;
    [SerializeField] private LobbySceneView _lobbySceneView;
    [SerializeField] private InputActionReference _cancelAction;
    [SerializeField] private float _backdropFadeDuration = 0.5f;
    [SerializeField] private float _exitFadeDuration = 0.5f;

    private static bool _startInLobby;

    private CanvasGroup _rootGroup;
    private EMainState _state;

    public static bool TryReturnToLobby()
    {
        MainFlow flow = FindFirstObjectByType<MainFlow>(FindObjectsInactive.Include);
        if (flow == null)
        {
            return false;
        }

        _startInLobby = true;
        DIContainer.Clear();
        Scene scene = flow.gameObject.scene;
#if UNITY_EDITOR
        if (scene.buildIndex < 0)
        {
            UnityEditor.SceneManagement.EditorSceneManager.LoadSceneInPlayMode(scene.path, new LoadSceneParameters(LoadSceneMode.Single));
            return true;
        }
#endif
        SceneManager.LoadScene(scene.buildIndex);
        return true;
    }

    private void Awake()
    {
        _rootGroup = GetComponent<CanvasGroup>();
        if (_lobbySceneView == null)
        {
            _lobbySceneView = FindFirstObjectByType<LobbySceneView>(FindObjectsInactive.Include);
        }

        if (_lobbySceneView != null)
        {
            _lobbySceneView.SetLobbyVisible(true);
        }

        _titleView.Bind(OpenSlotSelect, HandleContinue, OpenSettings, QuitGame);
        _slotSelectView.Bind(EnterGame, CloseSlotSelect);
        _settingsView.Bind(CloseSettings);
        _loadingView.Bind(QuitGame);

        _logoView.HideImmediately();
        _loadingView.HideImmediately();
        _titleView.HideImmediately();
        _slotSelectView.HideImmediately();
        _settingsView.HideImmediately();
        _bootBackdrop.gameObject.SetActive(true);
        _bootBackdrop.alpha = 1f;
    }

    private void OnEnable()
    {
        if (_cancelAction == null)
        {
            return;
        }

        _cancelAction.action.performed += HandleCancel;
        if (!_cancelAction.action.enabled)
        {
            _cancelAction.action.Enable();
        }
    }

    private void OnDisable()
    {
        if (_cancelAction != null)
        {
            _cancelAction.action.performed -= HandleCancel;
        }
    }

    private void Start()
    {
        StartCoroutine(BootRoutine());
    }

    private void Update()
    {
        if (_state == EMainState.Logo && IsAnyInputPressed())
        {
            _logoView.Skip();
        }
    }

    private IEnumerator BootRoutine()
    {
        _ = DataCenter.Instance;

        if (_startInLobby)
        {
            _startInLobby = false;
            _state = EMainState.Entering;
            yield return EnterLobbyRoutine();
            yield break;
        }

        _state = EMainState.Logo;
        bool logoDone = false;
        _logoView.Play(() => logoDone = true);
        yield return new WaitUntil(() => logoDone);

        _state = EMainState.Loading;
        _loadingView.ResetState();
        _loadingView.Show();

        float elapsed = 0f;
        while (true)
        {
            float progress = GetLoadProgress();
            _loadingView.SetProgress(progress);

            if (progress >= 1f && elapsed >= LoadingMinDuration)
            {
                break;
            }

            if (elapsed >= LoadingTimeout)
            {
                _loadingView.ShowError();
                yield break;
            }

            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }

        _loadingView.Hide();
        _bootBackdrop.blocksRaycasts = false;
        _bootBackdrop.DOFade(0f, _backdropFadeDuration)
                     .SetUpdate(true)
                     .OnComplete(() => _bootBackdrop.gameObject.SetActive(false));
        OpenTitle();
    }

    private static float GetLoadProgress()
    {
        int loaded = 0;
        foreach (Func<bool> check in LoadChecks)
        {
            if (check())
            {
                loaded++;
            }
        }
        return (float)loaded / LoadChecks.Length;
    }

    private static bool IsAnyInputPressed()
    {
        bool keyPressed = Keyboard.current != null && Keyboard.current.anyKey.wasPressedThisFrame;
        bool mousePressed = Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame;
        return keyPressed || mousePressed;
    }

    private void OpenTitle()
    {
        _state = EMainState.Title;
        _titleView.SetContinueInteractable(SlotPreviewSource.TryGetContinueSlot(out _));
        _titleView.Show();
    }

    private void OpenSlotSelect()
    {
        if (_state != EMainState.Title)
        {
            return;
        }

        _state = EMainState.SlotSelect;
        _titleView.Hide();
        _slotSelectView.Refresh(SlotPreviewSource.GetSlots());
        _slotSelectView.Show();
    }

    private void CloseSlotSelect()
    {
        if (_state != EMainState.SlotSelect)
        {
            return;
        }

        _slotSelectView.Hide();
        OpenTitle();
    }

    private void OpenSettings()
    {
        if (_state != EMainState.Title)
        {
            return;
        }

        _state = EMainState.Settings;
        _titleView.Hide();
        _settingsView.Show();
    }

    private void CloseSettings()
    {
        if (_state != EMainState.Settings)
        {
            return;
        }

        _settingsView.Hide();
        OpenTitle();
    }

    private void HandleContinue()
    {
        if (_state == EMainState.Title && SlotPreviewSource.TryGetContinueSlot(out int slot))
        {
            EnterGame(slot);
        }
    }

    private void HandleCancel(InputAction.CallbackContext context)
    {
        switch (_state)
        {
            case EMainState.SlotSelect:
                CloseSlotSelect();
                break;
            case EMainState.Settings:
                CloseSettings();
                break;
        }
    }

    private void EnterGame(int slot)
    {
        if (_state != EMainState.Title && _state != EMainState.SlotSelect)
        {
            return;
        }

        _state = EMainState.Entering;
        StartCoroutine(EnterLobbyRoutine());
    }

    private IEnumerator EnterLobbyRoutine()
    {
        if (_lobbySceneView != null)
        {
            _lobbySceneView.SetLobbyVisible(true);
        }

        _rootGroup.interactable = false;
        yield return _rootGroup.DOFade(0f, _exitFadeDuration)
                               .SetUpdate(true)
                               .WaitForCompletion();
        gameObject.SetActive(false);
    }

    private void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
