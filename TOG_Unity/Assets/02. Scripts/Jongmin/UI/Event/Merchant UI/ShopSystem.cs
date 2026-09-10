using System;
using UnityEngine;

namespace Jongmin
{
    public class ShopSystem : MonoBehaviour
    {
        private ShopView _view;
        
        public void Construct(ShopView view)
        {
            _view = view;
        }

        public void OpenView()
        {
            _view.Show();
        }

        public void CloseView()
        {
            _view.Hide();
        }

        public void SetSaleButtonState(bool canEnter)
        {
            _view.SetSaleButtonState(canEnter);
        }
    }
}