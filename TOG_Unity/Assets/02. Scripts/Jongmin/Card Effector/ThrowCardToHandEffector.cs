using UnityEngine;
using DG.Tweening;
using System.Collections.Generic;
using VContainer;

public class ThrowCardToHandEffector : CardEffector
{
    private IDiscardCardRemovePort _discardCardRemovePort;
    private CardContainer<IDiscardCardUI, DiscardCardPresenter> _discardCardContainer;
    private IHandCardCreatePort _handCardCreatePort;

    [Inject]
    private void Construct(IDiscardCardRemovePort discardCardRemovePort,
                          CardContainer<IDiscardCardUI, DiscardCardPresenter> discardCardContainer,
                          IHandCardCreatePort handCardCreatePort)
    {
        _discardCardRemovePort = discardCardRemovePort;
        _discardCardContainer = discardCardContainer;
        _handCardCreatePort = handCardCreatePort;

        _tempCardSettings = new()
        {
            Duration = 0.35f,

            UseJump = true,
            JumpPower = 0f,
            MoveEase = Ease.InQuad,

            UseScale = true,
            Scale = Vector3.zero,
            ScaleEase = Ease.InQuad,

            UseRotation = false,

            ForceStartScale = true,
            StartScale = Vector3.one * 0.66f,

            ForceStartRotation = false,
        };

        _tempCardAnimeRequest = new()
        {
            EndPosition = _endTransform == null ? Vector3.zero : _endTransform.position,

            Interval = 0f,

            Settings = _tempCardSettings,
        };
    }

    public override void Execute()
    {
        _tempCardAnimeRequest.CardDatas = _discardCardContainer.GetAllDatas();

        List<Vector3> discardCardPositionList = new();
        if(!_discardCardContainer.TryGetAllUIs(out IDiscardCardUI[] discardCardUIArray))
        {
            return;
        }

        foreach(IDiscardCardUI cardUI in discardCardUIArray)
        {
            DiscardCardUI concreteCardUI = cardUI as DiscardCardUI;
            discardCardPositionList.Add(concreteCardUI.transform.position);
        }

        _tempCardAnimeRequest.StartPositions = discardCardPositionList.ToArray(); 

        base.Execute();
    }

    protected override void OnTempCardAnimeStart(BattleCardData battleCardData)
        => _discardCardRemovePort.TryRemoveCard(battleCardData);

    protected override void OnTempCardAnimeEnd(BattleCardData battleCardData)
        => _handCardCreatePort.CreateCard(battleCardData);
}
