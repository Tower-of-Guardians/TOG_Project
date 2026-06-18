using System;
using System.Collections;
using DG.Tweening;
using JxModule;
using UnityEngine;

namespace Jongmin
{
    public delegate IEnumerator GachaInitAction();
    
    public class GachaView : ViewBase
    {
        [SerializeField] private Transform cardRoot;
        [SerializeField] private LabelBoxView rateLabel;
        [SerializeField] private ButtonView refreshButton;

        private Tween _toggleTween;

        public GachaInitAction OnGachaInit;
        public Action OnGachaEnd;
        
        public Transform CardRoot => cardRoot;

        public void Bind(GachaSystem system)
        {
            refreshButton.AddListener(system.Refresh);
            OnGachaInit = system.CreateSlots;
            OnGachaEnd = system.RemoveSlots;
        }

        public void SetRate(string rateText)
        {
            rateLabel.Label.text = rateText;
        }

        public void SetRefreshState(int refreshCost, bool canRefresh)
        {
            refreshButton.Label.text = canRefresh ? $"리롤: {refreshCost}" : $"리롤: <color=red>{refreshCost}</color>";
            refreshButton.Button.interactable = canRefresh;
        }
        
        public IEnumerator Show()
        {
            CanvasGroup.Hide();

            _toggleTween?.Kill();
            _toggleTween = CanvasGroup.DOFade(1f, 0.5f).OnComplete(CanvasGroup.Show);
            
            yield return _toggleTween.WaitForCompletion();
            yield return OnGachaInit?.Invoke();
        }

        public void Hide()
        {
            _toggleTween?.Kill();
            _toggleTween = CanvasGroup.DOFade(0f, 0.5f).OnComplete(() =>
            {
                CanvasGroup.Hide();
                OnGachaEnd();
            });
        }
    }
}