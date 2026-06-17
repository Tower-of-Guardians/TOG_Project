public class MerchantDialogueBubblePresenter : DialogueBubblePresenter
{
    public MerchantDialogueBubblePresenter(IDialogueBubbleUI ui) 
        : base(ui)
    {}

    public void OpenUI(MerchantDeckInvenPresenter deckInvenPresenter)
    {
        deckInvenPresenter.OnSelectedCardsChanged += UpdateSelectedCards;
        OpenUI();
    }

    public void CloseUI(MerchantDeckInvenPresenter deckInvenPresenter)
    {
        deckInvenPresenter.OnSelectedCardsChanged -= UpdateSelectedCards;
        CloseUI();
    }

    private void UpdateSelectedCards(int currentCardCount, int maxCardCount, int totalMoney)
    {
        if(currentCardCount == 0)
        {
            SetBubble($"<color=#99CCFF>{maxCardCount}</color>장까지 구매해줄 수 있다.");
        }
        else
        {
            if(totalMoney != 0)
                SetBubble($"총 <color=#99CCFF>{currentCardCount}</color>장을 판매하겠다고?\n전부 합해서 <color=#99CCFF>{totalMoney}G</color>에 구매하도록 하지.");
            else
                SetBubble($"미안하지만 그 카드는 돈을 줄 수 없어.\n쓸모 없으면 내가 대신 버려주도록 하지.");
        }
    }
}
