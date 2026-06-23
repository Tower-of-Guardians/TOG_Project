using System;
using UnityEngine;

namespace Jongmin
{
    public class CraftmanSystem : MonoBehaviour
    {
        private CraftmanView _view;

        public event Action RequestOpenView;
        public event Action RequestCloseView;

        public void Construct(CraftmanView view)
        {
            _view = view;
        }

        public void OpenView()
        {
            _view.Show();
            RequestOpenView?.Invoke();
        }

        public void CloseView()
        {
            _view.Hide();
            RequestCloseView?.Invoke();
        }
    }
}