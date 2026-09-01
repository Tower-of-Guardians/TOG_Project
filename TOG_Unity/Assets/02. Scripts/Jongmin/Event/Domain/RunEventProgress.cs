using System.Collections.Generic;

namespace Jongmin
{
    public sealed class RunEventProgress : IRunProgress, IBattleRecord, IRunCardRecord, ISynergyRecord
    {
        private readonly HashSet<int> _reachedStages = new();
        private readonly Dictionary<int, int> _gainedCardCountByGrade = new();
        private readonly Dictionary<string, int> _activatedSynergyCounts = new();
        private readonly HashSet<string> _firstActivatedSynergyKeys = new();

        public int MaxSingleAttackDamage { get; private set; }

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
        }

        public void RecordSingleAttackDamage(int damage)
        {
            if (damage > MaxSingleAttackDamage)
            {
                MaxSingleAttackDamage = damage;
            }
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
                }
            }
        }

        public void Reset()
        {
            _reachedStages.Clear();
            _gainedCardCountByGrade.Clear();
            _activatedSynergyCounts.Clear();
            _firstActivatedSynergyKeys.Clear();
            MaxSingleAttackDamage = 0;
        }

        private static string GetSynergyThresholdKey(string synergyID, int count)
        {
            return $"{synergyID}:{count}";
        }
    }
}
