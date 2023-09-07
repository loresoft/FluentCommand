using MessagePack;
using MessagePack.Resolvers;

namespace FluentCommand.Caching;

/// <summary>
/// A MessagePack implementation of IDistributedCacheSerializer
/// </summary>
/// <seealso cref="FluentCommand.Caching.IDistributedCacheSerializer" />
public class MessagePackCacheSerializer : IDistributedCacheSerializer
{
    private readonly MessagePackSerializerOptions _messagePackSerializerOptions;

    /// <summary>
    /// Initializes a new instance of the <see cref="MessagePackCacheSerializer"/> class.
    /// </summary>
    /// <param name="messagePackSerializerOptions">The message pack serializer options.</param>
    public MessagePackCacheSerializer(MessagePackSerializerOptions messagePackSerializerOptions = null)
    {
        _messagePackSerializerOptions = messagePackSerializerOptions
            ?? ContractlessStandardResolver.Options.WithCompression(MessagePackCompression.Lz4BlockArray);
    }

    /// <summary>
    /// Deserializes the specified byte array into an instance of <typeparamref name="T" />.
    /// </summary>
    /// <typeparam name="T">The type to deserialize</typeparam>
    /// <param name="byteArray">The byte array to deserialize.</param>
    /// <returns>
    /// An instance of <typeparamref name="T" /> deserialized
    /// </returns>
    public T Deserialize<T>(byte[] byteArray)
    {
        var value = MessagePackSerializer.Deserialize<T>(byteArray, _messagePackSerializerOptions);
        return value;
    }

    /// <summary>
    /// Deserializes the specified byte array into an instance of <typeparamref name="T" />.
    /// </summary>
    /// <typeparam name="T">The type to deserialize</typeparam>
    /// <param name="byteArray">The byte array to deserialize.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>
    /// An instance of <typeparamref name="T" /> deserialized
    /// </returns>
    public Task<T> DeserializeAsync<T>(byte[] byteArray, CancellationToken cancellationToken = default)
    {
        var value = MessagePackSerializer.Deserialize<T>(byteArray, _messagePackSerializerOptions, cancellationToken);
        return Task.FromResult(value);
    }

    /// <summary>
    /// Serializes the specified instance to a byte array for caching.
    /// </summary>
    /// <typeparam name="T">The type to serialize</typeparam>
    /// <param name="instance">The instance to serialize.</param>
    /// <returns>
    /// The byte array of the serialized instance
    /// </returns>
    public byte[] Serialize<T>(T instance)
    {
        var value = MessagePackSerializer.Serialize(instance, _messagePackSerializerOptions);
        return value;
    }

    /// <summary>
    /// Serializes the specified instance to a byte array for caching.
    /// </summary>
    /// <typeparam name="T">The type to serialize</typeparam>
    /// <param name="instance">The instance to serialize.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>
    /// The byte array of the serialized instance
    /// </returns>
    public Task<byte[]> SerializeAsync<T>(T instance, CancellationToken cancellationToken = default)
    {
        var value = MessagePackSerializer.Serialize(instance, _messagePackSerializerOptions, cancellationToken);
        return Task.FromResult(value);
    }
}
