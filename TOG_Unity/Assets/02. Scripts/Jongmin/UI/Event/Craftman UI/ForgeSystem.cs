using JxModule.DataTable;
using UnityEngine;

namespace Jongmin
{
    public class ForgeSystem : MonoBehaviour
    {
        private ForgeView _view;
        private DataTable _forgeDataTable;

        public void Construct(ForgeView view, DataTable forgeDataTable)
        {
            _view = view;
            _forgeDataTable = forgeDataTable;
        }

        public void OpenView(CardData cardData)
        {
            _view.Show(cardData);
        }

        public void CloseView()
        {
            _view.Hide();
        }

        public void UpgradeAtkRate()
        {
            // TODO: 현재 스테이지만큼으로 조정
            var forgeData = _forgeDataTable.Find<ForgeDataTableRow>(x => x.stage == 1);
            var atkRate = forgeData.growthAtk;
            _view.Card.CardData.ATK += atkRate;
            
            _view.UpgradeAtkRate();
        }
        
        public void UpgradeBothRate()
        {
            // TODO: 현재 스테이지만큼으로 조정
            var forgeData = _forgeDataTable.Find<ForgeDataTableRow>(x => x.stage == 1);
            var atkRate = forgeData.growthBoth[0];
            var defRate = forgeData.growthBoth[1];
            _view.Card.CardData.ATK += atkRate;
            _view.Card.CardData.DEF += defRate;
            
            _view.UpgradeBothRate();
        }

        public void UpgradeDefRate()
        {
            // TODO: 현재 스테이지만큼으로 조정
            var forgeData = _forgeDataTable.Find<ForgeDataTableRow>(x => x.stage == 1);
            var defRate = forgeData.growthDef;
            _view.Card.CardData.DEF += defRate;
            
            _view.UpgradeDefRate();
        }
    }
}