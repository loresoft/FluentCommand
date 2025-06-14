using Microsoft.Data.SqlClient;

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
}
