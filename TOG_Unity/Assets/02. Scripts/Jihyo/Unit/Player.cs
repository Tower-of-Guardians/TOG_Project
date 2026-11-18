using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using Jongmin;

public class Player : BaseUnit, ITooltipProvider
{
    [Header("Stats")]
    private int baseAttack;
    private float cardAttackBonus;
    private float cardDefenseBonus;
    private int battleSynergyAttackBonus;
    private int turnSynergyAttackBonus;

    [Header("Animation")]
    private PlayerAnimation playerAnimation;
    [SerializeField] private SpriteRenderer spriteRenderer;

    [Header("Effects")]
    [SerializeField] private GameObject effectShield;
    private const string ShieldEffectStateName = "Effect_Shield";
    private const float ShieldEffectDuration = 1f;
    private Coroutine shieldEffectCoroutine;
    private int shieldEffectVersion;

    [Header("Movement")]
    [SerializeField] private float singleTargetAttackOffset = 3.5f;
    [SerializeField] private float attackMoveDuration = 0.3f;

    private Vector3 initialSpriteLocalPosition;
    private bool hasCachedSpriteOrigin;
    private float lastAttackBonus;
    private float lastDefenseBonus;
    
    [Header("UI Animation")]
    [SerializeField] private float statAnimationDuration = 0.5f;
    [SerializeField] private Ease statAnimationEase = Ease.OutQuad;
    
    [Header("Status Effect UI")]
    [SerializeField] private Sprite weaknessStatusSprite;
    [SerializeField] private Sprite curseStatusSprite;
    [SerializeField] private Transform statusBottomRoot;
    [SerializeField] private GameObject statusEffectIconPrefab;
    
    private Tweener attackTextTweener;
    private Tweener protectionTweener;
    private readonly Queue<int> pendingAttackTweenTargets = new Queue<int>();
    private const int NormalSortingOrder = 5;
    private const int AttackSortingOrder = 7;

    private StatusEffectController statusEffectController;

    public int AttackValue => Mathf.RoundToInt(baseAttack + cardAttackBonus)
        + battleSynergyAttackBonus
        + turnSynergyAttackBonus
        + StatusEffectQueries.GetUserStrengthBonus(this)
        + StatusEffectQueries.GetChastisementBonus(this, baseAttack);
    public float DefenseValue => cardDefenseBonus;
    public float GetStatAnimationWaitTime() => statAnimationDuration;
    public bool CanShowTooltip => true;
    
    protected override void Awake()
    {
        base.Awake();
        InitializeFromDataCenter();
        InitializeAnimation();
        CacheSpriteOrigin();
        InitializeStatusEffectUI();
        SubscribeStatusEffectEvents();
        SubscribeDataCenterHpEvent();
    }

    private void InitializeFromDataCenter()
    {
        if (DataCenter.Instance != null)
        {
            DataCenter.Instance.LoadPlayerData();
            var playerState = DataCenter.Instance.playerstate;
            
            baseAttack = playerState.atk;
            maxHealth = playerState.maxhp > 0 ? playerState.maxhp : playerState.hp;
            currentHealth = playerState.hp;
            
            UpdateCardStats();
            RefreshUI();
        }
        else
        {
            Debug.LogWarning("Player: DataCenter.Instance is null. Using default values.");
            baseAttack = 5;
        }
    }

    private void SubscribeDataCenterHpEvent()
    {
        if (DataCenter.Instance != null)
        {
            DataCenter.Instance.playerHpEvent += OnPlayerHPChanged;
        }
    }

    private void OnPlayerHPChanged(int hpChange)
    {
        // DataCenter의 SetPlayerHP에서 변경량을 전달받아 현재 체력에 적용
        int newHealth = Mathf.Clamp(currentHealth + hpChange, 0, maxHealth);
        SetCurrentHealth(newHealth);
    }

    /// 공격 시 카드 스탯을 업데이트합니다.
    public void UpdateCardStats()
    {
        if (GameData.Instance != null)
        {
            cardAttackBonus = GameData.Instance.AttackField();
            float newDefenseBonus = GameData.Instance.DefenseField();
            
            if (newDefenseBonus > cardDefenseBonus)
            {
                float protectionIncrease = newDefenseBonus - cardDefenseBonus;
                AddProtection(protectionIncrease);
            }
            else if (newDefenseBonus < cardDefenseBonus)
            {
                float protectionDecrease = cardDefenseBonus - newDefenseBonus;
                if (protectionValue > 0)
                {
                    protectionValue = Mathf.Max(0, protectionValue - protectionDecrease);
                    if (protectionValue <= 0)
                    {
                        hasDefense = false;
                    }
                }
            }
            
            cardDefenseBonus = newDefenseBonus;
            lastAttackBonus = cardAttackBonus;
            lastDefenseBonus = cardDefenseBonus;
            
            RefreshUI();
        }
    }

    private void InitializeAnimation()
    {
        if (playerAnimation == null)
        {
            playerAnimation = GetComponent<PlayerAnimation>();
        }
    }

    public void Heal(int amount)
    {
        if (amount <= 0 || StatusEffectQueries.BlocksHealing(this))
        {
            return;
        }

        currentHealth = Mathf.Clamp(currentHealth + amount, 0, maxHealth);
        RefreshUI();
    }

    public IEnumerator PerformAttack(IEnumerable<IDamageable> targets, bool isAreaAttack = false, Vector3? primaryTargetWorldPosition = null)
    {
        UpdateCardStats();
        CacheSpriteOrigin();

        int currentAttack = AttackValue;

        if (HasDefenseCardsOnField())
        {
            yield return ApplyDefenseStatsWithEffect();
        }

        if (playerAnimation != null)
        {
            playerAnimation.TriggerAttackByValue(currentAttack);
            yield return playerAnimation.WaitForEnforceAnimationComplete(currentAttack);
            yield return playerAnimation.WaitUntilMoveState(currentAttack);
        }

        Vector3 attackPosition = initialSpriteLocalPosition;

        if (isAreaAttack)
        {
            attackPosition.x = 0f;
        }
        else if (primaryTargetWorldPosition.HasValue && spriteRenderer != null)
        {
            Transform spriteParent = spriteRenderer.transform.parent;
            Vector3 targetLocal = spriteParent != null
                ? spriteParent.InverseTransformPoint(primaryTargetWorldPosition.Value)
                : primaryTargetWorldPosition.Value;

            attackPosition.x = targetLocal.x - Mathf.Abs(singleTargetAttackOffset);
        }
        else
        {
            attackPosition.x = initialSpriteLocalPosition.x - Mathf.Abs(singleTargetAttackOffset);
        }

        yield return StartCoroutine(MoveSpriteToPosition(attackPosition, attackMoveDuration));

        if (playerAnimation != null)
        {
            playerAnimation.TriggerAttack();
            yield return playerAnimation.WaitUntilAttackHitFrame(currentAttack);
            AttackEffectSpawner.SpawnOnTargets(currentAttack, targets);
        }
        else if (targets != null)
        {
            AttackEffectSpawner.SpawnOnTargets(currentAttack, targets);
        }

        if (targets != null)
        {
            foreach (IDamageable target in targets)
            {
                if (target != null && target.IsAlive)
                {
                    BaseUnit targetUnit = target as BaseUnit;
                    int finalDamage = ApplyOutgoingStatusEffects(currentAttack, targetUnit);
                    if (targetUnit != null)
                    {
                        targetUnit.TakeDamage(finalDamage, this);
                    }
                    else
                    {
                        target.TakeDamage(finalDamage);
                    }
                }
            }
        }

        if (playerAnimation != null)
        {
            yield return playerAnimation.WaitForAttackAnimationComplete(currentAttack);
        }

        yield return ReturnToOriginalPosition();
    }

    private IEnumerator MoveSpriteToPosition(Vector3 targetPosition, float duration)
    {
        if (spriteRenderer == null)
        {
            yield break;
        }

        targetPosition.y = initialSpriteLocalPosition.y;
        
        if (duration <= 0f)
        {
            Vector3 finalPosition = spriteRenderer.transform.localPosition;
            finalPosition.x = targetPosition.x;
            finalPosition.y = initialSpriteLocalPosition.y;
            spriteRenderer.transform.localPosition = finalPosition;
            yield break;
        }

        Vector3 startPosition = spriteRenderer.transform.localPosition;
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(elapsedTime / duration);
            Vector3 currentPosition = Vector3.Lerp(startPosition, targetPosition, t);
            currentPosition.y = initialSpriteLocalPosition.y;
            spriteRenderer.transform.localPosition = currentPosition;
            yield return null;
        }

        Vector3 finalPos = targetPosition;
        finalPos.y = initialSpriteLocalPosition.y;
        spriteRenderer.transform.localPosition = finalPos;
    }

    private void CacheSpriteOrigin()
    {
        if (!hasCachedSpriteOrigin && spriteRenderer != null)
        {
            initialSpriteLocalPosition = spriteRenderer.transform.localPosition;
            hasCachedSpriteOrigin = true;
        }
    }
    
    public IEnumerator MoveToAttackPosition(Vector3? attackAnchorPosition, bool isAreaAttack)
    {
        CacheSpriteOrigin();
        
        SetSortingOrder(AttackSortingOrder);
        
        Vector3 attackPosition = initialSpriteLocalPosition;
        
        if (isAreaAttack)
        {
            attackPosition.x = 0f;
        }
        else if (attackAnchorPosition.HasValue && spriteRenderer != null)
        {
            Transform spriteParent = spriteRenderer.transform.parent;
            Vector3 targetLocal = spriteParent != null
                ? spriteParent.InverseTransformPoint(attackAnchorPosition.Value)
                : attackAnchorPosition.Value;
            
            attackPosition.x = targetLocal.x - Mathf.Abs(singleTargetAttackOffset);
        }
        else
        {
            attackPosition.x = initialSpriteLocalPosition.x - Mathf.Abs(singleTargetAttackOffset);
        }
        
        yield return StartCoroutine(MoveSpriteToPosition(attackPosition, attackMoveDuration));
    }
    
    public IEnumerator ReturnToOriginalPosition()
    {
        yield return StartCoroutine(MoveSpriteToPosition(initialSpriteLocalPosition, attackMoveDuration));
        SetSortingOrder(NormalSortingOrder);

        if (playerAnimation != null)
        {
            playerAnimation.TriggerSetPosition();
        }
    }

    protected override void ApplyIncomingDamage(int amount, BaseUnit source)
    {
        UpdateCardStats();
        base.ApplyIncomingDamage(amount, source);
    }

    protected override void OnAfterIncomingDamageApplied()
    {
        if (playerAnimation != null)
        {
            if (IsAlive)
            {
                playerAnimation.PlayHitAnimation();
            }
            else
            {
                playerAnimation.PlayDeadAnimation();
            }
        }
    }
    
    public void ApplyAttackStats()
    {
        if (GameData.Instance != null)
        {
            // 기본 공격력에 카드 필드의 공격력을 더함
            float fieldAttackPower = GameData.Instance.AttackField();
            int currentAttack = AttackValue;
            cardAttackBonus = fieldAttackPower;
            int targetAttack = AttackValue;
            lastAttackBonus = cardAttackBonus;
            
            // 공격력 애니메이션
            AnimateAttackText(currentAttack, targetAttack);
        }
    }

    public void SetBattleSynergyAttackBonus(int bonus)
    {
        int fromValue = AttackValue;
        battleSynergyAttackBonus = Mathf.Max(0, bonus);
        int toValue = AttackValue;
        if (fromValue == toValue)
        {
            return;
        }
        AnimateAttackText(fromValue, toValue);
    }

    public void SetTurnSynergyAttackBonus(int bonus)
    {
        int fromValue = AttackValue;
        turnSynergyAttackBonus = Mathf.Max(0, bonus);
        int toValue = AttackValue;
        if (fromValue == toValue)
        {
            return;
        }
        AnimateAttackText(fromValue, toValue);
    }

    public void ApplyDefenseStats()
    {
        if (GameData.Instance != null)
        {
            // 기존 보호력이 있으면 그 값에 더하고, 없으면 새로 설정
            float fieldDefensePower = GameData.Instance.DefenseField();
            float currentProtection = protectionValue;
            float targetProtection = protectionValue > 0 ? protectionValue + fieldDefensePower : fieldDefensePower;
            
            cardDefenseBonus = fieldDefensePower;
            lastDefenseBonus = cardDefenseBonus;
            
            // 보호력 애니메이션
            AnimateProtection(currentProtection, targetProtection);
        }
    }

    public void GainProtectionWithEffect(float amount)
    {
        if (amount <= 0f)
        {
            return;
        }

        bool hadShield = protectionValue > 0f;
        AnimateProtection(protectionValue, protectionValue + amount);
        if (!hadShield)
        {
            PlayShieldEffect();
        }
    }

    public void PlayShieldEffect()
    {
        if (shieldEffectCoroutine != null)
        {
            StopCoroutine(shieldEffectCoroutine);
        }

        shieldEffectCoroutine = StartCoroutine(ShowDefenseEffect());
    }

    public IEnumerator ShowDefenseEffect()
    {
        if (effectShield == null)
        {
            yield break;
        }

        int version = ++shieldEffectVersion;
        effectShield.SetActive(true);

        Animator shieldAnimator = effectShield.GetComponent<Animator>();
        if (shieldAnimator != null)
        {
            shieldAnimator.Play(ShieldEffectStateName, 0, 0f);
            shieldAnimator.Update(0f);
        }

        yield return new WaitForSeconds(ShieldEffectDuration);

        if (version != shieldEffectVersion || effectShield == null)
        {
            yield break;
        }

        effectShield.SetActive(false);
        shieldEffectCoroutine = null;
    }

    public IEnumerator ApplyDefenseStatsWithEffect()
    {
        if (GameData.Instance == null)
        {
            yield break;
        }

        float fieldDefensePower = GameData.Instance.DefenseField();
        if (fieldDefensePower <= 0f)
        {
            yield break;
        }

        bool hadShield = protectionValue > 0f;
        ApplyDefenseStats();
        if (!hadShield)
        {
            yield return ShowDefenseEffect();
        }
    }

    public bool HasDefenseCardsOnField()
    {
        return GameData.Instance != null && GameData.Instance.DefenseField() > 0f;
    }
    
    private void AnimateAttackText(int fromValue, int toValue)
    {
        if (attackText == null)
        {
            return;
        }

        if (attackTextTweener != null && attackTextTweener.IsActive())
        {
            pendingAttackTweenTargets.Enqueue(toValue);
            return;
        }

        StartAttackTextTween(fromValue, toValue);
    }

    private void StartAttackTextTween(int fromValue, int toValue)
    {
        int currentValue = fromValue;
        attackTextTweener = DOTween.To(
            () => currentValue,
            x =>
            {
                currentValue = x;
                attackText.text = currentValue.ToString();
            },
            toValue,
            statAnimationDuration
        )
        .SetEase(statAnimationEase)
        .OnComplete(ProcessPendingAttackTextTween);
    }

    private void ProcessPendingAttackTextTween()
    {
        if (pendingAttackTweenTargets.Count == 0)
        {
            return;
        }

        int nextTarget = pendingAttackTweenTargets.Dequeue();
        int fromValue = int.TryParse(attackText.text, out int parsedValue) ? parsedValue : AttackValue;
        StartAttackTextTween(fromValue, nextTarget);
    }
    
    private void AnimateProtection(float fromValue, float toValue)
    {
        if (Mathf.Abs(fromValue - toValue) < 0.01f)
        {
            RefreshUI();
            return;
        }
        
        if (protectionTweener != null && protectionTweener.IsActive())
        {
            protectionTweener.Kill();
        }
        
        float currentValue = fromValue;
        protectionTweener = DOTween.To(
            () => currentValue,
            x => {
                currentValue = x;
                protectionValue = currentValue;
                hasDefense = currentValue > 0;
                RefreshUI();
            },
            toValue,
            statAnimationDuration
        ).SetEase(statAnimationEase);
    }
    
    public void ResetAttackToBase()
    {
        cardAttackBonus = 0;
        turnSynergyAttackBonus = 0;
        lastAttackBonus = 0;
        
        // TODO: 추후 보호력이 남는 효과 추가 시 수정
        SetProtection(0);
        cardDefenseBonus = 0;
        lastDefenseBonus = 0;
        
        RefreshUI();
    }
    
    protected override void RefreshUI()
    {
        base.RefreshUI();

        if (attackText != null && (attackTextTweener == null || !attackTextTweener.IsActive()))
        {
            attackText.text = AttackValue.ToString();
        }
    }

    private void SetSortingOrder(int sortingOrder)
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.sortingOrder = sortingOrder;
        }
    }

    protected override void OnDestroy()
    {
        UnsubscribeDataCenterHpEvent();
        UnsubscribeStatusEffectEvents();
        
        base.OnDestroy();
        
        if (attackTextTweener != null && attackTextTweener.IsActive())
        {
            attackTextTweener.Kill();
        }
        pendingAttackTweenTargets.Clear();
        
        if (protectionTweener != null && protectionTweener.IsActive())
        {
            protectionTweener.Kill();
        }
    }

    private void UnsubscribeDataCenterHpEvent()
    {
        if (DataCenter.Instance != null)
        {
            DataCenter.Instance.playerHpEvent -= OnPlayerHPChanged;
        }
    }

    private void InitializeStatusEffectUI()
    {
        StatusEffectIcon.RegisterPrefab(statusEffectIconPrefab);
        RefreshStatusIcons();
    }

    private void SubscribeStatusEffectEvents()
    {
        statusEffectController = GetComponent<StatusEffectController>();
        if (statusEffectController == null)
        {
            return;
        }

        statusEffectController.OnStatusAdded += OnPlayerStatusUpdated;
        statusEffectController.OnStatusRemoved += OnPlayerStatusUpdated;
        statusEffectController.OnStatusChanged += OnPlayerStatusUpdated;
        RefreshStatusIcons();
    }

    private void UnsubscribeStatusEffectEvents()
    {
        if (statusEffectController == null)
        {
            return;
        }

        statusEffectController.OnStatusAdded -= OnPlayerStatusUpdated;
        statusEffectController.OnStatusRemoved -= OnPlayerStatusUpdated;
        statusEffectController.OnStatusChanged -= OnPlayerStatusUpdated;
        statusEffectController = null;
    }

    private void OnPlayerStatusUpdated(StatusEffectRuntime runtime)
    {
        RefreshStatusIcons();
    }

    private void RefreshStatusIcons()
    {
        if (statusBottomRoot == null || statusEffectController == null)
        {
            return;
        }

        StatusEffectIconStrip.Refresh(
            statusBottomRoot,
            statusEffectController.ActiveStatuses,
            statusEffectIconPrefab,
            ResolveStatusIcon);
    }

    private Sprite ResolveStatusIcon(string statusEffectId)
    {
        if (StatusEffectIconCatalog.TryGetIcon(statusEffectId, out Sprite sprite))
        {
            return sprite;
        }

        if (statusEffectId == StatusEffectController.WeaknessExposureStatusId)
        {
            return weaknessStatusSprite;
        }

        if (statusEffectId == StatusEffectController.CurseStatusId)
        {
            return curseStatusSprite;
        }

        return null;
    }

    public TooltipContent GetTooltipContent()
    {
        var tooltipId = "Player";
        var tooltipDict = new Dictionary<string, object>()
        {
            { "attackPower", AttackValue }
        };
        
        return new TooltipContent(tooltipId, tooltipDict);
    }
}

