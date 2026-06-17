using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Jongmin
{
    public class GachaSystem : MonoBehaviour
    {
        private GachaView _view;
        private GachaSlotFactory _factory;
        
        private const int MinRefreshCost = 5;
        private const int MaxRefreshCost = 20;
        private const int GrowthRefreshCost = 5;

        private List<BattleCardData> _datas;
        private List<GachaSlot> _slots = new();
        private int _currentRefreshCost;
        private bool _isCreatingSlots;

        public void Construct(GachaView view, GachaSlotFactory factory)
        {
            _view = view;
            _factory = factory;
        }

        public IEnumerator OpenView()
        {
            InitRefreshCost();
            SetSlotDatas();
            SetRateFromData();
            UpdateRefreshState(DataCenter.Instance.playerstate.money);

            yield return _view.Show();
        }

        public void CloseView()
        {
            _view.Hide();
        }

        public IEnumerator CreateSlots()
        {
            _isCreatingSlots = true;
            UpdateRefreshState(DataCenter.Instance.playerstate.money);
            
            foreach (var data in _datas)
            {
                var slot = _factory.Create();
                slot.Initialize(data.data, DataCenter.Instance.playerstate.money);
                _slots.Add(slot);

                yield return new WaitForSeconds(0.25f);
            }

            _isCreatingSlots = false;
            UpdateRefreshState(DataCenter.Instance.playerstate.money);
        }

        public void RemoveSlots()
        {
            var tempSlots = new List<GachaSlot>(_slots);
            
            foreach(var slot in tempSlots)
            {
                _slots.Remove(slot);
                _factory.Release(slot);
            }
        }

        public void UpdateRefreshState(int gold)
        {
            var canRefresh = !_isCreatingSlots && gold >= _currentRefreshCost;
            _view.SetRefreshState(_currentRefreshCost, canRefresh);
        }

        public void Refresh()
        {
            if (DataCenter.Instance.playerstate.money < _currentRefreshCost)
            {
                return;
            }

            DataCenter.Instance.SetMoney(-_currentRefreshCost);

            RemoveSlots();
            SetSlotDatas();
            UpdateRefreshCost();

            UpdateRefreshState(DataCenter.Instance.playerstate.money);

            StartCoroutine(CreateSlots());
        }
        
        public void UpdateSlotsState(int gold)
        {
            foreach (var slot in _slots)
            {
                slot.UpdateState(gold);
            }
        }

        private void InitRefreshCost()
        {
            _currentRefreshCost = MinRefreshCost;
        }

        private void UpdateRefreshCost()
        {
            _currentRefreshCost += GrowthRefreshCost;
            _currentRefreshCost = Mathf.Clamp(_currentRefreshCost, MinRefreshCost, MaxRefreshCost);
            Debug.Log(_currentRefreshCost);
        }

        private void SetSlotDatas()
        {
            _datas = GameData.Instance.GetResultItems();
        }

        private void SetRateFromData()
        {
            var colors = new List<string>{ "828282", "4AA8D8", "FEFD48", "F06464" };
            var rates = GameData.Instance.GetResultPercent();

            var builder = new StringBuilder();
            for (var i = 0; i < rates.Count; i++)
            {
                builder.Append(i < rates.Count - 1 ? $"<color=#{colors[i]}>{rates[i]}%</color>   "
                                                   : $"<color=#{colors[i]}>{rates[i]}%</color>");
            }
            
            _view.SetRate(builder.ToString());
        }
    }
}