using JxModule;
using UnityEngine;

namespace Jongmin
{
    [RequireComponent(typeof(JxEmptyGraphic))]
    public class HandView : MonoBehaviour
    {
        [SerializeField] private Transform cardRoot;
        [SerializeField] private PreviewCard previewCard;
        [SerializeField] private CanvasGroup interactionGroup;

        public Transform CardRoot => cardRoot; 
        
        public void TogglePreview(bool isActive)
        {
            previewCard.gameObject.SetActive(isActive);
            previewCard.transform.SetAsFirstSibling();
        }

        public void UpdatePreviewPosition(CardLayoutData layoutData)
        {
            previewCard.RectTransform.anchoredPosition = layoutData.position;
            previewCard.RectTransform.rotation = Quaternion.Euler(layoutData.rotation);
            previewCard.RectTransform.localScale = layoutData.scale;
        }

        public void SetInteraction(bool isActive)
        {
            interactionGroup.blocksRaycasts = isActive;
            interactionGroup.interactable = isActive;
        }
    }
}
