using System.Collections;
using UnityEngine;

public class BattleActionController : MonoBehaviour, IBattleController
{
    private BattleManager battleManager;
    private bool isInitialized;

    private int throwCountBeforeAction;
    private int lastThrowCountBeforeRemoveAll;

    public bool IsInitialized => isInitialized;

    public void Initialize(BattleManager manager)
    {
        if (isInitialized)
        {
            Debug.LogWarning("BattleActionController has already been initialized.");
            return;
        }

        battleManager = manager;
        isInitialized = true;
        
        AttachThrowPresenter();
    }

    public void Cleanup()
    {
        DetachThrowPresenter();
        throwCountBeforeAction = 0;
        lastThrowCountBeforeRemoveAll = 0;
        battleManager = null;
        isInitialized = false;
    }

    public void OnTurnStart()
    {
        // 매 턴마다 Throw 시스템 초기화
        if (DIContainer.IsRegistered<TurnManager>())
        {
            var turnManager = DIContainer.Resolve<TurnManager>();
            turnManager.Initialize();

            throwCountBeforeAction = 0;
        }
    }

    private void AttachThrowPresenter()
    {
        // TurnManager가 등록될 때까지 대기 후 이벤트 구독
        StartCoroutine(AttachTurnManagerDelayed());
    }

    private IEnumerator AttachTurnManagerDelayed()
    {
        yield return new WaitUntil(() => DIContainer.IsRegistered<TurnManager>());
        
        var turnManager = DIContainer.Resolve<TurnManager>();
        turnManager.OnUpdatedThrowActionState += OnThrowActionStateChanged;
        turnManager.OnUpdatedThrowCount += OnThrowCountChanged;
    }

    private void DetachThrowPresenter()
    {
        if (DIContainer.IsRegistered<TurnManager>())
        {
            var turnManager = DIContainer.Resolve<TurnManager>();
            turnManager.OnUpdatedThrowActionState -= OnThrowActionStateChanged;
            turnManager.OnUpdatedThrowCount -= OnThrowCountChanged;
        }
    }

    private void OnThrowCountChanged(ActionData actionData)
    {
        // Throw 카운트가 증가할 때 저장
        if (actionData.Current > throwCountBeforeAction)
        {
            throwCountBeforeAction = actionData.Current;
            lastThrowCountBeforeRemoveAll = actionData.Current;
        }
    }

    private void OnThrowActionStateChanged(bool canThrow)
    {
        if (!canThrow)
        {
            int throwCount = throwCountBeforeAction;
            
            if (throwCount == 0 && lastThrowCountBeforeRemoveAll > 0)
            {
                throwCount = lastThrowCountBeforeRemoveAll;
            }
            
            if (throwCount > 0 && battleManager != null)
            {
                // 애니메이션 완료를 위해 약간의 지연 후 드로우
                StartCoroutine(DrawCardsAfterThrowDelayed(throwCount));
            }
            
            lastThrowCountBeforeRemoveAll = 0;
            throwCountBeforeAction = 0;
        }
    }
    
    private IEnumerator DrawCardsAfterThrowDelayed(int throwCount)
    {
        float waitTime = throwCount * 0.3f;
        yield return new WaitForSeconds(waitTime);
        
        if (throwCount > 0 && battleManager != null)
        {
            // Throw한 카드 수만큼 드로우
            battleManager.RequestDrawCards(throwCount);
        }
    }
}

