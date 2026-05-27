using TMPro;
using UnityEngine;

public class DiscardManualUI : MonoBehaviour, IDiscardManualUI
{
    [Header("UI 관련 컴포넌트")]
    [Header("동작 횟수 텍스트")]
    [SerializeField] private TMP_Text actionLabel;

    public void UpdateUI(ActionData actionData, bool isCanAction)
    {
        string actionText = $"{actionData.Current} / {actionData.Max}";

        actionLabel.text = isCanAction ? actionText
                                    : $"<color=red>{actionText}</color>";
    }
}
