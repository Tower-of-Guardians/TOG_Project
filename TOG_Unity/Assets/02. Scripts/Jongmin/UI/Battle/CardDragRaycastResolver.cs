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
    }
}
