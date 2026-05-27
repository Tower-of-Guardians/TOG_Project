using TMPro;
using UnityEngine;

public class ActionManualUI : MonoBehaviour, IActionManualUI
{
    [Header("Text References")]
    [SerializeField] private TMP_Text actionLabel;

    public void UpdateUI(ActionData actionData, bool isCanAction)
    {
        string actionText = $"{actionData.Current} / {actionData.Max}";

        actionLabel.text = isCanAction ? actionText
                                    : $"<color=red>{actionText}</color>";
    }
}
