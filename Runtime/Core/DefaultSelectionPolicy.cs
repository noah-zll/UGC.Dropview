using System.Collections.Generic;

namespace UGC.Dropview
{
    public class DefaultSelectionPolicy : ISelectionPolicy
    {
        int max;
        public int MaxSelectionCount => max;
        public DefaultSelectionPolicy(int maxSelection = 0)
        {
            max = maxSelection;
        }
        public bool CanSelect(IReadOnlyList<string> currentSelected, DropItemData target)
        {
            if (target == null) return false;
            if (!target.isEnabled) return false;
            return true;
        }
    }
}
