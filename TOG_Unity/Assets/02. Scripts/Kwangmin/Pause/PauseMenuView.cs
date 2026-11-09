using JxModule;
using UnityEngine;
using UnityEngine.Events;

public class PauseMenuView : MainPanelView
{
    [SerializeField] private ButtonView _continueButton;
    [SerializeField] private ButtonView _settingsButton;
    [SerializeField] private ButtonView _giveUpButton;
    [SerializeField] private ButtonView _titleButton;
    [SerializeField] private Color _giveUpColor = new(1f, 0.42f, 0.42f);
    [SerializeField] private Color _disabledColor = new(0.5f, 0.5f, 0.5f);

    public void Bind(UnityAction onContinue, UnityAction onSettings, UnityAction onGiveUp, UnityAction onTitle)
    {
        ReleaseEvents();
        _continueButton.AddListener(onContinue);
        _settingsButton.AddListener(onSettings);
        _giveUpButton.AddListener(onGiveUp);
        _titleButton.AddListener(onTitle);
    }

    public void SetGiveUpInteractable(bool interactable)
    {
        _giveUpButton.Button.interactable = interactable;
        if (_giveUpButton.Label != null)
        {
            _giveUpButton.Label.color = interactable ? _giveUpColor : _disabledColor;
        }
    }

    private void ReleaseEvents()
    {
        _continueButton.RemoveAllListeners();
        _settingsButton.RemoveAllListeners();
        _giveUpButton.RemoveAllListeners();
        _titleButton.RemoveAllListeners();
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        ReleaseEvents();
    }
}
