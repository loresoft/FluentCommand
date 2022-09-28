using System;
using System.Collections.Generic;
using System.Text;

namespace FluentCommand.Extensions;

/// <summary>
/// <see cref="T:String"/> extension methods
/// </summary>
public static class StringExtensions
{
    /// <summary>
    /// Indicates whether the specified String object is null or an empty string
    /// </summary>
    /// <param name="item">A String reference</param>
    /// <returns>
    ///     <c>true</c> if is null or empty; otherwise, <c>false</c>.
    /// </returns>
    public static bool IsNullOrEmpty(this string item)
    {
        return string.IsNullOrEmpty(item);
    }

    /// <summary>
    /// Indicates whether a specified string is null, empty, or consists only of white-space characters
    /// </summary>
    /// <param name="item">A String reference</param>
    /// <returns>
    ///      <c>true</c> if is null or empty; otherwise, <c>false</c>.
    /// </returns>
    public static bool IsNullOrWhiteSpace(this string item)
    {
        if (item == null)
            return true;

        for (int i = 0; i < item.Length; i++)
            if (!char.IsWhiteSpace(item[i]))
                return false;

        return true;
    }

    /// <summary>
    /// Determines whether the specified string is not <see cref="IsNullOrEmpty"/>.
    /// </summary>
    /// <param name="value">The value to check.</param>
    /// <returns>
    ///   <c>true</c> if the specified <paramref name="value"/> is not <see cref="IsNullOrEmpty"/>; otherwise, <c>false</c>.
    /// </returns>
    public static bool HasValue(this string value)
    {
        return !string.IsNullOrEmpty(value);
    }

    /// <summary>
    /// Uses the string as a format
    /// </summary>
    /// <param name="format">A String reference</param>
    /// <param name="args">Object parameters that should be formatted</param>
    /// <returns>Formatted string</returns>
    public static string FormatWith(this string format, params object[] args)
    {
        if (format == null)
            throw new ArgumentNullException("format");

        return string.Format(format, args);
    }

    /// <summary>
    /// Appends a copy of the specified string followed by the default line terminator to the end of the StringBuilder object.
    /// </summary>
    /// <param name="sb">The StringBuilder instance to append to.</param>
    /// <param name="format">A composite format string.</param>
    /// <param name="args">An object array that contains zero or more objects to format.</param>
    public static StringBuilder AppendLine(this StringBuilder sb, string format, params object[] args)
    {
        sb.AppendFormat(format, args);
        sb.AppendLine();
        return sb;
    }

    /// <summary>
    /// Appends a copy of the specified string if <paramref name="condition"/> is met.
    /// </summary>
    /// <param name="sb">The StringBuilder instance to append to.</param>
    /// <param name="text">The string to append.</param>
    /// <param name="condition">The condition delegate to evaluate. If condition is null, String.IsNullOrWhiteSpace method will be used.</param>
    public static StringBuilder AppendIf(this StringBuilder sb, string text, Func<string, bool> condition = null)
    {
        var c = condition ?? (s => !string.IsNullOrEmpty(s));

        if (c(text))
            sb.Append(text);

        return sb;
    }

    /// <summary>
    /// Appends a copy of the specified string if <paramref name="condition"/> is met.
    /// </summary>
    /// <param name="sb">The StringBuilder instance to append to.</param>
    /// <param name="text">The string to append.</param>
    /// <param name="condition">The condition delegate to evaluate. If condition is null, String.IsNullOrWhiteSpace method will be used.</param>
    public static StringBuilder AppendIf(this StringBuilder sb, string text, bool condition)
    {
        if (condition)
            sb.Append(text);

        return sb;
    }

    /// <summary>
    /// Appends a copy of the specified string followed by the default line terminator if <paramref name="condition"/> is met.
    /// </summary>
    /// <param name="sb">The StringBuilder instance to append to.</param>
    /// <param name="text">The string to append.</param>
    /// <param name="condition">The condition delegate to evaluate. If condition is null, String.IsNullOrWhiteSpace method will be used.</param>
    public static StringBuilder AppendLineIf(this StringBuilder sb, string text, Func<string, bool> condition = null)
    {
        var c = condition ?? (s => !string.IsNullOrEmpty(s));

        if (c(text))
            sb.AppendLine(text);

        return sb;
    }

    /// <summary>
    /// Concatenates and appends the members of a collection, using the specified separator between each member.
    /// </summary>
    /// <typeparam name="T">The type of the members of values.</typeparam>
    /// <param name="sb">A reference to this instance after the append operation has completed.</param>
    /// <param name="separator">The string to use as a separator. separator is included in the concatenated and appended strings only if values has more than one element.</param>
    /// <param name="values">A collection that contains the objects to concatenate and append to the current instance of the string builder.</param>
    /// <returns>A reference to this instance after the append operation has completed.</returns>
    public static StringBuilder AppendJoin<T>(this StringBuilder sb, string separator, IEnumerable<T> values)
    {
        if (sb is null)
            throw new ArgumentNullException(nameof(sb));
        if (values is null)
            throw new ArgumentNullException(nameof(values));

        separator ??= string.Empty;

        var wroteValue = false;

        foreach (var value in values)
        {
            if (wroteValue)
                sb.Append(separator);

            sb.Append(value);
            wroteValue = true;
        }

        return sb;
    }
}
