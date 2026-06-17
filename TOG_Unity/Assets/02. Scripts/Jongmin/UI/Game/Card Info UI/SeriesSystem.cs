using UnityEngine;

namespace Jongmin
{
    public class SeriesSystem : MonoBehaviour
    {
        private SeriesView _view;

        public void Construct(SeriesView view)
        {
            _view = view;
        }

        public void OpenView(CardData cardData)
        {
            var cardDatas = DataCenter.Instance.GetSeriesCards(cardData.id);
            
            _view.Show();
            _view.SetSeries(cardDatas);
        }

        public void CloseView()
        {
            _view.Hide();
        }
    }
}