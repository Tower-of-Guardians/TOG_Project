using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.EventSystems;

public class SynergyDescriptor : MonoBehaviour, IDescriptableUI
{
    private readonly string[] m_gradeLabels = { "일반", "레어", "유니크", "에픽" };

    [Header("툴팁의 위치")]
    [SerializeField] private Vector3 m_tooltip_position;

    private readonly TooltipData m_tooltip_data = new TooltipData();
    private TooltipPresenter m_tooltip_presenter;

    public void Inject(TooltipPresenter tooltip_presenter)
    {
        m_tooltip_presenter = tooltip_presenter;
    }

    public void SetTooltipData(SynergyTotalData entry)
    {
        m_tooltip_data.Description = BuildSynergyTooltip(entry);
        m_tooltip_data.Position = m_tooltip_position;
    }

    public void SetOverflowTooltipData(List<SynergyTotalData> overflowEntries)
    {
        m_tooltip_data.Description = BuildOverflowTooltip(overflowEntries);
        m_tooltip_data.Position = m_tooltip_position;
    }

    public TooltipData GetTooltipData()
    {
        m_tooltip_data.Position = m_tooltip_position;
        return m_tooltip_data;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (string.IsNullOrEmpty(m_tooltip_data.Description))
        {
            return;
        }

        if (!TryResolvePresenter())
        {
            return;
        }

        m_tooltip_presenter.OpenUI(this);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (!TryResolvePresenter())
        {
            return;
        }

        m_tooltip_presenter.CloseUI();
    }

    private bool TryResolvePresenter()
    {
        if (m_tooltip_presenter != null)
        {
            return true;
        }

        if (!DIContainer.IsRegistered<TooltipPresenter>())
        {
            return false;
        }

        m_tooltip_presenter = DIContainer.Resolve<TooltipPresenter>();
        return m_tooltip_presenter != null;
    }

    private string BuildSynergyTooltip(SynergyTotalData entry)
    {
        SynergyData synergyData = entry != null ? entry.synergyData : null;
        if (synergyData == null)
        {
            return string.Empty;
        }

        string name = synergyData.Name ?? string.Empty;

        // TODO:: 현재 exqt SDF 폰트에 '☆'(U+2606) 글리프가 없어 임시로 별로 치환
        string description = (synergyData.Description ?? string.Empty).Replace("☆", "별");
        description = ApplyEffectValues(description, synergyData, entry.count);
        List<int> requirements = GetTierRequirements(synergyData);
        int activeTierIndex = GetActiveTierIndex(requirements, entry.count);

        StringBuilder sb = new StringBuilder(128);
        sb.Append(name);
        sb.Append("\n");
        sb.Append(description);

        if (requirements.Count <= 0)
        {
            return sb.ToString();
        }

        sb.Append("\n\n");
        for (int i = 0; i < requirements.Count; i++)
        {
            string line = $"({requirements[i]}) : {GetGradeName(i)} 등급";
            if (i == activeTierIndex)
            {
                sb.Append("<color=#FFFFFF>").Append(line).Append("</color>");
            }
            else
            {
                sb.Append("<color=#6E6E6E>").Append(line).Append("</color>");
            }

            if (i < requirements.Count - 1)
            {
                sb.Append("\n");
            }
        }

        return sb.ToString();
    }

    private string BuildOverflowTooltip(List<SynergyTotalData> overflowEntries)
    {
        if (overflowEntries == null || overflowEntries.Count == 0)
        {
            return string.Empty;
        }

        StringBuilder sb = new StringBuilder(128);
        sb.Append("+\n");

        bool appendedAny = false;
        for (int i = 0; i < overflowEntries.Count; i++)
        {
            SynergyTotalData entry = overflowEntries[i];
            if (entry?.synergyData == null)
            {
                continue;
            }

            if (appendedAny)
            {
                sb.Append("\n");
            }

            string name = entry.synergyData.Name ?? string.Empty;
            int activatedCount = GetActivatedCount(entry);
            sb.Append($"{name}({activatedCount})");
            appendedAny = true;
        }

        return appendedAny ? sb.ToString() : string.Empty;
    }

    private List<int> GetTierRequirements(SynergyData synergyData)
    {
        List<int> requirements = new List<int>();
        if (synergyData == null)
        {
            return requirements;
        }

        int maxTierCount = Mathf.Max(GetEffectCount(synergyData.Effect1Synergys),
                                     Mathf.Max(GetEffectCount(synergyData.Effect2Synergys),
                                               GetEffectCount(synergyData.Effect3Synergys)));

        for (int i = 0; i < maxTierCount; i++)
        {
            int requiredCount = i + 1;
            bool hasAnyEffectValue = GetEffectValueAtIndex(synergyData.Effect1Synergys, i) > 0
                                     || GetEffectValueAtIndex(synergyData.Effect2Synergys, i) > 0
                                     || GetEffectValueAtIndex(synergyData.Effect3Synergys, i) > 0;
            if (hasAnyEffectValue && !requirements.Contains(requiredCount))
            {
                requirements.Add(requiredCount);
            }
        }

        requirements.Sort();
        return requirements;
    }

    private int GetActiveTierIndex(List<int> requirements, int currentCount)
    {
        int activeIndex = -1;
        for (int i = 0; i < requirements.Count; i++)
        {
            if (currentCount >= requirements[i])
            {
                activeIndex = i;
            }
        }

        return activeIndex;
    }

    private string GetGradeName(int tierIndex)
    {
        if (tierIndex >= 0 && tierIndex < m_gradeLabels.Length)
        {
            return m_gradeLabels[tierIndex];
        }

        return $"{tierIndex + 1}단계";
    }

    private int GetActivatedCount(SynergyTotalData entry)
    {
        if (entry?.synergyData == null)
        {
            return 0;
        }

        List<int> requirements = GetTierRequirements(entry.synergyData);
        int activeTierIndex = GetActiveTierIndex(requirements, entry.count);
        return activeTierIndex >= 0 ? entry.count : 0;
    }

    private string ApplyEffectValues(string template, SynergyData synergyData, int currentCount)
    {
        int effect1Value = GetEffectValueAtCurrentCount(synergyData?.Effect1Synergys, currentCount);
        int effect2Value = GetEffectValueAtCurrentCount(synergyData?.Effect2Synergys, currentCount);
        int effect3Value = GetEffectValueAtCurrentCount(synergyData?.Effect3Synergys, currentCount);

        return template.Replace("{0}", effect1Value.ToString())
                       .Replace("{1}", effect2Value.ToString())
                       .Replace("{2}", effect3Value.ToString());
    }

    private int GetEffectValueAtCurrentCount(List<int> effectValues, int currentCount)
    {
        if (effectValues == null || effectValues.Count == 0 || currentCount <= 0)
        {
            return 0;
        }

        int index = Mathf.Clamp(currentCount - 1, 0, effectValues.Count - 1);
        return effectValues[index];
    }

    private int GetEffectCount(List<int> effectValues)
    {
        return effectValues != null ? effectValues.Count : 0;
    }

    private int GetEffectValueAtIndex(List<int> effectValues, int index)
    {
        if (effectValues == null || index < 0 || index >= effectValues.Count)
        {
            return 0;
        }

        return effectValues[index];
    }

}
