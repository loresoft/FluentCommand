using FluentCommand.Query.Generators;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace FluentCommand;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddFluentCommand(this IServiceCollection services, string connectionString)
    {
        services.AddFluentCommand(builder => builder.UseConnectionString(connectionString));

        return services;
    }

    public static IServiceCollection AddFluentCommand(this IServiceCollection services, Action<DataConfigurationBuilder> builder)
    {
        var configurationBuilder = new DataConfigurationBuilder(services);
        builder(configurationBuilder);

        // add defaults if not already added
        services.TryAddSingleton<IQueryGenerator, SqlServerGenerator>();
        services.TryAddSingleton<IDataQueryLogger, DataQueryLogger>();

        return services;
    }
}
