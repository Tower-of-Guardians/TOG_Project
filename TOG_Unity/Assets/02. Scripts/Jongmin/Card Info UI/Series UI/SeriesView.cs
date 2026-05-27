using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SeriesView : MonoBehaviour, ISeriesView
{
    [Header("UI 관련 컴포넌트")]
    [Header("캔버스 그룹")]
    [SerializeField] private CanvasGroup m_canvas_group;

    [Header("시리즈 카드 그룹")]
    [SerializeField] private Transform m_card_group;

    private CardUI[] m_card_list;
    private Coroutine m_fade_coroutine;

    private void Awake()
        => m_card_list = m_card_group.GetComponentsInChildren<CardUI>();

    public void OpenUI()
        => ToggleUI(true);

    public void CloseUI()
    {
        foreach(var card_view in m_card_list)
            card_view.gameObject.SetActive(false);
            
        ToggleUI(false);
    }

    public void UpdateSeries(List<CardData> card_data_list)
    {
        var card_data_count = card_data_list.Count;

        for(int i = 0; i < card_data_count; i++)
        {
            m_card_list[i].gameObject.SetActive(true);
            m_card_list[i].UpdateUI(card_data_list[i]);
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
