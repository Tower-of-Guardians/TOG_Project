using System;
using DG.Tweening;
using JxModule;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class EventUI : MonoBehaviour
{
    [BigHeader("UI")]
    [SerializeField, Required] private CanvasGroup eventGroup;
    [SerializeField] private Image backgroundImage;
    [SerializeField] private Button closeButton;
    [SerializeField, SceneOnly] private Transform prefabRoot;
    
    private CanvasGroup _buttonGroup;
    private Tween _viewTween;
    private Tween _buttonTween;
    
    public Transform PrefabRoot => prefabRoot;
    
    private void Awake()
    {
        _buttonGroup = closeButton.GetComponent<CanvasGroup>();
        if (_buttonGroup == null)
        {
            DebugExtension.LogColor($"EventView: Close Button's canvas group is null.", Color.red);
            enabled = false;
        }
    }

    public void BindCloseButton(UnityAction action)
    {
        closeButton.onClick.AddListener(action);
    }

    public void Show()
    {
        _viewTween?.Kill();
        _viewTween = eventGroup.DOFade(1f, 0.5f).OnComplete(() => eventGroup.Show());
    }

    public void Hide()
    {
        _viewTween?.Kill();
        _viewTween = eventGroup.DOFade(0f, 0.5f).OnComplete(() => eventGroup.Hide());
    }
    
    public void SetBackground(Sprite backgroundSprite)
    {
        backgroundImage.sprite = backgroundSprite;
    }
    
    public void ClearBackground()
    {
        backgroundImage.sprite = null;
    }

    public void ToggleCloseButton(bool isActive)
    {
        _buttonTween?.Kill();
        _buttonTween = _buttonGroup.DOFade(isActive ? 1f : 0f, 0.5f).OnComplete(() => _buttonGroup.SetVisible(isActive));
    }

    public void ToggleCloseButtonImmediately(bool isActive)
    {
        _buttonGroup.SetVisible(isActive);
    }
}