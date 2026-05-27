using JxDialogueBox;
using UnityEngine;

public class TestXyilbia : ClickableObject
{
    [Header("Dialogue Runner")]
    [SerializeField] private DialogueRunner m_dialogue_runner;

    bool m_dialogue_completed = false;

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
        string dialogue_id = string.Empty;
        if(!m_dialogue_completed)
        {
            dialogue_id = "demo_simple";
            m_dialogue_completed = true;
        }
        else
            dialogue_id = "test2";

        m_dialogue_runner.StartDialogue(dialogue_id);
    }
}
