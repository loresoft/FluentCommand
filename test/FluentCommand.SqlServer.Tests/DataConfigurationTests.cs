using System.Data.Common;

using FluentCommand.Caching;
using FluentCommand.Query.Generators;

using Microsoft.Data.SqlClient;
using Microsoft.Extensions.DependencyInjection;

namespace FluentCommand.SqlServer.Tests;

public class DataConfigurationTests : DatabaseTestBase
{
    public DataConfigurationTests(DatabaseFixture databaseFixture)
        : base(databaseFixture)
    {
    }

    [Fact]
    public void GetServices()
    {
        var services = Services;

        var factory = services.GetService<DbProviderFactory>();
        factory.Should().NotBeNull();

        var sqlFactory = services.GetService<SqlClientFactory>();
        sqlFactory.Should().NotBeNull();


        var generator = services.GetService<IQueryGenerator>();
        generator.Should().NotBeNull();

        var sqlGenerator = services.GetService<SqlServerGenerator>();
        sqlGenerator.Should().NotBeNull();
        
        var dataCache = services.GetService<IDataCache>();
        dataCache.Should().NotBeNull();

        var distributedCache = services.GetService<DistributedDataCache>();
        distributedCache.Should().NotBeNull();

        var sessionFactory = services.GetService<IDataSessionFactory>();
        sessionFactory.Should().NotBeNull();

        var dataConfiguration = services.GetService<IDataConfiguration>();
        dataConfiguration.Should().NotBeNull();
        dataConfiguration.ConnectionString.Should().NotContain("Application Intent=ReadOnly");

        var dataSession = services.GetService<IDataSession>();
        dataSession.Should().NotBeNull();
        
        var readonlyFactory = services.GetService<IDataSessionFactory<ReadOnlyIntent>>();
        readonlyFactory.Should().NotBeNull();

        var readonlyConfiguration = services.GetService<IDataConfiguration<ReadOnlyIntent>>();
        readonlyConfiguration.Should().NotBeNull();
        //readonlyConfiguration.ConnectionString.Should().Contain("Application Intent=ReadOnly");

        var readonlySession = services.GetService<IDataSession<ReadOnlyIntent>>();
        readonlySession.Should().NotBeNull();

    }
}
