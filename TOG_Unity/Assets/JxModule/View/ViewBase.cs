using UnityEngine;
using UnityEngine.EventSystems;

namespace JxModule
{
    [RequireComponent(typeof(RectTransform), typeof(CanvasGroup))]
    public class ViewBase : MonoBehaviour, 
                            IPointerEnterHandler, 
                            IPointerExitHandler, 
                            IPointerClickHandler, 
                            IPointerDownHandler, 
                            IPointerUpHandler, 
                            IPointerMoveHandler
    {
        private RectTransform _rectTransform;
        private CanvasGroup _canvasGroup;
        
        public RectTransform RectTransform => _rectTransform ??= GetComponent<RectTransform>();
        public CanvasGroup CanvasGroup => _canvasGroup ??= GetComponent<CanvasGroup>();


        public virtual void OnPointerEnter(PointerEventData eventData) { }
        public virtual void OnPointerExit(PointerEventData eventData) { }
        public virtual void OnPointerClick(PointerEventData eventData) { }
        public virtual void OnPointerDown(PointerEventData eventData) { }
        public virtual void OnPointerUp(PointerEventData eventData) { }
        public virtual void OnPointerMove(PointerEventData eventData) { }
    }
}