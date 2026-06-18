using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace JxModule
{
    [RequireComponent(typeof(JxEmptyGraphic))]
    public class JxToggle : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler, IPointerClickHandler
    {
        [Header("References")]
        [SerializeField] private JxPrimitiveAnimator animator;
        
        [Header("State")]
        [SerializeField] private bool isOn;
        
        public event Action<bool> OnValueChanged;
        public bool IsOn => isOn;

        private void Awake()
        {
            animator ??= GetComponent<JxPrimitiveAnimator>();
            ApplyState(false, false);
        }
        
        public void OnPointerEnter(PointerEventData eventData)
        {
            animator?.OnPointerEnter();
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            animator?.OnPointerExit();
        }
        
        public void OnPointerDown(PointerEventData eventData)
        {
            animator?.OnPointerDown();
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            animator?.OnPointerUp();
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            Toggle();
        }

        public void Toggle()
        {
            SetIsOn(!isOn);
        }

        public void SetIsOn(bool value, bool notify = true)
        {
            if (isOn == value)
            {
                return;
            }

            isOn = value;
            ApplyState(true, notify);
        }

        private void ApplyState(bool animate, bool notify)
        {
            if (animator != null)
            {
                animator.SetSelected(isOn, !animate);
            }

            if (notify)
            {
                OnValueChanged?.Invoke(isOn);
            }
        }
    }
}