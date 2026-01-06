using System;

namespace Coffee.DigitalPlatform.Controls.FilterBuilder
{
    public static class ConditionTreeItemExtensions
    {
        public static bool IsRoot(this ConditionTreeItem item)
        {
            ArgumentNullException.ThrowIfNull(item);

            return item.Parent is null;
        }
    }
}