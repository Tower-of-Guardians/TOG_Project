using System.Collections.Generic;

namespace Jongmin
{
    public interface IMultiSelectableCompactInvenSystem
    {
        IReadOnlyList<CardData> SelectedCards { get; }
        void ToggleSlot(CompactInvenSlot invenSlot);
        void ClearSelection();
    }
}
