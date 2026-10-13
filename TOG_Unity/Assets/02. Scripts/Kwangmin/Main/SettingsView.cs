using JxModule;
using UnityEngine;
using UnityEngine.Events;

public class SettingsView : MainPanelView
{
    [SerializeField] private ButtonView _closeButton;

    public void Bind(UnityAction onClose)
    {
        _closeButton.RemoveAllListeners();
        _closeButton.AddListener(onClose);
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        _closeButton.RemoveAllListeners();
    }
}
