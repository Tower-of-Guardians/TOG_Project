using DG.Tweening;
using JxModule;
using UnityEngine;

namespace Jongmin
{
    public class CardSortView : MonoBehaviour
    {
        [SerializeField] private ArrowButtonView leftButton;
        [SerializeField] private ArrowButtonView rightButton;
        [SerializeField] private ToggleButtonView toggleButton;
        [SerializeField] private LabelView sortLabel;

        private Tween _fadeTween;
        
        public void Bind(CardSortSystem sortSystem)
        {
            leftButton.AddListener(sortSystem.HandleOnClickedLeft);
            rightButton.AddListener(sortSystem.HandleOnClickedRight);
            toggleButton.AddListener(sortSystem.HandleOnClickedCriterion);
        }

        public void SetSortLabel(string sortText)
        {
            sortLabel.Label.text = sortText;
            
            _fadeTween?.Kill();
            sortLabel.CanvasGroup.alpha = 0f;
            _fadeTween = sortLabel.CanvasGroup.DOFade(1f, 0.2f);
        }

        public void ResetCriterion()
        {
            toggleButton.Reset();
        }
    }
}