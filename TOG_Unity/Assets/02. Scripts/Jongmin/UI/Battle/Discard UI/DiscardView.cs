using DG.Tweening;
using JxModule;
using UnityEngine;
using UnityEngine.UI;

namespace Jongmin
{
    public class DiscardView : MonoBehaviour
    {
        [Header("UI")] 
        [SerializeField] private CanvasGroup viewGroup;
        [SerializeField] private RectTransform viewPanel;
        [SerializeField] private Transform cardRoot;
        [SerializeField] private CanvasGroup manualGroup;
        [SerializeField] private CanvasGroup discardGroup;
        [SerializeField] private CanvasGroup closeGroup;
        [SerializeField] private Button openButton;
        [SerializeField] private PreviewCard previewCard;
        
        private Button _closeButton;
        private Button _discardButton;

        private Tween _toggleTween;
        
        public Transform CardRoot => cardRoot;

        private void Awake()
        {
            _closeButton = closeGroup.GetComponent<Button>();
            _discardButton = discardGroup.GetComponent<Button>();
        }

        public void Bind(DiscardSystem discardSystem)
        {
            openButton.onClick.AddListener(discardSystem.OpenView);
            _closeButton.onClick.AddListener(discardSystem.CloseView);
            _discardButton.onClick.AddListener(discardSystem.DiscardCards);
        }
        
        public void Show()
        {
            manualGroup.Hide();
            discardGroup.Hide();
            closeGroup.Hide();

            _toggleTween?.Kill();
            var sequence = DOTween.Sequence();

            sequence.Append(viewPanel.DOScaleY(0f, 0f));
            sequence.Append(viewPanel.DOScaleY(1f, 0.25f));
            sequence.Join(viewGroup.DOFade(1f, 0.25f));
            sequence.AppendCallback(() => viewGroup.Show());
            sequence.AppendCallback(() => manualGroup.Show());
            sequence.JoinCallback(() => discardGroup.Show());
            sequence.JoinCallback(() => closeGroup.Show());

            _toggleTween = sequence;
        }

        public void Hide()
        {
            manualGroup.Hide();
            discardGroup.Hide();
            closeGroup.Hide();
            
            _toggleTween?.Kill();
            var sequence = DOTween.Sequence();

            sequence.Append(viewPanel.DOScaleY(0f, 0.25f));
            sequence.Join(viewGroup.DOFade(0f, 0.25f));
            sequence.OnComplete(() => viewGroup.Hide());
            
            _toggleTween = sequence;
        }

        public void TogglePreview(bool isActive)
        {
            previewCard.gameObject.SetActive(isActive);
            previewCard.transform.SetAsFirstSibling();
        }

        public void UpdateOpenButtonState(bool isActive)
        {
            openButton.interactable = isActive;
        }

        public void UpdateDiscardButtonState(bool isActive)
        {
            _discardButton.interactable = isActive;
        }

        private void OnDestroy()
        {
            openButton.onClick.RemoveAllListeners();
            _closeButton.onClick.RemoveAllListeners();
            _discardButton.onClick.RemoveAllListeners();
        }
    }
}