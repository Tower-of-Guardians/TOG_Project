using TMPro;
using UnityEngine;

[RequireComponent(typeof(TMP_Text))]
public class InteractionTag : MonoBehaviour
{
    private TMP_Text m_tag_text;

    private void Awake()
        => m_tag_text = GetComponent<TMP_Text>();

    public void SetTag(string tag)
        => m_tag_text.text = tag;
}
