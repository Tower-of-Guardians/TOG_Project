using UnityEngine;

[DefaultExecutionOrder(-99)]
public sealed class LobbySceneView : MonoBehaviour
{
    [SerializeField] private bool showLobby;
    [SerializeField] private GameObject lobbyRoot;
    [SerializeField] private GameObject[] gameRoots;

    private bool[] previousActiveStates;
    private bool hasSnapshot;

    public bool IsLobbyVisible => lobbyRoot != null && lobbyRoot.activeInHierarchy;

    public void SetLobbyVisible(bool visible)
    {
        showLobby = visible;
        ApplyVisibility();
    }

    private void OnEnable()
    {
        ApplyVisibility();
    }

    private void ApplyVisibility()
    {
        if (!isActiveAndEnabled || lobbyRoot == null) return;

        if (showLobby)
        {
            if (!hasSnapshot)
            {
                int count = gameRoots != null ? gameRoots.Length : 0;
                previousActiveStates = new bool[count];
                for (int i = 0; i < count; i++)
                {
                    if (gameRoots[i] == null) continue;
                    previousActiveStates[i] = gameRoots[i].activeSelf;
                    gameRoots[i].SetActive(false);
                }
                hasSnapshot = true;
            }
            lobbyRoot.SetActive(true);
        }
        else
        {
            RestoreGame();
        }
    }

    private void RestoreGame()
    {
        lobbyRoot.SetActive(false);
        if (!hasSnapshot) return;

        if (gameRoots != null && previousActiveStates != null)
        {
            int count = Mathf.Min(gameRoots.Length, previousActiveStates.Length);
            for (int i = 0; i < count; i++)
            {
                if (gameRoots[i] != null) gameRoots[i].SetActive(previousActiveStates[i]);
            }
        }
        hasSnapshot = false;
        previousActiveStates = null;
    }
}
