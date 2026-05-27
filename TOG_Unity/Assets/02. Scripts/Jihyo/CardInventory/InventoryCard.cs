using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InventoryCard : MonoBehaviour
{
    [Header("UI 관련 컴포넌트")]
    [Header("카드 테두리 이미지")]
    [SerializeField] private Image m_outline_image;

    [Header("카드 이미지")]
    [SerializeField] private Image m_card_image;

    [Header("카드 이름 텍스트")]
    [SerializeField] private TMP_Text m_name_label;

    [Header("카드 설명 텍스트")]
    [SerializeField] private TMP_Text m_description_label;

    [Header("카드 공격력 텍스트")]
    [SerializeField] private TMP_Text m_atk_label;

    [Header("카드 방어력 텍스트")]
    [SerializeField] private TMP_Text m_def_label;

    [Header("카드 성급 그룹")]
    [SerializeField] private GameObject m_star_group;

    private Image[] m_star_objects;

    private void Awake()
        => m_star_objects = m_star_group.GetComponentsInChildren<Image>();


    public void InitUI(CardData card_data)
    {
        if (card_data == null)
            return;

        m_outline_image.sprite = card_data.cardimage;
        m_card_image.sprite = card_data.iconimage;
        m_name_label.text = card_data.itemName;
        m_description_label.text = card_data.effectDescription;
        m_atk_label.text = card_data.ATK.ToString();
        m_def_label.text = card_data.DEF.ToString();

        for(int i = 0; i < m_star_objects.Length; i++)
            m_star_objects[i].gameObject.SetActive(false);

        for(int i = 0; i < card_data.star + 1; i++)
            m_star_objects[i].gameObject.SetActive(true);
    }
}