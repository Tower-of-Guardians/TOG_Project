using VContainer;

public class AttackFieldPresenter : FieldPresenter, IATKCardDropTarget, IAttackFieldCardRemovePort
{
    private readonly INotifierUI _notifier;

    public AttackFieldPresenter([Key(FieldType.Attack)]IFieldUI fieldUI,
                                [Key(FieldType.Attack)]CardContainer<IFieldCardUI, FieldCardPresenter> fieldCardContainer,
                                [Key(FieldType.Attack)]ICardFactory<IFieldCardUI> fieldCardFactory,
                                [Key(FieldType.Attack)]FieldCardLayoutController fieldCardLayout,
                                TurnManager turnManager,
                                INotifierUI notifier) 
        : base(fieldUI, fieldCardContainer, fieldCardFactory, fieldCardLayout, true, turnManager)
    {
        _notifier = notifier;
    }

    public override void CreateCard(BattleCardData cardData)
    {
        if(!CanAdd)
        {
            _notifier.Notify("<color=red>공격 필드가 이미 가득 차 있습니다.</color>");
            return;
        }

        base.CreateCard(cardData);
        AddToAtkField(cardData);
    }

    public bool IsExist(IFieldCardUI cardUI)
        => _fieldCardContainer.IsExist(cardUI);

    private void AddToAtkField(BattleCardData battleCardData)
        => GameData.Instance.attackField.Add(battleCardData.data);
}
