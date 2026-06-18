using DG.Tweening;
using JxModule;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(CanvasGroup))]
public class NameplateUI : MonoBehaviour
{
    [SerializeField] private string targetName;

    private CanvasGroup _canvasGroup;
    private TMP_Text _nameLabel;
    private Tween _fadeTween;
    
    private void Awake()
    {
        _canvasGroup = GetComponent<CanvasGroup>();
        
        _nameLabel = GetComponentInChildren<TMP_Text>();
        if (_nameLabel == null)
        {
            DebugExtension.LogColor("NameplateView: Name label is null.", Color.red);
            enabled = false;
            return;
        }

        if (!string.IsNullOrEmpty(targetName))
        {
            _nameLabel.text = targetName;
        }
        
        _canvasGroup.Hide();
    }

    public void Show()
    {
        _fadeTween?.Kill();
        _fadeTween = _canvasGroup.DOFade(1f, 0.25f);
    }

    public void Hide()
    {
        _fadeTween?.Kill();
        _fadeTween = _canvasGroup.DOFade(0f, 0.25f);
    }
}