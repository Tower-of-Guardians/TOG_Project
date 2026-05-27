using UnityEngine;
using UnityEngine.EventSystems;
using System;

public class HandCardUI : CardUI, IHandCardUI
{
    [Space(20f), Header("Canvas Group")]
    [SerializeField] private CanvasGroup _canvasGroup;

    public event Action OnPointerEnterAction;
    public event Action OnPointerExitAction;
    public event Action OnBeginDragAction;
    public event Action<Vector2> OnDragAction;
    public event Action OnEndDragAction;
    public event Action OnPointerClickAction;

    public void OnPointerEnter(PointerEventData eventData)
        => OnPointerEnterAction?.Invoke();

    public void OnPointerExit(PointerEventData eventData)
        => OnPointerExitAction?.Invoke();

    public void OnBeginDrag(PointerEventData eventData)
        => OnBeginDragAction?.Invoke();
        
    public void OnDrag(PointerEventData eventData)
        => OnDragAction?.Invoke(eventData.position);

    /// <summary>
    /// 드래그 종료 처리 중 드랍 판정을 방해하지 않도록
    /// 레이캐스트를 잠시 비활성화한 뒤 다시 복구합니다.
    /// </summary>
    public void OnEndDrag(PointerEventData eventData)
    {
        ToggleRaycast(false);
        OnEndDragAction?.Invoke();
        ToggleRaycast(true);
    }

    /// <summary>
    /// 마우스 우클릭 시에만 반응합니다.
    /// </summary>
    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Right)
        {
            OnPointerClickAction?.Invoke();
        }
    }

    private void ToggleRaycast(bool isActive)
        => _canvasGroup.blocksRaycasts = isActive;
}
