using DG.Tweening;
using JxModule;
using UnityEngine;
using UnityEngine.UI;

namespace Jongmin
{
    public class FieldView : MonoBehaviour
    {
        [SerializeField] private Transform cardRoot;
        [SerializeField] private LabelBoxView fieldStatusView;
        [SerializeField] private Image disableImage;
        [SerializeField] private PreviewCard previewCard;

        public Transform CardRoot => cardRoot;
        private Tween _disableTween;
        private Tween _countTween;

        private void Awake()
        {
            fieldStatusView.Label.text = "0";
        }
        
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

        public void UpdateStatus(float currentValue, float targetValue)
        {
            var source = (int)currentValue;
            var destination = (int)targetValue;
            
            _countTween?.Kill();

            _countTween = DOTween.To(
                () => source,
                value =>
                {
                    source = value;
                    fieldStatusView.Label.text = $"{value}";
                },
                destination,
                0.5f
            ).SetEase(Ease.OutCubic);

            fieldStatusView.Label.text = $"{destination}";
        }
    }
}