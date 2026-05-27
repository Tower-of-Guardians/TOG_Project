using UnityEngine;
using System;

public class HandCardEventBundle
{
    public Action OnPointerEnter;
    public Action OnPointerExit;
    public Action OnBeginDrag;
    public Action<Vector2> OnDrag;
    public Action OnEndDrag;
    public Action OnPointerClick;
}
