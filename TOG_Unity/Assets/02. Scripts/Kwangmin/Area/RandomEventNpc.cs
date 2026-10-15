using System;
using UnityEngine;

public sealed class RandomEventNpc : ClickableObject
{
    [SerializeField] private string npcID;

    public string NpcID => npcID;

    public event Action<RandomEventNpc> Clicked;

    protected override void OnEnable()
    {
        base.OnEnable();
        m_interactable_object.OnMouseUpAction += HandleMouseUp;
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        m_interactable_object.OnMouseUpAction -= HandleMouseUp;
    }

    private void HandleMouseUp()
    {
        Clicked?.Invoke(this);
    }
}
