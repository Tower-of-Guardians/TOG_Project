using System.Collections.Generic;
using UnityEngine;

namespace Jongmin
{
    public class AttributeSystem : MonoBehaviour
    {
        private AttributeView _view;

        public void Construct(AttributeView view)
        {
            _view = view;
        }

        public void OpenView(CardData cardData)
        {
            var synergyDatas = new List<SynergyData>();

            if (DataCenter.synergy_datas.TryGetValue(cardData.synergy1ID, out var synergyData1))
            {
                synergyDatas.Add(synergyData1);
            }
            
            if (DataCenter.synergy_datas.TryGetValue(cardData.synergy2ID, out var synergyData2))
            {
                synergyDatas.Add(synergyData2);
            }
            
            if (DataCenter.synergy_datas.TryGetValue(cardData.synergy3ID, out var synergyData3))
            {
                synergyDatas.Add(synergyData3);
            }
            
            _view.Show();
            _view.SetAttribute(cardData, synergyDatas);
        }
        
        public void CloseView()
        {
            _view.Hide();
        }
    }
}