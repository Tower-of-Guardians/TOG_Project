namespace JxDialogueBox
{
    public interface IDialogueDataSource
    {
        bool TryGetNode(string nodeID, out DialogueNode node);
        string GetEntryNodeID(string dialogueID);
    }
}