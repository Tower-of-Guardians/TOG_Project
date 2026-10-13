using System;
using System.Collections.Generic;

namespace Jongmin
{
    public interface IExperienceRecord
    {
        int Get(EExperienceRecordType type, string targetID = null, string detailID = null);
    }

    public sealed class ExperienceRecordStore : IExperienceRecord
    {
        private readonly Dictionary<RecordKey, int> _values = new();

        public int Get(EExperienceRecordType type, string targetID = null, string detailID = null)
        {
            return _values.GetValueOrDefault(new RecordKey(type, targetID, detailID), 0);
        }

        public void Add(EExperienceRecordType type, string targetID = null, string detailID = null, int amount = 1)
        {
            if (amount <= 0)
            {
                return;
            }
            
            var key = new RecordKey(type, targetID, detailID);
            _values[key] = Get(type, targetID, detailID) + amount;
        }

        public void Max(EExperienceRecordType type, string targetID, int value, string detailID = null)
        {
            var key = new RecordKey(type, targetID, detailID);
            
            if (value > Get(type, targetID, detailID))
            {
                _values[key] = value;
            }
        }

        public void Set(EExperienceRecordType type, string targetID, string detailID, int value)
        {
            _values[new RecordKey(type, targetID, detailID)] = Math.Max(0, value);
        }

        public List<ExperienceRecordData> Capture()
        {
            var result = new List<ExperienceRecordData>(_values.Count);
            foreach (var pair in _values)
            {
                result.Add(new ExperienceRecordData
                {
                    type = pair.Key.Type,
                    targetID = pair.Key.TargetID,
                    detailID = pair.Key.DetailID,
                    value = pair.Value
                });
            }
            
            return result;
        }

        public void Restore(IEnumerable<ExperienceRecordData> records)
        {
            _values.Clear();
            
            if (records == null)
            {
                return;
            }
            
            foreach (var record in records)
            {
                if (record == null)
                {
                    continue;
                }
                
                Set(record.type, record.targetID, record.detailID, record.value);
            }
        }

        public void Clear()
        {
            _values.Clear();   
        }

        private readonly struct RecordKey : IEquatable<RecordKey>
        {
            public readonly EExperienceRecordType Type;
            public readonly string TargetID;
            public readonly string DetailID;

            public RecordKey(EExperienceRecordType type, string targetID, string detailID)
            {
                Type = type;
                TargetID = targetID ?? string.Empty;
                DetailID = detailID ?? string.Empty;
            }

            public bool Equals(RecordKey other) => Type == other.Type && TargetID == other.TargetID && DetailID == other.DetailID;
            public override bool Equals(object obj) => obj is RecordKey other && Equals(other);
            public override int GetHashCode() => HashCode.Combine((int)Type, TargetID, DetailID);
        }
    }
}
