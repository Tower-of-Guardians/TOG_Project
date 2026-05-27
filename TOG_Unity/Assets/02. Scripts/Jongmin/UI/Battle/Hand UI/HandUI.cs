using UnityEngine;

public class HandUI : MonoBehaviour, IHandUI
{
    [SerializeField] private CanvasGroup _canvasGroup;

    public void OpenUI() 
        => ToggleUI(true);

    public void CloseUI() 
        => ToggleUI(false);

    private void ToggleUI(bool isActive)
    {
        _canvasGroup.alpha = isActive ? 1f : 0f;
        _canvasGroup.interactable = isActive;
        _canvasGroup.blocksRaycasts = isActive;
    }
}