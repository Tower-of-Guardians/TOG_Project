using System.Collections.Generic;
using JxModule;
using TMPro;
using UnityEngine;

namespace Jongmin
{
    public class UITooltipView : TooltipViewBase
    {
        [BigHeader("UI")]
        [SerializeField] private TMP_Text headerLabel;
        [SerializeField] private TMP_Text tagLabel;
        [SerializeField] private TMP_Text bodyLabel;

        protected override void Bind(TooltipDataTableRow tooltipDataTableRow, TooltipContent tooltipContent)
        {
            var values = tooltipContent.Values;

            SetLabel(headerLabel, GetFormattedText(tooltipDataTableRow.headerText, values, "headerIndex"));
            SetLabel(tagLabel, GetFormattedText(tooltipDataTableRow.tagText, values, "tagIndex"));
            SetLabel(bodyLabel, GetFormattedText(tooltipDataTableRow.bodyText, values, "bodyIndex"));
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

        private static string GetFormattedText(IReadOnlyList<string> texts,
                                               IReadOnlyDictionary<string, object> values,
                                               string indexKey)
        {
            if (texts == null || texts.Count <= 0)
            {
                return string.Empty;
            }

            var index = GetIndex(values, indexKey);
            if (index < 0 || index >= texts.Count)
            {
                index = 0;
            }

            return TooltipTextFormatter.Format(texts[index], values);
        }

        private static int GetIndex(IReadOnlyDictionary<string, object> values, string indexKey)
        {
            if (values == null || !values.TryGetValue(indexKey, out var rawIndex))
            {
                return 0;
            }

            return rawIndex switch
            {
                int intValue => intValue,
                float floatValue => Mathf.RoundToInt(floatValue),
                double doubleValue => Mathf.RoundToInt((float)doubleValue),
                string stringValue when int.TryParse(stringValue, out var parsedValue) => parsedValue,
                _ => 0
            };
        }
    }
}
