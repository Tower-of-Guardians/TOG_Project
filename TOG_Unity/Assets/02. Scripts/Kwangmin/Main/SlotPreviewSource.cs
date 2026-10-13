using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Jongmin;
using JxModule;

public readonly struct SlotPreview
{
    public readonly int Slot;
    public readonly bool IsEmpty;
    public readonly string Location;
    public readonly int RelicCount;
    public readonly int RunCount;
    public readonly long SavedAt;
    public readonly long PlaySeconds;

    public SlotPreview(int slot, TogSaveData data, string location = null)
    {
        Slot = slot;
        IsEmpty = data == null;
        Location = location ?? "로비";
        RelicCount = data?.slotProgress?.relicIDs?.Where(id => !string.IsNullOrWhiteSpace(id)).Distinct().Count() ?? 0;
        RunCount = Math.Max(0, data?.runCount ?? 0);
        SavedAt = data?.slotInfo?.savedAt ?? 0;
        PlaySeconds = Math.Max(0, data?.slotInfo?.playTime ?? 0);
    }

    public string SavedAtText => SavedAt <= 0 ? "-" : DateTimeOffset.FromUnixTimeSeconds(SavedAt)
        .ToLocalTime().ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture);
    public string PlayTimeText => $"{PlaySeconds / 3600} 시간 {PlaySeconds % 3600 / 60} 분";
}

public static class SlotPreviewSource
{
    public const int SlotCount = SaveSlotSystem.MaxSlotCount;

    public static IReadOnlyList<SlotPreview> GetSlots()
    {
        RunSaveService.SaveSession();
        var slots = new List<SlotPreview>(SlotCount);
        for (var i = 0; i < SlotCount; i++)
        {
            SlotSaveDataReader.TryLoad(i, out var data);
            slots.Add(new SlotPreview(i, data, GetLocation(data)));
        }
        return slots;
    }

    public static string GetLocation(TogSaveData data)
    {
        var areaID = data?.activeRun?.areaID;
        if (string.IsNullOrWhiteSpace(areaID))
        {
            return "로비";
        }
        if (DataCenter.areaevent_datas != null && DataCenter.areaevent_datas.TryGetValue(areaID, out var area)
            && area != null && !string.IsNullOrWhiteSpace(area.Name))
        {
            return area.Name;
        }
        return $"지역 {areaID}";
    }

    public static bool TryGetContinueSlot(out int slot)
    {
        slot = -1;
        var latest = long.MinValue;
        foreach (var preview in GetSlots())
        {
            if (preview.IsEmpty || preview.SavedAt <= latest)
            {
                continue;
            }
            latest = preview.SavedAt;
            slot = preview.Slot;
        }
        return slot >= 0;
    }
}
