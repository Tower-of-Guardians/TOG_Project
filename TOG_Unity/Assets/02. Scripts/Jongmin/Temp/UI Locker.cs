using System.Collections;
using UnityEngine;

[RequireComponent(typeof(CanvasGroup))]
public class UILocker : MonoBehaviour
{
    private CanvasGroup m_canvas_group;

    [Range(0f, 1f)][Header("Duration")]
    [SerializeField] private float m_target_time;

    [Range(0f, 1f)][Header("fade alpha")]
    [SerializeField] private float m_target_alpha;

    private Coroutine m_fade_coroutine;

    private void Awake()
        => m_canvas_group = GetComponent<CanvasGroup>();

    public void Lock(bool is_lock)
    {
        if(m_fade_coroutine != null)
            StopCoroutine(m_fade_coroutine);

        m_fade_coroutine = StartCoroutine(Co_Fade(is_lock));
    }

    private IEnumerator Co_Fade(bool is_in)
    {
        float elapsed_time = 0f;
        float start_alpha = m_canvas_group.alpha;
        float end_alpha = is_in ? m_target_alpha : 0f;

        if (is_in)
            m_canvas_group.blocksRaycasts = true;

        while (elapsed_time < m_target_time)
        {
            elapsed_time += Time.deltaTime;

            float t = m_target_time > 0f ? elapsed_time / m_target_time : 1f;
            m_canvas_group.alpha = Mathf.Lerp(start_alpha, end_alpha, t);
            
            yield return null;
        }

        m_canvas_group.alpha = end_alpha;

        if (!is_in)
            m_canvas_group.blocksRaycasts = false;

        m_fade_coroutine = null;
    }
}
