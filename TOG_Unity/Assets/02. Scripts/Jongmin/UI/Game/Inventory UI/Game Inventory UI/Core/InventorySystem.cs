using System;
using UnityEngine;

namespace Jongmin
{
    public class InventorySystem : MonoBehaviour
    {
        private bool _isActive;

        public Action RequestOpenView;
        public Action RequestCloseView;
        
        public void ToggleView()
        {
            _isActive = !_isActive;

            if (_isActive)
            {
                RequestOpenView?.Invoke();
            }
            else
            {
                RequestCloseView?.Invoke();
            }
        }
    }
}