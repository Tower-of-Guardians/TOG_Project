using UnityEngine;

namespace Jongmin
{
    public class CardInfoDomain : MonoBehaviour
    {
        [SerializeField] private CardInfoView cardInfoView;
        [SerializeField] private AttributeView attributeView;
        [SerializeField] private SeriesView seriesView;

        [SerializeField] private CardInfoSystem cardInfoSystem;
        [SerializeField] private AttributeSystem attributeSystem;
        [SerializeField] private SeriesSystem seriesSystem;
        
        public CardInfoSystem System => cardInfoSystem;

        public void Construct()
        {
            cardInfoSystem.Construct(cardInfoView);
            attributeSystem.Construct(attributeView);
            seriesSystem.Construct(seriesView);

            BindEvents();
        }

        public void BindEvents()
        {
            cardInfoView.OnToggleValueChanged += HandleEnhancementToggleValueChanged;
            cardInfoView.Bind(cardInfoSystem);
        }

        public void ReleaseEvents()
        {
            cardInfoView.OnToggleValueChanged -= HandleEnhancementToggleValueChanged;
        }

        private void HandleEnhancementToggleValueChanged(bool isOn)
        {
            if (isOn)
            {
                attributeSystem.CloseView();
                seriesSystem.OpenView(cardInfoSystem.CardData);
            }
            else
            {
                attributeSystem.OpenView(cardInfoSystem.CardData);
                seriesSystem.CloseView();
            }
        }

        private void OnDestroy()
        {
            ReleaseEvents();
        }
    }
}