
using UnityEngine;

public class DeckCardFactory : MonoBehaviour, ICardFactory<IDeckCardUI>
{
    [Header("Object References")]
    [SerializeField] private Transform cardRoot;
    [SerializeField] private GameObject cardPrefab;
    
    /// <summary>
    /// 덱 카드를 오브젝트 풀로부터 꺼내어 초기화합니다.
    /// </summary>
    public IDeckCardUI Create()
    {
        GameObject cardObject = ObjectPoolManager.Instance.Get(cardPrefab);
        cardObject.transform.SetParent(cardRoot, false);
        cardObject.transform.localScale = Vector3.one;
        
        IDeckCardUI cardUI = cardObject.GetComponent<IDeckCardUI>();   
        return cardUI;
    }

    /// <summary>
    /// 덱 카드를 오브젝트 풀에 반환합니다.
    /// </summary>    
    public void Release(IDeckCardUI cardUI)
    {
        GameObject cardObject = (cardUI as DeckCardUI).gameObject;
        if (cardUI == null)
        {
            return;
        }
        
        ObjectPoolManager.Instance.Return(cardObject);
    }
}