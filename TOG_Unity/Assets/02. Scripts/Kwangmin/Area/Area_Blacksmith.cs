using JxDialogueBox;
using UnityEngine;

public class Area_Blacksmith : ClickableObject
{
    [SerializeField] private AreaEventUI_Blacksmith _blacksmithUI;

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
        if (_blacksmithUI != null)
        {
            _blacksmithUI.Open();
        }
    }
}
