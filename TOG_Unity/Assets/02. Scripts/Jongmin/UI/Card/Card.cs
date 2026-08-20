using DG.Tweening;
using UnityEngine;

namespace Jongmin
{
    public class Card : MonoBehaviour
    {
        private static readonly Vector2 CenterAnchor = new(0.5f, 0.5f);
        private static readonly Vector2 DefaultSize = new(180f, 270f);

        [SerializeField] private CardView view;
        [SerializeField] private CardPointer pointer;

        public BattleCardData BattleCardData { get; private set; }
        public CardData CardData { get; private set; }
        public CardType CardType { get; private set; }

        public CardPointer Pointer => pointer;
        public CardView View => view;
        public RectTransform RectTransform => transform as RectTransform;

        private void Awake()
        {
            view ??= GetComponent<CardView>();
            pointer ??= GetComponent<CardPointer>();
            pointer?.SetOwner(this);
        }

        public void SetBattleCardData(BattleCardData battleCardData, CardType cardType = CardType.None)
        {
            BattleCardData = battleCardData;
            CardData = battleCardData?.data;
            view.UpdateModel(CardData);
            
            CardType = cardType;
        }

        public void SetCardData(CardData cardData, CardType cardType = CardType.None)
        {
            BattleCardData = null;
            CardData = cardData;
            view.UpdateModel(CardData);
            
            CardType = cardType;
        }

        public void CompleteTweens()
        {
            DOTween.Complete(this);
            RectTransform?.DOComplete();
            transform.DOComplete();
            view?.CanvasGroup?.DOComplete();
            SetInteraction(true);
        }

        public void KillTweens(bool complete = false)
        {
            DOTween.Kill(this, complete);
            RectTransform?.DOKill(complete);
            transform.DOKill(complete);
            view?.CanvasGroup?.DOKill(complete);
            SetInteraction(true);
        }

        public void SetInteraction(bool isActive)
        {
            if (view?.CanvasGroup != null)
            {
                view.CanvasGroup.interactable = isActive;
                view.CanvasGroup.blocksRaycasts = isActive;
            }

            pointer?.SetInteraction(isActive);
        }

        public void ResetRectTransform(Vector3 scale)
        {
            var rectTransform = RectTransform;
            if (rectTransform == null)
            {
                return;
            }

            rectTransform.anchorMin = CenterAnchor;
            rectTransform.anchorMax = CenterAnchor;
            rectTransform.pivot = CenterAnchor;
            rectTransform.sizeDelta = DefaultSize;
            rectTransform.anchoredPosition = Vector2.zero;
            rectTransform.localRotation = Quaternion.identity;
            rectTransform.localScale = scale;
        }

        private void OnDisable()
        {
            KillTweens();
            SetInteraction(true);
            BattleCardData = null;
            CardData = null;
            CardType = CardType.None;
        }
    }
}
