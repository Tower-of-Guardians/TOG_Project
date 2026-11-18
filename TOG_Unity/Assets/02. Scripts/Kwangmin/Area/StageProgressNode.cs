public enum EStageNodeState
{
    Visited,
    Current,
    Locked
}

public readonly struct StageProgressNode
{
    public readonly int AreaNumber;
    public readonly EStageNodeState State;
    public readonly AreaEventType? EventType;
    public readonly bool IsBoss;
    public readonly bool IsPathSolid;

    public StageProgressNode(int areaNumber, EStageNodeState state, AreaEventType? eventType, bool isBoss, bool isPathSolid)
    {
        AreaNumber = areaNumber;
        State = state;
        EventType = eventType;
        IsBoss = isBoss;
        IsPathSolid = isPathSolid;
    }
}
