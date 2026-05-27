using System;
using VContainer.Unity;

public class ActionManualPresenter : IDisposable, IInitializable
{
    private readonly IActionManualUI _manualUI;
    private readonly TurnManager _turnManager;

    public ActionManualPresenter(IActionManualUI manualUI,
                              TurnManager turnManager)
    {
        _manualUI = manualUI;
        _turnManager = turnManager;
    }
    
    public void Initialize()
    {
        _turnManager.OnUpdatedActionCount += UpdateUI;
        _turnManager.Initialize();
    }

    public void Dispose()
        => _turnManager.OnUpdatedActionCount -= UpdateUI;

    private void UpdateUI(ActionData actionData)
        => _manualUI.UpdateUI(actionData, _turnManager.CanAction);
}