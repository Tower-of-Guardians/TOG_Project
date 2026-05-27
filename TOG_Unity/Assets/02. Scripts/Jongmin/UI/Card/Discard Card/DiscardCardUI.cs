using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class DiscardCardUI : CardUI, IDiscardCardUI
{
    [Space(20f), Header("Object References")]
    [SerializeField] private CanvasGroup m_canvas_group;

    public event Action OnBeginDragAction;
    public event Action<Vector2> OnDragAction;
    public event Action OnEndDragAction;

    public void OnBeginDrag(PointerEventData eventData)
        => OnBeginDragAction?.Invoke();

    /// <summary>
    /// 드래그 처리 중 드랍 판정을 방해하지 않도록
    /// 레이캐스트를 잠시 비활성화한 뒤 다시 복구합니다.
    /// </summary>
    public void OnDrag(PointerEventData eventData)
    {
        ToggleRaycast(false);
        OnDragAction?.Invoke(eventData.position);
        ToggleRaycast(true);
    }

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

    private void ToggleRaycast(bool active)
        => m_canvas_group.blocksRaycasts = active;
}
