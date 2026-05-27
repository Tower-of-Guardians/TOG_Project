using UnityEngine;

public class DeckInvenFactory : MonoBehaviour, ICardFactory<IDeckInvenCardUI>
{
    [Header("Object References")]
    [SerializeField] private Transform cardRoot;

    [SerializeField] private GameObject deckInvenCardPrefab;
    
    public IDeckInvenCardUI Create()
    {
        GameObject cardObject = ObjectPoolManager.Instance.Get(deckInvenCardPrefab);
        cardObject.transform.SetParent(cardRoot, false);
        cardObject.transform.localScale = Vector3.one;
        
        return cardObject.GetComponent<IDeckInvenCardUI>();
    }

    public void Release(IDeckInvenCardUI cardUI)
    {
        GameObject cardObject = (cardUI as DeckInvenCardUI).gameObject;
        ObjectPoolManager.Instance.Return(cardObject);
    }
}
