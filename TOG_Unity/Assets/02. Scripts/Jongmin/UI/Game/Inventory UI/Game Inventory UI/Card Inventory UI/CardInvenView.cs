using DG.Tweening;
using JxModule;
using UnityEngine;

namespace Jongmin
{
    public class CardInvenView : ViewBase
    {
        [SerializeField] private Transform cardRoot;
        
        private Tween _toggleTween;
        
        public Transform CardRoot => cardRoot;
        
        public void Show()
        {
            _toggleTween?.Kill();
            _toggleTween = CanvasGroup.DOFade(1f, 0.25f).OnComplete(CanvasGroup.Show);
        }

        public void Hide()
        {
            _toggleTween?.Kill();
            _toggleTween = CanvasGroup.DOFade(0f, 0.25f).OnComplete(CanvasGroup.Hide);
        }
    }
}