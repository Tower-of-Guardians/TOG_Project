using System;
using System.Collections.Generic;
using JxModule;
using UnityEngine;
using UnityEngine.Events;

public class SlotSelectView : MainPanelView
{
    [SerializeField] private SlotCardView[] _cards;
    [SerializeField] private ButtonView _backButton;

    private Action<int> _onStart;
    private Action<int> _onDelete;

    public void Bind(Action<int> onStart, Action<int> onDelete, UnityAction onBack)
    {
        _onStart = onStart;
        _onDelete = onDelete;
        _backButton.RemoveAllListeners();
        _backButton.AddListener(onBack);
    }

    public void Refresh(IReadOnlyList<SlotPreview> slots)
    {
        int count = Mathf.Min(_cards.Length, slots.Count);
        for (int i = 0; i < count; i++)
        {
            _cards[i].Bind(slots[i], _onStart, _onDelete);
        }
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        _backButton.RemoveAllListeners();
    }
}
