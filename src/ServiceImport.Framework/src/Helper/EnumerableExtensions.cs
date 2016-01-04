using System;
using System.Collections;
using System.Collections.Generic;

namespace BRail.Nis.ServiceImport.Framework.Helper
{
    public static class EnumerableExtensions
    {
        /// <summary>
        /// Returns the only element of a sequence that satisfies a specified condition or a default value if no such element exists; this method throws an exception if more than one element satisfies the condition.
        /// </summary>
        /// <typeparam name="T">The type of the elements of <paramref name="source"/>.</typeparam>
        /// <param name="source">An <see cref="IEnumerable"/> to return a single element from.</param>
        /// <param name="predicate">A function to test an element for a condition.</param>
        /// <returns>
        /// The single element of the input sequence that satisfies the condition, or <c>default</c>(<typeparamref name="T"/>) if no such element is found.
        /// </returns>
        /// <exception cref="ArgumentNullException"><paramref name="source"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="predicate"/> is <c>null</c>.</exception>
        /// <exception cref="InvalidOperationException">More than one element in <paramref name="source"/> satisfies the condition.</exception>
        public static T SingleOrDefault<T>(this IEnumerable source, Func<T, bool> predicate)
        {
            if (source == null)
                throw new ArgumentNullException("source");
            if (predicate == null)
                throw new ArgumentNullException("predicate");

            T ret = default(T);
            bool foundAny = false;

            foreach (T value in source)
            {
                if (predicate(value))
                {
                    if (foundAny)
                        throw new InvalidOperationException("Sequence contained multiple matching elements");

                    foundAny = true;
                    ret = value;
                }
            }

            return ret;
        }

        /// <summary>
        /// Performs the specified action on each element of the <see cref="IEnumerable{T}"/>.
        /// </summary>
        /// <typeparam name="T">The type of the elements of <paramref name="source"/>.</typeparam>
        /// <param name="source">A sequence of elements to perform an action on.</param>
        /// <param name="action">The <see cref="Action{T}"/> delegate to perform on each element of the <see cref="IEnumerable{T}"/>.</param>
        /// <exception cref="ArgumentNullException"><paramref name="source"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="action"/> is <c>null</c>.</exception>
        public static void ForEach<T>(this IEnumerable<T> source, Action<T> action)
        {
            if (source == null)
                throw new ArgumentNullException("source");
            if (action == null)
                throw new ArgumentNullException("action");

            foreach (var item in source)
                action(item);
        }
    }
}
