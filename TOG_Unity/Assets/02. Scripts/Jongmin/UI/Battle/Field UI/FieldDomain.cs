using DG.Tweening;
using JxModule;
using UnityEngine;

namespace Jongmin
{
    public class FieldDomain : MonoBehaviour
    {
        [SerializeField] private FieldView atkView;
        [SerializeField] private FieldView defView;
        [SerializeField] private FieldSystem atkFieldSystem;
        [SerializeField] private FieldSystem defFieldSystem;
        [SerializeField] private FieldEventSystem atkFieldEventSystem;
        [SerializeField] private FieldEventSystem defFieldEventSystem;
        [SerializeField] private PreviewCard atkPreviewCard;
        [SerializeField] private PreviewCard defPreviewCard;
        [SerializeField] private FieldUIDesigner designer;
        [SerializeField] private Canvas rootCanvas;
        [SerializeField] private TurnManager turnManager;
        [SerializeField] private DiscardSystem discardSystem;
        [SerializeField] private HandDomain handDomain;

        private CardContainer _atkCardContainer;
        private CardContainer _defCardContainer;
        private FieldCardLayout _atkCardLayout;
        private FieldCardLayout _defCardLayout;
        private FieldCardFactory _atkCardFactory;
        private FieldCardFactory _defCardFactory;
        
        public AtkFieldSystem AtkSystem => atkFieldSystem as AtkFieldSystem;
        public DefFieldSystem DefSystem => defFieldSystem as DefFieldSystem;
        public CardContainer AtkContainer => _atkCardContainer;
        public CardContainer DefContainer => _defCardContainer;
        public FieldView AtkView => atkView;
        public FieldView DefView => defView;
        
        public void Construct(CardDropSystem cardDropSystem)
        {
            _atkCardContainer = new CardContainer();
            _defCardContainer = new CardContainer();
            _atkCardLayout = new FieldCardLayout(atkFieldSystem, designer, _atkCardContainer, atkPreviewCard);
            _defCardLayout = new FieldCardLayout(defFieldSystem, designer, _defCardContainer, defPreviewCard);
            _atkCardFactory = new FieldCardFactory(atkView, atkFieldEventSystem);
            _defCardFactory = new FieldCardFactory(defView, defFieldEventSystem);
            
            atkFieldSystem.Construct(atkView, _atkCardContainer, _atkCardLayout, _atkCardFactory);
            defFieldSystem.Construct(defView, _defCardContainer, _defCardLayout, _defCardFactory);
            
            atkFieldEventSystem.Construct(atkFieldSystem, cardDropSystem, _atkCardContainer);
            defFieldEventSystem.Construct(defFieldSystem, cardDropSystem, _defCardContainer);

            BindEvents();
        }

        public void BindEvents()
        {
            handDomain.System.OnTogglePreviews += atkFieldSystem.TogglePreview;
            handDomain.System.OnTogglePreviews += defFieldSystem.TogglePreview;
            
            atkFieldSystem.RequestUpdateActionCount += turnManager.UpdateActionCount;
            defFieldSystem.RequestUpdateActionCount += turnManager.UpdateActionCount;

            discardSystem.OnDiscardViewVisibilityChanged += atkFieldSystem.UpdateInteraction;
            discardSystem.OnDiscardViewVisibilityChanged += defFieldSystem.UpdateInteraction;

            atkFieldEventSystem.RequestOnBeginDrag += HandleRequestOnBeginDrag;
            atkFieldEventSystem.RequestSwapInSameField += HandleRequestSwapInSameField;
            atkFieldEventSystem.RequestOnEndDrag += HandleRequestOnEndDrag;
            atkFieldEventSystem.RequestMoveHoverCardToOpposite += HandleMoveHoverCardToOpposite;
            
            defFieldEventSystem.RequestOnBeginDrag += HandleRequestOnBeginDrag;
            defFieldEventSystem.RequestSwapInSameField += HandleRequestSwapInSameField;
            defFieldEventSystem.RequestOnEndDrag += HandleRequestOnEndDrag;
            defFieldEventSystem.RequestMoveHoverCardToOpposite += HandleMoveHoverCardToOpposite;
        }

        public void ReleaseEvents()
        {
            handDomain.System.OnTogglePreviews -= atkFieldSystem.TogglePreview;
            handDomain.System.OnTogglePreviews -= defFieldSystem.TogglePreview;
            
            atkFieldSystem.RequestUpdateActionCount -= turnManager.UpdateActionCount;
            defFieldSystem.RequestUpdateActionCount -= turnManager.UpdateActionCount;
            
            discardSystem.OnDiscardViewVisibilityChanged -= atkFieldSystem.UpdateInteraction;
            discardSystem.OnDiscardViewVisibilityChanged -= defFieldSystem.UpdateInteraction;
            
            atkFieldEventSystem.RequestOnBeginDrag -= HandleRequestOnBeginDrag;
            atkFieldEventSystem.RequestSwapInSameField -= HandleRequestSwapInSameField;
            atkFieldEventSystem.RequestOnEndDrag -= HandleRequestOnEndDrag;
            atkFieldEventSystem.RequestMoveHoverCardToOpposite -= HandleMoveHoverCardToOpposite;
            
            defFieldEventSystem.RequestOnBeginDrag -= HandleRequestOnBeginDrag;
            defFieldEventSystem.RequestSwapInSameField -= HandleRequestSwapInSameField;
            defFieldEventSystem.RequestOnEndDrag -= HandleRequestOnEndDrag;
            defFieldEventSystem.RequestMoveHoverCardToOpposite -= HandleMoveHoverCardToOpposite;
        }

        private void HandleRequestOnBeginDrag(Card card, FieldType fieldType)
        {
            var system = GetSystem(fieldType);
            var view = GetView(fieldType);
            var container = GetContainer(fieldType);
            var layout = GetLayout(fieldType);

            var oppositeSystem = GetOppositeSystem(fieldType);

            system.HoverCard = card;

            MoveHoverCardToCanvas(card);

            if (!container.TryGetIndex(card, out var index))
                return;

            view.TogglePreview(true);

            layout.UpdateLayout(
                FieldPreviewMode.Swap,
                previewIndex: index,
                isAnime: true
            );
            
            oppositeSystem.TogglePreview(true);
        }

        private void HandleRequestSwapInSameField(Card card, FieldType fieldType, Vector2 position)
        {
            var system = GetSystem(fieldType);
            var container = GetContainer(fieldType);
            var layout = GetLayout(fieldType);
            var view = GetView(fieldType);

            var changed = InsertInSameField(card, system, container, position);

            if (!changed)
                return;

            if (!container.TryGetIndex(system.HoverCard, out var index))
                return;

            view.TogglePreview(true);

            layout.UpdateLayout(
                FieldPreviewMode.Swap,
                previewIndex: index,
                isAnime: true
            );
        }

        private void HandleRequestOnEndDrag(bool success, FieldType fieldType)
        {
            var currentView = GetView(fieldType);
            var oppositeView = GetOppositeView(fieldType);

            var system = GetSystem(fieldType);
            var eventSystem = GetEventSystem(fieldType);

            var hoverCard = system.HoverCard;
            if (hoverCard == null)
            {
                return;
            }

            var movedToOpposite = false;
            if (!success)
            {
                movedToOpposite = eventSystem.TryMoveHoverCardToOppositeField();

                var hoverCardWorldPosition = hoverCard.transform.position;
                var finalRoot = movedToOpposite ? oppositeView.CardRoot : currentView.CardRoot;

                hoverCard.transform.SetParent(finalRoot, false);

                var localPosition = hoverCard.transform.parent.InverseTransformPoint(hoverCardWorldPosition);
                hoverCard.transform.localPosition = localPosition;
            }

            atkView.TogglePreview(false);
            defView.TogglePreview(false);

            atkFieldSystem.HoverCard = null;
            defFieldSystem.HoverCard = null;

            _atkCardLayout.UpdateLayout(FieldPreviewMode.None);
            _defCardLayout.UpdateLayout(FieldPreviewMode.None);

            SyncAtkDataWithContainer();
            SyncDefDataWithContainer();
        }
        
        private void HandleMoveHoverCardToOpposite(FieldType sourceFieldType)
        {
            var sourceSystem = GetSystem(sourceFieldType);
            var targetSystem = GetOppositeSystem(sourceFieldType);

            var sourceContainer = GetContainer(sourceFieldType);
            var targetContainer = GetOppositeContainer(sourceFieldType);

            var sourceEventSystem = GetEventSystem(sourceFieldType);
            var targetEventSystem = GetOppositeEventSystem(sourceFieldType);

            var hoverCard = sourceSystem.HoverCard;

            if (hoverCard == null)
            {
                return;
            }

            sourceEventSystem.Unsubscribe(hoverCard);
            targetEventSystem.Subscribe(hoverCard);

            sourceContainer.Remove(hoverCard);
            targetContainer.Add(hoverCard);

            if (sourceFieldType == FieldType.Attack)
            {
                hoverCard.SetBattleCardData(hoverCard.BattleCardData, CardType.DefField);
                hoverCard.View.LockAtk();
            }
            else
            {
                hoverCard.SetBattleCardData(hoverCard.BattleCardData, CardType.AtkField);
                hoverCard.View.LockDef();
            }
        }

        private bool InsertInSameField(Card targetCard, FieldSystem system, CardContainer container, Vector2 position)
        {
            var hoverCard = system.HoverCard;

            if (hoverCard == null || targetCard == null || hoverCard == targetCard)
            {
                return false;
            }

            if (!container.TryGetIndex(hoverCard, out var hoverIndex))
            {
                return false;
            }

            if (!container.TryGetIndex(targetCard, out var targetIndex))
            {
                return false;
            }

            var isHoverBeforeTarget = hoverIndex < targetIndex;

            if (isHoverBeforeTarget)
            {
                if (position.x <= targetCard.transform.position.x)
                {
                    return false;
                }
            }
            else
            {
                if (position.x >= targetCard.transform.position.x)
                {
                    return false;
                }
            }

            container.Insert(hoverCard, targetCard);
            return true;
        }

        private void MoveHoverCardToCanvas(Card card)
        {
            card.DOKill();
            card.transform.SetParent(rootCanvas.transform, false);
        }

        private void SyncAtkDataWithContainer()
        {
            var cards = _atkCardContainer.Cards;

            for (var i = 0; i < cards.Count; i++)
            {
                if (i < GameData.Instance.attackField.Count)
                {
                    GameData.Instance.attackField[i] = cards[i].CardData;
                }
                else
                {
                    GameData.Instance.attackField.Add(cards[i].CardData);
                }
            }

            while (GameData.Instance.attackField.Count > cards.Count)
            {
                GameData.Instance.attackField.RemoveAt(GameData.Instance.attackField.Count - 1);
            }
        }

        private void SyncDefDataWithContainer()
        {
            var cards = _defCardContainer.Cards;

            for (var i = 0; i < cards.Count; i++)
            {
                if (i < GameData.Instance.defenseField.Count)
                {
                    GameData.Instance.defenseField[i] = cards[i].CardData;
                }
                else
                {
                    GameData.Instance.defenseField.Add(cards[i].CardData);
                }
            }

            while (GameData.Instance.defenseField.Count > cards.Count)
            {
                GameData.Instance.defenseField.RemoveAt(GameData.Instance.defenseField.Count - 1);
            }
        }
        
        private FieldSystem GetSystem(FieldType fieldType)
        {
            return fieldType == FieldType.Attack ? atkFieldSystem : defFieldSystem;
        }

        private FieldSystem GetOppositeSystem(FieldType fieldType)
        {
            return fieldType == FieldType.Attack ? defFieldSystem : atkFieldSystem;
        }

        private FieldView GetView(FieldType fieldType)
        {
            return fieldType == FieldType.Attack ? atkView : defView;
        }

        private FieldView GetOppositeView(FieldType fieldType)
        {
            return fieldType == FieldType.Attack ? defView : atkView;
        }

        private CardContainer GetContainer(FieldType fieldType)
        {
            return fieldType == FieldType.Attack ? _atkCardContainer : _defCardContainer;
        }

        private CardContainer GetOppositeContainer(FieldType fieldType)
        {
            return fieldType == FieldType.Attack ? _defCardContainer : _atkCardContainer;
        }

        private FieldCardLayout GetLayout(FieldType fieldType)
        {
            return fieldType == FieldType.Attack ? _atkCardLayout : _defCardLayout;
        }

        private FieldEventSystem GetEventSystem(FieldType fieldType)
        {
            return fieldType == FieldType.Attack ? atkFieldEventSystem : defFieldEventSystem;
        }

        private FieldEventSystem GetOppositeEventSystem(FieldType fieldType)
        {
            return fieldType == FieldType.Attack ? defFieldEventSystem : atkFieldEventSystem;
        }

        private void OnDestroy()
        {
            ReleaseEvents();
        }
    }
}