using System;
using System.Collections.Generic;
using JxModule;

namespace Jongmin
{
    public enum ERunEndReason
    {
        Death,
        Clear,
        Abandon,
        BossDefeated
    }

    public enum ERunCheckpointType
    {
        AreaEntry,
        AreaSelection,
        EventEntry
    }

    public enum EExperienceRecordType
    {
        MonsterKill,
        NpcEncounter,
        StageReached,
        SynergyActivated,
        CardGained,
        RelicGained,
        EventExperienced,
        HighestValue,
        RunResult
    }

    public enum EExperienceRecordScope
    {
        Run = 0,
        Slot = 1
    }

    [Serializable]
    public sealed class ExperienceRecordData
    {
        public EExperienceRecordType type;
        public string targetID;
        public string detailID;
        public int value;
    }

    [Serializable]
    public sealed class CardRunSaveData
    {
        public string cardID;
        public int grade;
        public int star;
        public float attack;
        public float defense;
        public string effect1Value;
        public string effect2Value;
        public long acquiredAt;
    }

    [Serializable]
    public sealed class PlayerRunSaveData
    {
        public int level;
        public int maxExperience;
        public int experience;
        public int hp;
        public int maxHp;
        public int attack;
        public int maxMagic;
        public int gold;
    }

    [Serializable]
    public sealed class RandomStreamSaveData
    {
        public string streamID;
        public int consumed;
    }

    [Serializable]
    public sealed class SlotProgressSaveData
    {
        public List<string> relicIDs = new();
        public List<string> seenEventIDs = new();
        public List<ExperienceRecordData> experienceRecords = new();
    }

    [Serializable]
    public sealed class ActiveRunSaveData
    {
        public string runID;
        public int seed;
        public ERunCheckpointType checkpointType;
        public string areaID;
        public PlayerEventStatus eventStatus;
        public List<int> areaChoices = new();
        public int eventType;
        public List<CardRunSaveData> deck = new();
        public bool hasBattleDeckState;
        public List<string> drawDeck = new();
        public List<string> handDeck = new();
        public List<string> discardDeck = new();
        public PlayerRunSaveData player = new();
        public List<ExperienceRecordData> experienceRecords = new();
        public List<string> experiencedRandomEventIDs = new();
        public List<RandomStreamSaveData> randomStreams = new();
    }

    [Serializable]
    public sealed class TogSaveData : ISaveSlotData
    {
        public int version = 1;
        public int runCount;
        public SaveSlotInfo slotInfo = new();
        public SlotProgressSaveData slotProgress = new();
        public ActiveRunSaveData activeRun;

        public SaveSlotInfo SlotInfo => slotInfo;
    }
}
