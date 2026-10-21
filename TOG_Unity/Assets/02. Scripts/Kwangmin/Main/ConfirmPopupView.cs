using JxModule;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class ConfirmPopupView : MainPanelView
{
    [SerializeField] private TMP_Text _messageLabel;
    [SerializeField] private ButtonView _confirmButton;
    [SerializeField] private ButtonView _cancelButton;

    public void Bind(UnityAction onConfirm, UnityAction onCancel)
    {
        ReleaseEvents();
        _confirmButton.AddListener(onConfirm);
        _cancelButton.AddListener(onCancel);
    }

    public void SetMessage(string message)
    {
        _messageLabel.text = message;
    }

    private void ReleaseEvents()
    {
        _confirmButton.RemoveAllListeners();
        _cancelButton.RemoveAllListeners();
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        ReleaseEvents();
    }
}
