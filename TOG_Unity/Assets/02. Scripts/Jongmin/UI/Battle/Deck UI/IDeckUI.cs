public interface IDeckUI : IOpenableUI
{
    void Construct(DeckPresenter presenter);
    
    void UpdateUI(string titleString);
    void UpdateThrowCardCount(int amount);
    void UpdateDrawCardCount(int amount);
}