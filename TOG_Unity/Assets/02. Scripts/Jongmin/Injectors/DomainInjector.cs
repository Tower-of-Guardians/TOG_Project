using UnityEngine;

namespace Jongmin
{
    public class DomainInjector : MonoBehaviour, IInjector
    {
        [SerializeField] private TurnManager turnManager;
        [SerializeField] private HandDomain handDomain;
        [SerializeField] private FieldDomain fieldDomain;
        [SerializeField] private DiscardDomain discardDomain;
        [SerializeField] private DeckDomain deckDomain;
        [SerializeField] private ManualDomain manualDomain;
        [SerializeField] private EffectDomain effectDomain;
        [SerializeField] private NotifyDomain notifyDomain;
        [SerializeField] private CardInfoDomain cardInfoDomain;
        [SerializeField] private InventoryDomain inventoryDomain;
        [SerializeField] private CardInvenDomain cardInvenDomain;
        [SerializeField] private ResultDomain resultDomain;
        
        public void Inject()
        {
            notifyDomain.Construct();
            
            var dropSystem = new CardDropSystem(
                handDomain.System, 
                fieldDomain.AtkSystem, 
                fieldDomain.DefSystem, 
                discardDomain.System, 
                turnManager, 
                notifyDomain.System
            );
            
            handDomain.Construct(dropSystem);
            fieldDomain.Construct(dropSystem);
            discardDomain.Construct(dropSystem);
            deckDomain.Construct();
            manualDomain.Construct();
            effectDomain.Construct();
            cardInfoDomain.Construct();
            cardInvenDomain.Construct();
            inventoryDomain.Construct();
            resultDomain.Construct();
            
            DIContainer.Register<CardDropSystem>(dropSystem);
            DIContainer.Register<HandDomain>(handDomain);
            DIContainer.Register<FieldDomain>(fieldDomain);
            DIContainer.Register<DiscardDomain>(discardDomain);
            DIContainer.Register<DeckDomain>(deckDomain);
            DIContainer.Register<ManualDomain>(manualDomain);
            DIContainer.Register<EffectDomain>(effectDomain);
            DIContainer.Register<NotifyDomain>(notifyDomain);
            DIContainer.Register<CardInfoDomain>(cardInfoDomain);
            DIContainer.Register<InventoryDomain>(inventoryDomain);
            DIContainer.Register<CardInvenDomain>(cardInvenDomain);
            DIContainer.Register<ResultDomain>(resultDomain);
        }
    }
}