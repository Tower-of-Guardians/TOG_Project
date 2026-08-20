using System.Collections.Generic;
using System.Text;
using JxModule;
using TMPro;
using UnityEngine;

namespace Jongmin
{
    public class SynergyTooltipView : TooltipViewBase
    {
        private const int EntryHeaderIndex = 0;
        private const int OverflowHeaderIndex = 1;
        private const int DescriptionIndex = 0;
        private const int ActiveTierLineIndex = 1;
        private const int InactiveTierLineIndex = 2;
        private const int OverflowLineIndex = 3;
        private const int StarTextIndex = 4;

        [BigHeader("UI")]
        [SerializeField] private TMP_Text headerLabel;
        [SerializeField] private TMP_Text descriptionLabel;
        [SerializeField] private TMP_Text tierLabel;

        protected override void Bind(TooltipDataTableRow tooltipDataTableRow, TooltipContent tooltipContent)
        {
            if (TryGetOverflowEntries(tooltipContent, out var overflowEntries))
            {
                BindOverflow(tooltipDataTableRow, overflowEntries);
                return;
            }

            if (!TryGetEntry(tooltipContent, out var entry) || entry?.synergyData == null)
            {
                Clear();
                return;
            }

            BindEntry(tooltipDataTableRow, entry);
        }

        private void BindEntry(TooltipDataTableRow tooltipDataTableRow, SynergyTotalData entry)
        {
            var synergyData = entry.synergyData;
            var starText = GetTemplate(tooltipDataTableRow.tagText, StarTextIndex);
            var description = (synergyData.Description ?? string.Empty).Replace("☆", starText);
            description = ApplyEffectValues(description, synergyData, entry.count);
            var values = new Dictionary<string, object>
            {
                ["synergyName"] = synergyData.Name,
                ["synergyDescription"] = description,
                ["currentCount"] = entry.count
            };

            SetLabel(headerLabel, FormatTemplate(GetTemplate(tooltipDataTableRow.headerText, EntryHeaderIndex), values));
            SetLabel(descriptionLabel, FormatTemplate(GetTemplate(tooltipDataTableRow.bodyText, DescriptionIndex), values));
            SetLabel(tierLabel, BuildTierText(tooltipDataTableRow, synergyData, entry.count));
        }

        private void BindOverflow(TooltipDataTableRow tooltipDataTableRow, IReadOnlyList<SynergyTotalData> overflowEntries)
        {
            SetLabel(headerLabel, FormatTemplate(GetTemplate(tooltipDataTableRow.headerText, OverflowHeaderIndex), null));
            SetLabel(descriptionLabel, BuildOverflowText(tooltipDataTableRow, overflowEntries));
            SetLabel(tierLabel, string.Empty);
        }

        private void Clear()
        {
            SetLabel(headerLabel, string.Empty);
            SetLabel(descriptionLabel, string.Empty);
            SetLabel(tierLabel, string.Empty);
        }

        private string BuildTierText(TooltipDataTableRow tooltipDataTableRow, SynergyData synergyData, int currentCount)
        {
            var requirements = GetTierRequirements(synergyData);
            if (requirements.Count <= 0)
            {
                return string.Empty;
            }

            var activeTierIndex = GetActiveTierIndex(requirements, currentCount);
            var builder = new StringBuilder(64);

            for (var i = 0; i < requirements.Count; i++)
            {
                var templateIndex = i == activeTierIndex ? ActiveTierLineIndex : InactiveTierLineIndex;
                var values = new Dictionary<string, object>
                {
                    ["requiredCount"] = requirements[i],
                    ["gradeName"] = GetGradeName(tooltipDataTableRow, i),
                    ["tierIndex"] = i + 1
                };

                builder.Append(FormatTemplate(GetTemplate(tooltipDataTableRow.bodyText, templateIndex), values));

                if (i < requirements.Count - 1)
                {
                    builder.Append('\n');
                }
            }

            return builder.ToString();
        }

        private string BuildOverflowText(TooltipDataTableRow tooltipDataTableRow,
                                         IReadOnlyList<SynergyTotalData> overflowEntries)
        {
            if (overflowEntries == null || overflowEntries.Count <= 0)
            {
                return string.Empty;
            }

            var builder = new StringBuilder(64);
            var appendedAny = false;

            for (var i = 0; i < overflowEntries.Count; i++)
            {
                var entry = overflowEntries[i];
                if (entry?.synergyData == null)
                {
                    continue;
                }

                if (appendedAny)
                {
                    builder.Append('\n');
                }

                var values = new Dictionary<string, object>
                {
                    ["synergyName"] = entry.synergyData.Name,
                    ["activatedCount"] = GetActivatedCount(entry),
                    ["currentCount"] = entry.count
                };

                builder.Append(FormatTemplate(GetTemplate(tooltipDataTableRow.bodyText, OverflowLineIndex), values));
                appendedAny = true;
            }

            return builder.ToString();
        }

        private List<int> GetTierRequirements(SynergyData synergyData)
        {
            var requirements = new List<int>();
            if (synergyData == null)
            {
                return requirements;
            }

            var maxTierCount = Mathf.Max(GetEffectCount(synergyData.Effect1Synergys),
                                         Mathf.Max(GetEffectCount(synergyData.Effect2Synergys),
                                                   GetEffectCount(synergyData.Effect3Synergys)));

            for (var i = 0; i < maxTierCount; i++)
            {
                var requiredCount = i + 1;
                var hasAnyEffectValue = GetEffectValueAtIndex(synergyData.Effect1Synergys, i) > 0
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

        private int GetActiveTierIndex(IReadOnlyList<int> requirements, int currentCount)
        {
            var activeIndex = -1;
            for (var i = 0; i < requirements.Count; i++)
            {
                if (currentCount >= requirements[i])
                {
                    activeIndex = i;
                }
            }

            return activeIndex;
        }

        private string GetGradeName(TooltipDataTableRow tooltipDataTableRow, int tierIndex)
        {
            if (tooltipDataTableRow?.tagText != null
                && tierIndex >= 0
                && tierIndex < tooltipDataTableRow.tagText.Count)
            {
                return tooltipDataTableRow.tagText[tierIndex];
            }

            return (tierIndex + 1).ToString();
        }

        private int GetActivatedCount(SynergyTotalData entry)
        {
            if (entry?.synergyData == null)
            {
                return 0;
            }

            var requirements = GetTierRequirements(entry.synergyData);
            var activeTierIndex = GetActiveTierIndex(requirements, entry.count);
            return activeTierIndex >= 0 ? entry.count : 0;
        }

        private string ApplyEffectValues(string template, SynergyData synergyData, int currentCount)
        {
            var effect1Value = GetEffectValueAtCurrentCount(synergyData?.Effect1Synergys, currentCount);
            var effect2Value = GetEffectValueAtCurrentCount(synergyData?.Effect2Synergys, currentCount);
            var effect3Value = GetEffectValueAtCurrentCount(synergyData?.Effect3Synergys, currentCount);

            return template.Replace("{0}", effect1Value.ToString())
                           .Replace("{1}", effect2Value.ToString())
                           .Replace("{2}", effect3Value.ToString());
        }

        private int GetEffectValueAtCurrentCount(IReadOnlyList<int> effectValues, int currentCount)
        {
            if (effectValues == null || effectValues.Count == 0 || currentCount <= 0)
            {
                return 0;
            }

            var index = Mathf.Clamp(currentCount - 1, 0, effectValues.Count - 1);
            return effectValues[index];
        }

        private int GetEffectCount(IReadOnlyCollection<int> effectValues)
        {
            return effectValues?.Count ?? 0;
        }

        private int GetEffectValueAtIndex(IReadOnlyList<int> effectValues, int index)
        {
            if (effectValues == null || index < 0 || index >= effectValues.Count)
            {
                return 0;
            }

            return effectValues[index];
        }

        private static bool TryGetEntry(TooltipContent tooltipContent, out SynergyTotalData entry)
        {
            entry = null;
            return TryGetValue(tooltipContent, "entry", out entry)
                   || TryGetValue(tooltipContent, "synergyEntry", out entry)
                   || TryGetValue(tooltipContent, "synergyTotalData", out entry);
        }

        private static bool TryGetOverflowEntries(TooltipContent tooltipContent,
                                                  out IReadOnlyList<SynergyTotalData> entries)
        {
            entries = null;

            if (TryGetValue<List<SynergyTotalData>>(tooltipContent, "overflowEntries", out var listEntries))
            {
                entries = listEntries;
                return true;
            }

            if (TryGetValue<SynergyTotalData[]>(tooltipContent, "overflowEntries", out var arrayEntries))
            {
                entries = arrayEntries;
                return true;
            }

            return false;
        }

        private static bool TryGetValue<T>(TooltipContent tooltipContent, string key, out T value)
        {
            value = default;
            if (tooltipContent?.Values == null)
            {
                return false;
            }

            if (!tooltipContent.Values.TryGetValue(key, out var rawValue) || rawValue is not T typedValue)
            {
                return false;
            }

            value = typedValue;
            return true;
        }

        private static string GetTemplate(IReadOnlyList<string> templates, int index)
        {
            if (templates == null || index < 0 || index >= templates.Count)
            {
                return string.Empty;
            }

            return templates[index];
        }

        private static string FormatTemplate(string template, IReadOnlyDictionary<string, object> values)
        {
            return TooltipTextFormatter.Format(template, values);
        }

        private static void SetLabel(TMP_Text label, string text)
        {
            if (label == null)
            {
                return;
            }

            label.text = text;
            label.gameObject.SetActive(!string.IsNullOrWhiteSpace(text));
        }
    }
}
