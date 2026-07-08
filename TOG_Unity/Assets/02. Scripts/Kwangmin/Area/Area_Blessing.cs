using JxDialogueBox;
using UnityEngine;

public class Area_Blessing : ClickableObject
{

    [SerializeField] private AreaEventUI_Blessing _blessingUI;

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
        _blessingUI.Open();
    }
}
