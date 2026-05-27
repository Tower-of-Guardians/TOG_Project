using UnityEngine;

[System.Serializable]
public class TurnRuleData
{
    [Header("조건")]
    [Header("조건의 최소 카드의 수")]
    [SerializeField] private int _mininumCount;
    public int Min => _mininumCount;

    [Header("조건의 최대 카드의 수")]
    [SerializeField] private int _maximumCount;
    public int Max => _maximumCount;

    [Space(30f), Header("효과")]
    [Header("동시에 보유할 수 있는 카드의 수")]
    [SerializeField] private int _handCount;
    public int MaxHandCount => _handCount;

    [Header("한 턴에 사용할 수 있는 카드의 수")]
    [SerializeField] private int _actionCount;
    public int MaxActionCount => _actionCount;
}
