using System.Collections;
using UnityEngine;

[RequireComponent(typeof(CanvasGroup))]
public class InteractionTip : MonoBehaviour
{
    [Header("Interactable Object")]
    [SerializeField] private InteractableObject m_interactable_object;

    [Header("Interaction Tag Root")]
    [SerializeField] private Transform m_tag_root;

    [Header("Interaction Tag List")]
    [SerializeField] private string[] m_interaction_tags;

    [Header("Interaction Tag Prefab")]
    [SerializeField] private GameObject m_prefab;

    private CanvasGroup m_canvas_group;
    private Coroutine m_fade_coroutine;

    private void Awake()
    {
        m_canvas_group = GetComponent<CanvasGroup>();
        
        foreach(string interaction_tag in m_interaction_tags)
        {
            GameObject tag_object = Instantiate(m_prefab, m_tag_root);
            
            InteractionTag tag = tag_object.GetComponent<InteractionTag>();
            tag.SetTag(interaction_tag);
        }

        if(m_canvas_group != null)
            m_canvas_group.alpha = 0f;
    }

    private void OnEnable()
    {
        if(m_interactable_object != null)
        {
            m_interactable_object.OnMouseEnterAction += ActiveUI;
            m_interactable_object.OnMouseExitAction += DeactiveUI;
        }
    }

    private void OnDisable()
    {
        if(m_interactable_object != null)
        {
            m_interactable_object.OnMouseEnterAction -= ActiveUI;
            m_interactable_object.OnMouseExitAction -= DeactiveUI;
        }
    }    

    private void ActiveUI()
    {
        ToggleFade(true);
    }

    private void DeactiveUI()
    {
        ToggleFade(false);
    }

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
