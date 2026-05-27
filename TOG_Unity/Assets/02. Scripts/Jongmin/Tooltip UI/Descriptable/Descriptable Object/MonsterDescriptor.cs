using UnityEngine;

[RequireComponent(typeof(Monster))]
public class MonsterDescriptor : BaseDescriptor
{
    private Monster m_monster_unit;

    private void Awake()
    {
        m_monster_unit = GetComponent<Monster>();
    }

    public override TooltipData GetTooltipData()
    {
        return new TooltipData{ Description = $"이 적은 5의 수치로 <color=red>공격</color>하려고 합니다.",
                                Position = m_tooltip_position};
    }
}
