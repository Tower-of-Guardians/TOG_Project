using VContainer;

public class DefendFieldPresenter : FieldPresenter, IDEFCardDropTarget, IDefendFieldCardRemovePort
{
    private readonly INotifierUI _notifier;

    public DefendFieldPresenter([Key(FieldType.Defense)]IFieldUI fieldUI,
                                [Key(FieldType.Defense)]CardContainer<IFieldCardUI, FieldCardPresenter> fieldCardContainer,
                                [Key(FieldType.Defense)]ICardFactory<IFieldCardUI> fieldCardFactory,
                                [Key(FieldType.Defense)]FieldCardLayoutController fieldCardLayout,
                                TurnManager turnManager,
                                INotifierUI notifier) 
        : base(fieldUI, fieldCardContainer, fieldCardFactory, fieldCardLayout, false, turnManager)
    {
        _notifier = notifier;
    }

    public override void CreateCard(BattleCardData cardData)
    {
        if(!CanAdd)
        {
            _notifier.Notify("<color=red>방어 필드가 이미 가득 차 있습니다.</color>");
            return;
        }

        base.CreateCard(cardData);
        AddToDefField(cardData);
    }

    public bool IsExist(IFieldCardUI cardUI)
        => _fieldCardContainer.IsExist(cardUI);

    private void AddToDefField(BattleCardData battleCardData)
        => GameData.Instance.defenseField.Add(battleCardData.data);
}
