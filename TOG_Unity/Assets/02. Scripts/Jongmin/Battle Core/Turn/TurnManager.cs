using UnityEngine;
using System;
using VContainer;

public class TurnManager : MonoBehaviour, ITurnHandLimitPort
{
    [Header("Test Option")]
    [SerializeField] private int _cardCount;

    private ITurnRuleService _turnRuleService;
    private bool _isCanThrow;

    public event Action<ActionData> OnUpdatedActionCount;
    public event Action<ActionData> OnUpdatedThrowCount;
    public event Action<bool> OnUpdatedThrowActionState;
    public event Action OnStartNewTurn;
    public event Action OnEndCurrentTurn;
    public event Action<int> OnTurnNumberChanged;

    public int CurrentActionCount { get; private set; }
    public int CurrentThrowCount { get; private set; }
    public int CurrentTurnNumber { get; private set; }

    public int MaxActionCount => _turnRuleService.GetRule(_cardCount).MaxActionCount;
    public int MaxHandCount => _turnRuleService.GetRule(_cardCount).MaxHandCount;
    public int MaxThrowCount => MaxActionCount;

    public bool CanAction => CurrentActionCount < MaxActionCount;
    public bool CanThrow => _isCanThrow && CurrentThrowCount < MaxThrowCount;

    [Inject]
    public void Construct(ITurnRuleService turnRuleService)
    {
        _turnRuleService = turnRuleService;

        Initialize();
    }

    public void Initialize()
    {
        CurrentActionCount = 0;
        CurrentThrowCount = 0;
        UpdateThrowAction(true);

        AlertToUpdateActionCount();
        AlertToUpdateThrowCount();
    }

    /// <summary>
    /// 0과 최대 행동 수 사이에서 액션 횟수를 제한하여 갱신합니다.
    /// </summary>
    public void UpdateActionCount(int amount)
    {
        CurrentActionCount += amount;
        CurrentActionCount = Mathf.Clamp(CurrentActionCount, 0, MaxActionCount);

        AlertToUpdateActionCount();
    }

    /// <summary>
    /// 0과 최대 교체 수 사이에서 교체 횟수를 제한하여 갱신합니다.
    /// </summary>
    public void UpdateThrowCount(int amount)
    {
        CurrentThrowCount += amount;
        CurrentThrowCount = Mathf.Clamp(CurrentThrowCount, 0, MaxThrowCount);

        AlertToUpdateThrowCount();
    }

    /// <summary>
    /// 교체 가능 여부를 변경합니다.
    /// </summary>
    public void UpdateThrowAction(bool isActive)
    {
        _isCanThrow = isActive;
        OnUpdatedThrowActionState?.Invoke(_isCanThrow);
    }

    public void StartTurn()
    {
        CurrentTurnNumber++;
        OnTurnNumberChanged?.Invoke(CurrentTurnNumber);
        OnStartNewTurn?.Invoke();
    }

    public void EndTurn()
        => OnEndCurrentTurn?.Invoke();

    public void ResetTurnNumber()
        => CurrentTurnNumber = 0;

    private void AlertToUpdateActionCount()
        => OnUpdatedActionCount?.Invoke(new ActionData(CurrentActionCount, MaxActionCount));

    private void AlertToUpdateThrowCount()
        => OnUpdatedThrowCount?.Invoke(new ActionData(CurrentThrowCount, MaxThrowCount));
}
