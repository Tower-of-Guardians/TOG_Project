using System;
using UnityEngine;
using DG.Tweening;

public class TemporaryCardAnimator : MonoBehaviour
{
    [SerializeField] private TemporaryCardFactory m_factory;

    public Tween AnimateOne(
        Transform target_root,
        BattleCardData data,
        Vector3 start_position,
        Vector3 end_position,
        Vector3 start_rotation,
        TemporaryCardSettings s,
        Action<BattleCardData> on_start = null,
        Action<BattleCardData> on_complete = null)
    {
        on_start?.Invoke(data);

        var card_object = m_factory.InstantiateCard(data, target_root);
        var t = card_object.transform;

        t.position = start_position;

        if (s.ForceStartScale) t.localScale = s.StartScale;
        if (s.ForceStartRotation) t.eulerAngles = start_rotation;

        var seq = DOTween.Sequence();

        var canvas_group = t.GetComponent<CanvasGroup>();
        canvas_group.alpha = s.ForceStartOpacity ? s.StartOpacity : 1f;

        if (s.UseOpacity)
            seq.Join(canvas_group.DOFade(s.Opacity, s.Duration).SetEase(s.OpacityEase));

        if (s.UseJump)
            seq.Join(t.DOJump(end_position, s.JumpPower, 1, s.Duration).SetEase(s.MoveEase));
        else
            seq.Join(t.DOMove(end_position, s.Duration).SetEase(s.MoveEase));

        if (s.UseScale)
            seq.Join(t.DOScale(s.Scale, s.Duration).SetEase(s.ScaleEase));

        if (s.UseRotation)
            seq.Join(t.DORotate(s.TargetEuler, s.Duration, s.RotateMode).SetEase(s.RotateEase));

        seq.OnComplete(() =>
        {
            on_complete?.Invoke(data);
            m_factory.ReturnCard(card_object);
        });

        return seq;
    }
}
