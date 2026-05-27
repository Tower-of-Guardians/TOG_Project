using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class StatusUI : MonoBehaviour, IStatusUI
{
    [Header("UI References")]
    [SerializeField] private TMP_Text levelLabel;
    [SerializeField] private DynamicHandleSlider expSlider;
    [SerializeField] private TMP_Text goldLabel;

    public void UpdateGold(int gold)
        => goldLabel.text = $"{gold}G";

    public void UpdateLevel(int level, float exp)
    {
        levelLabel.text = $"Lv.{level}";
        expSlider.SetValue(exp);
    }
}
