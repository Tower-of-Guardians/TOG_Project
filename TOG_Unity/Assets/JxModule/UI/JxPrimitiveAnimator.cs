using System.Collections;
using UnityEngine;

namespace JxModule
{
    public abstract class JxPrimitiveAnimator : MonoBehaviour
    {
        public virtual void SetSelected(bool selected, bool instant = false)
        {
            if (selected)
            {
                OnSelected(instant);
            }
            else
            {
                OnDeselected(instant);
            }
        }
        
        protected virtual void OnSelected(bool instant = false) { }
        protected virtual void OnDeselected(bool instant = false) { }
        
        public abstract void OnPointerEnter();
        public abstract void OnPointerExit();
        public abstract IEnumerator OnPointerClick();
        public abstract void OnPointerDown();
        public abstract void OnPointerUp();
    }
}