using System;
using System.Collections.Generic;
using Jongmin;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class RandomEventCardPanel : MonoBehaviour
{
    [SerializeField] private GameObject panelRoot;
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text emptyText;
    [SerializeField] private Transform cardRoot;
    [SerializeField] private CompactInvenSlot slotPrefab;
    [SerializeField] private Button confirmButton;
    [SerializeField] private Button refuseButton;

    private readonly List<CompactInvenSlot> slots = new();
    private CompactInvenSlot selectedSlot;
    private Action<CardData> onConfirm;
    private Action onRefuse;

    public bool IsOpen => panelRoot != null && panelRoot.activeSelf;

    private void Awake()
    {
        confirmButton.onClick.AddListener(Confirm);
        refuseButton.onClick.AddListener(Refuse);
    }

    public void Open(string title, IReadOnlyList<CardData> cards, bool canRefuse, Action<CardData> confirmed, Action refused)
    {
        ClearSlots();
        onConfirm = confirmed;
        onRefuse = refused;
        panelRoot.SetActive(true);

        titleText.text = title;
        emptyText.gameObject.SetActive(cards.Count == 0);
        refuseButton.gameObject.SetActive(canRefuse);
        confirmButton.interactable = false;

        foreach (CardData card in cards)
        {
            CreateSlot(card);
        }
    }

    public void Close()
    {
        ClearSlots();
        onConfirm = null;
        onRefuse = null;
        panelRoot.SetActive(false);
    }

    private void CreateSlot(CardData card)
    {
        CompactInvenSlot slot = Instantiate(slotPrefab, cardRoot);
        slot.Card.SetCardData(card);
        slot.SetSlotBehaviour(new SlotBehaviour(Select));
        slots.Add(slot);
    }

    private void Select(CompactInvenSlot slot)
    {
        if (selectedSlot != null)
        {
            selectedSlot.SetSelected(false);
        }

        selectedSlot = slot;
        selectedSlot.SetSelected(true);
        confirmButton.interactable = true;
    }

    private void Confirm()
    {
        if (selectedSlot == null)
        {
            return;
        }

        CardData card = selectedSlot.Card.CardData;
        Action<CardData> callback = onConfirm;
        Close();
        callback?.Invoke(card);
    }

    private void Refuse()
    {
        Action callback = onRefuse;
        Close();
        callback?.Invoke();
    }

    private void ClearSlots()
    {
        foreach (CompactInvenSlot slot in slots)
        {
            if (slot == null)
            {
                continue;
            }

            slot.gameObject.SetActive(false);
            Destroy(slot.gameObject);
        }

        slots.Clear();
        selectedSlot = null;
    }

    private void OnDestroy()
    {
        if (confirmButton != null) confirmButton.onClick.RemoveListener(Confirm);
        if (refuseButton != null) refuseButton.onClick.RemoveListener(Refuse);
    }

    private sealed class SlotBehaviour : CompactInvenSlotBehaviourBase
    {
        private readonly Action<CompactInvenSlot> onClick;

        public SlotBehaviour(Action<CompactInvenSlot> onClick)
        {
            this.onClick = onClick;
        }

        public override void OnPointerClick(CompactInvenSlot invenSlot)
        {
            onClick?.Invoke(invenSlot);
        }
    }
}
