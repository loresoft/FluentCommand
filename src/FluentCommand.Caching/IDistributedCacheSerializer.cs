namespace FluentCommand.Caching;

/// <summary>
/// Interface defining a serializer of distrubted cache
/// </summary>
public interface IDistributedCacheSerializer
{
    /// <summary>
    /// Serializes the specified instance to a byte array for caching.
    /// </summary>
    /// <typeparam name="T">The type to serialize</typeparam>
    /// <param name="instance">The instance to serialize.</param>
    /// <returns>
    /// The byte array of the serialized instance
    /// </returns>
    byte[] Serialize<T>(T instance);

    /// <summary>
    /// Serializes the specified instance to a byte array for caching.
    /// </summary>
    /// <typeparam name="T">The type to serialize</typeparam>
    /// <param name="instance">The instance to serialize.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>
    /// The byte array of the serialized instance
    /// </returns>
    Task<byte[]> SerializeAsync<T>(T instance, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deserializes the specified byte array into an instance of <typeparamref name="T" />.
    /// </summary>
    /// <typeparam name="T">The type to deserialize</typeparam>
    /// <param name="byteArray">The byte array to deserialize.</param>
    /// <returns>
    /// An instance of <typeparamref name="T" /> deserialized
    /// </returns>
    T Deserialize<T>(byte[] byteArray);

    /// <summary>
    /// Deserializes the specified byte array into an instance of <typeparamref name="T" />.
    /// </summary>
    /// <typeparam name="T">The type to deserialize</typeparam>
    /// <param name="byteArray">The byte array to deserialize.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>
    /// An instance of <typeparamref name="T" /> deserialized
    /// </returns>
    Task<T> DeserializeAsync<T>(byte[] byteArray, CancellationToken cancellationToken = default);
}
