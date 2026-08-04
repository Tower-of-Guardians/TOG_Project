using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using JxModule;
using TMPro;
using UnityEngine;

public class AreaEventUI : ViewBase
{
    [SerializeField] private TMP_Text _texTopLabel;
    [SerializeField] private AreaEventItemUI[] _items;

    private Action<AreaEventType> _onEventSelected;
    private Tween _toggleTween;

    private void Awake()
    {
        InitializeItems();
    }

    public void Bind(Action<AreaEventType> onEventSelected)
    {
        _onEventSelected = onEventSelected;
    }

    private void InitializeItems()
    {
        if (_items == null) return;

        foreach (var item in _items)
        {
            if (item != null)
            {
                item.Init(OnItemClicked);
            }
        }
    }

    private void OnItemClicked(AreaEventType type)
    {
        if (_onEventSelected != null)
        {
            _onEventSelected.Invoke(type);
        }
        else
        {
            DefaultOnItemClicked(type);
        }
    }

    private void DefaultOnItemClicked(AreaEventType type)
    {
        switch (type)
        {
            case AreaEventType.Shop:
                LoadingManager.Instance?.LoadScene("AreaEvent_Shop");
                Hide();
                break;
            case AreaEventType.Blacksmith:
                LoadingManager.Instance?.LoadScene("AreaEvent_Blacksmith");
                Hide();
                break;
            case AreaEventType.Blessing:
                LoadingManager.Instance?.LoadScene("AreaEvent_Blessing");
                Hide();
                break;
            default:
                Hide();
                break;
        }
    }

    public IEnumerator Show()
    {
        CanvasGroup.Hide();

        _toggleTween?.Kill();
        _toggleTween = CanvasGroup.DOFade(1f, 0.5f).OnComplete(CanvasGroup.Show);

        yield return _toggleTween.WaitForCompletion();
    }

    public IEnumerator Show(string title, List<AreaEventType> typeList)
    {
        RefreshData(title, typeList);
        yield return Show();
    }

    public IEnumerator ShowWithCurrentData()
    {
        if (DataCenter.areaevent_datas != null && DataCenter.areaevent_datas.Count > 0)
        {
            AreaEventData data = null;
            if (!string.IsNullOrEmpty(TestId) && DataCenter.areaevent_datas.ContainsKey(TestId))
            {
                data = DataCenter.areaevent_datas[TestId];
            }
            else
            {
                foreach (var kvp in DataCenter.areaevent_datas)
                {
                    data = kvp.Value;
                    break;
                }
            }

            if (data != null)
            {
                var status = new PlayerEventStatus(TestShopCountInStage, TestSmithyCountInStage, TestBlessingCooldownTurns);
                var list = AreaEventSelectorUtil.GetNextRegionChoices(data, status);
                RefreshData(data.Name, list);
            }
        }

        yield return Show();
    }

    public void Hide()
    {
        _toggleTween?.Kill();
        _toggleTween = CanvasGroup.DOFade(0f, 0.5f).OnComplete(CanvasGroup.Hide);
    }

    public void RefreshData(string title, List<AreaEventType> typeList)
    {
        if (_texTopLabel != null)
        {
            _texTopLabel.text = title;
        }

        if (_items == null) return;

        foreach (var item in _items)
        {
            if (item != null)
            {
                item.gameObject.SetActive(false);
            }
        }

        if (typeList == null) return;

        foreach (var type in typeList)
        {
            foreach (var item in _items)
            {
                if (item != null && item.Type == type)
                {
                    item.transform.SetAsFirstSibling();
                    item.gameObject.SetActive(true);
                    break;
                }
            }
        }
    }

    #region Test

    [Header("Test Mode")]
    public string TestId;
    public int TestShopCountInStage;
    public int TestSmithyCountInStage;
    public int TestBlessingCooldownTurns;

    [ContextMenu("Test")]
    public void Test()
    {
        if (DataCenter.areaevent_datas != null && DataCenter.areaevent_datas.ContainsKey(TestId))
        {
            var data = DataCenter.areaevent_datas[TestId];
            var status = new PlayerEventStatus(TestShopCountInStage, TestSmithyCountInStage, TestBlessingCooldownTurns);
            var list = AreaEventSelectorUtil.GetNextRegionChoices(data, status);

            foreach (var item in list)
            {
                switch (item)
                {
                    case AreaEventType.Shop:
                        TestShopCountInStage++;
                        break;
                    case AreaEventType.Blacksmith:
                        TestSmithyCountInStage++;
                        break;
                    case AreaEventType.Blessing:
                        TestBlessingCooldownTurns++;
                        break;
                }
            }

            RefreshData(data.Name, list);
        }
    }

    #endregion
}
