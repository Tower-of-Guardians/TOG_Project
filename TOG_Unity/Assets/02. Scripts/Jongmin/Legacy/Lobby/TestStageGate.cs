using UnityEngine;

public class TestStageGate : ClickableObject
{
    protected override void OnEnable()
    {
        base.OnEnable();
        m_interactable_object.OnMouseUpAction += TryEnterStage;
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        m_interactable_object.OnMouseUpAction -= TryEnterStage; 
    }

    private void TryEnterStage()
    {
        Jongmin.RunSaveService.BeginRun();

        LobbySceneView lobbySceneView = FindFirstObjectByType<LobbySceneView>();
        if (lobbySceneView != null)
        {
            lobbySceneView.SetLobbyVisible(false);
            return;
        }

        LoadingManager.Instance.LoadScene("Game");
    }
}
