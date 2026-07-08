using JxDialogueBox;
using UnityEngine;

public class Area_Blacksmith : ClickableObject
{

    [SerializeField] private AreaEventUI_Blacksmith _blacksmithUI;

    bool m_dialogue_completed = false;

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
        _blacksmithUI.Open();
    }
}
