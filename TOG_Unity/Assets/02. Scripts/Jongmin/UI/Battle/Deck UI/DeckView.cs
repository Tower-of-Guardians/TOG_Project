using DG.Tweening;
using JxModule;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Jongmin
{
    public class DeckView : MonoBehaviour
    {
        [Header("Inner References")]
        [SerializeField] private CanvasGroup deckGroup;
        [SerializeField] private Transform cardRoot;
        [SerializeField] private TMP_Text titleNameLabel;
        [SerializeField] private TMP_Text drawCardLabel;
        [SerializeField] private TMP_Text discardCardLabel;
        
        [Header("Outer References")]
        [SerializeField] private Button drawDeckButton;
        [SerializeField] private Button discardDeckButton;
        [SerializeField] private Button closeButton;
        
        public Transform CardRoot => cardRoot;

        public void Bind(DeckSystem deckSystem)
        {
            drawDeckButton.onClick.AddListener(deckSystem.OpenDrawView);
            discardDeckButton.onClick.AddListener(deckSystem.OpenDiscardView);
            closeButton.onClick.AddListener(deckSystem.CloseView);
        }

        public void Show(string titleString)
        {
            titleNameLabel.text = titleString;
            deckGroup.Show();
        }

        public void Hide()
        {
            deckGroup.Hide();
        }

        public void UpdateDrawCardCount(int amount)
        {
            drawCardLabel.text = $"{amount}";
        }

        public void UpdateDiscardCardCount(int amount)
        {
            discardCardLabel.text = $"{amount}";
        }
    }
}