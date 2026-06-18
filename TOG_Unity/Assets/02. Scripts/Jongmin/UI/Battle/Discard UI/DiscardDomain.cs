using DG.Tweening;
using JxModule;
using UnityEngine;

namespace Jongmin
{
    public class DiscardDomain : MonoBehaviour
    {
        [BigHeader("Inner References")]
        [SerializeField] private DiscardUIDesigner discardDesigner;
        [SerializeField] private DiscardView discardView;
        [SerializeField] private DiscardSystem discardSystem;
        [SerializeField] private DiscardEventSystem discardEventSystem;
        [SerializeField] private PreviewCard previewCard;
        
        [Space(30f), BigHeader("Outer References")]
        [SerializeField] private HandDomain handDomain;
        [SerializeField] private TurnManager turnManager;
        [SerializeField] private Canvas rootCanvas;
        [SerializeField] private EffectDomain effectDomain;
        
        private CardContainer _cardContainer;
        private DiscardCardLayout _cardLayout;
        private DiscardCardFactory _cardFactory;
        
        public DiscardSystem System => discardSystem;
        public CardContainer Container => _cardContainer;

        public void Construct(CardDropSystem cardDropSystem)
        {
            _cardContainer = new CardContainer();
            _cardLayout = new DiscardCardLayout(discardSystem, discardDesigner, _cardContainer, previewCard);
            _cardFactory = new DiscardCardFactory(discardView, discardEventSystem);
            
            discardSystem.Construct(discardView, _cardContainer, _cardLayout, _cardFactory);
            discardEventSystem.Construct(discardSystem, cardDropSystem, _cardContainer);

            BindEvents();
        }

        public void BindEvents()
        {
            discardView.Bind(discardSystem);
            
            discardSystem.RequestUpdateThrowAction += turnManager.UpdateThrowAction;
            discardSystem.RequestUpdateThrowCount += turnManager.UpdateThrowCount;
            discardSystem.DiscardCancelEffect += effectDomain.RevertDiscardCards;
            discardSystem.DiscardEffect += effectDomain.DiscardDiscardCards;
            
            turnManager.OnUpdatedThrowActionState += discardSystem.UpdateOpenButtonState;
            turnManager.OnUpdatedThrowCount += discardSystem.UpdateDiscardCount;

            discardEventSystem.RequestOnBeginDrag += HandleRequestOnBeginDrag;
            discardEventSystem.RequestSwapInSameField += HandleRequestSwapInSameField;
            discardEventSystem.RequestOnEndDrag += HandleRequestOnEndDrag;

            handDomain.System.OnTogglePreviews += TogglePreviewPosition;
        }

        public void ReleaseEvents()
        {
            discardSystem.RequestUpdateThrowAction -= turnManager.UpdateThrowAction;
            discardSystem.RequestUpdateThrowCount -= turnManager.UpdateThrowCount;
            discardSystem.DiscardCancelEffect -= effectDomain.RevertDiscardCards;
            discardSystem.DiscardEffect -= effectDomain.DiscardDiscardCards;
            
            turnManager.OnUpdatedThrowActionState -= discardSystem.UpdateOpenButtonState;
            turnManager.OnUpdatedThrowCount -= discardSystem.UpdateDiscardCount;
            
            discardEventSystem.RequestOnBeginDrag -= HandleRequestOnBeginDrag;
            discardEventSystem.RequestSwapInSameField -= HandleRequestSwapInSameField;
            discardEventSystem.RequestOnEndDrag -= HandleRequestOnEndDrag;
            
            handDomain.System.OnTogglePreviews -= TogglePreviewPosition;
        }

        private void HandleRequestOnBeginDrag(Card card)
        {
            discardSystem.HoverCard = card;

            MoveHoverCardToRoot(card);

            if (_cardContainer.TryGetIndex(card, out var index))
            {
                discardView.TogglePreview(true);
                _cardLayout.UpdateLayout(PreviewLayoutMode.Swap, previewIndex: index);
            }
        }

        private void HandleRequestSwapInSameField(Card card, Vector2 position)
        {
            var changed = InsertInSameField(card, position);

            if (!changed)
                return;

            if (!_cardContainer.TryGetIndex(discardSystem.HoverCard, out var index))
                return;

            discardView.TogglePreview(true);

            _cardLayout.UpdateLayout(
                PreviewLayoutMode.Swap,
                previewIndex: index
            );
        }

        private void HandleRequestOnEndDrag(bool dropSuccess)
        {
            if (!dropSuccess)
            {
                MoveHoverCardToParent();    
            }
            
            discardSystem.HoverCard = null;
            _cardLayout.UpdateLayout(PreviewLayoutMode.None);
            discardView.TogglePreview(false);
        }

        private void MoveHoverCardToRoot(Card card)
        {
            card.DOKill();
            card.transform.SetParent(rootCanvas.transform, false);
        }

        private void MoveHoverCardToParent()
        {
            discardSystem.HoverCard.transform.SetParent(discardView.CardRoot, false);
        }
        
        private bool InsertInSameField(Card card, Vector2 position)
        {
            var hoverCard = discardSystem.HoverCard;

            if (hoverCard == null || card == null || hoverCard == card)
            {
                return false;
            }

            if (!_cardContainer.TryGetIndex(hoverCard, out var hoverIndex))
            {
                return false;
            }

            if (!_cardContainer.TryGetIndex(card, out var targetIndex))
            {
                return false;
            }

            var isHoverBeforeTarget = hoverIndex < targetIndex;

            if (isHoverBeforeTarget)
            {
                if (position.x <= card.transform.position.x)
                {
                    return false;
                }
            }
            else
            {
                if (position.x >= card.transform.position.x)
                {
                    return false;
                }
            }

            _cardContainer.Insert(hoverCard, card);

            return true;
        }

        private void TogglePreviewPosition(bool isActive)
        {
            discardView.TogglePreview(isActive);
            _cardLayout.UpdateLayout(isActive ? PreviewLayoutMode.Insert : PreviewLayoutMode.None, isAnime: true);
        }

        private void OnDestroy()
        {
            ReleaseEvents();
        }
    }
}