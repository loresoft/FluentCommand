using System.Text;

namespace FluentCommand.Extensions;

/// <summary>
/// <see cref="StringBuilder"/> extension methods
/// </summary>
public static class StringBuilderExtensions
{
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
    /// Appends a copy of the specified string followed by the default line terminator if <paramref name="condition"/> is met.
    /// </summary>
    /// <param name="sb">The StringBuilder instance to append to.</param>
    /// <param name="condition">The condition delegate to evaluate. If condition is null, String.IsNullOrWhiteSpace method will be used.</param>
    public static StringBuilder AppendLineIf(this StringBuilder sb, Func<bool> condition)
    {
        if (condition())
            sb.AppendLine();

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
