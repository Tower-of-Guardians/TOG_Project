using UnityEngine;
using DG.Tweening;

public class DrawCardEffector : CardEffector
{
    private IHandCardCreatePort _handCardCreatePort;
    private ITurnHandLimitPort _turnHandLimitPort;

    public void Inject(IHandCardCreatePort handCardCreatePort,
                       ITurnHandLimitPort turnHandLimitPort)
    {
        _handCardCreatePort = handCardCreatePort;
        _turnHandLimitPort = turnHandLimitPort;

        _tempCardSettings = new()
        {
            Duration = 0.25f,

            UseJump = true,
            JumpPower = 0f,
            MoveEase = Ease.InQuad,

            UseScale = true,
            Scale = Vector3.one * 0.4f,
            ScaleEase = Ease.Unset,

            UseRotation = false,

            ForceStartScale = true,
            StartScale = Vector3.one * 0.2f,

            ForceStartRotation = false,
        };

        _tempCardAnimeRequest = new()
        {
            StartPositions = null,
            StartPosition = _startTransform != null ? _startTransform.position : Vector3.zero,
            EndPosition = _endTransform != null ? _endTransform.position : Vector3.zero,

            Interval = 0.075f,
            Settings = _tempCardSettings,
        };
    }

    public override void Execute()
    {
        _tempCardAnimeRequest.CardDatas = GameData.Instance.NextDeckSet(_turnHandLimitPort.MaxHandCount).ToArray();

        base.Execute();
    }

    public void Execute(int count)
    {
        _tempCardAnimeRequest.CardDatas = GameData.Instance.NextDeckSet(count).ToArray();

        base.Execute();
    }

    protected override void OnTempCardAnimeEnd(BattleCardData battleCardData)
        => _handCardCreatePort.CreateCard(battleCardData);
}
