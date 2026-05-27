using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using DG.Tweening;
using System.Collections;

namespace JxModule
{
    public delegate IEnumerator OnClickButtonRoutine();

    [RequireComponent(typeof(JxEmptyGraphic))]
    public class JxButton : MonoBehaviour, IPointerEnterHandler, IPointerClickHandler, IPointerExitHandler
    {
        [SerializeField] private Image buttonImage;
        [SerializeField] private Color hoverColor;
        
        private Color _originColor;
        private Vector3 _originScale;
        
        private event OnClickButtonRoutine OnClickRoutine;
        private event Action OnClick;

        private Tween _hoverTween;
        private Tween _clickTween;

        private void Awake()
        {
            buttonImage ??= GetComponentInChildren<Image>();
            if (buttonImage == null)
            {
                enabled = false;
                return;
            }
            
            _originColor = buttonImage.color;
            _originScale = transform.localScale;

            SetTween(GetDefaultHoverTween(), GetDefaultClickTween());
        }

        private void OnDestroy()
        {
            _hoverTween?.Kill();
            _clickTween?.Kill();
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

        public void SetTween(Tween hoverTween = null, Tween clickTween = null)
        {
            _hoverTween = hoverTween;
            _hoverTween?.SetAutoKill(false);
            _hoverTween?.Pause();
            
            _clickTween = clickTween;
            _clickTween?.SetAutoKill(false);
            _clickTween?.Pause();
        }


        public void OnPointerEnter(PointerEventData eventData)
        {
            _hoverTween?.Restart();
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            _clickTween?.Restart();

            StartCoroutine(ClickRoutine(eventData));
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            buttonImage.DOColor(_originColor, 0f);
        }

        private IEnumerator ClickRoutine(PointerEventData eventData)
        {
            if (_clickTween != null)
            {
                _clickTween.Restart();
                yield return _clickTween.WaitForCompletion();
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

        private Tween GetDefaultHoverTween()
        {
            var hoverSequence = DOTween.Sequence();

            hoverSequence.Join(buttonImage.DOColor(hoverColor, 0f));
            hoverSequence.Join(buttonImage.transform.DOPunchScale(new Vector3(0.05f, 0.05f, 0f), 1));
            
            return hoverSequence;
        }

        private Tween GetDefaultClickTween()
        {
            return buttonImage.transform.DORotate(new Vector3(360f, 0f, 0f), 0.5f, RotateMode.FastBeyond360);
        }
    }
}