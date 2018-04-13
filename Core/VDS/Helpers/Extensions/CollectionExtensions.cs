using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using VDS.Helpers.Exception;
using JetBrains.Annotations;

namespace VDS.Helpers.Extensions
{
    public static class CollectionExtensions
    {
        /// <summary>
        ///     Converts all.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="items">The items.</param>
        /// <param name="transformation">The transformation.</param>
        /// <returns></returns>
        public static TResult[] ConvertAll<T, TResult>(this T[] items, Converter<T, TResult> transformation)
        {
            return Array.ConvertAll(items, transformation);
        }

        /// <summary>
        ///     Finds the specified predicate.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="items">The items.</param>
        /// <param name="predicate">The predicate.</param>
        /// <returns></returns>
        public static T Find<T>(this T[] items, Predicate<T> predicate)
        {
            return Array.Find(items, predicate);
        }

        /// <summary>
        ///     Finds all.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="items">The items.</param>
        /// <param name="predicate">The predicate.</param>
        /// <returns></returns>
        public static T[] FindAll<T>(this T[] items, Predicate<T> predicate)
        {
            return Array.FindAll(items, predicate);
        }

        /// <summary>
        ///     Fors the each.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="items">The items.</param>
        /// <param name="action">The action.</param>
        public static void ForEach<T>(this IEnumerable<T> items, Action<T> action)
        {
            if (items == null)
            {
                return;
            }

            foreach (T obj in items)
            {
                action(obj);
            }
        }

        /// <summary>
        ///     Checks whether or not collection is null or empty. Assumes colleciton can be safely enumerated multiple times.
        /// </summary>
        /// <param name="this">The this.</param>
        /// <returns>
        ///     <c>true</c> if [is null or empty] [the specified this]; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsNullOrEmpty(this IEnumerable @this)
        {
            if (@this != null)
            {
                return !@this.GetEnumerator().MoveNext();
            }

            return true;
        }

        /// <summary>
        /// Combines multiple lookups into a single lookup
        /// </summary>
        /// <typeparam name="TKey">The type of the keys</typeparam>
        /// <typeparam name="TElement">The type of the elements</typeparam>
        /// <param name="lookups">A collection of lookups to combine</param>
        /// <returns>A single lookup which takes a key into all values with this key in all incoming lookups</returns>
        public static ILookup<TKey, TElement> Combine<TKey, TElement>(this IEnumerable<ILookup<TKey, TElement>> lookups)
        {
            return lookups
                .SelectMany(l => l)
                .SelectMany(l => l.Select(value => new { l.Key, Value = value }))
                .ToLookup(x => x.Key, x => x.Value);
        }

        public static IEnumerable<T> EmptyIfNull<T>(this IEnumerable<T> value)
        {
            return value ?? Enumerable.Empty<T>();
        }
        
        public static IEnumerable<KeyValuePair<TKey, TValue>> CombineKeysAndValues<TKey, TValue>(this IEnumerable<TKey> keys, IEnumerable<TValue> values)
        {
            Throw.IfArgumentNull(keys, "keys");
            Throw.IfArgumentNull(values, "values");
            Throw.If(keys.Count() != values.Count()).AnArgumentException("Keys and values should have the same size.");

            var keysList = new List<TKey>(keys);
            var valuesList = new List<TValue>(values);
            var result = new Dictionary<TKey, TValue>();
            for (var i = 0; i < keysList.Count; i++)
            {
                result.Add(keysList[i], valuesList[i]);
            }
            return result;
        }

        public static IDictionary<TKey, TValue> MergeDictionaries<TKey, TValue>(this IDictionary<TKey, TValue> first, IDictionary<TKey, TValue> second)
        {
            if (first == null)
            {
                return second;
            }
            if (second == null)
            {
                return first;
            }
            var mergedDictionary = new Dictionary<TKey, TValue>(first);
            foreach (var pair in second)
            {
                mergedDictionary[pair.Key] = pair.Value;
            }
            return mergedDictionary;
        }

        /// <summary>
        /// Adds an item to the collection if it's not already in the collection.
        /// </summary>
        /// <param name="source">Collection</param>
        /// <param name="item">Item to check and add</param>
        /// <typeparam name="T">Type of the items in the collection</typeparam>
        /// <returns>Returns True if added, returns False if not.</returns>
        public static bool AddIfNotContains<T>(this ICollection<T> source, T item)
        {
            Throw.IfArgumentNull(source, nameof(source));
            if (source.Contains(item))
            {
                return false;
            }

            source.Add(item);
            return true;
        }

        public static IList<T> RemoveAll<T>([NotNull] this ICollection<T> source, Func<T, bool> predicate)
        {
            var items = source.Where(predicate).ToList();

            foreach (var item in items)
            {
                source.Remove(item);
            }

            return items;
        }
    }
}