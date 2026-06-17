using System.Collections;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(CanvasGroup))]
public class TooltipView : MonoBehaviour, ITooltipView
{
    [Header("디자이너")]
    [SerializeField] private TooltipUIDesigner m_designer;

    [Space(30f), Header("UI 관련 컴포넌트")]
    [Header("캔버스 그룹")]
    [SerializeField] private CanvasGroup m_canvas_group;

    [Header("툴팁 텍스트")]
    [SerializeField] private TMP_Text m_tooltip_label; 

    private Coroutine m_fade_coroutine;

    private void Awake()
    {
        m_canvas_group.interactable = false;
        m_canvas_group.blocksRaycasts = false;
    }

    public void OpenUI()
    {
        ToggleCanvasGroup(true);
    }

    public void UpdateUI(TooltipData tooltip_data)
    {
        (transform as RectTransform).anchoredPosition = tooltip_data.Position;
        m_tooltip_label.text = tooltip_data.Description;
    }

    public void CloseUI()
    {
        ToggleCanvasGroup(false);
    }

    private void ToggleCanvasGroup(bool active)
    {
        if(m_fade_coroutine != null)
            StopCoroutine(m_fade_coroutine);

        m_fade_coroutine = StartCoroutine(Co_FadeAlpha(active));
    }

    private IEnumerator Co_FadeAlpha(bool active)
    {
        var elapsed_time = 0f;
        var target_time = m_designer.FadeDuration;

        var current_alpha = m_canvas_group.alpha;
        var target_alpha = active ? 1f : 0f; 

        while(elapsed_time < target_time)
        {
            elapsed_time += Time.deltaTime;

            var delta = elapsed_time / target_time;
            var new_alpha = Mathf.Lerp(current_alpha, target_alpha, delta);
            m_canvas_group.alpha = new_alpha;

            yield return null;
        }

        m_canvas_group.alpha = target_alpha;
    }
}
