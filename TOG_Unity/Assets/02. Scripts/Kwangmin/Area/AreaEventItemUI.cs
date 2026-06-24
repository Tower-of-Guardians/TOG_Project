using System;
using UnityEngine;
using UnityEngine.UI;

public enum AreaEventType
{
    Boss,
    Shop,
    Battle,
    Blacksmith,
    Blessing,
    Random
}

public class AreaEventItemUI : MonoBehaviour
{
    [SerializeField] private Button _btn;
    [SerializeField] private AreaEventType _type;

    public AreaEventType Type => _type;


    public void Init(Action<AreaEventType> clickAction)
    {
        _btn.onClick.RemoveAllListeners();
        _btn.onClick.AddListener(() => clickAction?.Invoke(_type));
        gameObject.SetActive(false);
    }
}
