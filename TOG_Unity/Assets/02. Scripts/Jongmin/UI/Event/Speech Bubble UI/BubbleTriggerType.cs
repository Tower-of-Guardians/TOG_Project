namespace Jongmin
{
    public enum BubbleTriggerType
    {
        // Craftman
        OpenCraftmanView = 10,          // 강화 UI를 열었을때 + 카드 선택을 번복했을 때
        SelectedCraftmanSlot = 11,      // 강화할 카드를 선택해서 강화창이 열렸을 때
        CompletedUpgradeCard = 12,      // 카드 강화를 다 마쳤을때
        
        // Merchant
        OpenMerchantView = 20,          // 상점 UI를 열었을때 + 카드 판매를 취소했을 때
        PurchasedCard = 21,             // 카드를 구매했을 때
        PurchasedHpPotion = 22,         // 포션을 구매했을 때
        ClickedSellCard = 23,           // 카드 판매 버튼을 클릭했을 때
        SelectedMerchantSlot = 24,      // 판매할 카드를 선택할 때(기본 카드(판매 가격이 없는 경우 포함))
        CompletedSellCards = 25,        // 카드 판매를 마쳤을 때
    }
}