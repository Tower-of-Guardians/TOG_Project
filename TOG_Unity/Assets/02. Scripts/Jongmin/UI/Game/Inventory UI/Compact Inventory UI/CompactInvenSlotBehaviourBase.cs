using DG.Tweening;

namespace Jongmin
{
    public abstract class CompactInvenSlotBehaviourBase
    {
        private Tween _pointerTween;
        
        public virtual void OnPointerDown(CompactInvenSlot invenSlot)
        {
            _pointerTween?.Kill();
            _pointerTween = invenSlot.CanvasGroup.DOFade(0.5f, 0.25f);
        }

        public virtual void OnPointerUp(CompactInvenSlot invenSlot)
        {
            _pointerTween?.Kill();
            _pointerTween = invenSlot.CanvasGroup.DOFade(1f, 0.25f);
        }

        public virtual void OnPointerClick(CompactInvenSlot invenSlot) {}
    }
}