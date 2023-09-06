using System.Threading;

using MessagePack;
using MessagePack.Resolvers;

namespace FluentCommand.Caching;

public class MessagePackCacheSerializer : IDistributedCacheSerializer
{
    private readonly MessagePackSerializerOptions _messagePackSerializerOptions;

    public MessagePackCacheSerializer(MessagePackSerializerOptions messagePackSerializerOptions = null)
    {
        _messagePackSerializerOptions = messagePackSerializerOptions
            ?? ContractlessStandardResolver.Options.WithCompression(MessagePackCompression.Lz4BlockArray);
    }

    public T Deserialize<T>(byte[] byteArray)
    {
        var value = MessagePackSerializer.Deserialize<T>(byteArray, _messagePackSerializerOptions);
        return value;
    }

    public Task<T> DeserializeAsync<T>(byte[] byteArray, CancellationToken cancellationToken = default)
    {
        var value = MessagePackSerializer.Deserialize<T>(byteArray, _messagePackSerializerOptions, cancellationToken);
        return Task.FromResult(value);
    }

    public byte[] Serialize<T>(T instance)
    {
        var value = MessagePackSerializer.Serialize(instance, _messagePackSerializerOptions);
        return value;
    }

    public Task<byte[]> SerializeAsync<T>(T instance, CancellationToken cancellationToken = default)
    {
        var value = MessagePackSerializer.Serialize(instance, _messagePackSerializerOptions, cancellationToken);
        return Task.FromResult(value);
    }
}
