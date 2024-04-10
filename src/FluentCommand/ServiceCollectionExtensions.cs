using Microsoft.Extensions.DependencyInjection;

namespace FluentCommand;

/// <summary>
/// Extension methods for <see cref="IServiceCollection"/>
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds the fluent command services with the specified connection string.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection" /> to add services to.</param>
    /// <param name="nameOrConnectionString">The connection string or the name of connection string located in the application configuration.</param>
    /// <returns>
    /// The same service collection so that multiple calls can be chained.
    /// </returns>
    public static IServiceCollection AddFluentCommand(this IServiceCollection services, string nameOrConnectionString)
    {
        services.AddFluentCommand(builder => builder.UseConnectionName(nameOrConnectionString));

        return services;
    }

    /// <summary>
    /// Adds the fluent command services with the specified configuration action.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection" /> to add services to.</param>
    /// <param name="builder">An action builder to configure the fluent command services.</param>
    /// <returns>
    /// The same service collection so that multiple calls can be chained.
    /// </returns>
    public static IServiceCollection AddFluentCommand(this IServiceCollection services, Action<DataConfigurationBuilder> builder)
    {
        var configurationBuilder = new DataConfigurationBuilder(services);
        builder(configurationBuilder);

        configurationBuilder.AddConfiguration();

        return services;
    }

    /// <summary>
    /// Adds the fluent command services using the <typeparamref name="TDiscriminator"/> to support typed registration
    /// </summary>
    /// <typeparam name="TDiscriminator">The type of the discriminator.</typeparam>
    /// <param name="services">The <see cref="IServiceCollection" /> to add services to.</param>
    /// <param name="nameOrConnectionString">The connection string or the name of connection string located in the application configuration.</param>
    /// <returns>
    /// The same service collection so that multiple calls can be chained.
    /// </returns>
    public static IServiceCollection AddFluentCommand<TDiscriminator>(this IServiceCollection services, string nameOrConnectionString)
    {
        services.AddFluentCommand<TDiscriminator>(builder => builder.UseConnectionName(nameOrConnectionString));

        return services;
    }

    /// <summary>
    /// Adds the fluent command services using the <typeparamref name="TDiscriminator"/> to support typed registration
    /// </summary>
    /// <typeparam name="TDiscriminator">The type of the discriminator.</typeparam>
    /// <param name="services">The <see cref="IServiceCollection" /> to add services to.</param>
    /// <param name="builder">An action builder to configure the fluent command services.</param>
    /// <returns>
    /// The same service collection so that multiple calls can be chained.
    /// </returns>
    public static IServiceCollection AddFluentCommand<TDiscriminator>(this IServiceCollection services, Action<DataConfigurationBuilder> builder)
    {
        var configurationBuilder = new DataConfigurationBuilder(services);
        builder(configurationBuilder);

        configurationBuilder.AddConfiguration<TDiscriminator>();

        return services;
    }
}
