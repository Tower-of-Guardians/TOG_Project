using System;

public class CardDropSystem
{
    private readonly ICardDropTarget<IHandCardUI> _handDropTarget;
    private readonly IATKCardDropTarget _atkFieldDropTarget;
    private readonly IDEFCardDropTarget _defFieldDropTarget;
    private readonly ICardDropTarget<IDiscardCardUI> _discardDropTarget;

    private readonly TurnManager _turnManager;
    private readonly INotifierUI _notifier;

    public CardDropSystem(ICardDropTarget<IHandCardUI> handDropTarget,
                          IATKCardDropTarget atkfieldDropTarget,
                          IDEFCardDropTarget defFieldDropTarget,
                          ICardDropTarget<IDiscardCardUI> discardDropTarget,
                          TurnManager turnManager,
                          INotifierUI notifier)
    {
        _handDropTarget = handDropTarget;
        _atkFieldDropTarget = atkfieldDropTarget;
        _defFieldDropTarget = defFieldDropTarget;
        _discardDropTarget = discardDropTarget;
        _turnManager = turnManager;
        _notifier = notifier;
    }

    /// <summary>
    /// 해당 필드 카드를 [공격/방어 필드]에서 [핸드 필드]로 내립니다.
    /// </summary>
    public void OnDropedFieldToHand(IFieldCardUI cardUI)
    {
        bool isAtkFieldCard = _atkFieldDropTarget.IsExist(cardUI);
        var sourceFieldDropTarget = isAtkFieldCard ? (ICardDropTarget<IFieldCardUI>)_atkFieldDropTarget
                                                   : _defFieldDropTarget;

        if(!sourceFieldDropTarget.TryGetBattleCardData(cardUI, out BattleCardData battleCardData))
        {
            return;
        }

        if (isAtkFieldCard)
        {
            GameData.Instance.attackField.Remove(battleCardData.data);
        }
        else
        {
            GameData.Instance.defenseField.Remove(battleCardData.data);
        }

        GameData.Instance.FieldToHandMove(battleCardData);

        if(isAtkFieldCard)
        {
            _atkFieldDropTarget.RemoveCard(cardUI);
        }
        else
        {
            _defFieldDropTarget.RemoveCard(cardUI);
        }

        _handDropTarget.CreateCard(battleCardData);        
    }

    /// <summary>
    /// 해당 교체 카드를 [교체 필드]에서 [핸드 필드]로 내립니다. 
    /// </summary>
    public void OnDropedDiscardToHand(IDiscardCardUI cardUI)
    {
        if(!_discardDropTarget.TryGetBattleCardData(cardUI, out BattleCardData battleCardData))
        {
            return;
        }

        GameData.Instance.FieldToHandMove(battleCardData);

        _discardDropTarget.RemoveCard(cardUI);
        _handDropTarget.CreateCard(battleCardData);
    }

    /// <summary>
    /// 해당 핸드 카드를 [핸드 필드]에서 [공격/방어 필드]로 올립니다.
    /// </summary>
    public void OnDropedHandToField(IHandCardUI cardUI, bool isAtk)
    {
        if(isAtk && !_atkFieldDropTarget.CanInteraction)
        {
            return;
        }
        
        if(!isAtk && !_defFieldDropTarget.CanInteraction)
        {
            return;
        }

        if(!_turnManager.CanAction)
        {
            _notifier.Notify("<color=red>더 이상 행동할 수 없습니다.</color>");
            return;
        }

        if(!_handDropTarget.TryGetBattleCardData(cardUI, out BattleCardData battleCardData))
        {
            return;
        }

        if(isAtk)
        {
            _atkFieldDropTarget.CreateCard(battleCardData);
        }
        else
        {
            _defFieldDropTarget.CreateCard(battleCardData);
        }

        _handDropTarget.RemoveCard(cardUI);
        GameData.Instance.HandToFieldMove(battleCardData);
    }

    /// <summary>
    /// 해당 핸드 카드를 [핸드 필드]에서 [교체 필드]로 올립니다.
    /// </summary>
    public void OnDropedHandToDiscard(IHandCardUI cardUI)
    {
        if(!_turnManager.CanThrow)
        {
            _notifier.Notify("<color=red>더 이상 버릴 수 없습니다.</color>");
            return;
        }

        if(!_handDropTarget.TryGetBattleCardData(cardUI, out BattleCardData battleCardData))
        {
            return;
        }

        _discardDropTarget.CreateCard(battleCardData);
        _handDropTarget.RemoveCard(cardUI);
        GameData.Instance.HandToFieldMove(battleCardData);
    }
}
