using System;
using JxModule;
using TMPro;
using UnityEngine;

public class SlotCardView : MonoBehaviour
{
    [SerializeField] private TMP_Text _slotLabel;
    [SerializeField] private GameObject _emptyRoot;
    [SerializeField] private ButtonView _startButton;

    private int _slot;
    private Action<int> _onStart;

    private void Awake()
    {
        _startButton.AddListener(HandleStart);
    }

    public void Bind(SlotPreview preview, Action<int> onStart)
    {
        _slot = preview.Slot;
        _onStart = onStart;
        _slotLabel.text = $"슬롯 {preview.Slot + 1}";
        _emptyRoot.SetActive(preview.IsEmpty);
    }

    private void HandleStart()
    {
        _onStart?.Invoke(_slot);
    }

    private void OnDestroy()
    {
        _startButton.RemoveListener(HandleStart);
    }
}
