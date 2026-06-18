using TMPro;
using UnityEngine;

namespace Jongmin
{
    public class ActionManualView : MonoBehaviour
    {
        [SerializeField] private TMP_Text actionLabel;

        public void UpdateUI(ActionData actionData, bool isCanAction)
        {
            var actionText = $"{actionData.Current} / {actionData.Max}";
            actionLabel.text = isCanAction ? actionText : $"<color=red>{actionText}</color>";
        }
    }
}

