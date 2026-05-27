using UnityEngine;

namespace JxModule
{
    public static class CanvasGroupExtension
    {
        public static void Show(this CanvasGroup canvasGroup)
        {
            canvasGroup.alpha = 1f;
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
        }

        public static void Hide(this CanvasGroup canvasGroup)
        {
            canvasGroup.alpha = 0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }

        public static void SetVisible(this CanvasGroup canvasGroup, bool visible)
        {
            if (visible)
            {
                canvasGroup.Show();
            }
            else
            {
                canvasGroup.Hide();
            }
        }

        public static void SetInteractable(this CanvasGroup canvasGroup, bool interactable)
        {
            canvasGroup.interactable = interactable;
            canvasGroup.blocksRaycasts = interactable;
        }

        public static void SetAlpha(this CanvasGroup canvasGroup, float alpha)
        {
            canvasGroup.alpha = Mathf.Clamp01(alpha);
        }
    }
}