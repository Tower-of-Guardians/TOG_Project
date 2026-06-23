using DG.Tweening;
using JxModule;
using TMPro;
using UnityEngine;

namespace Jongmin
{
    public class InventoryView : ViewBase
    {
        [SerializeField] private ButtonView toggleButton;
        [SerializeField] private TMP_Text titleLabel;

        public void Bind(InventorySystem system)
        {
            toggleButton.AddListener(system.ToggleView);
        }

        public void SetTitle(string titleText)
        {
            titleLabel.text = titleText;
        }

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