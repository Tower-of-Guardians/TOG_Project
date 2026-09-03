using UnityEngine;

public class PlayerSynergyVisual : MonoBehaviour
{
    [SerializeField] private SpriteRenderer auraRenderer;
    [SerializeField] private Animator synergyEffectAnimator;
    [SerializeField] private SpriteRenderer synergyEffectRenderer;
    [SerializeField] private int synergyEffectSortingOrder = 6;

    private Color defaultAuraColor = Color.white;
    private bool hasCachedDefaultAuraColor;

    private void Awake()
    {
        CacheDefaultAuraColor();
        ApplyEffectSortingOrder();
        HideEffect();
    }

    public void PrepareIntro()
    {
        CacheDefaultAuraColor();
        SetAuraColor(defaultAuraColor);
        HideEffect();
    }

    public void Prepare(string synergyId)
    {
        CacheDefaultAuraColor();
        SetAuraColor(SynergyVisualCatalog.GetAuraColor(synergyId));
        HideEffect();
    }

    public void PlayHit(string synergyId)
    {
        SetAuraColor(SynergyVisualCatalog.GetAuraColor(synergyId));

        if (!SynergyVisualCatalog.TryGetEffectStateName(synergyId, out string stateName))
        {
            HideEffect();
            return;
        }

        if (synergyEffectRenderer != null)
        {
            synergyEffectRenderer.enabled = true;
        }

        if (synergyEffectAnimator != null)
        {
            synergyEffectAnimator.gameObject.SetActive(true);
            synergyEffectAnimator.Play(stateName, 0, 0f);
            synergyEffectAnimator.Update(0f);
        }

        ApplyEffectSortingOrder();
    }

    public void Clear()
    {
        if (hasCachedDefaultAuraColor)
        {
            SetAuraColor(defaultAuraColor);
        }

        HideEffect();
    }

    private void HideEffect()
    {
        if (synergyEffectRenderer != null)
        {
            synergyEffectRenderer.sprite = null;
            synergyEffectRenderer.enabled = false;
        }

        if (synergyEffectAnimator != null)
        {
            synergyEffectAnimator.Play(SynergyVisualCatalog.IdleEffectStateName, 0, 0f);
            synergyEffectAnimator.Update(0f);
        }

        ApplyEffectSortingOrder();
    }

    private void ApplyEffectSortingOrder()
    {
        if (synergyEffectRenderer != null)
        {
            synergyEffectRenderer.sortingOrder = synergyEffectSortingOrder;
        }
    }

    private void SetAuraColor(Color color)
    {
        if (auraRenderer == null)
        {
            return;
        }

        auraRenderer.color = color;
    }

    private void CacheDefaultAuraColor()
    {
        if (hasCachedDefaultAuraColor || auraRenderer == null)
        {
            return;
        }

        defaultAuraColor = auraRenderer.color;
        hasCachedDefaultAuraColor = true;
    }
}
