using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

/// <summary>
/// 필드 기준 시너지 집계를 표시합니다. 상위 3개는 슬롯에, 그 외는 Overflow 영역에 표시합니다.
/// </summary>
public class SynergyUI : MonoBehaviour
{
    [System.Serializable]
    public class SynergySlot
    {
        public GameObject root;
        public Image icon;
        public Image gauge;
    }

    /// <summary>
    /// SynergyData.ID와 동일한 문자열로 매칭
    /// </summary>
    [System.Serializable]
    public class SynergyVisualBinding
    {
        public string synergyId;
        public Sprite icon;
        [Tooltip("이 시너지의 최소 발동 개수. 이 개수부터 1단계가 되고, 이후 1장 추가될 때마다 단계가 1씩 증가합니다.")]
        [Min(1)]
        [FormerlySerializedAs("countPerGaugeBlock")]
        public int miniRequiredCount = 1;
        public Sprite[] gaugeSprites;
    }

    [SerializeField] private SynergySlot[] _slots = new SynergySlot[3];

    [Header("시너지별 스프라이트 (synergyId = SynergyData.ID)")]
    [SerializeField] private SynergyVisualBinding[] _synergyVisuals;

    [Header("3개 초과 시")]
    [SerializeField] private GameObject _overflowRoot;

    [Header("등장 / 발동 연출")]
    [SerializeField] private float _fadeDuration = 0.35f;
    [SerializeField] private float _synergyActivationInterval = 0.35f;
    [SerializeField] private float _highlightScale = 1.12f;
    [SerializeField] private float _highlightDuration = 0.2f;

    private CanvasGroup _canvasGroup;
    private Coroutine _fadeCoroutine;
    private Coroutine _highlightCoroutine;
    private Dictionary<string, SynergyVisualBinding> _visualById;
    private bool _isVisible;
    private bool _overflowWasActive;
    private readonly bool[] _slotWasActive = new bool[3];
    private readonly List<SynergyTotalData> _displayedEntries = new List<SynergyTotalData>();

    private void Awake()
    {
        BuildVisualLookup();
        EnsureCanvasGroup();
        _canvasGroup.alpha = 0f;
        UpdateRaycastBlocking(false);
    }

    private void BuildVisualLookup()
    {
        _visualById = new Dictionary<string, SynergyVisualBinding>(StringComparer.OrdinalIgnoreCase);
        if (_synergyVisuals == null)
        {
            return;
        }

        foreach (SynergyVisualBinding binding in _synergyVisuals)
        {
            if (binding == null || string.IsNullOrEmpty(binding.synergyId))
            {
                continue;
            }

            _visualById[binding.synergyId.Trim()] = binding;
        }
    }

    private void Start()
    {
        if (GameData.Instance == null)
        {
            return;
        }

        GameData.Instance.SynergyChange += OnSynergyChange;
    }

    private void OnDestroy()
    {
        if (GameData.Instance != null)
        {
            GameData.Instance.SynergyChange -= OnSynergyChange;
        }
    }

    private void OnSynergyChange(Dictionary<string, SynergyTotalData> synergyMap)
    {
        if (!_isVisible || _slots == null || _slots.Length == 0 || synergyMap == null)
        {
            return;
        }

        List<SynergyTotalData> ordered = synergyMap.Values
                                                   .Where(s => s.synergyData != null)
                                                   .OrderByDescending(IsSynergyActivated)
                                                   .ThenByDescending(s => s.count)
                                                   .ThenBy(s => s.synergyData.Tier)
                                                   .ToList();

        _displayedEntries.Clear();
        _displayedEntries.AddRange(ordered);

        int total = ordered.Count;
        int showSlots = Mathf.Min(_slots.Length, total);

        for (int i = 0; i < _slots.Length; i++)
        {
            SynergySlot slot = _slots[i];
            if (slot?.root == null)
            {
                continue;
            }

            bool show = i < showSlots;
            if (show)
            {
                SynergyTotalData entry = ordered[i];
                SynergyData sd = entry.synergyData;

                TryGetBinding(sd, out SynergyVisualBinding visual);
                ApplyIcon(slot.icon, visual);
                ApplyGauge(slot.gauge, visual, entry);
                ApplyTooltip(slot, entry);

                if (!_slotWasActive[i])
                {
                    ShowElementWithFade(slot.root);
                    _slotWasActive[i] = true;
                }
                else
                {
                    slot.root.SetActive(true);
                    SetElementAlpha(slot.root, 1f);
                }
            }
            else
            {
                slot.root.SetActive(false);
                _slotWasActive[i] = false;
            }
        }

        int overflow = total - _slots.Length;
        if (_overflowRoot != null)
        {
            bool hasOverflow = overflow > 0;
            if (hasOverflow)
            {
                ApplyOverflowTooltip(ordered.Skip(_slots.Length).ToList());

                if (!_overflowWasActive)
                {
                    ShowElementWithFade(_overflowRoot);
                    _overflowWasActive = true;
                }
                else
                {
                    _overflowRoot.SetActive(true);
                    SetElementAlpha(_overflowRoot, 1f);
                }
            }
            else
            {
                _overflowRoot.SetActive(false);
                _overflowWasActive = false;
            }
        }
    }

    public void SetVisible(bool isVisible)
    {
        if (!isVisible)
        {
            StartHideWithFade();
            return;
        }

        _isVisible = true;
        ResetSlotVisibilityState();
        gameObject.SetActive(true);
        EnsureCanvasGroup();
        _canvasGroup.alpha = 0f;
        UpdateRaycastBlocking(false);

        if (GameData.Instance != null)
        {
            GameData.Instance.GetSynergyData();
        }

        StartFade(1f, () => UpdateRaycastBlocking(true));
    }

    /// <summary>
    /// 시너지 발동 후 창을 그라데이션으로 숨깁니다.
    /// </summary>
    public IEnumerator HideWithFade()
    {
        bool completed = false;
        StartHideWithFade(() => completed = true);

        while (!completed)
        {
            yield return null;
        }
    }

    private void StartHideWithFade(Action onComplete = null)
    {
        _isVisible = false;
        gameObject.SetActive(true);
        EnsureCanvasGroup();
        UpdateRaycastBlocking(false);

        StartFade(0f, () =>
        {
            ResetSlotVisibilityState();
            onComplete?.Invoke();
        });
    }

    private void ResetSlotVisibilityState()
    {
        for (int i = 0; i < _slotWasActive.Length; i++)
        {
            _slotWasActive[i] = false;
        }

        _overflowWasActive = false;
        HideAllSlots();
    }

    private void ShowElementWithFade(GameObject target)
    {
        if (target == null)
        {
            return;
        }

        target.SetActive(true);

        CanvasGroup canvasGroup = target.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = target.AddComponent<CanvasGroup>();
        }

        canvasGroup.alpha = 0f;
        StartCoroutine(CoFadeElement(canvasGroup, 1f));

        CanvasGroup group = target.GetComponent<CanvasGroup>();
        if (group != null)
        {
            group.blocksRaycasts = true;
        }
    }

    private static void SetElementAlpha(GameObject target, float alpha)
    {
        if (target == null)
        {
            return;
        }

        CanvasGroup canvasGroup = target.GetComponent<CanvasGroup>();
        if (canvasGroup != null)
        {
            canvasGroup.alpha = alpha;
        }
    }

    private IEnumerator CoFadeElement(CanvasGroup canvasGroup, float targetAlpha)
    {
        if (canvasGroup == null)
        {
            yield break;
        }

        float startAlpha = canvasGroup.alpha;
        float elapsed = 0f;
        float duration = _fadeDuration <= 0f ? 0.01f : _fadeDuration;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = SmoothStep(Mathf.Clamp01(elapsed / duration));
            canvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, t);
            yield return null;
        }

        canvasGroup.alpha = targetAlpha;
    }

    /// <summary>
    /// 발동 중인 시너지를 순서대로 강조한 뒤, 호출 측에서 효과를 적용합니다.
    /// </summary>
    public IEnumerator PlaySynergyActivationSequence()
    {
        List<SynergyTotalData> activatedSynergies = GetActivatedSynergiesOrdered();
        if (activatedSynergies.Count == 0)
        {
            yield break;
        }

        for (int i = 0; i < activatedSynergies.Count; i++)
        {
            yield return HighlightSynergyEntry(activatedSynergies[i]);
            yield return new WaitForSeconds(_synergyActivationInterval);
        }

        ResetAllSlotScales();
    }

    private List<SynergyTotalData> GetActivatedSynergiesOrdered()
    {
        if (GameData.Instance?.synergyIDList == null)
        {
            return new List<SynergyTotalData>();
        }

        return GameData.Instance.synergyIDList.Values
            .Where(entry => entry?.synergyData != null && IsSynergyActivated(entry))
            .OrderByDescending(IsSynergyActivated)
            .ThenByDescending(entry => entry.count)
            .ThenBy(entry => entry.synergyData.Tier)
            .ToList();
    }

    private IEnumerator HighlightSynergyEntry(SynergyTotalData entry)
    {
        Transform target = FindDisplayedSlotTransform(entry);
        if (target == null)
        {
            yield break;
        }

        if (_highlightCoroutine != null)
        {
            StopCoroutine(_highlightCoroutine);
        }

        Vector3 originalScale = target.localScale;
        Vector3 peakScale = originalScale * _highlightScale;
        float elapsed = 0f;
        float halfDuration = _highlightDuration * 0.5f;

        while (elapsed < halfDuration)
        {
            elapsed += Time.deltaTime;
            float t = SmoothStep(Mathf.Clamp01(elapsed / halfDuration));
            target.localScale = Vector3.Lerp(originalScale, peakScale, t);
            yield return null;
        }

        elapsed = 0f;
        while (elapsed < halfDuration)
        {
            elapsed += Time.deltaTime;
            float t = SmoothStep(Mathf.Clamp01(elapsed / halfDuration));
            target.localScale = Vector3.Lerp(peakScale, originalScale, t);
            yield return null;
        }

        target.localScale = originalScale;
    }

    private Transform FindDisplayedSlotTransform(SynergyTotalData entry)
    {
        if (entry?.synergyData == null || _displayedEntries.Count == 0)
        {
            return null;
        }

        int index = _displayedEntries.FindIndex(item => IsSameSynergy(item, entry));
        if (index < 0 || index >= _slots.Length)
        {
            return _overflowRoot != null && _overflowRoot.activeSelf ? _overflowRoot.transform : null;
        }

        SynergySlot slot = _slots[index];
        return slot?.root != null ? slot.root.transform : null;
    }

    private static bool IsSameSynergy(SynergyTotalData left, SynergyTotalData right)
    {
        if (left?.synergyData == null || right?.synergyData == null)
        {
            return false;
        }

        return string.Equals(left.synergyData.ID, right.synergyData.ID, StringComparison.OrdinalIgnoreCase);
    }

    private void ResetAllSlotScales()
    {
        if (_slots == null)
        {
            return;
        }

        for (int i = 0; i < _slots.Length; i++)
        {
            SynergySlot slot = _slots[i];
            if (slot?.root != null)
            {
                slot.root.transform.localScale = Vector3.one;
            }
        }

        if (_overflowRoot != null)
        {
            _overflowRoot.transform.localScale = Vector3.one;
        }
    }

    private void EnsureCanvasGroup()
    {
        _canvasGroup = GetComponent<CanvasGroup>();
        if (_canvasGroup == null)
        {
            _canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }

        _canvasGroup.interactable = false;
    }

    private void UpdateRaycastBlocking(bool allowPointerEvents)
    {
        EnsureCanvasGroup();
        _canvasGroup.blocksRaycasts = allowPointerEvents;
    }

    private void StartFade(float targetAlpha, Action onComplete = null)
    {
        if (_fadeCoroutine != null)
        {
            StopCoroutine(_fadeCoroutine);
        }

        _fadeCoroutine = StartCoroutine(CoFadeAlpha(targetAlpha, onComplete));
    }

    private IEnumerator CoFadeAlpha(float targetAlpha, Action onComplete)
    {
        EnsureCanvasGroup();

        float startAlpha = _canvasGroup.alpha;
        float elapsed = 0f;
        float duration = _fadeDuration <= 0f ? 0.01f : _fadeDuration;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = SmoothStep(Mathf.Clamp01(elapsed / duration));
            _canvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, t);
            yield return null;
        }

        _canvasGroup.alpha = targetAlpha;
        onComplete?.Invoke();
        _fadeCoroutine = null;
    }

    private static float SmoothStep(float t)
    {
        return t * t * (3f - 2f * t);
    }

    private void HideAllSlots()
    {
        if (_slots != null)
        {
            for (int i = 0; i < _slots.Length; i++)
            {
                SynergySlot slot = _slots[i];
                if (slot?.root != null)
                {
                    slot.root.SetActive(false);
                    SetElementAlpha(slot.root, 0f);
                }
            }
        }

        if (_overflowRoot != null)
        {
            _overflowRoot.SetActive(false);
            SetElementAlpha(_overflowRoot, 0f);
        }
    }

    private bool TryGetBinding(SynergyData sd, out SynergyVisualBinding binding)
    {
        binding = null;
        if (sd == null || _visualById == null || string.IsNullOrEmpty(sd.ID))
        {
            return false;
        }

        return _visualById.TryGetValue(sd.ID.Trim(), out binding);
    }

    private bool IsSynergyActivated(SynergyTotalData entry)
    {
        if (entry?.synergyData == null)
        {
            return false;
        }

        int count = entry.count;
        if (count <= 0)
        {
            return false;
        }

        bool effect1Active = GetEffectValueAtCount(entry.synergyData.Effect1Synergys, count) > 0;
        bool effect2Active = GetEffectValueAtCount(entry.synergyData.Effect2Synergys, count) > 0;
        bool effect3Active = GetEffectValueAtCount(entry.synergyData.Effect3Synergys, count) > 0;

        return effect1Active || effect2Active || effect3Active;
    }

    private int GetEffectValueAtCount(List<int> values, int count)
    {
        if (values == null || values.Count == 0 || count <= 0)
        {
            return 0;
        }

        int index = Mathf.Clamp(count - 1, 0, values.Count - 1);
        return values[index];
    }

    private void ApplyIcon(Image iconImage, SynergyVisualBinding visual)
    {
        if (iconImage == null)
        {
            return;
        }

        if (visual != null && visual.icon != null)
        {
            iconImage.sprite = visual.icon;
            iconImage.enabled = true;
            iconImage.raycastTarget = true;
        }
        else
        {
            iconImage.sprite = null;
            iconImage.enabled = false;
        }
    }

    private void ApplyGauge(Image gaugeImage, SynergyVisualBinding visual, SynergyTotalData entry)
    {
        if (gaugeImage == null)
        {
            return;
        }

        if (visual == null || visual.gaugeSprites == null || visual.gaugeSprites.Length == 0)
        {
            gaugeImage.sprite = null;
            gaugeImage.enabled = false;
            return;
        }

        int idx = GetGaugeSpriteIndex(entry, visual);
        Sprite frame = visual.gaugeSprites[idx];
        if (frame != null)
        {
            gaugeImage.sprite = frame;
            gaugeImage.enabled = true;
        }
        else
        {
            gaugeImage.sprite = null;
            gaugeImage.enabled = false;
        }
    }

    /// <summary>
    /// 최소 발동 개수 이전에는 0단계, 이후에는 카드 1장당 1단계씩 증가하며 Gauge Sprites 개수로 클램프합니다.
    /// </summary>
    private int GetGaugeSpriteIndex(SynergyTotalData entry, SynergyVisualBinding visual)
    {
        int spriteCount = visual.gaugeSprites.Length;
        if (spriteCount <= 0)
        {
            return 0;
        }

        int miniRequiredCount = visual.miniRequiredCount < 1 ? 1 : visual.miniRequiredCount;
        int stage = entry.count - miniRequiredCount + 1;
        return Mathf.Clamp(stage, 0, spriteCount - 1);
    }

    private void ApplyTooltip(SynergySlot slot, SynergyTotalData entry)
    {
        if (slot == null)
        {
            return;
        }

        GameObject tooltipTarget = slot.icon != null ? slot.icon.gameObject : slot.root;
        if (tooltipTarget == null)
        {
            return;
        }

        SynergyDescriptor tooltip = tooltipTarget.GetComponent<SynergyDescriptor>();
        if (tooltip == null)
        {
            tooltip = tooltipTarget.AddComponent<SynergyDescriptor>();
        }

        tooltip.SetTooltipData(entry);
        TryInjectTooltipPresenter(tooltip);
    }

    private void ApplyOverflowTooltip(List<SynergyTotalData> overflowEntries)
    {
        if (_overflowRoot == null)
        {
            return;
        }

        SynergyDescriptor tooltip = _overflowRoot.GetComponent<SynergyDescriptor>();
        if (tooltip == null)
        {
            tooltip = _overflowRoot.AddComponent<SynergyDescriptor>();
        }

        tooltip.SetOverflowTooltipData(overflowEntries);
        TryInjectTooltipPresenter(tooltip);

        Image overflowImage = _overflowRoot.GetComponent<Image>();
        if (overflowImage != null)
        {
            overflowImage.raycastTarget = true;
        }
    }

    private static void TryInjectTooltipPresenter(SynergyDescriptor descriptor)
    {
        if (descriptor == null || !DIContainer.IsRegistered<TooltipPresenter>())
        {
            return;
        }

        descriptor.Inject(DIContainer.Resolve<TooltipPresenter>());
    }
}
