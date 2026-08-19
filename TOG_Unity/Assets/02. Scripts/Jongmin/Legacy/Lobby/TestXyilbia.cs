using JxDialogueBox;
using UnityEngine;

public class TestXyilbia : ClickableObject
{
    [Header("Dialogue Runner")]
    [SerializeField] private DialogueRunner dialogueRunner;

    protected override void OnEnable()
    {
        base.OnEnable();
        m_interactable_object.OnMouseUpAction += TryDialogue;
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        m_interactable_object.OnMouseUpAction -= TryDialogue; 
    }

    private void TryDialogue()
    {
        var dialogueId = "EventPriest_First";
        dialogueRunner.StartDialogue(dialogueId);
    }
}
