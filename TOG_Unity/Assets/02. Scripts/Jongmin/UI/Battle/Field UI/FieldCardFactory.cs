using System;
using UnityEngine;

public class FieldCardFactory : MonoBehaviour, ICardFactory<IFieldCardUI>
{
    [Header("Object References")]
    [SerializeField] private Transform _cardRoot;
    [SerializeField] private GameObject _fieldCardPrefab;

    private FieldCardEventController _fieldCardEvent;

    public void Construct(FieldCardEventController fieldCardEvent)
        => _fieldCardEvent = fieldCardEvent;

    /// <summary>
    /// 핸드 카드를 오브젝트 풀로부터 꺼내어 초기화합니다.
    /// 이벤트 리스너에 자동으로 등록됩니다.
    /// </summary>
    public IFieldCardUI Create()
    {
        var cardObject = ObjectPoolManager.Instance.Get(_fieldCardPrefab);
        cardObject.transform.SetParent(_cardRoot, false);
        cardObject.transform.localScale = Vector3.one;

        var cardUI = cardObject.GetComponent<IFieldCardUI>();
        _fieldCardEvent.Subscribe(cardUI);

        return cardUI;
    }

    /// <summary>
    /// 핸드 카드를 오브젝트 풀에 반환합니다.
    /// 이벤트 리스너로부터 자동으로 해제됩니다.
    /// </summary>
    public void Release(IFieldCardUI cardUI)
    {
        _fieldCardEvent.Unsubscribe(cardUI);

        GameObject cardObject = (cardUI as FieldCardUI).gameObject;
        ObjectPoolManager.Instance.Return(cardObject);
    }
}
