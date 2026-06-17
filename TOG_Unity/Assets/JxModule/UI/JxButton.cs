using System;
using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;

namespace JxModule
{
    public delegate IEnumerator OnClickButtonRoutine();

    [RequireComponent(typeof(JxEmptyGraphic))]
    public class JxButton : MonoBehaviour, IPointerEnterHandler, IPointerClickHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
    {
        [SerializeField] private JxPrimitiveAnimator animator;
        
        private event OnClickButtonRoutine OnClickRoutine;
        private event Action OnClick;

        private void Awake()
        {
            animator ??= GetComponent<JxPrimitiveAnimator>();
        }
        
#region Events
        public void AddListener(OnClickButtonRoutine listener)
        {
            OnClickRoutine += listener;
        }

        public void AddListener(Action listener)
        {
            OnClick += listener;
        }
        
        public void RemoveListener(OnClickButtonRoutine listener)
        {
            OnClickRoutine -= listener;
        }

        public void RemoveListener(Action listener)
        {
            OnClick -= listener;
        }

        public void RemoveAllListeners()
        {
            OnClickRoutine = null;
            OnClick = null;
        }
#endregion Events

        public void OnPointerEnter(PointerEventData eventData)
        {
            animator?.OnPointerEnter();
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            StartCoroutine(ClickRoutine(eventData));
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

        private IEnumerator ClickRoutine(PointerEventData eventData)
        {
            if (animator != null)
            {
                yield return animator.OnPointerClick();
            }
            
            OnClick?.Invoke();
            yield return InvokeClickRoutines();
        }

        private IEnumerator InvokeClickRoutines()
        {
            if (OnClickRoutine == null)
            {
                yield break;
            }

            var routines = OnClickRoutine.GetInvocationList();
            var totalCount = 0;

            foreach (OnClickButtonRoutine routine in routines)
            {
                StartCoroutine(InvokeClickRoutine(routine, () => totalCount++));
            }

            yield return new WaitUntil(() => totalCount >= routines.Length);  
        }
        
        private IEnumerator InvokeClickRoutine(OnClickButtonRoutine routine, Action waitAction)
        {
            var enumerator = routine?.Invoke();

            if (enumerator != null)
            {
                yield return enumerator;
            }

            waitAction?.Invoke();
        }
    }
}