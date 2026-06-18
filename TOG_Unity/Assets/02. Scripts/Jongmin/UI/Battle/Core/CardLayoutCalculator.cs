using UnityEngine;

namespace Jongmin
{
    public static class CardLayoutCalculator
{
    public static CardLayoutData CalculatedHandCardTransform(int index, 
                                                             int cardCount,
                                                             float targetRadius,
                                                             float arcAngle,
                                                             float depthMultiplier)
    {
        var stepCount  =   cardCount > 1 ? cardCount - 1 : 1f;
        var startAngle =   cardCount > 1 ? -arcAngle / 2f : 0f;
        var angleStep  =   cardCount > 1 ? arcAngle / stepCount : 0f;
        var radius     =   cardCount * targetRadius;  

        var angle = startAngle + index * angleStep;
        var rad = angle * Mathf.Deg2Rad;      

        var targetPosition = new Vector3(Mathf.Sin(rad) * radius,
                                         Mathf.Cos(rad) * radius - radius,
                                         -Mathf.Abs(angle) * depthMultiplier);
        var targetRotation = new Vector3(0f, 0f, -angle);
        var targetScale = Vector3.one;

        return new CardLayoutData(targetPosition, targetRotation, targetScale);
    }

    public static Vector2 CalculatedThrowCardPosition(int index,
                                                      int cardCount,
                                                      float space)
    {
        var totalWidth = (cardCount - 1) * space;
        var startX = -totalWidth * 0.5f;

        var targetX = startX + (space * index);

        return new Vector2(targetX, 0f);
    }

    public static Vector2 CalculatedFieldCardPosition(int index,
                                                      int maxCardCount,
                                                      float space)
    {
        var startX = -(maxCardCount - 1) * space * 0.5f;
        var targetX = startX + index * space;

        return new Vector2(targetX, 0f);
    }
}

}
