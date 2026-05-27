using System.Collections;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(CanvasGroup))]
public class SpriteNameplate : MonoBehaviour
{
    [Header("Interactable Object")]
    [SerializeField] private InteractableObject m_interactable_object;

    [Header("Name")]
    [SerializeField] private string m_name;

    private TMP_Text m_name_text;

    private CanvasGroup m_canvas_group;
    private Coroutine m_fade_coroutine;

    private void Awake()
    {
        m_canvas_group = GetComponent<CanvasGroup>();
        
        m_name_text = GetComponentInChildren<TMP_Text>();
        if(!string.IsNullOrEmpty(m_name))
            m_name_text.text = m_name;

        if(m_canvas_group != null)
            m_canvas_group.alpha = 0f;
    }

    private void OnEnable()
    {
        if(m_interactable_object != null)
        {
            m_interactable_object.OnMouseEnterAction += FadeIn;
            m_interactable_object.OnMouseExitAction += FadeOut;
        }
    }

    private void OnDisable()
    {
        if(m_interactable_object != null)
        {
            m_interactable_object.OnMouseEnterAction -= FadeIn;
            m_interactable_object.OnMouseExitAction -= FadeOut;
        }
    }    

    private void FadeIn()
        => ToggleFade(true);

    private void FadeOut()
        => ToggleFade(false);

    private void ToggleFade(bool active)
    {
        if(m_fade_coroutine != null)
            StopCoroutine(m_fade_coroutine);

        m_fade_coroutine = StartCoroutine(Faderoutine(active));
    }

    private IEnumerator Faderoutine(bool active)
    {
        float elapsed_time = 0f;
        float target_time = 0.3f;

        float current_alpha = m_canvas_group.alpha;
        float target_alpha = active ? 1f : 0f;

        while(elapsed_time < target_time)
        {
            elapsed_time += Time.deltaTime;

            float delta = elapsed_time / target_time;
            m_canvas_group.alpha = Mathf.Lerp(current_alpha, target_alpha, delta);

            yield return null;
        }

        m_canvas_group.alpha = target_alpha;
        m_fade_coroutine = null;
    }
}
