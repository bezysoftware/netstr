using System.Runtime.CompilerServices;

namespace Netstr.Extensions
{
    public static class LinqExtensions
    {
        /// <summary>
        /// Determines whether a sequence is empty or any element satisfies a condition.
        /// </summary>
        public static bool EmptyOrAny<T>(this IEnumerable<T> enumerable, Func<T, bool> predicate)
        {
            return !enumerable.Any() || enumerable.Any(predicate);
        }

        /// <summary>
        /// If the given array is null it returns an empty array.
        /// </summary>
        public static T[] EmptyIfNull<T>(this T[]? enumerable)
        {
            return enumerable ?? Array.Empty<T>();
        }

        /// <summary>
        /// Returns <paramref name="defaultValue"/> if the sequence is empty, otherwise find the max int value.
        /// </summary>
        public static Tvalue? MaxOrDefault<T, Tvalue>(this IEnumerable<T> enumerable, Func<T, Tvalue> func, Tvalue? defaultValue = default)
        {
            return enumerable.Any() ? enumerable.Max(func) : defaultValue;
        }

        /// <summary>
        /// Filters the sequence and returns only not null elements.
        /// </summary>
        public static IEnumerable<T> WhereNotNull<T>(this IEnumerable<T?> enumerable)
        {
            return enumerable.Where(x => x != null)!;
        }
    }
}