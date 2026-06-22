using System.Collections;
using JxModule;
using UnityEngine;

namespace Jongmin
{
    public class EffectDomain : MonoBehaviour
    {
        [SerializeField] private RectTransform cardRoot;
        [SerializeField] private EffectSystem effectSystem;
        
        [Header("References")]
        [SerializeField] private TurnManager turnManager;
        [SerializeField] private HandDomain handDomain;
        [SerializeField] private ImageView battleView;
        [SerializeField] private FieldDomain fieldDomain;
        [SerializeField] private Transform drawButton;
        [SerializeField] private Transform discardButton;

        private CardContainer _cardContainer;
        private EffectCardFactory _cardFactory;

        public void Construct()
        {
            _cardContainer = new CardContainer();
            _cardFactory = new EffectCardFactory(cardRoot);
            
            effectSystem.Construct(_cardContainer, _cardFactory);
        }

        public IEnumerator DrawHandCards(int count = -1)
        {
            yield return effectSystem.DrawHandCards(GameData.Instance.NextDeckSet(count == -1 ? turnManager.MaxHandCount : count), 
                                                    handDomain.System, 
                                                    drawButton.transform.position,
                                                    handDomain.View.transform.position);
        }

        public IEnumerator DiscardHandCards()
        {
            yield return effectSystem.DiscardHandCards(handDomain.Container.Cards, 
                                                       handDomain.System, 
                                                       battleView,
                                                       discardButton.transform.position);
        }

        public void RevertDiscardCards(DiscardSystem discardSystem, CardContainer cardContainer)
        {
            effectSystem.RevertDiscardCards(cardContainer.Cards, 
                                            handDomain.System,
                                            discardSystem, 
                                            handDomain.View.transform.position);
        }

        public void DiscardDiscardCards(DiscardSystem discardSystem, CardContainer cardContainer)
        {
            effectSystem.DiscardDiscardCards(cardContainer.Cards,
                                             discardSystem,
                                             discardButton.transform.position);
        }

        public IEnumerator DiscardFieldCards(FieldType fieldType)
        {
            var fieldContainer = fieldType == FieldType.Attack ? fieldDomain.AtkContainer : fieldDomain.DefContainer;
            FieldSystem fieldSystem = fieldType == FieldType.Attack ? fieldDomain.AtkSystem : fieldDomain.DefSystem;
            var fieldView = fieldType == FieldType.Attack ? fieldDomain.AtkView : fieldDomain.DefView;
            
            yield return effectSystem.DiscardFieldCards(fieldContainer.Cards, 
                                                        fieldSystem, 
                                                        fieldView, 
                                                        discardButton.transform.position);
        }

        public void EnableBattleView()
        {
            effectSystem.EnableBattleView(battleView, fieldDomain.AtkView, fieldDomain.DefView);
        }
    }
}