using System.Collections.Generic;

namespace UGC.Dropview
{
    public interface ISelectionPolicy
    {
        int MaxSelectionCount { get; }
        bool CanSelect(IReadOnlyList<string> currentSelected, DropItemData target);
    }
}
