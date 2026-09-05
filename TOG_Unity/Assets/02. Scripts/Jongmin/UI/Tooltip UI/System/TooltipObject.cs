using JxModule;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Jongmin
{
    public class TooltipObject : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [BigHeader("Settings")]
        [SerializeField] private Vector2 tooltipOffset;

        private ITooltipProvider _tooltipProvider;
        private bool _isPointerOver;

        private void Awake()
        {
            _tooltipProvider = GetComponent<ITooltipProvider>();
            if (_tooltipProvider == null)
            {
                enabled = false;
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            _isPointerOver = true;
            Show();
        }

        private void LateUpdate()
        {
            Refresh();
        }

        public void Refresh()
        {
            if (!_isPointerOver)
            {
                return;
            }

            if (_tooltipProvider is not { CanShowTooltip: true })
            {
                TooltipSystem.Instance?.Hide();
                return;
            }

            var content = _tooltipProvider.GetTooltipContent();
            if (content is not { IsValid: true })
            {
                TooltipSystem.Instance?.Hide();
                return;
            }

            var tooltipPresenter = TooltipSystem.Instance;
            tooltipPresenter?.Refresh(content);
            tooltipPresenter?.Move(GetScreenPosition(), tooltipOffset);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            _isPointerOver = false;
            TooltipSystem.HideIfExists();
        }

        private void Show()
        {
            if (_tooltipProvider is not { CanShowTooltip: true })
            {
                TooltipSystem.Instance?.Hide();
                return;
            }

            var content = _tooltipProvider.GetTooltipContent();
            if (content is not { IsValid: true })
            {
                TooltipSystem.Instance?.Hide();
                return;
            }

            TooltipSystem.Instance?.Show(content, GetScreenPosition(), tooltipOffset);
        }

        private Vector2 GetScreenPosition()
        {
            var camera = Camera.main;
            return camera != null ? camera.WorldToScreenPoint(transform.position) : transform.position;
        }

        private void OnDisable()
        {
            _isPointerOver = false;
            TooltipSystem.HideIfExists();
        }
    }
}
