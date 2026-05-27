using System;

public class CraftmanDialogueBubblePresenter : DialogueBubblePresenter, IDisposable
{
    private readonly CraftmanDeckInvenPresenter _craftmanDeckInvenPresenter;
    private readonly ForgePresenter _forgePresenter;

    public CraftmanDialogueBubblePresenter(IDialogueBubbleUI dialogueBubbleUI,
                                           CraftmanDeckInvenPresenter craftmanDeckInvenPresenter,
                                           ForgePresenter forgePresenter)
        : base(dialogueBubbleUI)
    {
        _craftmanDeckInvenPresenter = craftmanDeckInvenPresenter;
        _forgePresenter = forgePresenter;
        
        _craftmanDeckInvenPresenter.OnCardSelected += SetSelectedCardBubble;
        _craftmanDeckInvenPresenter.OnInvenOpened += OpenUI;
        _craftmanDeckInvenPresenter.OnInvenClosed += CloseUI;
        _craftmanDeckInvenPresenter.OnRequestedDefaultBubble += SetDefaultBubble;
        
        _forgePresenter.OnReinforceCompleted += SetReinforcedBubble;
    }
    
    public void Dispose()
    {
        if (_craftmanDeckInvenPresenter != null)
        {
            _craftmanDeckInvenPresenter.OnCardSelected -= SetSelectedCardBubble;
            _craftmanDeckInvenPresenter.OnInvenOpened -= OpenUI;
            _craftmanDeckInvenPresenter.OnInvenClosed -= CloseUI;
            _craftmanDeckInvenPresenter.OnInvenClosed -= SetDefaultBubble;  
        }

        if (_forgePresenter != null)
        {
            _forgePresenter.OnReinforceCompleted -= SetReinforcedBubble;
        }
    }

    public void SetDefaultBubble()
        => SetBubble("어떤 카드를 강화해줄까?");   

    public void SetSelectedCardBubble(CardData cardData)
        => SetBubble("그래, 이 카드를 강화하고 싶다고?\n어떻게 해줄까?");

    public void SetReinforcedBubble()
        => SetBubble("열심히 해.");
}
