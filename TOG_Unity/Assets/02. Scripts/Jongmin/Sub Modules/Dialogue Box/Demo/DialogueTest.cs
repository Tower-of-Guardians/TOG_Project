using JxDialogueBox;
using UnityEngine;

public sealed class DialogueTest : MonoBehaviour
{
    [SerializeField] private DialogueRunner runner;
    [SerializeField] private string dialogueId = "test";

    public void StartDialogue()
    {
        if (!runner)
        {
            Debug.LogWarning("[DialogueTestButton] runner가 비어있음");
            return;
        }

        runner.StartDialogue(dialogueId);
    }
}
