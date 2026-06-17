using DG.Tweening;
using JxModule;
using UnityEngine;

namespace Jongmin
{
    public class NotifyView : LabelView
    {
        private Tween _popTween;
        private bool _isInitialized;

        private void OnEnable()
        {
            if (!_isInitialized)
            {
                _isInitialized = true;
                return;
            }
            
            var rectTransform = Label.transform as RectTransform;
            if(rectTransform == null)
            {
                return;
            }
            
            CanvasGroup.alpha = 0f;
            rectTransform.anchoredPosition = new Vector2(0f, 100f);
            
            _popTween?.Kill();
            
            var sequence = DOTween.Sequence();
            sequence.Join(rectTransform.DOAnchorPosY(150f, 1.5f));
            sequence.Join(CanvasGroup.DOFade(1f, 0.5f));
            sequence.Append(CanvasGroup.DOFade(0f, 0.5f));
            sequence.OnComplete(Return);
        }
        
        private void OnDisable()
        {
            _popTween?.Kill();
        }

        private void Return()
        {
            ObjectPoolManager.Instance.Return(gameObject);
        }
    }
}