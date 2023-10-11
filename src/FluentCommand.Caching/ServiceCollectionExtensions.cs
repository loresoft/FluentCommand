using MessagePack;
using MessagePack.Resolvers;

using Microsoft.Extensions.DependencyInjection.Extensions;

namespace FluentCommand.Caching;

public static class ServiceCollectionExtensions
{
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
