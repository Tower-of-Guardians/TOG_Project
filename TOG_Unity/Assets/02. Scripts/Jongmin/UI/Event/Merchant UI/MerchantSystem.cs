using System;
using UnityEngine;

namespace Jongmin
{
    public class MerchantSystem : MonoBehaviour
    {
        private MerchantView _view;

        public event Action RequestOpenView;
        public event Action RequestCloseView;

        public void Construct(MerchantView view)
        {
            _view = view;
        }

        public void OpenView()
        {
            _view.Show();
            RequestOpenView?.Invoke();
        }

        public void CloseView(Action onClosed = null)
        {
            _view.Hide(onClosed);
            RequestCloseView?.Invoke();
        }
    }
}