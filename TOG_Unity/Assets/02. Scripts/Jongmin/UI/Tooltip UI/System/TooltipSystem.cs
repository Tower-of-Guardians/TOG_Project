using System.Collections.Generic;
using JxModule;
using JxModule.DataTable;
using UnityEngine;

namespace Jongmin
{
    public class TooltipSystem : LocalSingleton<TooltipSystem>
    {
        [BigHeader("UI")]
        [SerializeField] private Canvas tooltipCanvas;
        [SerializeField] private TooltipViewBase[] tooltipViews;

        private readonly Dictionary<ETooltipLayout, TooltipViewBase> _tooltipDict = new();

        private RectTransform _canvasRectTransform;
        private TooltipViewBase _currentTooltipView;

        public static void HideIfExists()
        {
            var tooltipSystem = FindFirstObjectByType<TooltipSystem>();
            if (tooltipSystem != null)
            {
                tooltipSystem.Hide();
            }
        }

        protected override void Awake()
        {
            base.Awake();

            tooltipCanvas ??= GetComponent<Canvas>();
            if (tooltipCanvas == null)
            {
                enabled = false;
                return;
            }
            
            _canvasRectTransform = tooltipCanvas.GetComponent<RectTransform>();
            Initialize();
        }

        private void Initialize()
        {
            _tooltipDict.Clear();

            if (tooltipViews == null)
            {
                return;
            }

            foreach (var tooltipView in tooltipViews)
            {
                if (tooltipView == null)
                {
                    continue;
                }
                
                var layout = tooltipView.Layout;
                if (layout == ETooltipLayout.None)
                {
                    tooltipView.HideImmediate();
                    continue;
                }

                if (!_tooltipDict.TryAdd(layout, tooltipView))
                {
                    Debug.LogError($"Duplicate tooltip layout: {layout}");   
                }
                
                tooltipView.HideImmediate();
            }
        }

        public void Show(TooltipContent tooltipContent,
                         Vector2 screenPosition,
                         Vector2 tooltipOffset)
        {
            if (tooltipContent is not { IsValid: true })
            {
                return;
            }

            var tooltipDataTableRow = GetTooltipDataTableRow(tooltipContent.TooltipId);
            if (tooltipDataTableRow == null)
            {
                Hide();
                return;
            }

            if (!TryGetTooltipView(tooltipDataTableRow.layout, out var tooltipView))
            {
                Hide();
                return;
            }
            
            if (_currentTooltipView != null && _currentTooltipView != tooltipView)
            {
                _currentTooltipView.HideImmediate();
            }
            
            _currentTooltipView = tooltipView;
            _currentTooltipView.RectTransform.SetAsLastSibling();
            _currentTooltipView.Show(tooltipDataTableRow, tooltipContent);
            
            Canvas.ForceUpdateCanvases();
            
            Move(screenPosition, tooltipOffset);
        }

        public void Refresh(TooltipContent tooltipContent)
        {
            if (_currentTooltipView == null)
            {
                return;
            }

            if (tooltipContent is not { IsValid: true })
            {
                Hide();
                return;
            }

            var tooltipDataTableRow = GetTooltipDataTableRow(tooltipContent.TooltipId);
            if (tooltipDataTableRow == null)
            {
                Hide();
                return;
            }

            if (_currentTooltipView.Layout != tooltipDataTableRow.layout)
            {
                return;
            }

            _currentTooltipView.Refresh(tooltipDataTableRow, tooltipContent);
            Canvas.ForceUpdateCanvases();
        }

        public void Move(Vector2 screenPosition, Vector2 tooltipOffset)
        {
            if (_currentTooltipView == null || tooltipCanvas == null || _canvasRectTransform == null)
            {
                return;
            }
            
            var tooltipRectTransform = _currentTooltipView.RectTransform;
            var canvasCamera = tooltipCanvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : tooltipCanvas.worldCamera;

            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(_canvasRectTransform,
                                                                         screenPosition,
                                                                         canvasCamera,
                                                                         out var localPosition))
            {
                return;
            }
            
            var finalPosition = ResolvePosition(tooltipRectTransform, localPosition, tooltipOffset);
            _currentTooltipView.RectTransform.anchoredPosition = finalPosition;
            
            FitToCanvas(tooltipRectTransform);
        }

        public void Hide()
        {
            if (_currentTooltipView == null)
            {
                return;
            }
            
            var tooltipView = _currentTooltipView;
            _currentTooltipView = null;
            
            tooltipView.Hide();
        }

        public void HideImmediate()
        {
            if (_currentTooltipView != null)
            {
                _currentTooltipView.HideImmediate();
                _currentTooltipView = null;
            }

            foreach (var tooltipView in _tooltipDict.Values)
            {
                tooltipView.HideImmediate();
            }
        }

        private bool TryGetTooltipView(ETooltipLayout tooltipLayout, out TooltipViewBase tooltipView)
        {
            if (tooltipLayout == ETooltipLayout.None)
            {
                tooltipView = null;
                return false;
            }

            return _tooltipDict.TryGetValue(tooltipLayout, out tooltipView);
        }

        private void FitToCanvas(RectTransform tooltipRectTransform)
        {
            var canvasRect = _canvasRectTransform.rect;
            var tooltipRect = tooltipRectTransform.rect;
            var position = tooltipRectTransform.anchoredPosition;
            var pivot = tooltipRectTransform.pivot;
            
            var left = position.x - tooltipRect.width * pivot.x;
            var right = position.x + tooltipRect.width * (1f - pivot.x);
            var bottom = position.y - tooltipRect.height * pivot.y;
            var top = position.y + tooltipRect.height * (1f - pivot.y);
            
            if (right > canvasRect.xMax)
            {
                position.x -= right - canvasRect.xMax;
            }
            else if (left < canvasRect.xMin)
            {
                position.x += canvasRect.xMin - left;
            }

            if (top > canvasRect.yMax)
            {
                position.y -= top - canvasRect.yMax;
            }
            else if (bottom < canvasRect.yMin)
            {
                position.y += canvasRect.yMin - bottom;
            }

            tooltipRectTransform.anchoredPosition = position;
        }

        private Vector2 ResolvePosition(RectTransform tooltipRectTransform, 
                                        Vector2 localPosition,
                                        Vector2 tooltipOffset)
        {
            var position = localPosition + tooltipOffset;
            var canvasRect = _canvasRectTransform.rect;
            var tooltipRect = tooltipRectTransform.rect;

            if (IsOverflowX(tooltipRectTransform, position, canvasRect, tooltipRect))
            {
                position.x = localPosition.x - tooltipOffset.x;
            }

            if (IsOverflowY(tooltipRectTransform, position, canvasRect, tooltipRect))
            {
                position.y = localPosition.y - tooltipOffset.y;
            }
            
            return position;
        }
        
        private static bool IsOverflowX(RectTransform tooltipRectTransform,
                                        Vector2 position,
                                        Rect canvasRect,
                                        Rect tooltipRect)
        {
            var pivot = tooltipRectTransform.pivot;
            var minX = position.x - tooltipRect.width * pivot.x;
            var maxX = position.x + tooltipRect.width * (1f - pivot.x);

            return minX < canvasRect.xMin || maxX > canvasRect.xMax;
        }

        private static bool IsOverflowY(RectTransform tooltipRectTransform,
                                        Vector2 position,
                                        Rect canvasRect,
                                        Rect tooltipRect)
        {
            var pivot = tooltipRectTransform.pivot;
            var minY = position.y - tooltipRect.height * pivot.y;
            var maxY = position.y + tooltipRect.height * (1f - pivot.y);

            return minY < canvasRect.yMin || maxY > canvasRect.yMax;
        }

        private static TooltipDataTableRow GetTooltipDataTableRow(string tooltipID)
        {
            return string.IsNullOrWhiteSpace(tooltipID) ? null : DataTableManager.FindRow<TooltipDataTableRow>(tooltipID);
        }
    }
}
