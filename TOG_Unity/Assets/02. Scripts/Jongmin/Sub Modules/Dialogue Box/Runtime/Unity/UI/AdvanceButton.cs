using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace JxDialogueBox
{
    [RequireComponent(typeof(Button))]
    public class AdvanceButton : MonoBehaviour
    {
        private Button _advanceButton;
        private RectTransform _buttonRect;

        private Tween _highlightTween;
        private Tween _visibleTween;

        private Vector2 _originAnchoredPosition;

        public bool Hiding { get; private set; }

        private void Awake()
        {
            _advanceButton = GetComponent<Button>();
            _buttonRect = _advanceButton.transform as RectTransform;

            if (_buttonRect != null)
            {
                _originAnchoredPosition = _buttonRect.anchoredPosition;
            }
        }

        private void OnDestroy()
        {
            _highlightTween?.Kill();
            _visibleTween?.Kill();
        }

        public void AddListener(UnityAction advanceAction)
        {
            if (_advanceButton == null)
            {
                return;
            }

            _advanceButton.onClick.RemoveAllListeners();

            if (advanceAction != null)
            {
                _advanceButton.onClick.AddListener(advanceAction);
            }
        }

        public void SetHighlight()
        {
            ToggleHighlight(true);
        }

        public void SetNormal()
        {
            ToggleHighlight(false);
        }

        public void Show()
        {
            ToggleVisible(true);
        }

        public void Hide()
        {
            ToggleVisible(false);
        }

        private void ToggleHighlight(bool isActive)
        {
            _highlightTween?.Kill();
            _highlightTween = null;

            if (_buttonRect == null)
            {
                return;
            }

            if (!isActive)
            {
                _buttonRect.DOAnchorPos(_originAnchoredPosition, 0.15f);
                return;
            }

            _highlightTween = _buttonRect
                .DOAnchorPosY(_originAnchoredPosition.y + 10f, 0.3f)
                .SetEase(Ease.InOutSine)
                .SetLoops(-1, LoopType.Yoyo);
        }

        private void ToggleVisible(bool isActive)
        {
            Hiding = !isActive;

            if (_advanceButton != null)
            {
                _advanceButton.interactable = isActive;
            }

            if (_advanceButton == null || _advanceButton.image == null)
            {
                return;
            }

            _visibleTween?.Kill();

            _visibleTween = _advanceButton.image
                .DOFade(isActive ? 1f : 0f, 0.3f)
                .SetEase(Ease.OutQuad);
        }
    }
}