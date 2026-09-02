using System;
using System.Collections.Generic;
using System.Linq;
using JxModule.DataTable;
using UnityEngine;

namespace Jongmin
{
    public class RelicDomain : MonoBehaviour, IRelicInventory
    {
        private DataTable _relicDataTable;

        private readonly List<string> _ownedRelicIDs = new();
        private readonly HashSet<string> _ownedRelicIDSet = new();
        
        public event Action<RelicDataTableRow> OnRelicAdded;
        public event Action<RelicDataTableRow> OnRelicRemoved;
        
        public int RelicCount => _ownedRelicIDs.Count;
        public IReadOnlyList<string> OwnedRelicIDs => _ownedRelicIDs;

        public void Construct()
        {
            _relicDataTable = DataTableManager.FindTable<RelicDataTableRow>("DT_Relic");
        }

        public bool HasRelic(string relicID)
        {
            if (string.IsNullOrWhiteSpace(relicID))
            {
                return false;
            }
            
            return _ownedRelicIDSet.Contains(relicID);
        }

        public bool TryAddRelic(string relicID)
        {
            if (!TryGetRelic(relicID, out var relicDataTableRow))
            {
                return false;
            }

            if (!_ownedRelicIDSet.Add(relicID))
            {
                return false;
            }
            
            _ownedRelicIDs.Add(relicID);
            Debug.Log($"{relicDataTableRow.displayName}을 성공적으로 획득했습니다.");
            OnRelicAdded?.Invoke(relicDataTableRow);
            return true;
        }

        public bool TryRemoveRelic(string relicID)
        {
            if (string.IsNullOrWhiteSpace(relicID))
            {
                return false;
            }

            if (!_ownedRelicIDSet.Remove(relicID))
            {
                return false;
            }
            
            _ownedRelicIDs.Remove(relicID);

            if (TryGetRelic(relicID, out var relicDataTableRow))
            {
                Debug.Log($"{relicDataTableRow.displayName}을 성공적으로 제거했습니다.");
                OnRelicRemoved?.Invoke(relicDataTableRow);
            }

            return true;
        }

        public bool TryGetRelic(string relicID, out RelicDataTableRow relicDataTableRow)
        {
            relicDataTableRow = null;

            if (string.IsNullOrWhiteSpace(relicID) || _relicDataTable == null)
            {
                return false;
            }
            
            relicDataTableRow = _relicDataTable.Find<RelicDataTableRow>(relicID);
            return relicDataTableRow != null && relicDataTableRow.isEnable;
        }

        public List<RelicDataTableRow> GetOwnedRelics()
        {
            if (_relicDataTable == null)
            {
                return new List<RelicDataTableRow>();
            }
            
            return _ownedRelicIDs
                .Select(id => _relicDataTable.Find<RelicDataTableRow>(id))
                .Where(row => row != null && row.isEnable)
                .ToList();
        }

        public bool IsConditionVisible(string relicID)
        {
            return TryGetRelic(relicID, out var relicDataTableRow) &&
                   relicDataTableRow.relicType == ERelicType.Public;
        }

        public void RestoreOwnedRelics(IEnumerable<string> relicIDs)
        {
            _ownedRelicIDs.Clear();
            _ownedRelicIDSet.Clear();

            if (relicIDs == null)
            {
                return;
            }

            foreach (var relicID in relicIDs)
            {
                if (!TryGetRelic(relicID, out _))
                {
                    continue;
                }

                if (_ownedRelicIDSet.Add(relicID))
                {
                    _ownedRelicIDs.Add(relicID);
                }
            }
        }

        public void Clear()
        {
            _ownedRelicIDs.Clear();
            _ownedRelicIDSet.Clear();
        }
    }
}

