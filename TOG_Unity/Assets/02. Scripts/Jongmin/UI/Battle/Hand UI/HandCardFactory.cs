using System;
using UnityEngine;

public class HandCardFactory : MonoBehaviour, ICardFactory<IHandCardUI>
{
    [Header("Object References")]
    [SerializeField] private Transform _slotRoot;
    [SerializeField] private GameObject _cardPrefab;

    private HandCardEventController _handCardEvent;

    public void Construct(HandCardEventController handCardEvent)
        => _handCardEvent = handCardEvent;

    /// <summary>
    /// 핸드 카드를 오브젝트 풀로부터 꺼내어 초기화합니다.
    /// 이벤트 리스너에 자동으로 등록됩니다.
    /// </summary>
    public IHandCardUI Create()
    {
        GameObject cardObject = ObjectPoolManager.Instance.Get(_cardPrefab);
        cardObject.transform.SetParent(_slotRoot, false);
        cardObject.transform.localScale = Vector3.one;

        IHandCardUI cardUI = cardObject.GetComponent<IHandCardUI>();
        _handCardEvent.Subscribe(cardUI);

        return cardUI;
    }

    /// <summary>
    /// 핸드 카드를 오브젝트 풀에 반환합니다.
    /// 이벤트 리스너로부터 자동으로 해제됩니다.
    /// </summary>
    public void Release(IHandCardUI cardUI)
    {
        _handCardEvent.Unsubscribe(cardUI);

        GameObject cardObject = (cardUI as HandCardUI).gameObject;
        ObjectPoolManager.Instance.Return(cardObject);
    }
}
