using System;
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

    [Header("시너지별 스프라이트 (synergyId = SynergyData.ID 또는 Name, 또는 Synergy_ 접두 제거한 뒤 이름)")]
    [SerializeField] private SynergyVisualBinding[] _synergyVisuals;

    [Header("3개 초과 시")]
    [SerializeField] private GameObject _overflowRoot;

    private Dictionary<string, SynergyVisualBinding> _visualById;

    private void Awake()
    {
        BuildVisualLookup();
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
        GameData.Instance.GetSynergyData();
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
        if (_slots == null || _slots.Length == 0 || synergyMap == null)
        {
            return;
        }

        List<SynergyTotalData> ordered = synergyMap.Values
                                                   .Where(s => s.synergyData != null)
                                                   .OrderByDescending(IsSynergyActivated)
                                                   .ThenByDescending(s => s.count)
                                                   .ThenBy(s => s.synergyData.Tier)
                                                   .ToList();

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
            slot.root.SetActive(show);
            if (!show)
            {
                continue;
            }

            SynergyTotalData entry = ordered[i];
            SynergyData sd = entry.synergyData;

            TryGetBinding(sd, out SynergyVisualBinding visual);
            ApplyIcon(slot.icon, visual);
            ApplyGauge(slot.gauge, visual, entry);
            ApplyTooltip(slot, entry);
        }

        int overflow = total - _slots.Length;
        if (_overflowRoot != null)
        {
            bool hasOverflow = overflow > 0;
            _overflowRoot.SetActive(hasOverflow);
            if (hasOverflow)
            {
                ApplyOverflowTooltip(ordered.Skip(_slots.Length).ToList());
            }
        }
    }

    public void SetVisible(bool isVisible)
    {
        if (!isVisible)
        {
            HideAllSlots();
            return;
        }

        if (GameData.Instance != null)
        {
            GameData.Instance.GetSynergyData();
        }
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
                }
            }
        }

        if (_overflowRoot != null)
        {
            _overflowRoot.SetActive(false);
        }
    }

    private bool TryGetBinding(SynergyData sd, out SynergyVisualBinding binding)
    {
        binding = null;
        if (sd == null || _visualById == null)
        {
            return false;
        }

        if (!string.IsNullOrEmpty(sd.ID) && _visualById.TryGetValue(sd.ID.Trim(), out binding))
        {
            return true;
        }

        string name = sd.Name != null ? sd.Name.Trim() : string.Empty;
        if (name.Length > 0)
        {
            if (_visualById.TryGetValue(name, out binding))
            {
                return true;
            }

            const string prefix = "Synergy_";
            if (name.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            {
                string shortName = name.Substring(prefix.Length);
                if (shortName.Length > 0 && _visualById.TryGetValue(shortName, out binding))
                {
                    return true;
                }
            }
        }

        return false;
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
        if (slot?.root == null)
        {
            return;
        }

        SynergyDescriptor tooltip = slot.root.GetComponent<SynergyDescriptor>();
        if (tooltip == null)
        {
            tooltip = slot.root.AddComponent<SynergyDescriptor>();
        }

        tooltip.SetTooltipData(entry);
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
    }
}
