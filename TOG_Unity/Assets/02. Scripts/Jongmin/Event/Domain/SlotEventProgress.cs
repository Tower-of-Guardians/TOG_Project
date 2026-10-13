using System.Collections.Generic;

namespace Jongmin
{
    public sealed class SlotEventProgress : IMutableEventProgress
    {
        private readonly HashSet<string> _seenEventIDSet = new();
        private readonly ExperienceRecordStore _experience = new();

        public int MaxSingleAttackDamage { get; private set; }
        public IExperienceRecord Experience => _experience;

        public bool HasSeen(string eventID)
        {
            return !string.IsNullOrWhiteSpace(eventID) && _seenEventIDSet.Contains(eventID);
        }

        public void MarkSeen(string eventID)
        {
            if (string.IsNullOrWhiteSpace(eventID))
            {
                return;
            }

            _seenEventIDSet.Add(eventID);
        }

        public void RecordSingleAttackDamage(int damage)
        {
            if (damage > MaxSingleAttackDamage)
            {
                MaxSingleAttackDamage = damage;
            }
            _experience.Max(EExperienceRecordType.HighestValue, "SingleAttackDamage", damage);
        }

        public void AddExperience(EExperienceRecordType type, string targetID = null, string detailID = null, int amount = 1)
            => _experience.Add(type, targetID, detailID, amount);

        public void MarkExperience(EExperienceRecordType type, string targetID, string detailID = null)
            => _experience.Max(type, targetID, 1, detailID);

        public SlotProgressSaveData Capture(IEnumerable<string> relicIDs)
        {
            return new SlotProgressSaveData
            {
                relicIDs = relicIDs != null ? new List<string>(relicIDs) : new List<string>(),
                seenEventIDs = new List<string>(_seenEventIDSet),
                experienceRecords = _experience.Capture()
            };
        }

        public void Restore(SlotProgressSaveData data)
        {
            _seenEventIDSet.Clear();
            if (data?.seenEventIDs != null)
            {
                foreach (string eventID in data.seenEventIDs)
                {
                    if (!string.IsNullOrWhiteSpace(eventID))
                    {
                        _seenEventIDSet.Add(eventID);
                    }
                }
            }
            _experience.Restore(data?.experienceRecords);
            MaxSingleAttackDamage = _experience.Get(EExperienceRecordType.HighestValue, "SingleAttackDamage");
        }
    }
}
