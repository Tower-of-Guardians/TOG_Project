using JxModule;
using UnityEngine;
using UnityEngine.Events;

public class TitleView : MainPanelView
{
    [SerializeField] private ButtonView _startButton;
    [SerializeField] private ButtonView _continueButton;
    [SerializeField] private ButtonView _settingsButton;
    [SerializeField] private ButtonView _quitButton;

    public void Bind(UnityAction onStart, UnityAction onContinue, UnityAction onSettings, UnityAction onQuit)
    {
        ReleaseEvents();
        _startButton.AddListener(onStart);
        _continueButton.AddListener(onContinue);
        _settingsButton.AddListener(onSettings);
        _quitButton.AddListener(onQuit);
    }

    public void SetContinueInteractable(bool interactable)
    {
        _continueButton.Button.interactable = interactable;
    }

    private void ReleaseEvents()
    {
        _startButton.RemoveAllListeners();
        _continueButton.RemoveAllListeners();
        _settingsButton.RemoveAllListeners();
        _quitButton.RemoveAllListeners();
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        ReleaseEvents();
    }
}
