using System.Collections;
using DG.Tweening;
using JxModule;
using UnityEngine;

namespace Jongmin
{
    public class ResultView : ViewBase
    {
        [SerializeField] private CloseButton closeButton;

        private Tween _toggleTween;
        
        public void Bind(ResultDomain domain)
        {
            closeButton.Bind(domain.Hide);
        }

        public IEnumerator Show()
        {
            CanvasGroup.Hide();
            
            _toggleTween?.Kill();
            _toggleTween = CanvasGroup.DOFade(1f, 0.5f).OnComplete(CanvasGroup.Show);
            
            yield return _toggleTween.WaitForCompletion();
        }

        public void Hide()
        {
            _toggleTween?.Kill();
            _toggleTween = CanvasGroup.DOFade(0f, 0.5f).OnComplete(CanvasGroup.Hide);
        }

        public void ShowCloseButton()
        {
            closeButton.Show();
        }
    }
}