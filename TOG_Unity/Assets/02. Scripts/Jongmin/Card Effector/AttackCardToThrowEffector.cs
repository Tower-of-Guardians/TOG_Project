using UnityEngine;
using DG.Tweening;
using System.Collections.Generic;

public class AttackCardToThrowEffector : CardEffector
{
    [Header("Chain Effector")]
    [SerializeField] private DefendCardToThrowEffector _defendCardEffector;

    [Header("Attack Field Locker")]
    [SerializeField] private UILocker _atkFieldLocker;

    [Header("Defense Field Locker")]
    [SerializeField] private UILocker _defFieldLocker;

    private IAttackFieldCardRemovePort _atkFieldCardRemovePort;
    private CardContainer<IFieldCardUI, FieldCardPresenter> _atkFieldCardContainer;

    public void Construct(IAttackFieldCardRemovePort atkFieldCardRemovePort,
                          CardContainer<IFieldCardUI, FieldCardPresenter> atkFieldCardContainer)
    {
        _atkFieldCardRemovePort = atkFieldCardRemovePort;
        _atkFieldCardContainer = atkFieldCardContainer;

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
        _atkFieldLocker.Lock(true);
        _defFieldLocker.Lock(true);

        _tempCardAnimeRequest.CardDatas = _atkFieldCardContainer.GetAllDatas();

        List<Vector3> fieldCardPositionList = new();
        if(!_atkFieldCardContainer.TryGetAllUIs(out IFieldCardUI[] fieldUIArray))
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
        => _atkFieldCardRemovePort.TryRemoveCard(battleCardData);

    protected override void OnTempCardAnimeEnd(BattleCardData battleCardData)
    {
        GameData.Instance.attackField.Remove(battleCardData.data);
        GameData.Instance.UseCard(battleCardData.data.id);
        GameData.Instance.InvokeDeckCountChange(DeckType.Throw);
        GameData.Instance.GetSynergyData();
    }

    protected override void OnFinalAnimeEnd()
    {
        _defendCardEffector.Execute();
    }
}
