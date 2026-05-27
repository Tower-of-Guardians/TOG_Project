using TMPro;
using UnityEngine;
using DG.Tweening;

[RequireComponent(typeof(CanvasGroup))]
public class HighlightText : MonoBehaviour
{
    [SerializeField] private TMP_Text targetText;
    private CanvasGroup _canvasGroup;
    private Tween _fadeTween;
    
    private void Awake()
        => _canvasGroup = GetComponent<CanvasGroup>();

    public void SetText(string text)
        => targetText.text = text;
    
    public Tween Show(bool isImmediately)
    {
        _fadeTween?.Kill();

        if (isImmediately)
        {
            _canvasGroup.alpha = 1f;
            return null;
        }

        _canvasGroup.alpha = 0f;
        _fadeTween = _canvasGroup.DOFade(1f, 0.5f);
        
        return _fadeTween;
    }

    public Tween Hide(bool isImmediately)
    {
        _fadeTween?.Kill();

        if (isImmediately)
        {
            _canvasGroup.alpha = 0f;
            return null;
        }

        _fadeTween = _canvasGroup.DOFade(0f, 0.5f);

        return _fadeTween;
    }
}
