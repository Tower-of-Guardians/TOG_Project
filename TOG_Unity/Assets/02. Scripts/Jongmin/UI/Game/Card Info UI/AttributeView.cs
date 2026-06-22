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
        private Tween _toggleTween;
        
        private void Awake()
        {
            _synergyDescViews = synergyGroup.GetComponentsInChildren<LabelBoxView>();
        }

        public void Show()
        {
            _toggleTween?.Kill();
            _toggleTween = CanvasGroup.DOFade(1f, 0.25f).OnComplete(CanvasGroup.Show);
        }

        public void Hide()
        {
            _toggleTween?.Kill();
            _toggleTween = CanvasGroup.DOFade(0f, 0.25f).OnComplete(CanvasGroup.Hide);
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
                synergyDescView.Label.text = $"{synergyDatas[i].Name}\n{GetDisplayDesc(synergyDatas[i].Description)}";
            }
        }

        private static string GetDisplayDesc(string description)
        { 
            return Regex.Replace(description, @"\{.*?\}", "N");
        }
    }
}