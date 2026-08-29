using UnityEngine;
using UnityEngine.SceneManagement;

public class AreaSceneBootstrapper : MonoBehaviour
{

    private const string GameSceneName = "Game";
    private const string BattleCanvasName = "PF_BattleCanvas";

    private void Awake()
    {
        DisableGameBattleObjects();
    }

    private void DisableGameBattleObjects()
    {
        // TODO: Game 씬의 전투 종료/정리 시스템에 Player, Monster, Battle UI Clear API를 추가한 뒤 교체해야 한다.

        Scene gameScene = SceneManager.GetSceneByName(GameSceneName);
        if (!gameScene.IsValid() || !gameScene.isLoaded)
        {
            Debug.LogWarning($"{GameSceneName} 씬이 로드되어 있지 않습니다.", this);
            return;
        }

        GameObject battleCanvas = null;
        GameObject[] rootObjects = gameScene.GetRootGameObjects();

        for (int i = 0; i < rootObjects.Length; i++)
        {
            GameObject rootObject = rootObjects[i];

            Player[] players = rootObject.GetComponentsInChildren<Player>(true);
            for (int j = 0; j < players.Length; j++)
            {
                if (players[j] != null)
                {
                    players[j].gameObject.SetActive(false);
                }
            }

            Monster[] monsters = rootObject.GetComponentsInChildren<Monster>(true);
            for (int j = 0; j < monsters.Length; j++)
            {
                if (monsters[j] != null)
                {
                    monsters[j].gameObject.SetActive(false);
                }
            }

            if (battleCanvas != null)
            {
                continue;
            }

            Transform[] transforms = rootObject.GetComponentsInChildren<Transform>(true);
            for (int j = 0; j < transforms.Length; j++)
            {
                if (transforms[j].gameObject.name == BattleCanvasName)
                {
                    battleCanvas = transforms[j].gameObject;
                    break;
                }
            }
        }

        if (battleCanvas != null)
        {
            battleCanvas.SetActive(false);
        }
        else
        {
            Debug.LogWarning($"{GameSceneName} 씬에서 {BattleCanvasName} 오브젝트를 찾지 못했습니다.", this);
        }
    }
}
