#pragma warning disable IDE0130 // Namespace does not match folder structure

using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace System;

/// <summary>
/// An immutable array wrapper that implements value-based equality, suitable for use
/// in incremental generator models where structural comparison is required.
/// </summary>
/// <typeparam name="T">The element type, which must implement <see cref="IEquatable{T}"/>.</typeparam>
[ExcludeFromCodeCoverage]
[CollectionBuilder(typeof(EquatableArray), nameof(EquatableArray.Create))]
public readonly struct EquatableArray<T> : IEquatable<EquatableArray<T>>, IReadOnlyList<T>, IEnumerable<T>
    where T : IEquatable<T>
{
    /// <summary>
    /// An empty <see cref="EquatableArray{T}"/> instance.
    /// </summary>
    public static readonly EquatableArray<T> Empty = new();

    private readonly T[]? _array;

    /// <summary>
    /// Initializes a new empty <see cref="EquatableArray{T}"/>.
    /// </summary>
    public EquatableArray() => _array = [];

    /// <summary>
    /// Initializes a new <see cref="EquatableArray{T}"/> from an array.
    /// </summary>
    /// <param name="array">The array to wrap.</param>
    public EquatableArray(T[] array) => _array = array ?? [];

    /// <summary>
    /// Initializes a new <see cref="EquatableArray{T}"/> from an enumerable.
    /// </summary>
    /// <param name="items">The items to copy into the array.</param>
    public EquatableArray(IEnumerable<T> items) => _array = [.. items];

    /// <summary>
    /// Gets the number of elements in the array.
    /// </summary>
    public int Length => _array?.Length ?? 0;

    /// <inheritdoc />
    public int Count => _array?.Length ?? 0;

    /// <inheritdoc />
    public T this[int index]
    {
        get
        {
            if (_array is null)
                throw new IndexOutOfRangeException("The array is empty.");

            return _array[index];
        }
    }


    /// <summary>
    /// Returns the array as a <see cref="ReadOnlySpan{T}"/>.
    /// </summary>
    /// <returns>A read-only span over the array elements.</returns>
    public ReadOnlySpan<T> AsSpan() => _array.AsSpan();

    /// <inheritdoc />
    public bool Equals(EquatableArray<T> other)
    {
        var self = _array;
        var otherArray = other._array;

        if (ReferenceEquals(self, otherArray))
            return true;

        return self.AsSpan().SequenceEqual(otherArray.AsSpan());
    }

    /// <inheritdoc />
    public override bool Equals(object? obj)
        => obj is EquatableArray<T> array && Equals(array);

    /// <inheritdoc />
    public override int GetHashCode()
    {
        var array = _array;
        if (array == null || array.Length == 0)
            return 0;

        var hashCode = 16777619;

        for (int i = 0; i < array.Length; i++)
            hashCode = unchecked((hashCode * -1521134295) + EqualityComparer<T>.Default.GetHashCode(array[i]));

        return hashCode;
    }


    /// <summary>
    /// Returns a non-allocating struct enumerator for foreach.
    /// </summary>
    public Enumerator GetEnumerator() => new(_array ?? []);

    /// <inheritdoc />
    IEnumerator<T> IEnumerable<T>.GetEnumerator()
        => ((IEnumerable<T>)(_array ?? [])).GetEnumerator();

    /// <inheritdoc />
    IEnumerator IEnumerable.GetEnumerator()
        => (_array ?? []).GetEnumerator();


    /// <summary>
    /// Implicitly converts an array to an <see cref="EquatableArray{T}"/>.
    /// </summary>
    /// <param name="array">The array to wrap.</param>
    public static implicit operator EquatableArray<T>(T[] array) => new(array);

    /// <summary>
    /// Implicitly converts a list to an <see cref="EquatableArray{T}"/>.
    /// </summary>
    /// <param name="items">The list to copy.</param>
    public static implicit operator EquatableArray<T>(List<T> items) => new(items);


    /// <summary>
    /// A struct enumerator for <see cref="EquatableArray{T}"/> that avoids heap allocation.
    /// </summary>
    public struct Enumerator
    {
        private readonly T[] _array;
        private int _index;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal Enumerator(T[] array)
        {
            _array = array;
            _index = -1;
        }

        /// <summary>
        /// Gets the element at the current position of the enumerator.
        /// </summary>
        public readonly T Current
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _array[_index];
        }

        /// <summary>
        /// Advances the enumerator to the next element of the array.
        /// </summary>
        /// <returns><see langword="true"/> if the enumerator was successfully advanced; <see langword="false"/> if the enumerator has passed the end of the array.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool MoveNext() => ++_index < _array.Length;
    }
}

/// <summary>
/// Factory for creating <see cref="EquatableArray{T}"/> instances from spans.
/// </summary>
[ExcludeFromCodeCoverage]
internal static class EquatableArray
{
    /// <summary>
    /// Creates an <see cref="EquatableArray{T}"/> from a <see cref="ReadOnlySpan{T}"/>.
    /// </summary>
    /// <typeparam name="T">The element type.</typeparam>
    /// <param name="items">The span of items to copy.</param>
    /// <returns>A new <see cref="EquatableArray{T}"/> containing the items.</returns>
    public static EquatableArray<T> Create<T>(ReadOnlySpan<T> items)
        where T : IEquatable<T>
        => new(items.ToArray());


    /// <summary>
    /// Creates an <see cref="EquatableArray{T}"/> from the specified items.
    /// </summary>
    /// <typeparam name="T">The element type.</typeparam>
    /// <param name="items">The items to include in the array.</param>
    /// <returns>A new <see cref="EquatableArray{T}"/> containing the items.</returns>
    public static EquatableArray<T> Create<T>(params T[] items)
        where T : IEquatable<T>
        => new(items);

}
