using MessagePack;
using MessagePack.Resolvers;

using Microsoft.Extensions.DependencyInjection.Extensions;

namespace FluentCommand.Caching;

/// <summary>
/// Extension methods for <see cref="DataConfigurationBuilder"/>
/// </summary>
public static class DataConfigurationBuilderExtensions
{
    /// <summary>
    /// Adds the distributed data cache.
    /// </summary>
    /// <param name="builder">The data configuration builder.</param>
    /// <returns></returns>
    public static DataConfigurationBuilder AddDistributedDataCache(this DataConfigurationBuilder builder)
    {
        builder.AddService(sp =>
        {
            sp.TryAddSingleton(ContractlessStandardResolver.Options.WithCompression(MessagePackCompression.Lz4BlockArray));
            sp.TryAddSingleton<IDistributedCacheSerializer, MessagePackCacheSerializer>();
        });

        builder.AddDataCache<DistributedDataCache>();

        return builder;
    }
}
