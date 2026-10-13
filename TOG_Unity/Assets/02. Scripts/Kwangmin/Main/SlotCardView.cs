using System;
using JxModule;
using TMPro;
using UnityEngine;

public class SlotCardView : MonoBehaviour
{
    [SerializeField] private TMP_Text _slotLabel;
    [SerializeField] private GameObject _emptyRoot;
    [SerializeField] private ButtonView _startButton;
    [SerializeField] private GameObject _detailsRoot;
    [SerializeField] private TMP_Text _locationLabel;
    [SerializeField] private TMP_Text _relicCountLabel;
    [SerializeField] private TMP_Text _runCountLabel;
    [SerializeField] private TMP_Text _savedAtLabel;
    [SerializeField] private TMP_Text _playTimeLabel;

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
        _detailsRoot.SetActive(!preview.IsEmpty);
        if (preview.IsEmpty)
        {
            return;
        }
        _locationLabel.text = preview.Location;
        _relicCountLabel.text = $"{preview.RelicCount} 개";
        _runCountLabel.text = preview.RunCount.ToString();
        _savedAtLabel.text = preview.SavedAtText;
        _playTimeLabel.text = preview.PlayTimeText;
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
