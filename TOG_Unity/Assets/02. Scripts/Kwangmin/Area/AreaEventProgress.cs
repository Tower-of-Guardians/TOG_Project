using System;
using System.Collections.Generic;

public sealed class AreaEventProgress
{
    private List<AreaEventData> areas = new();
    private int currentIndex = -1;

    public bool IsInitialized => currentIndex >= 0;
    public bool IsCompleted { get; private set; }
    public AreaEventType? PendingEvent { get; private set; }
    public AreaEventData Current => IsInitialized ? areas[currentIndex] : null;

    public bool TryInitialize(IEnumerable<AreaEventData> source, string areaId = null)
    {
        if (source == null) return false;

        var ordered = new List<AreaEventData>();
        foreach (AreaEventData data in source)
        {
            if (data != null && !string.IsNullOrWhiteSpace(data.Id)) ordered.Add(data);
        }

        ordered.Sort((left, right) =>
        {
            int stage = left.Stage.CompareTo(right.Stage);
            if (stage != 0) return stage;
            int area = left.Area.CompareTo(right.Area);
            return area != 0 ? area : StringComparer.Ordinal.Compare(left.Id, right.Id);
        });

        int index = string.IsNullOrWhiteSpace(areaId) ? 0 : ordered.FindIndex(data => data.Id == areaId);
        if (ordered.Count == 0 || index < 0) return false;

        areas = ordered;
        currentIndex = index;
        IsCompleted = false;
        PendingEvent = null;
        return true;
    }

    public bool TryBeginEvent(AreaEventType type)
    {
        if (!IsInitialized || IsCompleted || PendingEvent.HasValue) return false;
        PendingEvent = type;
        return true;
    }

    public bool TryCompleteEvent(AreaEventType type, bool succeeded)
    {
        if (!IsInitialized || IsCompleted || PendingEvent != type) return false;
        PendingEvent = null;
        if (!succeeded) return true;

        if (currentIndex + 1 < areas.Count) currentIndex++;
        else IsCompleted = true;
        return true;
    }
}
