using UnityEngine;

[RequireComponent(typeof(InteractableObject))]
public class SpriteOutliner : MonoBehaviour
{
    [Header("Target Renderer")]
    [SerializeField] private SpriteRenderer m_sprite_renderer;

    [Space(20f), Header("Outline Settings")]
    [Header("Use Outline")]
    [SerializeField] private bool m_use_outline = true;

    [Header("Outline Color")]
    [SerializeField] private Color m_outline_color = Color.white;

    [Min(1)][Header("Outline Thickness")]
    [SerializeField] private int m_outline_thickness = 1;

    private InteractableObject m_interactable_object;
    private MaterialPropertyBlock m_prop_block;

    private static readonly int m_outline_id = Shader.PropertyToID("_Outline");
    private static readonly int m_outline_color_id = Shader.PropertyToID("_OutlineColor");
    private static readonly int m_outline_thickness_id = Shader.PropertyToID("_OutlineSize");

    private void Awake()
    {
        m_interactable_object = GetComponent<InteractableObject>();

        m_prop_block = new MaterialPropertyBlock();
        ApplyOutline(false);
    }

    private void OnEnable()
    {
        if(m_interactable_object != null)
        {
            m_interactable_object.OnMouseEnterAction += SetOutline;
            m_interactable_object.OnMouseExitAction += SetDefault;
        }
    }

    private void OnDisable()
    {
        if(m_interactable_object != null)
        {
            m_interactable_object.OnMouseEnterAction -= SetOutline;
            m_interactable_object.OnMouseExitAction -= SetDefault;
        }
    }

    private void SetDefault()
    {
        if(!m_use_outline)
            return;

        ApplyOutline(false);
    }

    private void SetOutline()
    {
        if(!m_use_outline)
            return;

        ApplyOutline(true);
    }

    private void ApplyOutline(bool active)
    {
        if(m_sprite_renderer == null)
            return;

        m_sprite_renderer.GetPropertyBlock(m_prop_block);

        m_prop_block.SetFloat(m_outline_id, active ? 1f : 0f);
        m_prop_block.SetColor(m_outline_color_id, m_outline_color);
        m_prop_block.SetInt(m_outline_thickness_id, Mathf.Max(1, m_outline_thickness));

        m_sprite_renderer.SetPropertyBlock(m_prop_block);
    }
}
