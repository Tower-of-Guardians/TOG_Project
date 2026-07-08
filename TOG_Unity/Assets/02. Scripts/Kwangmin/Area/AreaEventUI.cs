using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class AreaEventUI : MonoBehaviour
{
    [SerializeField] private TMP_Text _texTopLabel;
    [SerializeField] private AreaEventItemUI[] _items;
    [SerializeField] private GameObject _obPanel;




    public void Start()
    {
        foreach (var item in _items)
        {
            item.Init(OnClickAction);
        }
    }

    private void OnClickAction(AreaEventType type)
    {
        switch (type)
        {
            case AreaEventType.Boss:
                break;
            case AreaEventType.Shop:
                LoadingManager.Instance.LoadScene("AreaEvent_Shop");
                _obPanel.gameObject.SetActive(false);
                break;
            case AreaEventType.Battle:
                break;
            case AreaEventType.Blacksmith:
                LoadingManager.Instance.LoadScene("AreaEvent_Blacksmith");
                _obPanel.gameObject.SetActive(false);
                break;
            case AreaEventType.Blessing:
                LoadingManager.Instance.LoadScene("AreaEvent_Blessing");
                _obPanel.gameObject.SetActive(false);
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
