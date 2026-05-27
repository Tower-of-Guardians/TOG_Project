using UnityEngine;

[RequireComponent(typeof(Player))]
public class PlayerDescriptor : BaseDescriptor
{
    private Player m_player_unit;

    private void Awake()
    {
        m_player_unit = GetComponent<Player>();
    }

    public override TooltipData GetTooltipData()
    {
        // TODO: 외부데이터로 받아올 예정
        int attackValue = m_player_unit != null ? m_player_unit.AttackValue : 0;
        return new TooltipData{ Description = $"현재 {attackValue}의 기본 공격력을 가지고 있습니다.",
                                Position = m_tooltip_position};
    }
}
