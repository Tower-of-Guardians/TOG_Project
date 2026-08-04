using JxDialogueBox;
using UnityEngine;

public class Area_Shop : ClickableObject
{
    [SerializeField] private AreaEventUI_Shop _shopUI;

    protected override void OnEnable()
    {
        base.OnEnable();
        m_interactable_object.OnMouseUpAction += TryShopUI;
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        m_interactable_object.OnMouseUpAction -= TryShopUI; 
    }

    private void TryShopUI()
    {
        if (_shopUI != null)
        {
            _shopUI.Open();
        }
    }
}
