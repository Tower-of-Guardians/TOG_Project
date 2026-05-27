using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public abstract class BaseUnit : MonoBehaviour, IDamageable
{
    [Header("Stats")]
    [SerializeField] protected int maxHealth = 100;
    [SerializeField] protected int currentHealth;
    [SerializeField] protected bool hasDefense;
    [SerializeField] protected float protectionValue = 0f;

    [Header("Status UI")]
    [SerializeField] protected TMP_Text attackText;
    [SerializeField] protected Slider hpSlider;
    [SerializeField] protected TMP_Text hpText;
    [SerializeField] protected Image hpFillImage;
    [SerializeField] protected GameObject defenseIcon;
    [SerializeField] protected Color defaultHpColor = Color.red;
    [SerializeField] protected Color defenseHpColor = new Color(0.5f, 0.5f, 0.5f, 1f);

    [Header("Health Animation")]
    [SerializeField] protected float healthAnimationDuration = 0.3f;
    [SerializeField] protected Ease healthAnimationEase = Ease.OutQuad;

    private float previousHealthRatio = 1f;
    private Tweener healthTweener;

    public int CurrentHealth => currentHealth;
    public int MaxHealth => maxHealth;
    public bool IsAlive => currentHealth > 0;
    public bool HasDefense => hasDefense;
    public float ProtectionValue => protectionValue;

    protected virtual void Awake()
    {
        ClampHealth(forceMaxIfZero: true);
        float initialRatio = maxHealth > 0 ? (float)currentHealth / maxHealth : 0f;
        previousHealthRatio = initialRatio;
        RefreshUI();
    }

    public virtual void SetCurrentHealth(int value)
    {
        currentHealth = Mathf.Clamp(value, 0, maxHealth);
        RefreshUI();
    }

    public virtual void TakeDamage(int amount)
    {
        int beforeHealth = currentHealth;
        StatusEffectController statusEffectController = GetComponent<StatusEffectController>();
        DamageContext incomingDamageContext = null;
        if (statusEffectController != null)
        {
            incomingDamageContext = new DamageContext(amount, null, this);
            statusEffectController.OnBeforeTakeDamage(incomingDamageContext);
            amount = incomingDamageContext.FinalDamage;
        }

        // 보호력이 있으면 보호력부터 감소
        if (protectionValue > 0)
        {
            float damageToProtection = Mathf.Min(protectionValue, amount);
            protectionValue = Mathf.Max(0, protectionValue - damageToProtection);
            amount = Mathf.Max(0, amount - Mathf.RoundToInt(damageToProtection));
            
            // 보호력이 모두 소진되면 방어 상태 해제
            if (protectionValue <= 0)
            {
                hasDefense = false;
            }
        }
        
        // 남은 데미지를 체력에 적용
        currentHealth = Mathf.Clamp(currentHealth - Mathf.Max(0, amount), 0, maxHealth);
        RefreshUI();

        if (statusEffectController != null && incomingDamageContext != null)
        {
            int appliedHealthDamage = Mathf.Max(0, beforeHealth - currentHealth);
            incomingDamageContext.SetFinalDamage(appliedHealthDamage);
            statusEffectController.OnAfterTakeDamage(incomingDamageContext);
        }
    }

    /// <summary>
    /// 공격 직전, 보유 상태효과가 피해값을 수정할 수 있도록 호출합니다.
    /// </summary>
    public int ApplyOutgoingStatusEffects(int baseDamage, BaseUnit target, bool isCritical = false)
    {
        StatusEffectController statusEffectController = GetComponent<StatusEffectController>();
        if (statusEffectController == null)
        {
            return Mathf.Max(0, baseDamage);
        }

        DamageContext outgoingDamageContext = new DamageContext(baseDamage, this, target, isCritical);
        statusEffectController.OnBeforeDealDamage(outgoingDamageContext);
        return outgoingDamageContext.FinalDamage;
    }

    public void NotifyTurnStartStatusEffects()
    {
        StatusEffectController statusEffectController = GetComponent<StatusEffectController>();
        statusEffectController?.OnTurnStart();
    }

    public void NotifyTurnEndStatusEffects()
    {
        StatusEffectController statusEffectController = GetComponent<StatusEffectController>();
        statusEffectController?.OnTurnEnd();
    }

    public virtual void SetDefense(bool active)
    {
        hasDefense = active;
        RefreshUI();
    }

    public virtual void SetProtection(float value)
    {
        protectionValue = Mathf.Max(0, value);
        hasDefense = protectionValue > 0;
        RefreshUI();
    }

    public virtual void AddProtection(float amount)
    {
        protectionValue = Mathf.Max(0, protectionValue + amount);
        hasDefense = protectionValue > 0;
        RefreshUI();
    }

    protected virtual void ClampHealth(bool forceMaxIfZero = false)
    {
        if (forceMaxIfZero && currentHealth == 0)
        {
            currentHealth = maxHealth;
        }

        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
    }

    protected virtual void RefreshUI()
    {
        float ratio = maxHealth > 0 ? (float)currentHealth / maxHealth : 0f;

        if (hpSlider != null)
        {
            if (ratio < previousHealthRatio)
            {
                float startValue = (healthTweener != null && healthTweener.IsActive()) 
                    ? hpSlider.normalizedValue 
                    : previousHealthRatio;
                
                AnimateHealthBar(startValue, ratio);
            }
            else
            {
                if (healthTweener != null && healthTweener.IsActive())
                {
                    healthTweener.Kill();
                }
                hpSlider.normalizedValue = ratio;
            }
            
            previousHealthRatio = ratio;
        }
        else
        {
            previousHealthRatio = ratio;
        }

        if (hpText != null)
        {
            if (protectionValue > 0)
            {
                hpText.text = $"HP {currentHealth}/{maxHealth}(+{Mathf.RoundToInt(protectionValue)})";
            }
            else
            {
                hpText.text = $"HP {currentHealth}/{maxHealth}";
            }
        }

        if (hpFillImage != null)
        {
            hpFillImage.color = hasDefense ? defenseHpColor : defaultHpColor;
        }

        if (defenseIcon != null)
        {
            defenseIcon.SetActive(hasDefense);
        }
    }

    private void AnimateHealthBar(float fromValue, float toValue)
    {
        if (hpSlider == null)
        {
            return;
        }

        if (healthTweener != null && healthTweener.IsActive())
        {
            healthTweener.Kill();
        }

        hpSlider.normalizedValue = fromValue;

        healthTweener = DOTween.To(
            () => hpSlider.normalizedValue,
            x => hpSlider.normalizedValue = x,
            toValue,
            healthAnimationDuration
        ).SetEase(healthAnimationEase);
    }

    protected virtual void OnDestroy()
    {
        if (healthTweener != null && healthTweener.IsActive())
        {
            healthTweener.Kill();
        }
    }
}

