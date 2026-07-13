using System.Diagnostics.CodeAnalysis;
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
    public static StringBuilder AppendLine(this StringBuilder sb, [StringSyntax("CompositeFormat")] string format, params object[] args)
    {
        ArgumentNullException.ThrowIfNull(sb);

        sb.AppendFormat(format, args);
        sb.AppendLine();
        return sb;
    }

    /// <summary>
    /// Appends a copy of the specified string if <paramref name="condition"/> is met.
    /// </summary>
    /// <param name="sb">The StringBuilder instance to append to.</param>
    /// <param name="text">The string to append.</param>
    /// <param name="condition">The condition delegate to evaluate. If condition is null, String.IsNullOrEmpty method will be used.</param>
    public static StringBuilder AppendIf(this StringBuilder sb, string text, Func<string, bool>? condition = null)
    {
        ArgumentNullException.ThrowIfNull(sb);

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
    /// <param name="condition">The condition delegate to evaluate.</param>
    public static StringBuilder AppendIf(this StringBuilder sb, string text, bool condition)
    {
        ArgumentNullException.ThrowIfNull(sb);

        if (condition)
            sb.Append(text);

        return sb;
    }

    /// <summary>
    /// Appends a copy of the specified string followed by the default line terminator if <paramref name="condition"/> is met.
    /// </summary>
    /// <param name="sb">The StringBuilder instance to append to.</param>
    /// <param name="text">The string to append.</param>
    /// <param name="condition">The condition delegate to evaluate. If condition is null, String.IsNullOrEmpty method will be used.</param>
    public static StringBuilder AppendLineIf(this StringBuilder sb, string text, Func<string, bool>? condition = null)
    {
        ArgumentNullException.ThrowIfNull(sb);

        var c = condition ?? (s => !string.IsNullOrEmpty(s));

        if (c(text))
            sb.AppendLine(text);

        return sb;
    }

    /// <summary>
    /// Appends a copy of the specified string followed by the default line terminator if <paramref name="condition"/> is met.
    /// </summary>
    /// <param name="sb">The StringBuilder instance to append to.</param>
    /// <param name="condition">The condition delegate to evaluate.</param>
    public static StringBuilder AppendLineIf(this StringBuilder sb, Func<bool> condition)
    {
        ArgumentNullException.ThrowIfNull(sb);

        if (condition())
            sb.AppendLine();

        return sb;
    }

    /// <summary>
    /// Appends a copy of the specified string truncated to the specified maximum length, using an ellipsis when truncated.
    /// </summary>
    /// <param name="sb">The StringBuilder instance to append to.</param>
    /// <param name="text">The string to append.</param>
    /// <param name="maxLength">The maximum number of characters to append.</param>
    /// <param name="ellipsis">The string to append when the value is truncated.</param>
    public static StringBuilder AppendTruncated(this StringBuilder sb, string? text, int maxLength, string? ellipsis = "...")
    {
        ArgumentNullException.ThrowIfNull(sb);

        if (text == null)
            return sb;

        ArgumentOutOfRangeException.ThrowIfLessThan(maxLength, 0);

        if (text.Length <= maxLength)
        {
            sb.Append(text);
            return sb;
        }

        if (maxLength == 0)
            return sb;

        if (string.IsNullOrEmpty(ellipsis))
        {
            sb.Append(text, 0, maxLength);
            return sb;
        }

        var truncationSuffix = ellipsis ?? string.Empty;

        if (truncationSuffix.Length >= maxLength)
        {
            sb.Append(truncationSuffix, 0, maxLength);
            return sb;
        }

        sb.Append(text, 0, maxLength - truncationSuffix.Length);
        sb.Append(truncationSuffix);

        return sb;
    }

    /// <summary>
    /// Appends a string representation of the specified value truncated to the specified maximum length, using an ellipsis when truncated.
    /// </summary>
    /// <param name="sb">The StringBuilder instance to append to.</param>
    /// <param name="value">The value to append.</param>
    /// <param name="maxLength">The maximum number of characters to append.</param>
    /// <param name="ellipsis">The string to append when the value is truncated.</param>
    public static StringBuilder AppendTruncated(this StringBuilder sb, object? value, int maxLength, string? ellipsis = "...")
    {
        ArgumentNullException.ThrowIfNull(sb);

        if (value == null)
            return sb;

        ArgumentOutOfRangeException.ThrowIfLessThan(maxLength, 0);

        if (maxLength == 0)
            return sb;

        return value is string text
            ? AppendTruncated(sb, text, maxLength, ellipsis)
            : AppendTruncated(sb, value.ToString(), maxLength, ellipsis);
    }
}
