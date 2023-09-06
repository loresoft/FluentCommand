namespace FluentCommand.Caching;

public interface IDistributedCacheSerializer
{
    byte[] Serialize<T>(T instance);

    Task<byte[]> SerializeAsync<T>(T instance, CancellationToken cancellationToken = default);

    T Deserialize<T>(byte[] byteArray);

    Task<T> DeserializeAsync<T>(byte[] byteArray, CancellationToken cancellationToken = default);
}
