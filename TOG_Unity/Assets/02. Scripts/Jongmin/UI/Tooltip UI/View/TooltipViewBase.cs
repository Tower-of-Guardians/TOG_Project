using DG.Tweening;
using JxModule;
using UnityEngine;

namespace Jongmin
{
    public abstract class TooltipViewBase : ViewBase
    {
        [BigHeader("Layout")]
        [SerializeField] private ETooltipLayout layout;
        
        [Space(30f)]
        [BigHeader("Effect")]
        [SerializeField] private TooltipEffect tooltipEffect;
        
        private Tween _fadeTween;
        
        public ETooltipLayout Layout => layout;
        
        public void Show(TooltipDataTableRow tooltipDataTableRow,
                         TooltipContent tooltipContent)
        {
            if (tooltipDataTableRow == null || tooltipContent == null)
            {
                return;
            }
            
            gameObject.SetActive(true);
            Bind(tooltipDataTableRow, tooltipContent);
            
            _fadeTween?.Kill();
            _fadeTween = tooltipEffect.PlayShowTooltipEffect(CanvasGroup);
        }
        
        public void Refresh(TooltipDataTableRow tooltipDataTableRow,
                            TooltipContent tooltipContent)
        {
            if (tooltipDataTableRow == null || tooltipContent == null)
            {
                return;
            }

            Bind(tooltipDataTableRow, tooltipContent);
        }

        public void Hide()
        {
            if (!gameObject.activeSelf)
            {
                return;
            }
            
            _fadeTween?.Kill();
            _fadeTween = tooltipEffect.PlayHideTooltipEffect(CanvasGroup, HideImmediate);
        }

        public void HideImmediate()
        {
            _fadeTween?.Kill();
            _fadeTween = null;
            
            gameObject.SetActive(false);
        }
        
        protected abstract void Bind(TooltipDataTableRow tooltipDataTableRow,
                                     TooltipContent tooltipContent);
    }
}