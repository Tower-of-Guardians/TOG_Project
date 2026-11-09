using Jongmin;
using JxDialogueBox;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class PauseMenuController : MonoBehaviour
{
    private enum EPauseState
    {
        Closed,
        Menu,
        Settings,
        GiveUpConfirm,
        TitleConfirm,
        Leaving
    }

    private const string GiveUpMessage = "정말 포기하시겠습니까?\n포기한 런은 사망과 같이 처리됩니다.";
    private const string TitleMessage = "정말 타이틀로 돌아가시겠습니까?";

    [SerializeField] private Button[] _gearButtons;
    [SerializeField] private Color _gearDisabledColor = new(0.5f, 0.5f, 0.5f, 1f);
    [SerializeField] private MainPanelView _overlayView;
    [SerializeField] private PauseMenuView _menuView;
    [SerializeField] private SettingsView _settingsView;
    [SerializeField] private ConfirmPopupView _giveUpConfirmView;
    [SerializeField] private ConfirmPopupView _titleConfirmView;
    [SerializeField] private InputActionReference _cancelAction;
    [SerializeField] private LobbySceneView _lobbySceneView;
    [SerializeField] private DialogueRunner[] _dialogueRunners;

    private EPauseState _state;
    private Color[] _gearColors;
    private bool _gearInteractable = true;

    private bool CanOpen => _state == EPauseState.Closed && !PauseMenu.IsBlocked && !IsDialogueRunning();
    private bool CanGiveUp => RunSaveService.HasActiveRun && (_lobbySceneView == null || !_lobbySceneView.IsLobbyVisible);

    private void Awake()
    {
        PauseMenu.Register(this);

        if (_lobbySceneView == null)
        {
            _lobbySceneView = FindFirstObjectByType<LobbySceneView>(FindObjectsInactive.Include);
        }

        if (_dialogueRunners == null || _dialogueRunners.Length == 0)
        {
            _dialogueRunners = FindObjectsByType<DialogueRunner>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        }

        _gearColors = new Color[_gearButtons.Length];
        for (var i = 0; i < _gearButtons.Length; i++)
        {
            _gearColors[i] = _gearButtons[i].image.color;
            _gearButtons[i].onClick.AddListener(HandleGearClicked);
        }

        _menuView.Bind(Close, OpenSettings, RequestGiveUp, RequestTitle);
        _settingsView.Bind(CloseSettings);
        _giveUpConfirmView.Bind(ConfirmGiveUp, CancelConfirm);
        _titleConfirmView.Bind(ConfirmTitle, CancelConfirm);
        _giveUpConfirmView.SetMessage(GiveUpMessage);
        _titleConfirmView.SetMessage(TitleMessage);

        _overlayView.HideImmediately();
        _menuView.HideImmediately();
        _settingsView.HideImmediately();
        _giveUpConfirmView.HideImmediately();
        _titleConfirmView.HideImmediately();
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

    private void OnDestroy()
    {
        foreach (var gear in _gearButtons)
        {
            if (gear != null)
            {
                gear.onClick.RemoveListener(HandleGearClicked);
            }
        }

        PauseMenu.Unregister(this);
    }

    private void Update()
    {
        var interactable = _state != EPauseState.Closed || CanOpen;
        if (_gearInteractable == interactable)
        {
            return;
        }

        _gearInteractable = interactable;
        for (var i = 0; i < _gearButtons.Length; i++)
        {
            _gearButtons[i].interactable = interactable;
            _gearButtons[i].image.color = interactable ? _gearColors[i] : _gearDisabledColor;
        }
    }

    public bool Open()
    {
        if (!CanOpen)
        {
            return false;
        }

        _state = EPauseState.Menu;
        _menuView.SetGiveUpInteractable(CanGiveUp);
        _overlayView.Show();
        _menuView.Show();
        PauseMenu.SetPaused(true);
        return true;
    }

    public void Close()
    {
        if (_state == EPauseState.Closed || _state == EPauseState.Leaving)
        {
            return;
        }

        _state = EPauseState.Closed;
        HideIfOpen(_menuView);
        HideIfOpen(_settingsView);
        HideIfOpen(_giveUpConfirmView);
        HideIfOpen(_titleConfirmView);
        HideIfOpen(_overlayView);
        PauseMenu.SetPaused(false);
    }

    private void HandleGearClicked()
    {
        Open();
    }

    private void HandleCancel(InputAction.CallbackContext context)
    {
        switch (_state)
        {
            case EPauseState.Closed:
                Open();
                break;
            case EPauseState.Menu:
                Close();
                break;
            case EPauseState.Settings:
                CloseSettings();
                break;
        }
    }

    private void OpenSettings()
    {
        if (_state != EPauseState.Menu)
        {
            return;
        }

        _state = EPauseState.Settings;
        _menuView.Hide();
        _settingsView.Show();
    }

    private void CloseSettings()
    {
        if (_state != EPauseState.Settings)
        {
            return;
        }

        _state = EPauseState.Menu;
        _settingsView.Hide();
        _menuView.Show();
    }

    private void RequestGiveUp()
    {
        if (_state != EPauseState.Menu || !CanGiveUp)
        {
            return;
        }

        _state = EPauseState.GiveUpConfirm;
        _giveUpConfirmView.Show();
    }

    private void ConfirmGiveUp()
    {
        if (_state != EPauseState.GiveUpConfirm)
        {
            return;
        }

        _state = EPauseState.Leaving;
        RunSaveService.EndRun(ERunEndReason.Abandon);
        PauseMenu.RaiseGiveUp();
        MainFlow.TryReturnToLobby();
    }

    private void RequestTitle()
    {
        if (_state != EPauseState.Menu)
        {
            return;
        }

        _state = EPauseState.TitleConfirm;
        _titleConfirmView.Show();
    }

    private void ConfirmTitle()
    {
        if (_state != EPauseState.TitleConfirm)
        {
            return;
        }

        _state = EPauseState.Leaving;
        MainFlow.TryReturnToTitle();
    }

    private void CancelConfirm()
    {
        if (_state != EPauseState.GiveUpConfirm && _state != EPauseState.TitleConfirm)
        {
            return;
        }

        HideIfOpen(_giveUpConfirmView);
        HideIfOpen(_titleConfirmView);
        _state = EPauseState.Menu;
    }

    private bool IsDialogueRunning()
    {
        foreach (var runner in _dialogueRunners)
        {
            if (runner != null && runner.IsRunning)
            {
                return true;
            }
        }
        return false;
    }

    private static void HideIfOpen(MainPanelView view)
    {
        if (view.IsOpen)
        {
            view.Hide();
        }
    }
}
