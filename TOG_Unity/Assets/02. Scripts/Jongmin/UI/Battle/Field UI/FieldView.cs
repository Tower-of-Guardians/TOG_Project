using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace Jongmin
{
    public class FieldView : MonoBehaviour
    {
        [SerializeField] private Transform cardRoot;
        [SerializeField] private Image disableImage;
        [SerializeField] private PreviewCard previewCard;

        public Transform CardRoot => cardRoot;
        private Tween _disableTween;
        
        public void TogglePreview(bool isActive)
        {
            previewCard.gameObject.SetActive(isActive);
            previewCard.transform.SetAsFirstSibling();
        }

        public Tween ToggleViewActive(bool isActive)
        {
            _disableTween?.Kill();
            _disableTween = disableImage.DOFade(isActive ? 0f : 0.7f, 0.3f);

            return _disableTween;
        }
    }
}