namespace System.Collections.Generic
{
    public static class CollectionExtensions
    {
        public static ICollection<T> AddToCollection<T>(this ICollection<T> collection, T itemToAdd)
        {
            collection.Add(itemToAdd);
            return collection;
        }

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

        public static void ForEach<T>(this IEnumerable<T> enumerable, Action<T> predicate)
        {
            foreach (var item in enumerable)
            {
                predicate.Invoke(item);
            }
        }
    }
}