using System;
using TMPro;
using UnityEngine;

public sealed class AreaEventUI_Random : MonoBehaviour
{
    [SerializeField] private TMP_Text eventName;
    [SerializeField] private UnityEngine.UI.Button nextButton;

    private Action onNext;

    public RandomAreaEventType? CurrentEvent { get; private set; }

    public bool IsUIOpen()
    {
        return isActiveAndEnabled && CurrentEvent.HasValue;
    }

    public void Show(RandomAreaEventType type, Action next)
    {
        CurrentEvent = type;
        eventName.text = RandomAreaEventSelector.GetName(type);
        onNext = next;
        nextButton.onClick.RemoveListener(Next);
        nextButton.onClick.AddListener(Next);
        nextButton.interactable = true;
    }

    private void Next()
    {
        if (!IsUIOpen() || onNext == null) return;
        var callback = onNext;
        onNext = null;
        nextButton.interactable = false;
        callback.Invoke();
    }

    private void OnDestroy()
    {
        if (nextButton != null) nextButton.onClick.RemoveListener(Next);
        onNext = null;
    }
}
