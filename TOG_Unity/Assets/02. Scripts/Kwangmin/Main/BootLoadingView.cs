using JxModule;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class BootLoadingView : MainPanelView
{
    [SerializeField] private RectTransform _progressFill;
    [SerializeField] private TMP_Text _progressLabel;
    [SerializeField] private GameObject _errorRoot;
    [SerializeField] private ButtonView _quitButton;

    public void Bind(UnityAction onQuit)
    {
        _quitButton.RemoveAllListeners();
        _quitButton.AddListener(onQuit);
    }

    public void ResetState()
    {
        SetProgress(0f);
        _errorRoot.SetActive(false);
    }

    public void SetProgress(float progress)
    {
        float value = Mathf.Clamp01(progress);
        _progressFill.anchorMax = new Vector2(value, _progressFill.anchorMax.y);
        _progressLabel.text = $"{Mathf.RoundToInt(value * 100f)}%";
    }

    public void ShowError()
    {
        _errorRoot.SetActive(true);
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        _quitButton.RemoveAllListeners();
    }
}
