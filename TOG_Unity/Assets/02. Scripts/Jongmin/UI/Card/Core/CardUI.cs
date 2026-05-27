using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CardUI : MonoBehaviour, ICardUI
{
    [Header("Visual References")]
    [SerializeField] private Image _cardFrameImage;
    [SerializeField] private Image _cardItemImage;

    [Header("Text References")]
    [SerializeField] private TMP_Text _cardNameLabel;
    [SerializeField] private TMP_Text _cardDescriptionLabel;
    [SerializeField] protected TMP_Text _cardATKLabel;
    [SerializeField] protected TMP_Text _cardDEFLabel;

    [Header("Group Object References")]
    [SerializeField] private Image[] _starObjectArray;
    [SerializeField] private Image[] _synergyImageArray;

    const int MaxStarCount = 3;

    /// <summary>
    /// CardData를 이용하여 카드의 비주얼을 업데이트합니다.
    /// </summary>
    public virtual void UpdateUI(CardData cardData)
    {
        if(cardData == null)
        {
            Debug.LogWarning("CardUI: 전달된 cardData가 null입니다.");
            return;
        }

        InitCardInfo(cardData);
        InitCardStars(cardData);
        InitCardSynergies(cardData);
    }

    private void InitCardInfo(CardData cardData)
    {
        _cardFrameImage.sprite = cardData.cardimage;
        _cardItemImage.sprite = cardData.iconimage;

        _cardNameLabel.text = cardData.itemName;
        _cardDescriptionLabel.text = cardData.effectDescription;
        _cardATKLabel.text = cardData.ATK.ToString();
        _cardDEFLabel.text = cardData.DEF.ToString();
    }

    private void InitCardStars(CardData cardData)
    {
        foreach(Image starImage in _starObjectArray)
        {
            starImage.gameObject.SetActive(false);
        }

        int starCount = Mathf.Min(_starObjectArray.Length, Mathf.Min(MaxStarCount, cardData.star));
        for(int i = 0; i < starCount; i++)
        {
            _starObjectArray[i].gameObject.SetActive(true);
        }
    }

    private void InitCardSynergies(CardData cardData)
    {
        foreach(Image synergyImage in _synergyImageArray)
        {
            synergyImage.gameObject.SetActive(false);
        }

        // TODO: 시너지 데이터를 채워야 합니다.
    }
}
