public class DialogueBubblePresenter
{
    private readonly IDialogueBubbleUI _dialogueBubbleUI;

    public DialogueBubblePresenter(IDialogueBubbleUI dialogueBubbleUI)
        => _dialogueBubbleUI = dialogueBubbleUI;

    public void OpenUI()
        => _dialogueBubbleUI.OpenUI();

    public void CloseUI()
        => _dialogueBubbleUI.CloseUI();

    public void SetBubble(string dialogueString)
        => _dialogueBubbleUI.SetBubble(dialogueString);
}
