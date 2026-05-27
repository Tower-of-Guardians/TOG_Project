using UnityEngine;

public class ResultCardFactory : MonoBehaviour, ICardFactory<IResultCardUI>
{
    [Header("Object References")]
    [SerializeField] private Transform cardRoot;
    [SerializeField] private GameObject resultCardPrefab;

    public IResultCardUI Create()
    {
        GameObject cardObject = ObjectPoolManager.Instance.Get(resultCardPrefab);
        cardObject.transform.SetParent(cardRoot, false);
        
        return cardObject.GetComponent<IResultCardUI>();
    }

    public void Release(IResultCardUI cardUI)
    {
        GameObject cardObject = (cardUI as ResultCardUI).gameObject;
        ObjectPoolManager.Instance.Return(cardObject);
    }
}