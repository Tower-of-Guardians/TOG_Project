using System;
using UnityEngine;

namespace Jongmin
{
    public class CardInfoSystem : MonoBehaviour
    {
        private CardInfoView _view;
            
        public CardData CardData { get; private set; } 

        public void Construct(CardInfoView view)
        {
            _view = view;
        }
        
        public void OpenView(CardData cardData)
        {
            CardData = cardData;
         
            var cardDatas = DataCenter.Instance.GetSeriesCards(cardData.id);
            _view.Show();
            _view.ToggleActiveToggle(cardDatas.Count > 1);
        }

        public void CloseView()
        {
            _view.Hide();
        }
    }
}