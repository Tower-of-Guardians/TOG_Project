using TMPro;
using UnityEngine;

public class SynergyDescriptionView : MonoBehaviour, ISynergyDescriptionView
{
    [Header("UI 관련 컴포넌트")]
    [SerializeField] TMP_Text m_description_label;

    public void ToggleView(bool active)
        => gameObject.SetActive(active);

    public void SetDescription(string desc_text)
        => m_description_label.text = desc_text;
}
