using System;
using Jongmin;
using UnityEngine;

public class CardDropSystem
{
    private readonly ICardDropTarget<HandDomain> _handDropTarget;
    private readonly IATKCardDropTarget _atkFieldDropTarget;
    private readonly IDEFCardDropTarget _defFieldDropTarget;
    private readonly ICardDropTarget<DiscardDomain> _discardDropTarget;

    private readonly TurnManager _turnManager;
    private readonly NotifySystem _notifySystem;

    public CardDropSystem(ICardDropTarget<HandDomain> handDropTarget,
                          IATKCardDropTarget atkFieldDropTarget,
                          IDEFCardDropTarget defFieldDropTarget,
                          ICardDropTarget<DiscardDomain> discardDropTarget,
                          TurnManager turnManager,
                          NotifySystem notifySystem)
    {
        _handDropTarget = handDropTarget;
        _atkFieldDropTarget = atkFieldDropTarget;
        _defFieldDropTarget = defFieldDropTarget;
        _discardDropTarget = discardDropTarget;
        _turnManager = turnManager;
        _notifySystem = notifySystem;
    }

    /// <summary>
    /// 해당 필드 카드를 [공격/방어 필드]에서 [핸드 필드]로 내립니다.
    /// </summary>
    public void OnDroppedFieldToHand(Card card)
    {
        switch (card.CardType)
        {
            case CardType.AtkField:
                GameData.Instance.attackField.Remove(card.CardData);
                GameData.Instance.FieldToHandMove(card.BattleCardData);
                _handDropTarget.CreateCard(card.BattleCardData);
                _atkFieldDropTarget.RemoveCard(card);
                break;
            
            case CardType.DefField:
                GameData.Instance.defenseField.Remove(card.CardData);
                GameData.Instance.FieldToHandMove(card.BattleCardData);
                _handDropTarget.CreateCard(card.BattleCardData);
                _defFieldDropTarget.RemoveCard(card);
                break;
            
            case CardType.None:
            case CardType.Deck:
            case CardType.Hand:
            case CardType.Effect:
                break;
        }
    }

    /// <summary>
    /// 해당 교체 카드를 [교체 필드]에서 [핸드 필드]로 내립니다. 
    /// </summary>
    public void OnDroppedDiscardToHand(Card card)
    {
        GameData.Instance.FieldToHandMove(card.BattleCardData);
        _handDropTarget.CreateCard(card.BattleCardData);
        _discardDropTarget.RemoveCard(card);
    }

    /// <summary>
    /// 해당 핸드 카드를 [핸드 필드]에서 [공격/방어 필드]로 올립니다.
    /// </summary>
    public void OnDroppedHandToField(Card card, FieldType fieldType)
    {
        if(fieldType == FieldType.Attack && !_atkFieldDropTarget.CanInteraction)
        {
            return;
        }
        
        if(fieldType == FieldType.Defense && !_defFieldDropTarget.CanInteraction)
        {
            return;
        }
        
        if(!_turnManager.CanAction)
        {
            _notifySystem.Notify("<color=red>더 이상 행동할 수 없습니다.</color>");
            return;
        }

        switch (fieldType)
        {
            case FieldType.Attack:
                _atkFieldDropTarget.CreateCard(card.BattleCardData);
                break;
            
            case FieldType.Defense:
                _defFieldDropTarget.CreateCard(card.BattleCardData);
                break;
        }
        
        GameData.Instance.HandToFieldMove(card.BattleCardData);
        _handDropTarget.RemoveCard(card);
    }

    /// <summary>
    /// 해당 핸드 카드를 [핸드 필드]에서 [교체 필드]로 올립니다.
    /// </summary>
    public void OnDroppedHandToDiscard(Card card)
    {
        if(!_turnManager.CanThrow)
        {
            _notifySystem.Notify("<color=red>더 이상 버릴 수 없습니다.</color>");
            return;
        }
        
        _discardDropTarget.CreateCard(card.BattleCardData);
        GameData.Instance.HandToFieldMove(card.BattleCardData);
        _handDropTarget.RemoveCard(card);
    }
}
