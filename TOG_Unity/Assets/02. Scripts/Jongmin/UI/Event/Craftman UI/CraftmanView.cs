using JxModule;
using UnityEngine;

namespace Jongmin
{
    public class CraftmanView : ViewBase
    {
        [BigHeader("Effect")]
        [SerializeField] private CraftmanVisualEffect craftmanVisualEffect;
        
        public void Show()
        {
            craftmanVisualEffect.PlayShowEffect(CanvasGroup, CanvasGroup.Show);
        }

        public void Hide()
        {
            craftmanVisualEffect.PlayHideEffect(CanvasGroup, CanvasGroup.Hide);
        }
    }
}