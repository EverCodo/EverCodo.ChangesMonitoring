using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace EverCodo.ChangesMonitoring
{
    /// <summary>
    /// Equality comparer that uses reference equality of objects.
    /// </summary>
    /// <typeparam name="T">Type of the objects to compare.</typeparam>
    internal class ReferenceEqualityComparer<T> : IEqualityComparer<T>
        where T : class
    {
        /// <summary>
        /// Determines whether the specified objects are equal.
        /// </summary>
        /// <returns>true if the specified objects are equal; otherwise, false.</returns>
        public bool Equals(T x, T y)
        {
            return ReferenceEquals(x, y);
        }

        /// <summary>
        /// Returns a hash code for the specified object.
        /// </summary>
        /// <returns>A hash code for the specified object.</returns>
        public int GetHashCode(T obj)
        {
            return RuntimeHelpers.GetHashCode(obj);
        }
    }
}