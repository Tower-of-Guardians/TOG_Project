using System.Collections.Generic;

public readonly struct SlotPreview
{
    public readonly int Slot;
    public readonly bool IsEmpty;

    public SlotPreview(int slot, bool isEmpty)
    {
        Slot = slot;
        IsEmpty = isEmpty;
    }
}

public static class SlotPreviewSource
{
    public const int SlotCount = 3;

    public static IReadOnlyList<SlotPreview> GetSlots()
    {
        var slots = new List<SlotPreview>(SlotCount);
        for (int i = 0; i < SlotCount; i++)
        {
            slots.Add(new SlotPreview(i, true));
        }
        return slots;
    }

    public static bool TryGetContinueSlot(out int slot)
    {
        slot = -1;
        return false;
    }
}
