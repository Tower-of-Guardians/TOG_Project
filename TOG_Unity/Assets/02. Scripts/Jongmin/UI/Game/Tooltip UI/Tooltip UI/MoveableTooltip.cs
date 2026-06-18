using UnityEngine;

public class MoveableTooltip : MonoBehaviour
{
    [Header("디자이너")]
    [SerializeField] private TooltipUIDesigner m_designer;

    [Space(30f), Header("UI 관련 컴포넌트")]
    [Header("캔버스")]
    [SerializeField] private Canvas m_canvas;

    private void Update()
    {
        if(m_designer.Moveable)
            CalculateMousePosition();
    }

    private void CalculateMousePosition()
    {
        var mouse_position = new Vector2(Input.mousePosition.x, Input.mousePosition.y);

        var canvas_transform = m_canvas.transform as RectTransform;
        var this_transform = transform as RectTransform;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(canvas_transform,
                                                                mouse_position,
                                                                null,
                                                                out var local_position);
        
        local_position.x = mouse_position.x > Screen.width * 0.5f ? local_position.x - this_transform.sizeDelta.x * m_designer.PivotX
                                                                  : local_position.x + this_transform.sizeDelta.x * m_designer.PivotX;

        local_position.y = mouse_position.y > Screen.height * 0.5f ? local_position.y - this_transform.sizeDelta.y * m_designer.PivotY
                                                                   : local_position.y + this_transform.sizeDelta.y * m_designer.PivotY;

        this_transform.anchoredPosition = local_position;
    }
}
