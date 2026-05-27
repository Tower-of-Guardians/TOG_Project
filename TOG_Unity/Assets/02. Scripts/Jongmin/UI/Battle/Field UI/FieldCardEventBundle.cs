using System;
using UnityEngine;

public class FieldCardEventBundle
{
    public Action OnBeginDrag;
    public Action<Vector2> OnDrag;
    public Action OnEndDrag;
}
