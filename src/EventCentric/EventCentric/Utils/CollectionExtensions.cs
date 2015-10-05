using System.Collections.Generic;

namespace EventCentric.Utils
{
    /// <summary>
    /// Usability extensions for collections.
    /// </summary>
    public static class CollectionExtensions
    {
        /// <summary>
        /// Adds a set of items to a collection.
        /// </summary>
        public static void AddRange<T>(this ICollection<T> collection, IEnumerable<T> items)
        {
            foreach (var item in items)
            {
                collection.Add(item);
            }
        }
    }
}
