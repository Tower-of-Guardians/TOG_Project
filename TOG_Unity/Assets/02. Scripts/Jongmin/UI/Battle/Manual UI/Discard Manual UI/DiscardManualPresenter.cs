using System;
using VContainer.Unity;

public class DiscardManualPresenter : IDisposable, IInitializable
{
    private readonly IDiscardManualUI _manualUI;
    private readonly TurnManager _turnManager;

    public DiscardManualPresenter(IDiscardManualUI manualUI,
                                TurnManager turnManager)
    {
        _manualUI = manualUI;
        _turnManager = turnManager;
    }
    public void Initialize()
    {
        _turnManager.OnUpdatedThrowCount += UpdateUI;
        _turnManager.Initialize();
    }

    public void Dispose()
        => _turnManager.OnUpdatedActionCount -= UpdateUI;

    private void UpdateUI(ActionData actionData)
        => _manualUI.UpdateUI(actionData, _turnManager.CanThrow);
}