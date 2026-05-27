using System;
using UnityEngine;

public class DiscardCardEventBundle
{
    public Action OnBeginDrag;
    public Action<Vector2> OnDrag;
    public Action OnEndDrag;
}
