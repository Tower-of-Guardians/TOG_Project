using TMPro;
using UnityEngine;

public class InventorySortUI : MonoBehaviour, IInventorySortUI
{
    [Header("UI References")]
    [SerializeField] private BoxButton leftButton;
    [SerializeField] private BoxButton rightButton;
    [SerializeField] private ToggleButton criterionButton;
    [SerializeField] private TMP_Text sortLabel;
    
    public void Construct(InventorySortPresenter inventorySortPresenter)
    {
        leftButton.OnClick += inventorySortPresenter.OnClickedLeftButton;
        rightButton.OnClick += inventorySortPresenter.OnClickedRightButton;
        criterionButton.OnClick += inventorySortPresenter.OnClickedCriterionButton;
    } 

    public void UpdateSortLabel(string sortText)
        => sortLabel.text = sortText;

    public void ResetCriterion()
    {
        criterionButton.Reset();
    }
}
