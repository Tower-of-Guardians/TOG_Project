using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using DG.Tweening;
using JxModule;
using UnityEngine;

namespace Jongmin
{
    public class AttributeView : ViewBase
    {
        [SerializeField] private Card card;
        [SerializeField] private Transform synergyGroup;

        private LabelBoxView[] _synergyDescViews;
        
        private void Awake()
        {
            _synergyDescViews = synergyGroup.GetComponentsInChildren<LabelBoxView>();
        }

        public void Show()
        {
            CanvasGroup.Show();
        }

        public void Hide()
        {
            CanvasGroup.Hide();
        }

        public void SetAttribute(CardData cardData, IReadOnlyList<SynergyData> synergyDatas)
        {
            card.SetCardData(cardData);

            foreach (var synergyDescView in _synergyDescViews)
            {
                synergyDescView.gameObject.SetActive(false);
            }

            for (var i = 0; i < synergyDatas.Count; i++)
            {
                if (synergyDatas[i] == null)
                {
                    continue;
                }
                
                var synergyDescView = _synergyDescViews[i]; 
                synergyDescView.gameObject.SetActive(true);
                synergyDescView.Label.text = $"{GetDisplayName(synergyDatas[i].Name)}\n{GetDisplayDesc(synergyDatas[i].Description)}";
            }
        }

        private static string GetDisplayName(string synergyName)
        {
            if (string.IsNullOrEmpty(synergyName))
            {
                return string.Empty;
            }

            const string prefix = "Synergy_";
            return synergyName.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)
                ? synergyName.Substring(prefix.Length)
                : synergyName;
        }

        private static string GetDisplayDesc(string description)
        { 
            return Regex.Replace(description, @"\{.*?\}", "N");
        }
    }
}