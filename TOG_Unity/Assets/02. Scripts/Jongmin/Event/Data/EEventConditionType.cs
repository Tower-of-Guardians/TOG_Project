namespace Jongmin
{
    public enum EEventConditionType
    {
        None,                                   // 조건 없이 항상 통과

        FirstNpcTalk,                           // 해당 NPC에게 처음 말을 거는가?
        NpcTalkCountAtLeast,                    // 해당 NPC와 대화한 횟수가 N번 이상인가?
        EventSeen,                              // 특정 이벤트를 이미 본 적이 있는가?
        EventNotSeen,                           // 특정 이벤트를 아직 본 적이 없는가?
        
        ReachedStageAtLeast,                    // 이번 게임 또는 전체 기록에서 특정 스테이지 이상에 도달한 적이 있는가?
        FirstReachedStage,                      // 특정 스테이지에 처음 도달했는가?
        
        ShopAllItemsPurchased,                  // 상점의 모든 품목을 구매했는가?
        
        MaxSingleAttackDamageAtLeast,           // 한 번의 공격으로 준 최대 피해량이 N 이상인가?
        
        HasRelic,                               // 특정 유물을 보유하는 있는가?
        HasRelicCountAtLeast,                   // 보유한 유물의 개수가 N개 이상인가?
        
        HasCard,                                // 특정 카드를 보유하고 있는가?
        HasCardCountAtLeast,                    // 보유한 카드의 개수가 N장 이상인가?
        GainedCardGradeCountInRunAtLeast,       // 이번 게임에서 특정 등급의 카드를 N장 이상 획득했는가?
        
        ActivatedSynergyAtLeast,                // 특정 시너지가 N 단계 이상 활성화된 적이 있는가?
        FirstActivatedSynergyAtLeast,           // 특정 시너지가 N 단계 이상 최초로 활성화되었는가?
    }
}