using System;
using System.Collections;
using DG.Tweening;
using JxModule;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.UI;

public abstract class EventNpc : MonoBehaviour, 
                                 IPointerEnterHandler, 
                                 IPointerExitHandler, 
                                 IPointerDownHandler, 
                                 IPointerUpHandler, 
                                 IPointerClickHandler
{
    [BigHeader("Configure")]
    [SerializeField] private CanvasGroup npcGroup;
    [SerializeField] private Image npcImage;
    [FormerlySerializedAs("nameplateView")] [SerializeField] private NameplateUI nameplateUI;
    [FormerlySerializedAs("interactionTipView")] [SerializeField] private InteractionTipUI interactionTipUI;
    [SerializeField] private UIOutliner uiOutliner;

    private Tween _fadeTween;
    
    public abstract IEnumerator HandleOnEventBegin();
    public abstract IEnumerator HandleOnEventEnd();

    public event Action OnClickNpc;

    public void SetInteractable(bool isActive)
    {
        npcGroup.interactable = isActive;
        npcGroup.blocksRaycasts = isActive;
    }
    
    public void OnPointerEnter(PointerEventData eventData)
    {
        nameplateUI?.Show();
        interactionTipUI?.Show();
        uiOutliner?.Show();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        nameplateUI?.Hide();
        interactionTipUI?.Hide();
        uiOutliner?.Hide();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        _fadeTween?.Kill();
        _fadeTween = npcImage.DOFade(0.5f, 0.1f);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        _fadeTween?.Kill();
        _fadeTween = npcImage.DOFade(1f, 0.1f);
    }

    public virtual void OnPointerClick(PointerEventData eventData)
    {
        OnClickNpc?.Invoke();
    }
}