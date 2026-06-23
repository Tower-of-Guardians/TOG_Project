using DG.Tweening;
using JxModule;
using UnityEngine;

namespace Jongmin
{
    public class CraftmanView : ViewBase
    {
        private Tween _toggleTween;
        
        public void Show()
        {
            _toggleTween?.Kill();
            _toggleTween = CanvasGroup.DOFade(1f, 0.5f).OnComplete(CanvasGroup.Show);
        }

        public void Hide()
        {
            _toggleTween?.Kill();
            _toggleTween = CanvasGroup.DOFade(0f, 0.5f).OnComplete(CanvasGroup.Hide);
        }
    }
}