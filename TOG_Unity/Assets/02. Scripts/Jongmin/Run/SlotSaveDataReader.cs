using System;
using JxModule;

namespace Jongmin
{
    public static class SlotSaveDataReader
    {
        public static bool TryLoad(int slot, out TogSaveData data)
        {
            data = null;
            if (!SaveSlotSystem.TryLoad<TogSaveData>(slot, out var loaded) || !IsValid(loaded))
            {
                return false;
            }
            
            if (loaded.slotInfo.slot != slot)
            {
                return false;
            }
            
            data = loaded;
            
            return true;
        }

        public static bool IsValid(TogSaveData data)
        {
            if (data == null || data.version != 1 || data.slotInfo == null || data.slotProgress == null)
            {
                return false;
            }
            
            var info = data.slotInfo;
            if (info.slot < 0 || info.slot >= SaveSlotSystem.MaxSlotCount || info.playTime < 0 || data.runCount < 0
                || info.savedAt < 0 || info.savedAt > DateTimeOffset.MaxValue.ToUnixTimeSeconds())
            {
                return false;
            }
            
            var progress = data.slotProgress;
            if (progress.relicIDs == null || progress.seenEventIDs == null || progress.experienceRecords == null)
            {
                return false;
            }
            
            var run = data.activeRun;
            if (run == null)
            {
                return true;
            }
            
            return !string.IsNullOrWhiteSpace(run.runID)
                && Enum.IsDefined(typeof(ERunCheckpointType), run.checkpointType)
                && run.player != null && run.deck != null && run.drawDeck != null
                && run.handDeck != null && run.discardDeck != null && run.areaChoices != null
                && run.randomStreams != null && run.experienceRecords != null
                && run.experiencedRandomEventIDs != null;
        }
    }
}
