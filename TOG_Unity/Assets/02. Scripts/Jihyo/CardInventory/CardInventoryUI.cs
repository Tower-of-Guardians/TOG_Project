using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class CardInventoryUI : MonoBehaviour
{    
    [Header("Card Info UI")]
    [SerializeField] private CardInfoUI cardInfoUI;

    [Header("Card UI")]
    [SerializeField] private GameObject cardInventoryContent;
    [SerializeField] private GameObject cardPrefab;
    [SerializeField] private List<GameObject> instantiatedCards = new List<GameObject>();

    [Header("캔버스 그룹")]
    [SerializeField] private CanvasGroup canvasGroup;

    void Start()
        => InitializeCardInventoryUI();

    private void InitializeCardInventoryUI()
    {
        if (!cardInfoUI)
            cardInfoUI = FindAnyObjectByType<CardInfoUI>();
    }

    // 패널 열기
    public void OpenPanel()
    {
        ToggleCanvasGroup(true);
        RefreshCardInventory();
    }

    /// 인벤토리 열 때마다 카드 목록을 새로고침
    public void RefreshCardInventory()
    {
        if (cardInventoryContent == null)
        {
            Debug.LogWarning("CardInventoryUI: cardInventoryContent가 설정되지 않았습니다.");
            return;
        }

        if (cardPrefab == null)
        {
            Debug.LogWarning("CardInventoryUI: cardPrefab이 설정되지 않았습니다.");
            return;
        }

        if (DataCenter.Instance == null || !DataCenter.IsCardDataLoaded)
        {
            Debug.LogWarning("CardInventoryUI: DataCenter가 아직 데이터를 로드하지 않았습니다.");
            return;
        }

        ClearCards();

        // 덱에서 카드 가져오기
        Dictionary<string, CardData> displayCard = GetUserCards();

        foreach (var kvp in displayCard)
        {
            string cardId = kvp.Key;
            CardData cardData = kvp.Value;

            if (cardData == null)
            {
                continue;
            }

            GameObject cardObject = ObjectPoolManager.Instance.Get(cardPrefab);
            cardObject.transform.SetParent(cardInventoryContent.transform, false);
            instantiatedCards.Add(cardObject);

            SetupCardData(cardObject, cardData, cardId);

            SetupCardClickHandler(cardObject);
        }
    }

    /// 사용자가 보유한 카드 목록 가져오기
    private Dictionary<string, CardData> GetUserCards()
    {
        Dictionary<string, CardData> cards = new Dictionary<string, CardData>();

        if (DataCenter.Instance == null || DataCenter.Instance.userDeck == null)
        {
            return cards;
        }

        foreach (CardData data in DataCenter.Instance.userDeck)
        {
            cards[data.id] = data;
        }

        return cards;
    }

    /// 카드 오브젝트에 데이터 설정
    private void SetupCardData(GameObject cardObject, CardData cardData, string cardId)
    {
        if (cardObject == null || cardData == null)
        {
            return;
        }

        // 카드 오브젝트 이름에 카드 ID 저장
        cardObject.name = $"Card_{cardId}";

        InventoryCard inventoryCard = cardObject.GetComponent<InventoryCard>();
        if (inventoryCard == null)
        {
            inventoryCard = cardObject.AddComponent<InventoryCard>();
        }

        inventoryCard.InitUI(cardData);
    }

    /// 카드 클릭 핸들러 설정
    private void SetupCardClickHandler(GameObject cardObject)
    {
        if (cardObject == null)
        {
            return;
        }

        Button button = cardObject.GetComponent<Button>();
        if (button == null)
        {
            button = cardObject.AddComponent<Button>();
        }

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() =>
        {
            CardData cardData = GetClickedCardData(cardObject);
            OnCardClicked(cardData);
        });
    }

    private void ClearCards()
    {
        foreach (GameObject card in instantiatedCards)
            ObjectPoolManager.Instance.Return(card);

        instantiatedCards.Clear();
    }

    // 패널 닫기
    public void ClosePanel()
    {
        if (cardInfoUI != null)
        {
            cardInfoUI.HidePanel();
        }

        ToggleCanvasGroup(false);
    }

    // 카드 클릭 시 호출되는 메서드
    public void OnCardClicked(CardData cardData)
    {
        if (cardData == null)
        {
            return;
        }

        // CardInfoUI에 카드 정보 표시 요청
        if (cardInfoUI != null)
        {
            cardInfoUI.ShowCardInfo(cardData);
        }
        else
        {
            Debug.LogWarning("CardInventoryUI: cardInfoUI가 설정되지 않았습니다.");
        }
    }

    // 클릭한 카드의 정보를 받아오기
    private CardData GetClickedCardData(GameObject clickedCard)
    {
        if (clickedCard == null)
        {
            return null;
        }

        string cardName = clickedCard.name;
        if (string.IsNullOrEmpty(cardName) || !cardName.StartsWith("Card_"))
        {
            return null;
        }

        string cardId = cardName.Substring(5); // "Card_" 제거

        // DataCenter에서 카드 데이터 가져오기
        if (DataCenter.Instance != null && DataCenter.IsCardDataLoaded)
        {
            if (DataCenter.card_datas.TryGetValue(cardId, out CardData cardData))
            {
                return cardData;
            }
        }

        return null;
    }

    private void ToggleCanvasGroup(bool active)
    {
        canvasGroup.alpha = active ? 1f : 0f;
        canvasGroup.blocksRaycasts = active;
        canvasGroup.interactable = active;
    }
}
