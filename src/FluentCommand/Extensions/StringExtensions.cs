using System.Buffers;
using System.Diagnostics.CodeAnalysis;

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
    public static bool IsNullOrEmpty([NotNullWhen(false)] this string? item)
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
    public static bool IsNullOrWhiteSpace([NotNullWhen(false)] this string? item)
    {
        return string.IsNullOrWhiteSpace(item);
    }

    /// <summary>
    /// Determines whether the specified string is not <see cref="IsNullOrEmpty"/>.
    /// </summary>
    /// <param name="value">The value to check.</param>
    /// <returns>
    ///   <c>true</c> if the specified <paramref name="value"/> is not <see cref="IsNullOrEmpty"/>; otherwise, <c>false</c>.
    /// </returns>
    public static bool HasValue([NotNullWhen(true)] this string? value)
    {
        return !string.IsNullOrEmpty(value);
    }

    /// <summary>
    /// Replaces the format item in a specified string with the string representation of a corresponding object in a specified array.
    /// </summary>
    /// <param name="format">A composite format string</param>
    /// <param name="args">An object array that contains zero or more objects to format</param>
    /// <returns>A copy of format in which the format items have been replaced by the string representation of the corresponding objects in args</returns>
    public static string FormatWith([StringSyntax("CompositeFormat")] this string format, params object?[] args)
    {
        return string.Format(format, args);
    }

    /// <summary>
    /// Truncates the specified string to a maximum length, optionally appending an ellipsis or custom suffix if truncation occurs.
    /// </summary>
    /// <param name="text">The string to truncate.</param>
    /// <param name="keep">The number of characters to keep (including the ellipsis, if used).</param>
    /// <param name="ellipsis">The string to append if truncation occurs. Defaults to "..." if not specified.</param>
    /// <returns>
    /// The truncated string with the ellipsis (or custom suffix) appended if truncation occurred; otherwise, the original string.
    /// Returns <c>null</c> if <paramref name="text"/> is <c>null</c>.
    /// </returns>
    [return: NotNullIfNotNull(nameof(text))]
    public static string? Truncate(this string? text, int keep, string? ellipsis = "...")
    {
        if (IsNullOrEmpty(text) || text.Length <= keep)
            return text;

        ellipsis ??= string.Empty;

        int ellipsisLength = ellipsis.Length;

        // If there's no room for ellipsis, just return truncated prefix
        if (keep <= ellipsisLength)
            return text[..keep];

        int prefixLength = keep - ellipsisLength;

#if NETSTANDARD2_0
        return string.Concat(text[..prefixLength], ellipsis);
#else
        int totalLength = prefixLength + ellipsisLength;

        // Use stack allocation for short strings, or rent from the pool for longer ones
        char[]? rentedArray = null;
        Span<char> buffer = totalLength <= 256
            ? stackalloc char[totalLength]
            : (rentedArray = ArrayPool<char>.Shared.Rent(totalLength));

        try
        {
            text.AsSpan(0, prefixLength).CopyTo(buffer);
            ellipsis.AsSpan().CopyTo(buffer[prefixLength..]);

            return new string(buffer[..totalLength]);
        }
        finally
        {
            // Return rented array to the pool to avoid memory leaks
            if (rentedArray != null)
                ArrayPool<char>.Shared.Return(rentedArray);
        }
#endif
    }

}
