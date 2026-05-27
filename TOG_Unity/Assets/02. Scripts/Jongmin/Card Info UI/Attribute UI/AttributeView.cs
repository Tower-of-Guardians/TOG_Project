using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttributeView : MonoBehaviour, IAttributeView
{
    [Header("UI 관련 컴포넌트")]
    [Header("캔버스 그룹")]
    [SerializeField] private CanvasGroup m_canvas_group;

    [Header("카드 뷰")]
    [SerializeField] private CardUI m_card_view;

    [Header("시너지 그룹")]
    [SerializeField] private Transform m_synergy_group;

    private ISynergyDescriptionView[] m_synergy_descriptor_list;
    private Coroutine m_fade_coroutine;

    private void Awake()
        => m_synergy_descriptor_list = m_synergy_group.GetComponentsInChildren<ISynergyDescriptionView>();

    public void OpenUI()
        => ToggleUI(true);

    public void CloseUI()
    {
        foreach(var synergy_descriptor in m_synergy_descriptor_list)
            synergy_descriptor.ToggleView(false);

        ToggleUI(false);
    }

    public void UpdateCard(CardData card_data)
        => m_card_view.UpdateUI(card_data);

    public void UpdateSynergy(List<string> synergy_desc_list)
    {
        var synergy_count = synergy_desc_list.Count;

        for(int i = 0; i < synergy_count; i++)
        {
            m_synergy_descriptor_list[i].ToggleView(true);
            m_synergy_descriptor_list[i].SetDescription(synergy_desc_list[i]);
        }
    }

    private void ToggleUI(bool active)
    {
        if(m_fade_coroutine != null)
            StopCoroutine(m_fade_coroutine);

        m_fade_coroutine = StartCoroutine(FadeGroup(active));
    }

    private IEnumerator FadeGroup(bool is_in)
    {
        var elapsed_time = 0f;
        var target_time = 0.5f;

        var start_alpha = m_canvas_group.alpha;
        var end_alpha = is_in ? 1f : 0f;

        while(elapsed_time < target_time)
        {
            elapsed_time += Time.deltaTime;

            float delta = elapsed_time / target_time;
            m_canvas_group.alpha = Mathf.Lerp(start_alpha, end_alpha, delta);

            yield return null;
        }

        m_canvas_group.alpha = end_alpha;
        m_canvas_group.interactable = is_in;
        m_canvas_group.blocksRaycasts = is_in;
        
        m_fade_coroutine = null;
    }
}
