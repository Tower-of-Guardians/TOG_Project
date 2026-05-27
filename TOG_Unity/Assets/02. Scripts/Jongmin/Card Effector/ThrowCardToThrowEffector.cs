using UnityEngine;
using DG.Tweening;
using System.Collections.Generic;
using VContainer;

public class ThrowCardToThrowEffector : CardEffector
{
    private IDiscardCardRemovePort _discardCardRemovePort;
    private CardContainer<IDiscardCardUI, DiscardCardPresenter> _discardCardContainer;

    [Inject]
    private void Construct(IDiscardCardRemovePort discardCardRemovePort,
                           CardContainer<IDiscardCardUI, DiscardCardPresenter> discardCardContainer)
    {
        _discardCardRemovePort = discardCardRemovePort;
        _discardCardContainer = discardCardContainer;

        _tempCardSettings = new()
        {
            Duration = 0.5f,

            UseJump = true,
            JumpPower = -50f,
            MoveEase = Ease.InQuad,

            UseScale = true,
            Scale = Vector3.zero,
            ScaleEase = Ease.InQuad,

            UseRotation = false,

            UseOpacity = true,
            Opacity = 0.5f,
            OpacityEase = Ease.Unset,

            ForceStartScale = true,
            StartScale = Vector3.one * 0.44f,

            ForceStartRotation = false,
        };

        _tempCardAnimeRequest = new()
        {
            EndPosition = _endTransform == null ? Vector3.zero : _endTransform.position,

            Interval = 0.05f,

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
    {
        GameData.Instance.UseCard(battleCardData.data.id);
        GameData.Instance.InvokeDeckCountChange(DeckType.Throw);
    }
}
