using DG.Tweening;
using JxModule;
using UnityEngine;

namespace Jongmin
{
    public class CompactInvenView : ViewBase
    {
        [SerializeField] private Transform cardRoot;
        
        private Vector2 _originAnchoredPosition;
        private Tween _toggleTween;
        
        public Transform CardRoot => cardRoot;

        private void Awake()
        {
            _originAnchoredPosition = RectTransform.anchoredPosition;
        }

        public void Show()
        {
            _toggleTween?.Kill();

            var sequence = DOTween.Sequence();
            sequence.Join(CanvasGroup.DOFade(1f, 0.5f));
            sequence.Join(RectTransform.DOAnchorPosX(_originAnchoredPosition.x + 960f, 0.5f));
            sequence.OnComplete(CanvasGroup.Show);

            _toggleTween = sequence;
        }

        public void Hide(TweenCallback callback = null)
        {
            _toggleTween?.Kill();
            
            var sequence = DOTween.Sequence();
            sequence.Join(CanvasGroup.DOFade(0f, 0.5f));
            sequence.Join(RectTransform.DOAnchorPosX(_originAnchoredPosition.x, 0.5f));
            sequence.OnComplete(() =>
            {
                CanvasGroup.Hide();
                callback?.Invoke();
            });

            _toggleTween = sequence;
        }
    }
}