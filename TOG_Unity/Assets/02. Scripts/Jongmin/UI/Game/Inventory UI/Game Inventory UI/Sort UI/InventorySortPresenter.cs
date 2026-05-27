public class InventorySortPresenter
{
    private readonly IInventorySortUI _inventorySortUI;
    private readonly CardInventoryUI _cardInventoryUI;

    private SortType _currentSortType = SortType.Time;
    private bool _isAscending;

    public InventorySortPresenter(IInventorySortUI inventorySortUI,
                                  CardInventoryUI cardInventoryUI)
    {
        _inventorySortUI = inventorySortUI;
        _cardInventoryUI = cardInventoryUI;

        _inventorySortUI.Construct(this);
        UpdateSortText();
    }

    public void Initialize()
    {
        ResetSorting();
        ResetCriterion();
        CommitChange();
    }

    private void SetPrevSortType()
    {
        _currentSortType = _currentSortType switch
        {
            SortType.Time       => SortType.Defense,
            SortType.Grade      => SortType.Time,
            SortType.Attack     => SortType.Grade,
            SortType.Defense    => SortType.Attack,
            _                   => SortType.Time
        };
    }

    private void SetNextSortType()
    {
        _currentSortType = _currentSortType switch
        {
            SortType.Time       => SortType.Grade,
            SortType.Grade      => SortType.Attack,
            SortType.Attack     => SortType.Defense,
            SortType.Defense    => SortType.Time,
            _                   => SortType.Time
        };
    }

    private void UpdateSortText()
    {
        var sortText = _currentSortType switch
        {
            SortType.Time       => "획득순",
            SortType.Grade      => "등급순",
            SortType.Attack     => "공격력순",
            SortType.Defense    => "보호력순",
            _                   => "획득순"
        };        

        _inventorySortUI.UpdateSortLabel(sortText);
    }

    public void OnClickedLeftButton()
    {
        SetPrevSortType();
        UpdateSortText();
        CommitChange();
    }

    public void OnClickedRightButton()
    {
        SetNextSortType();
        UpdateSortText();
        CommitChange();
    }

    public void OnClickedCriterionButton()
    {
        _isAscending = !_isAscending;
        CommitChange();
    }

    private void CommitChange()
    {   
        DataCenter.Instance.SortUserCards(_currentSortType);
        _cardInventoryUI.RefreshCardInventory();
    }

    private void ResetSorting()
    {
        _currentSortType = SortType.Time;
        UpdateSortText();
    }

    private void ResetCriterion()
    {
        _isAscending = false;
        _inventorySortUI.ResetCriterion();
    }
}
