using Microsoft.Data.SqlClient;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace FluentCommand;

/// <summary>
/// Extension methods for configuring <see cref="DataConfigurationBuilder"/> to use SQL Server.
/// </summary>
public static class DataConfigurationBuilderExtensions
{
    /// <summary>
    /// Configures the <see cref="DataConfigurationBuilder"/> to use SQL Server as the database provider.
    /// Registers the <see cref="SqlClientFactory"/> and adds the SQL Server query generator.
    /// </summary>
    /// <param name="builder">The data configuration builder to configure.</param>
    /// <returns>
    /// The same <see cref="DataConfigurationBuilder"/> instance so that multiple calls can be chained.
    /// </returns>
    public static DataConfigurationBuilder UseSqlServer(this DataConfigurationBuilder builder)
    {
        builder
            .AddProviderFactory(SqlClientFactory.Instance)
            .AddSqlServerGenerator();

        return builder;
    }

    /// <summary>
    /// Configures the <see cref="DataConfigurationBuilder"/> to capture SQL Server informational messages such as PRINT output.
    /// </summary>
    /// <param name="builder">The data configuration builder to configure.</param>
    /// <returns>
    /// The same <see cref="DataConfigurationBuilder"/> instance so that multiple calls can be chained.
    /// </returns>
    public static DataConfigurationBuilder CaptureMessages(this DataConfigurationBuilder builder)
    {
        builder.AddInterceptor<MessageInterceptor>();

        return builder;
    }

    /// <summary>
    /// Configures the <see cref="DataConfigurationBuilder"/> to capture SQL Server informational messages such as PRINT output.
    /// </summary>
    /// <param name="builder">The data configuration builder to configure.</param>
    /// <param name="maxMessageLength">The maximum length of the rendered SQL Server message.</param>
    /// <returns>
    /// The same <see cref="DataConfigurationBuilder"/> instance so that multiple calls can be chained.
    /// </returns>
    public static DataConfigurationBuilder CaptureMessages(this DataConfigurationBuilder builder, int maxMessageLength)
    {
        builder.AddInterceptor(sp => new MessageInterceptor(sp.GetRequiredService<ILogger<MessageInterceptor>>(), maxMessageLength));

        return builder;
    }
}
