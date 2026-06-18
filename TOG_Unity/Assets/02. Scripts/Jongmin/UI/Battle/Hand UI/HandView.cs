using JxModule;
using UnityEngine;

namespace Jongmin
{
    [RequireComponent(typeof(JxEmptyGraphic))]
    public class HandView : MonoBehaviour
    {
        [SerializeField] private Transform cardRoot;
        [SerializeField] private PreviewCard previewCard;

        public Transform CardRoot => cardRoot; 
        
        public void TogglePreview(bool isActive)
        {
            previewCard.gameObject.SetActive(isActive);
        }

        public void UpdatePreviewPosition(CardLayoutData layoutData)
        {
            previewCard.RectTransform.anchoredPosition = layoutData.position;
            previewCard.RectTransform.rotation = Quaternion.Euler(layoutData.rotation);
            previewCard.RectTransform.localScale = layoutData.scale;
        }
    }
}
