using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Jongmin
{
    public static class CardDragRaycastResolver
    {
        public static IReadOnlyList<RaycastResult> Raycast(Vector2 position,
                                                           GameObject pointerDrag,
                                                           out PointerEventData eventData)
        {
            eventData = new PointerEventData(EventSystem.current)
            {
                position = position,
                pointerDrag = pointerDrag
            };

            var rayHits = new List<RaycastResult>();
            EventSystem.current.RaycastAll(eventData, rayHits);
            return rayHits;
        }

        public static Card GetCard(RaycastResult hit)
        {
            return hit.gameObject.GetComponentInParent<Card>();
        }

        public static T GetComponentInParent<T>(RaycastResult hit) where T : Component
        {
            return hit.gameObject.GetComponentInParent<T>();
        }

        public static IDropHandler GetDropHandler(RaycastResult hit, out GameObject handlerObject)
        {
            handlerObject = ExecuteEvents.GetEventHandler<IDropHandler>(hit.gameObject);
            if (handlerObject == null)
            {
                return null;
            }

            var components = handlerObject.GetComponents<Component>();
            foreach (var component in components)
            {
                if (component is IDropHandler dropHandler)
                {
                    return dropHandler;
                }
            }

            return null;
        }

        public static bool IsInsideScreen(Vector2 position)
        {
            return position.x >= 0f
                && position.x <= Screen.width
                && position.y >= 0f
                && position.y <= Screen.height;
        }

        public static bool TrySetAnchoredPositionFromScreenPoint(RectTransform rectTransform,
                                                                 Vector2 screenPosition,
                                                                 Camera eventCamera)
            => TrySetAnchoredPositionFromScreenPoint(rectTransform, screenPosition, eventCamera, Vector2.zero);

        public static bool TrySetAnchoredPositionFromScreenPoint(RectTransform rectTransform,
                                                                 Vector2 screenPosition,
                                                                 Camera eventCamera,
                                                                 Vector2 offset)
        {
            if (!TryGetLocalPointInParent(rectTransform, screenPosition, eventCamera, out var localPosition))
            {
                return false;
            }

            rectTransform.anchoredPosition = localPosition + offset;
            return true;
        }

        public static bool TryGetPointerOffset(RectTransform rectTransform,
                                               Vector2 screenPosition,
                                               Camera eventCamera,
                                               out Vector2 offset)
        {
            offset = Vector2.zero;

            if (!TryGetLocalPointInParent(rectTransform, screenPosition, eventCamera, out var localPosition))
            {
                return false;
            }

            offset = rectTransform.anchoredPosition - localPosition;
            return true;
        }

        private static bool TryGetLocalPointInParent(RectTransform rectTransform,
                                                     Vector2 screenPosition,
                                                     Camera eventCamera,
                                                     out Vector2 localPosition)
        {
            localPosition = Vector2.zero;

            if (rectTransform == null || rectTransform.parent is not RectTransform parentRect)
            {
                return false;
            }

            var camera = ResolveEventCamera(parentRect, eventCamera);
            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(parentRect,
                                                                         screenPosition,
                                                                         camera,
                                                                         out localPosition))
            {
                return false;
            }

            return true;
        }

        private static Camera ResolveEventCamera(RectTransform rectTransform, Camera fallbackCamera)
        {
            var canvas = rectTransform.GetComponentInParent<Canvas>();
            if (canvas != null && canvas.renderMode == RenderMode.ScreenSpaceOverlay)
            {
                return null;
            }

            return fallbackCamera != null ? fallbackCamera : canvas != null ? canvas.worldCamera : null;
        }
    }
}
