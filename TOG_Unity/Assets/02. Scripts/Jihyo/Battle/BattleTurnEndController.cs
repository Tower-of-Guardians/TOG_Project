using System.Collections.Generic;
using Jongmin;
using UnityEngine;

public class BattleTurnEndController : MonoBehaviour, IBattleController
{
    private BattleManager battleManager;
    private bool isInitialized;

    public bool IsInitialized => isInitialized;

    [SerializeField] private EffectDomain effectDomain; 

    public void Initialize(BattleManager manager)
    {
        if (isInitialized)
        {
            Debug.LogWarning("BattleTurnEndController has already been initialized.");
            return;
        }

        battleManager = manager;
        isInitialized = true;

        StartCoroutine(AttachTurnManagerDelayed());
    }

    private System.Collections.IEnumerator AttachTurnManagerDelayed()
    {
        yield return new WaitUntil(() => DIContainer.IsRegistered<TurnManager>());

        var turnManager = DIContainer.Resolve<TurnManager>();
        if (turnManager != null)
        {
            turnManager.OnStartNewTurn += OnStartNewTurn;
        }
    }

    public void Cleanup()
    {
        if (DIContainer.IsRegistered<TurnManager>())
        {
            var turnManager = DIContainer.Resolve<TurnManager>();
            if (turnManager != null)
            {
                turnManager.OnStartNewTurn -= OnStartNewTurn;
            }
        }

        battleManager = null;
        isInitialized = false;
    }

    private void OnStartNewTurn()
    {
        // 새 턴 시작 시 카드 드로우
        DrawCards();
    }

    public void ProcessTurnEnd()
    {
        int currentTurnNumber = -1;
        if (DIContainer.IsRegistered<TurnManager>())
        {
            TurnManager turnManager = DIContainer.Resolve<TurnManager>();
            if (turnManager != null)
            {
                currentTurnNumber = turnManager.CurrentTurnNumber;
            }
        }

        // 턴 종료 시 플레이어의 공격력을 기본 공격력으로 되돌림
        if (battleManager != null)
        {
            var setupController = battleManager.GetSetupController();
            if (setupController != null)
            {
                Player player = setupController.GetPlayer();
                if (player != null)
                {
                    player.ResetAttackToBase();
                }

                List<Monster> monsters = setupController.GetPrimaryMonsters();
                for (int i = 0; i < monsters.Count; i++)
                {
                    if (monsters[i] == null)
                    {
                        continue;
                    }

                    monsters[i].ExpireGuardShieldIfNeeded(currentTurnNumber);
                }
            }
        }
        
        // 사용하지 않은 핸드 카드 모두 버리기
        DiscardAllHandCards();

    }

    public void DrawCards(int count = -1)
    {
        if (!DIContainer.IsRegistered<TurnManager>())
        {
            return;
        }

        StartCoroutine(effectDomain.DrawHandCards(count));
    }

    private void DiscardAllHandCards()
    {

        // // 사용 카드 버리기
        // if (GameData.Instance != null)
        // {
        //     var attackFieldCards = new List<CardData>(GameData.Instance.attackField);
        //     foreach (var cardData in attackFieldCards)
        //     {
        //         if (cardData != null)
        //         {
        //             GameData.Instance.UseCard(cardData.id);
        //         }
        //     }
        //     GameData.Instance.attackField.Clear();
            
        //     var defenseFieldCards = new List<CardData>(GameData.Instance.defenseField);
        //     foreach (var cardData in defenseFieldCards)
        //     {
        //         if (cardData != null)
        //         {
        //             GameData.Instance.UseCard(cardData.id);
        //         }
        //     }
        //     GameData.Instance.defenseField.Clear();
        // }
        
        // // Hand UI 카드 제거
        // // if (DIContainer.IsRegistered<HandPresenter>())
        // // {
        // //     var handPresenter = DIContainer.Resolve<HandPresenter>();
            
        // //     // 핸드 카드 버리기
        // //     var handCards = handPresenter.GetCardDatas();
        // //     if (handCards != null && handCards.Length > 0)
        // //     {
        // //         foreach (var cardData in handCards)
        // //         {
        // //             if (cardData != null && cardData.data != null)
        // //             {
        // //                 // 버리는 덱에 추가
        // //                 GameData.Instance.UseCard(cardData.data.id);
        // //             }
        // //         }
        // //     }

        // //     handPresenter.ClearAllCards();
        // //     GameData.Instance.handDeck.Clear();
        // // }
        
        // // Attacking Field UI 카드 제거
        // if (DIContainer.IsRegistered<AttackFieldPresenter>())
        // {
        //     var attackFieldPresenter = DIContainer.Resolve<AttackFieldPresenter>();
        //     attackFieldPresenter.RemoveAll();
        // }
        
        // // Defending Field UI 카드 제거
        // if (DIContainer.IsRegistered<DefendFieldPresenter>())
        // {
        //     var defendFieldPresenter = DIContainer.Resolve<DefendFieldPresenter>();
        //     defendFieldPresenter.RemoveAll();
        // }
    }
}

