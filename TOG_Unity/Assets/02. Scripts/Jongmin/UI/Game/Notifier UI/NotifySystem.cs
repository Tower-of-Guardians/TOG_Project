using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using JxModule;
using UnityEngine;

namespace Jongmin
{
    public class NotifySystem : MonoBehaviour
    {
        private NotifyView _prefab;
        
        private const int MaxVisibleCount = 3;
        private const float Lifetime = 1f;
        private const float Spacing = 50f;
        private const float Duration = 0.25f;
        
        private readonly List<NotifyView> _activeNotifyViews = new();
        private readonly Dictionary<NotifyView, Coroutine> _lifetimeRoutines = new();
        
        private void Awake()
        {
            _prefab = PrefabManager.CachePrefab<NotifyView>("PF_NotifyView");
        }

        public void Notify(string text)
        {
            if (_activeNotifyViews.Count >= MaxVisibleCount)
            {
                var oldestNotifyView = _activeNotifyViews[^1];
                _activeNotifyViews.Remove(oldestNotifyView);
                FadeOut(oldestNotifyView, true);
            }

            for (var i = 0; i < _activeNotifyViews.Count; i++)
            {
                MoveToSlot(_activeNotifyViews[i], GetSlotPosition(i + 1));
            }
            
            var notifyObject = JxModule.ObjectPoolManager.Instance.Get(_prefab.gameObject);
            var notifyView = notifyObject.GetComponent<NotifyView>();
            if (notifyView == null)
            {
                return;
            }
            
            notifyView.Label.text = text;
            notifyView.transform.SetParent(transform, false);
            _activeNotifyViews.Insert(0, notifyView);
            FadeIn(notifyView);
        }

        private void FadeIn(NotifyView notifyView)
        {
            var targetAnchoredPosition = GetSlotPosition(0);
            var startAnchoredPosition = targetAnchoredPosition + Vector2.down * 30f;
            
            notifyView.CanvasGroup.alpha = 0f;
            notifyView.RectTransform.anchoredPosition = startAnchoredPosition;
            
            DOTween.Sequence()
                .Join(notifyView.CanvasGroup.DOFade(1f, Duration))
                .Join(notifyView.RectTransform.DOAnchorPos(targetAnchoredPosition, Duration).SetEase(Ease.OutCubic));

            var routine = StartCoroutine(LifetimeRoutine(notifyView, Lifetime));
            _lifetimeRoutines.Add(notifyView, routine);
        }

        private void FadeOut(NotifyView notifyView, bool force = false)
        {
            if (force)
            {
                if (_lifetimeRoutines.TryGetValue(notifyView, out var routine))
                {
                    StopCoroutine(routine);
                    _lifetimeRoutines.Remove(notifyView);
                }
            }

            _activeNotifyViews.Remove(notifyView);

            var targetAnchoredPosition = notifyView.RectTransform.anchoredPosition + Vector2.up * 40f;

            DOTween.Sequence()
                .Join(notifyView.CanvasGroup.DOFade(0f, Duration))
                .Join(notifyView.RectTransform.DOAnchorPos(targetAnchoredPosition, Duration).SetEase(Ease.OutCubic))
                .OnComplete(() =>
                {
                    JxModule.ObjectPoolManager.Instance.Return(notifyView.gameObject);
                });
        }

        private void MoveToSlot(NotifyView notifyView, Vector2 position)
        {
            notifyView.RectTransform.DOAnchorPos(position, Duration).SetEase(Ease.OutCubic);
        }

        private Vector2 GetSlotPosition(int slotIndex)
        {
            return new Vector2(0f, slotIndex * Spacing);
        }

        private IEnumerator LifetimeRoutine(NotifyView notifyView, float lifetime)
        {
            yield return new WaitForSeconds(lifetime);
            _lifetimeRoutines.Remove(notifyView);
            FadeOut(notifyView);
        }
    }
}