using System;
using System.Collections.Generic;
using System.Linq;

namespace FluentCommand.Extensions
{
    /// <summary>
    /// Collection extension methods
    /// </summary>
    public static class CollectionExtensions
    {
        /// <summary>
        /// Gets the first element from the <paramref name="source"/> that passes the test specified by <paramref name="predicate" />; 
        /// otherwise the <paramref name="valueFactory"/> is called and the result is added to the source.
        /// </summary>
        /// <typeparam name="T">The type of the elements of <paramref name="source" />.</typeparam>
        /// <param name="source">An <see cref="T:System.Collections.Generic.ICollection`1" /> to get or add an element from.</param>
        /// <param name="predicate">A function to test each element for a condition.</param>
        /// <param name="valueFactory">The function used to generate a value when not found in the collection.</param>
        /// <returns>
        /// The value from <paramref name="valueFactory"/> if <paramref name="source" /> is empty or if no element passes the test specified by <paramref name="predicate" />; 
        /// otherwise, the first element in <paramref name="source" /> that passes the test specified by <paramref name="predicate" />.
        /// </returns>
        public static T FirstOrAdd<T>(this ICollection<T> source, Func<T, bool> predicate, Func<T> valueFactory)
        {
            if (source == null)
                throw new ArgumentNullException("source");
            if (predicate == null)
                throw new ArgumentNullException("predicate");
            if (valueFactory == null)
                throw new ArgumentNullException("valueFactory");

            // return first match
            foreach (T element in source.Where(predicate))
                return element;

            // no match, use factory
            T value = valueFactory();
            source.Add(value);

            return value;
        }

        /// <summary>
        /// Removes the all the elements that match the conditions defined by the specified predicate.
        /// </summary>
        /// <typeparam name="T"><see cref="Type"/> of the items.</typeparam>
        /// <param name="collection">The collection to remove items from.</param>
        /// <param name="filter">The delegate that defines the conditions of the elements to remove.</param>
        /// <returns>The number of items removed.</returns>
        public static int RemoveAll<T>(this ICollection<T> collection, Func<T, bool> filter)
        {
            if (collection == null)
                throw new ArgumentNullException("collection");
            if (filter == null)
                throw new ArgumentNullException("filter");

            var removed = collection.Where(filter).ToArray();
            foreach (var remove in removed)
                collection.Remove(remove);

            return removed.Length;
        }
    }
}
