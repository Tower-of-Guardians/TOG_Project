using UnityEngine;

public class DiscardCardFactory : MonoBehaviour, ICardFactory<IDiscardCardUI>
{
    [Header("의존성 목록")]
    [Header("카드 부모 트랜스폼")]
    [SerializeField] private Transform _cardRoot;

    [Header("교체 카드 프리펩")]
    [SerializeField] private GameObject _discardCardPrefab;

    private DiscardCardEventController _discardCardEvent;

    public void Construct(DiscardCardEventController discardCardEvent)
        => _discardCardEvent = discardCardEvent;

    /// <summary>
    /// 교체 카드를 오브젝트 풀로부터 꺼내어 초기화합니다.
    /// 이벤트 리스너에 자동으로 등록됩니다.
    /// </summary>
    public IDiscardCardUI Create()
    {
        GameObject cardObject = ObjectPoolManager.Instance.Get(_discardCardPrefab);
        cardObject.transform.SetParent(_cardRoot, false);
        cardObject.transform.localScale = Vector3.one;

        IDiscardCardUI cardUI = cardObject.GetComponent<IDiscardCardUI>();
        _discardCardEvent.Subscribe(cardUI);

        return cardUI;
    }

    /// <summary>
    /// 교체 카드를 오브젝트 풀에 반환합니다.
    /// 이벤트 리스너로부터 자동으로 해제됩니다.
    /// </summary>
    public void Release(IDiscardCardUI cardUI)
    {
        _discardCardEvent.Unsubscribe(cardUI);

        GameObject cardObject = (cardUI as DiscardCardUI).gameObject;
        ObjectPoolManager.Instance.Return(cardObject);
    }
}