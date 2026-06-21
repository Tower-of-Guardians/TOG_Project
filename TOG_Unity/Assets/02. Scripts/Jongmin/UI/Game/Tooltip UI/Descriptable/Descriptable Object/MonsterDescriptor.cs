using UnityEngine;

[RequireComponent(typeof(Monster))]
public class MonsterDescriptor : BaseDescriptor
{
    public override TooltipData GetTooltipData()
    {
        // TODO: MonsterData 연동 후 Description을 채웁니다.
        return new TooltipData { Position = m_tooltip_position };
    }

    protected override void OnMouseEnter() { }

    protected override void OnMouseOver() { }

    protected override void OnMouseExit() { }
}
