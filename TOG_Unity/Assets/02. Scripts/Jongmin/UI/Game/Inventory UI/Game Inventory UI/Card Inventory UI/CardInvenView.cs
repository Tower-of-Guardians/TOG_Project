using DG.Tweening;
using JxModule;
using UnityEngine;

namespace Jongmin
{
    public class CardInvenView : ViewBase
    {
        [SerializeField] private Transform cardRoot;
        
        public Transform CardRoot => cardRoot;
        
        public void Show()
        {
            CanvasGroup.Show();
        }

        public void Hide()
        {
            CanvasGroup.Hide();
        }
    }
}