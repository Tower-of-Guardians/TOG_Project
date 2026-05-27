using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BattleManagerInjector : MonoBehaviour, IInjector
{
    [SerializeField] private BattleManager battleManager;
    [SerializeField] private Player player;
    [SerializeField] private List<Monster> initialMonsters = new();
    [SerializeField] private Button attackButton;

    public void Inject()
    {
        if (battleManager == null)
        {
            battleManager = GetComponent<BattleManager>();
        }

        if (battleManager == null)
        {
            Debug.LogError("BattleManagerInjector requires a BattleManager reference.");
            return;
        }

        if (player == null)
        {
            player = FindAnyObjectByType<Player>();
        }
        
        battleManager.Initialize(player, initialMonsters, attackButton);

        if (!DIContainer.IsRegistered<BattleManager>())
        {
            DIContainer.Register<BattleManager>(battleManager);
        }
    }
}

