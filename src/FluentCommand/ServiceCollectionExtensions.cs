using Microsoft.Extensions.DependencyInjection;

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

        configurationBuilder.AddConfiguration();

        return services;
    }

    public static IServiceCollection AddFluentCommand<TDiscriminator>(this IServiceCollection services, string connectionString)
    {
        services.AddFluentCommand<TDiscriminator>(builder => builder.UseConnectionString(connectionString));

        return services;
    }

    public static IServiceCollection AddFluentCommand<TDiscriminator>(this IServiceCollection services, Action<DataConfigurationBuilder> builder)
    {
        var configurationBuilder = new DataConfigurationBuilder(services);
        builder(configurationBuilder);

        configurationBuilder.AddConfiguration<TDiscriminator>();

        return services;
    }
}
