using System;
using System.Collections.Generic;

namespace EverCodo.ChangesMonitoring
{
    /// <summary>
    /// Compares attributes according to priority rules.
    /// </summary>
    /// <remarks>
    /// 1. Attribute with Id specified is always greater (has more priority) than attribute with Id == null.
    /// 2. Attributes with equal Id compared using DoNotMonitor flag: DoNotMonitor == true has greater priority.
    /// 3. Attributes with different but non-null Id considered as having equal priority.
    /// </remarks>
    internal class MonitorChangesAttributePriorityComparer : IComparer<MonitorChangesAttribute>
    {
        /// <inheritdoc />
        public int Compare(MonitorChangesAttribute first, MonitorChangesAttribute second)
        {
            if (ReferenceEquals(first, second))
                return 0;

            if (ReferenceEquals(null, second))
                return 1;

            if (ReferenceEquals(null, first))
                return -1;

            if (first.Id != null && second.Id == null)
                return 1;

            if (first.Id == null && second.Id != null)
                return -1;

            if (first.Id == second.Id)
            {
                if (first.DoNotMonitor && !second.DoNotMonitor)
                    return 1;

                if (!first.DoNotMonitor && second.DoNotMonitor)
                    return -1;
            }

            return 0;
        }

        /// <summary>
        /// Comparer instance to use.
        /// </summary>
        public static readonly MonitorChangesAttributePriorityComparer Instance = new MonitorChangesAttributePriorityComparer();
    }
}