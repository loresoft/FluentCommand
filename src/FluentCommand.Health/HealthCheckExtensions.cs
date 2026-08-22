using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;

namespace FluentCommand.Health;

/// <summary>
/// Extension methods for registering FluentCommand health checks.
/// </summary>
public static class HealthCheckExtensions
{
    /// <summary>
    /// Adds a FluentCommand health check using the default <see cref="IDataSessionFactory"/>.
    /// </summary>
    /// <param name="health">The <see cref="IHealthChecksBuilder"/> to add the health check to.</param>
    /// <param name="commandText">
    /// An optional SQL command text to execute as the health check query.
    /// Defaults to <c>SELECT 1</c> when <see langword="null"/>.
    /// </param>
    public static void AddFluentCommand(this IHealthChecksBuilder health, string? commandText = null)
    {
        var healthCheckRegistration = new HealthCheckRegistration(
            name: "FluentCommand",
            factory: sp =>
            {
                var logger = sp.GetRequiredService<ILogger<FluentCommandHealthCheck>>();
                var sessionFactory = sp.GetRequiredService<IDataSessionFactory>();

                return new FluentCommandHealthCheck(logger, sessionFactory, commandText);
            },
            failureStatus: HealthStatus.Unhealthy,
            tags: ["SQL"]);

        health.Add(healthCheckRegistration);
    }

    /// <summary>
    /// Adds a FluentCommand health check using a discriminated <see cref="IDataSessionFactory{TDiscriminator}"/>,
    /// allowing multiple named data session factories to be registered in the same application.
    /// </summary>
    /// <typeparam name="TDiscriminator">
    /// A marker type used to resolve the correct <see cref="IDataSessionFactory{TDiscriminator}"/> from the service container.
    /// </typeparam>
    /// <param name="health">The <see cref="IHealthChecksBuilder"/> to add the health check to.</param>
    /// <param name="commandText">
    /// An optional SQL command text to execute as the health check query.
    /// Defaults to <c>SELECT 1</c> when <see langword="null"/>.
    /// </param>
    public static void AddFluentCommand<TDiscriminator>(this IHealthChecksBuilder health, string? commandText = null)
    {
        var healthCheckRegistration = new HealthCheckRegistration(
            name: $"FluentCommand: {typeof(TDiscriminator).Name}",
            factory: sp =>
            {
                var logger = sp.GetRequiredService<ILogger<FluentCommandHealthCheck>>();
                var sessionFactory = sp.GetRequiredService<IDataSessionFactory<TDiscriminator>>();

                return new FluentCommandHealthCheck(logger, sessionFactory, commandText);
            },
            failureStatus: HealthStatus.Unhealthy,
            tags: ["SQL"]);

        health.Add(healthCheckRegistration);
    }

}
