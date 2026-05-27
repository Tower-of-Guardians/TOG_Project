using UnityEngine;

[CreateAssetMenu(fileName = "Turn Rule Designer", menuName = "SO/Design/Turn Rule Designer")]
public class TurnRuleDesigner : ScriptableObject, ITurnRuleService
{
    [Header("턴 규칙 목록")]
    [SerializeField] private TurnRuleData[] _turnRuleArray;

    public TurnRuleData GetRule(int cardCount)
    {
        foreach(var rule in _turnRuleArray)
        {
            if(rule.Min <= cardCount && cardCount < rule.Max)
            {
                return rule;
            }
        }

        return null;
    }
}
