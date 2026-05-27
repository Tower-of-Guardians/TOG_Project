using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DeckUI : MonoBehaviour, IDeckUI
{
    [Header("UI References")]
    [SerializeField] private Button drawDeckButton;
    [SerializeField] private Button discardDeckButton;
    [SerializeField] private Button closeButton;
    [SerializeField] private Animator animator;
    
    [Header("Text References")]
    [SerializeField] private TMP_Text titleNameLabel;
    [SerializeField] private TMP_Text drawCardBackLabel;
    [SerializeField] private TMP_Text drawCardLabel;
    [SerializeField] private TMP_Text discardCardBackLabel;
    [SerializeField] private TMP_Text discardCardLabel;

    private static readonly int CachedOpenParam = Animator.StringToHash("Open");

    public void Construct(DeckPresenter deckPresenter)
    {
        drawDeckButton.onClick.AddListener(() => { deckPresenter.OpenUI(DeckType.Draw); });
        discardDeckButton.onClick.AddListener(() => { deckPresenter.OpenUI(DeckType.Throw); });
        closeButton.onClick.AddListener(deckPresenter.CloseUI);
    }

    public void OpenUI()
        => ToggleActive(true);

    public void CloseUI()
        => ToggleActive(false);

    public void UpdateUI(string titleString)
        => titleNameLabel.text = titleString;

    public void UpdateDrawCardCount(int amount)
    {
        drawCardBackLabel.text = amount.ToString();
        drawCardLabel.text = amount.ToString();
    }
    
    public void UpdateThrowCardCount(int amount)
    {
        discardCardBackLabel.text = amount.ToString();
        discardCardLabel.text = amount.ToString();
    }
    
    private void ToggleActive(bool isActive)
        => animator.SetBool(CachedOpenParam, isActive);
}
