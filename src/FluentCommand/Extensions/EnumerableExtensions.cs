using System;
using System.Collections.Generic;

namespace FluentCommand.Extensions;

/// <summary>
/// Extension methods for <see cref="T:System.Collection.IEnumerable{T}"/>
/// </summary>
public static class EnumerableExtensions
{
    /// <summary>
    /// Converts an IEnumerable of values to a delimited string.
    /// </summary>
    /// <typeparam name="T">
    /// The type of objects to delimit.
    /// </typeparam>
    /// <param name="values">
    /// The IEnumerable string values to convert.
    /// </param>
    /// <returns>
    /// A delimited string of the values.
    /// </returns>
    public static string ToDelimitedString<T>(this IEnumerable<T> values)
    {
        return ToDelimitedString<T>(values, ",");
    }

    /// <summary>
    /// Converts an IEnumerable of values to a delimited string.
    /// </summary>
    /// <typeparam name="T">
    /// The type of objects to delimit.
    /// </typeparam>
    /// <param name="values">
    /// The IEnumerable string values to convert.
    /// </param>
    /// <param name="delimiter">
    /// The delimiter.
    /// </param>
    /// <returns>
    /// A delimited string of the values.
    /// </returns>
    public static string ToDelimitedString<T>(this IEnumerable<T> values, string delimiter)
    {
        var sb = StringBuilderCache.Acquire();
        foreach (var i in values)
        {
            if (sb.Length > 0)
                sb.Append(delimiter ?? ",");
            sb.Append(i.ToString());
        }

        return StringBuilderCache.ToString(sb);
    }

    /// <summary>
    /// Converts an IEnumerable of values to a delimited string.
    /// </summary>
    /// <param name="values">The IEnumerable string values to convert.</param>
    /// <returns>A delimited string of the values.</returns>
    public static string ToDelimitedString(this IEnumerable<string> values)
    {
        return ToDelimitedString(values, ",");
    }

    /// <summary>
    /// Converts an IEnumerable of values to a delimited string.
    /// </summary>
    /// <param name="values">The IEnumerable string values to convert.</param>
    /// <param name="delimiter">The delimiter.</param>
    /// <returns>A delimited string of the values.</returns>
    public static string ToDelimitedString(this IEnumerable<string> values, string delimiter)
    {
        return ToDelimitedString(values, delimiter, null);
    }

    /// <summary>
    /// Converts an IEnumerable of values to a delimited string.
    /// </summary>
    /// <param name="values">The IEnumerable string values to convert.</param>
    /// <param name="delimiter">The delimiter.</param>
    /// <param name="escapeDelimiter">A delegate used to escape the delimiter contained in the value.</param>
    /// <returns>A delimited string of the values.</returns>
    public static string ToDelimitedString(this IEnumerable<string> values, string delimiter, Func<string, string> escapeDelimiter)
    {
        var sb = StringBuilderCache.Acquire();
        foreach (var value in values)
        {
            if (sb.Length > 0)
                sb.Append(delimiter);

            var v = escapeDelimiter != null
                ? escapeDelimiter(value ?? string.Empty)
                : value ?? string.Empty;

            sb.Append(v);
        }

        return StringBuilderCache.ToString(sb);
    }

    /// <summary>
    /// Creates a <see cref="T:System.Collections.Generic.HashSet`1"/> from an <see cref="T:System.Collections.Generic.IEnumerable`1"/>.
    /// </summary>
    /// <typeparam name="T">The type of the elements of source.</typeparam>
    /// <param name="source">The <see cref="T:System.Collections.Generic.IEnumerable`1"/> to create a <see cref="T:System.Collections.Generic.HashSet`1"/> from.</param>
    /// <returns>A <see cref="T:System.Collections.Generic.HashSet`1"/> that contains elements from the input sequence.</returns>
    public static HashSet<T> ToHashSet<T>(this IEnumerable<T> source)
    {
        return new HashSet<T>(source);
    }

    /// <summary>
    /// Creates a <see cref="T:System.Collections.Generic.HashSet`1"/> from an <see cref="T:System.Collections.Generic.IEnumerable`1"/>.
    /// </summary>
    /// <typeparam name="T">The type of the elements of source.</typeparam>
    /// <param name="source">The <see cref="T:System.Collections.Generic.IEnumerable`1"/> to create a <see cref="T:System.Collections.Generic.HashSet`1"/> from.</param>
    /// <param name="comparer">An <see cref="T:System.Collections.Generic.IEqualityComparer`1"/> to compare elements.</param>
    /// <returns>
    /// A <see cref="T:System.Collections.Generic.HashSet`1"/> that contains elements from the input sequence.
    /// </returns>
    public static HashSet<T> ToHashSet<T>(this IEnumerable<T> source, IEqualityComparer<T> comparer)
    {
        return new HashSet<T>(source, comparer);
    }
}
