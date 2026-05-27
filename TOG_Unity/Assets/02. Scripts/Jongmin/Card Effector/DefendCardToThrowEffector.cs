using UnityEngine;
using DG.Tweening;
using System.Collections.Generic;

public class DefendCardToThrowEffector : CardEffector
{
    [Header("Battle Locker")]
    [SerializeField] private UILocker _battleLocker;

    [Header("Attack Field Locker")]
    [SerializeField] private UILocker _atkFieldLocker;

    [Header("Defense Field Locker")]
    [SerializeField] private UILocker _defFieldLocker;

    private IDefendFieldCardRemovePort _defFieldCardRemovePort;
    private CardContainer<IFieldCardUI, FieldCardPresenter> _defFieldCardContainer;

    public void Construct(IDefendFieldCardRemovePort defFieldCardRemovePort,
                          CardContainer<IFieldCardUI, FieldCardPresenter> defFieldCardContainer)
    {
        _defFieldCardRemovePort = defFieldCardRemovePort;
        _defFieldCardContainer = defFieldCardContainer;

        _tempCardSettings = new()
        {
            Duration = 0.5f,

            UseJump = true,
            JumpPower = 150f,
            MoveEase = Ease.InOutQuad,

            UseScale = true,
            Scale = Vector3.one * 0.11f,
            ScaleEase = Ease.InQuad,

            UseRotation = true,
            TargetEuler = new Vector3(0f, 0f, -180f),
            RotateMode = RotateMode.LocalAxisAdd,
            RotateEase = Ease.InOutQuad,

            UseOpacity = true,
            Opacity = 0.5f,
            OpacityEase = Ease.Unset,

            ForceStartScale = true,
            StartScale = Vector3.one * 0.66f,

            ForceStartRotation = true,
        };

        _tempCardAnimeRequest = new()
        {
            EndPosition = _endTransform == null ? Vector3.zero : _endTransform.position,
            StartRotation = Vector3.zero,
            Interval = 0.1f,
            Settings = _tempCardSettings,
        };
    }

    public override void Execute()
    {
        _tempCardAnimeRequest.CardDatas = _defFieldCardContainer.GetAllDatas();

        List<Vector3> fieldCardPositionList = new();
        if(!_defFieldCardContainer.TryGetAllUIs(out IFieldCardUI[] fieldUIArray))
        {
            return;
        }

        foreach(IFieldCardUI cardUI in fieldUIArray)
        {
            FieldCardUI concreteCardUI = cardUI as FieldCardUI;
            fieldCardPositionList.Add(concreteCardUI.transform.position);
        }

        _tempCardAnimeRequest.StartPositions = fieldCardPositionList.ToArray();

        base.Execute();
    }

    protected override void OnTempCardAnimeStart(BattleCardData battleCardData)
        => _defFieldCardRemovePort.TryRemoveCard(battleCardData);

    protected override void OnTempCardAnimeEnd(BattleCardData battleCardData)
    {
        GameData.Instance.defenseField.Remove(battleCardData.data);
        GameData.Instance.UseCard(battleCardData.data.id);
        GameData.Instance.InvokeDeckCountChange(DeckType.Throw);
        GameData.Instance.GetSynergyData();
    }

    protected override void OnFinalAnimeEnd()
    {
        _battleLocker.Lock(false);
        _atkFieldLocker.Lock(false);
        _defFieldLocker.Lock(false);
    }
}
