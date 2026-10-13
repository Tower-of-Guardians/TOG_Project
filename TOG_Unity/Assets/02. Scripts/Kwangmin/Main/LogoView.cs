using System;
using DG.Tweening;
using UnityEngine;

public class LogoView : MainPanelView
{
    [SerializeField] private float _holdDuration = 1.5f;

    private Sequence _sequence;
    private Action _onComplete;

    public void Play(Action onComplete)
    {
        _onComplete = onComplete;
        _sequence?.Kill();
        CanvasGroup.alpha = 0f;
        _sequence = DOTween.Sequence()
                           .Append(CanvasGroup.DOFade(1f, FadeDuration))
                           .AppendInterval(_holdDuration)
                           .Append(CanvasGroup.DOFade(0f, FadeDuration))
                           .SetUpdate(true)
                           .OnComplete(Complete);
    }

    public void Skip()
    {
        if (_onComplete == null)
        {
            return;
        }

        _sequence?.Kill();
        Complete();
    }

    private void Complete()
    {
        HideImmediately();
        Action callback = _onComplete;
        _onComplete = null;
        callback?.Invoke();
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        _sequence?.Kill();
    }
}
