using Microsoft.Data.SqlClient;

namespace FluentCommand;

public static class DataConfigurationBuilderExtensions
{
    public static DataConfigurationBuilder UseSqlServer(this DataConfigurationBuilder builder)
    {
        builder
            .AddProviderFactory(SqlClientFactory.Instance)
            .AddSqlServerGenerator();

        return builder;
    }
}
