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
    [SerializeField] private GameObject _obPanel;

    private Tween _toggleTween;

    public void Start()
    {
        foreach (var item in _items)
        {
            item.Init(OnClickAction);
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
        if (!string.IsNullOrEmpty(TestId) && DataCenter.areaevent_datas.ContainsKey(TestId))
        {
            var data = DataCenter.areaevent_datas[TestId];
            var list = AreaEventSelectorUtil.GetNextRegionChoices(data, new PlayerEventStatus(TestShopCountInStage, TestSmithyCountInStage, TestBlessingCooldownTurns));
            RefreshData(data.Name, list);
        }
        else
        {
            foreach (var kvp in DataCenter.areaevent_datas)
            {
                var data = kvp.Value;
                var list = AreaEventSelectorUtil.GetNextRegionChoices(data, new PlayerEventStatus(TestShopCountInStage, TestSmithyCountInStage, TestBlessingCooldownTurns));
                RefreshData(data.Name, list);
                break;
            }
        }

        yield return Show();
    }

    public void Hide()
    {
        _toggleTween?.Kill();
        _toggleTween = CanvasGroup.DOFade(0f, 0.5f).OnComplete(CanvasGroup.Hide);
    }

    private void OnClickAction(AreaEventType type)
    {
        switch (type)
        {
            case AreaEventType.Boss:
                break;
            case AreaEventType.Shop:
                LoadingManager.Instance.LoadScene("AreaEvent_Shop");
                Hide();
                break;
            case AreaEventType.Battle:
                break;
            case AreaEventType.Blacksmith:
                LoadingManager.Instance.LoadScene("AreaEvent_Blacksmith");
                Hide();
                break;
            case AreaEventType.Blessing:
                LoadingManager.Instance.LoadScene("AreaEvent_Blessing");
                Hide();
                break;
            case AreaEventType.Random:
                break;
            default:
                break;
        }
        Debug.Log($"Clicked on {type}");
    }

    public void RefreshData(string title, List<AreaEventType> typeList)
    {
        _texTopLabel.text = title;

        foreach (var item in _items)
        {
            item.gameObject.SetActive(false);
        }

        foreach (var type in typeList)
        {
            foreach (var item in _items)
            {
                if (item.Type == type)
                {
                    item.transform.SetAsFirstSibling();
                    item.gameObject.SetActive(true);
                    break;
                }
            }
        }
    }

    #region Test

    public string TestId;

    public int TestShopCountInStage;
    public int TestSmithyCountInStage;
    public int TestBlessingCooldownTurns;

    [ContextMenu("Test")]
    public void Test()
    {
        if (DataCenter.areaevent_datas.ContainsKey(TestId))
        {
            var data = DataCenter.areaevent_datas[TestId];

            var list = AreaEventSelectorUtil.GetNextRegionChoices(data, new PlayerEventStatus(TestShopCountInStage, TestSmithyCountInStage, TestBlessingCooldownTurns));


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
