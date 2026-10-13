using System;
using DG.Tweening;
using JxModule;
using UnityEngine;

public class MainPanelView : ViewBase
{
    [SerializeField] private float _fadeDuration = 0.25f;

    private Tween _toggleTween;

    public bool IsOpen { get; private set; }

    protected float FadeDuration => _fadeDuration;

    public void Show(Action onComplete = null)
    {
        IsOpen = true;
        _toggleTween?.Kill();
        CanvasGroup.SetInteractable(false);
        _toggleTween = CanvasGroup.DOFade(1f, _fadeDuration)
                                  .SetUpdate(true)
                                  .OnComplete(() =>
                                  {
                                      CanvasGroup.Show();
                                      onComplete?.Invoke();
                                  });
    }

    public void Hide(Action onComplete = null)
    {
        IsOpen = false;
        _toggleTween?.Kill();
        CanvasGroup.SetInteractable(false);
        _toggleTween = CanvasGroup.DOFade(0f, _fadeDuration)
                                  .SetUpdate(true)
                                  .OnComplete(() =>
                                  {
                                      CanvasGroup.Hide();
                                      onComplete?.Invoke();
                                  });
    }

    public void HideImmediately()
    {
        IsOpen = false;
        _toggleTween?.Kill();
        CanvasGroup.Hide();
    }

    protected virtual void OnDestroy()
    {
        _toggleTween?.Kill();
    }
}
