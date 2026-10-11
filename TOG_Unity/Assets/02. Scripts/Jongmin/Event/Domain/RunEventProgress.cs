using System.Collections.Generic;

namespace Jongmin
{
    public sealed class RunEventProgress : IRunProgress, IBattleRecord, IRunCardRecord, ISynergyRecord
    {
        private readonly Dictionary<string, int> _npcEncounterCounts = new();
        private readonly HashSet<int> _reachedStages = new();
        private readonly Dictionary<int, int> _gainedCardCountByGrade = new();
        private readonly Dictionary<string, int> _activatedSynergyCounts = new();
        private readonly HashSet<string> _firstActivatedSynergyKeys = new();
        private readonly ExperienceRecordStore _experience = new();

        public int MaxSingleAttackDamage { get; private set; }
        public IExperienceRecord Experience => _experience;

        public int GetNpcEncounterCount(string npcID)
        {
            return !string.IsNullOrWhiteSpace(npcID) &&
                   _npcEncounterCounts.TryGetValue(npcID, out var count) ? count : 0;
        }

        public bool HasFirstReachedStage(int stage)
        {
            return _reachedStages.Contains(stage);
        }

        public bool HasReachedStage(int stage)
        {
            foreach (var reachedStage in _reachedStages)
            {
                if (reachedStage >= stage)
                {
                    return true;
                }
            }

            return false;
        }

        public int GetGainedCountByGrade(int grade)
        {
            return _gainedCardCountByGrade.GetValueOrDefault(grade, 0);
        }

        public bool HasActivatedAtLeast(string synergyID, int count)
        {
            return !string.IsNullOrWhiteSpace(synergyID) &&
                   _activatedSynergyCounts.TryGetValue(synergyID, out var currentCount) &&
                   currentCount >= count;
        }

        public bool HasFirstActivatedAtLeast(string synergyID, int count)
        {
            return _firstActivatedSynergyKeys.Contains(GetSynergyThresholdKey(synergyID, count));
        }

        public void RecordReachedStage(int stage)
        {
            if (stage <= 0)
            {
                return;
            }

            _reachedStages.Add(stage);
            _experience.Max(EExperienceRecordType.StageReached, stage.ToString(), 1);
        }

        public void RecordNpcEncounter(string npcID)
        {
            if (string.IsNullOrWhiteSpace(npcID))
            {
                return;
            }

            _npcEncounterCounts.TryGetValue(npcID, out var count);
            _npcEncounterCounts[npcID] = count + 1;
            _experience.Add(EExperienceRecordType.NpcEncounter, npcID);
        }

        public void SetNpcEncounter(string npcID, int count)
        {
            if (string.IsNullOrWhiteSpace(npcID))
            {
                return;
            }
            
            _npcEncounterCounts[npcID] = count;
            _experience.Set(EExperienceRecordType.NpcEncounter, npcID, null, count);
        }

        public void RecordSingleAttackDamage(int damage)
        {
            if (damage > MaxSingleAttackDamage)
            {
                MaxSingleAttackDamage = damage;
            }
            _experience.Max(EExperienceRecordType.HighestValue, "SingleAttackDamage", damage);
        }

        public void RecordGainedCard(CardData cardData)
        {
            if (cardData == null)
            {
                return;
            }

            var grade = cardData.grade;
            _gainedCardCountByGrade.TryGetValue(grade, out var count);
            _gainedCardCountByGrade[grade] = count + 1;
            _experience.Add(EExperienceRecordType.CardGained, cardData.id);
            _experience.Add(EExperienceRecordType.CardGained, "Grade", grade.ToString());
            _experience.Add(EExperienceRecordType.CardGained, "Star", cardData.star.ToString());
        }

        public void RefreshSynergyRecords(Dictionary<string, SynergyTotalData> synergyMap)
        {
            var previousCounts = new Dictionary<string, int>(_activatedSynergyCounts);
            _activatedSynergyCounts.Clear();

            if (synergyMap == null)
            {
                return;
            }

            foreach (var pair in synergyMap)
            {
                if (string.IsNullOrWhiteSpace(pair.Key) || pair.Value == null)
                {
                    continue;
                }

                var previousCount = previousCounts.GetValueOrDefault(pair.Key, 0);
                var currentCount = pair.Value.count;
                _activatedSynergyCounts[pair.Key] = currentCount;

                for (var count = previousCount + 1; count <= currentCount; count++)
                {
                    _firstActivatedSynergyKeys.Add(GetSynergyThresholdKey(pair.Key, count));
                    _experience.Max(EExperienceRecordType.SynergyActivated, pair.Key, 1, count.ToString());
                }
            }
        }

        public void Reset()
        {
            _npcEncounterCounts.Clear();
            _reachedStages.Clear();
            _gainedCardCountByGrade.Clear();
            _activatedSynergyCounts.Clear();
            _firstActivatedSynergyKeys.Clear();
            _experience.Clear();
            MaxSingleAttackDamage = 0;
        }

        public List<ExperienceRecordData> Capture() => _experience.Capture();

        public void ExperienceRecordAdd(EExperienceRecordType type, string targetID, string detailID = null, int amount = 1)
            => _experience.Add(type, targetID, detailID, amount);

        public void Restore(IEnumerable<ExperienceRecordData> records)
        {
            Reset();
            _experience.Restore(records);
            if (records != null)
            {
                foreach (ExperienceRecordData record in records)
                {
                    if (record == null)
                    {
                        continue;
                    }
                    switch (record.type)
                    {
                        case EExperienceRecordType.NpcEncounter:
                            _npcEncounterCounts[record.targetID] = record.value;
                            break;
                        case EExperienceRecordType.StageReached when int.TryParse(record.targetID, out int stage):
                            if (record.value > 0)
                            {
                                _reachedStages.Add(stage);
                            }
                            break;
                        case EExperienceRecordType.CardGained when record.targetID == "Grade"
                                                                 && int.TryParse(record.detailID, out int grade):
                            _gainedCardCountByGrade[grade] = record.value;
                            break;
                        case EExperienceRecordType.SynergyActivated when int.TryParse(record.detailID, out int count):
                            if (record.value > 0)
                            {
                                _firstActivatedSynergyKeys.Add(GetSynergyThresholdKey(record.targetID, count));
                            }
                            break;
                    }
                }
            }
            MaxSingleAttackDamage = _experience.Get(EExperienceRecordType.HighestValue, "SingleAttackDamage");
        }

        private static string GetSynergyThresholdKey(string synergyID, int count)
        {
            return $"{synergyID}:{count}";
        }
    }
}
