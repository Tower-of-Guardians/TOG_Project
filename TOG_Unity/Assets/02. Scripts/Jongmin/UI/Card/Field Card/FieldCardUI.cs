using System;
using UnityEngine.EventSystems;
using UnityEngine;

public class FieldCardUI : CardUI, IFieldCardUI
{
    [Space(20f), Header("Object References")]
    [SerializeField] private CanvasGroup _canvasGroup;
    [SerializeField] private GameObject _atkLockImage;
    [SerializeField] private GameObject _defLockImage;

    public event Action OnBeginDragAction;
    public event Action<Vector2> OnDragAction;
    public event Action OnEndDragAction;

    /// <summary>
    /// CardData를 이용하여 카드의 비주얼을 업데이트하고, 능력치 잠금을 설정합니다.
    /// </summary>
    public void UpdateUI(CardData cardData, bool isAtk)
    {
        UpdateUI(cardData);

        _atkLockImage.SetActive(!isAtk);
        _defLockImage.SetActive(isAtk);
    }

    /// <summary>
    /// 각 공격력/방어력 능력치 잠금 표시를 토글합니다.
    /// </summary>
    public void ToggleLock()
    {
        _atkLockImage.SetActive(!_atkLockImage.activeInHierarchy);
        _defLockImage.SetActive(!_defLockImage.activeInHierarchy);
    }

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
        => _canvasGroup.blocksRaycasts = active;
}