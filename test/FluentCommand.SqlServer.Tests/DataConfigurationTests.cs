using System.Data.Common;

using FluentAssertions;

using FluentCommand.Caching;
using FluentCommand.Query.Generators;

using Microsoft.Data.SqlClient;
using Microsoft.Extensions.DependencyInjection;

using Xunit;
using Xunit.Abstractions;

namespace FluentCommand.SqlServer.Tests;

public class DataConfigurationTests : DatabaseTestBase
{
    public DataConfigurationTests(ITestOutputHelper output, DatabaseFixture databaseFixture)
        : base(output, databaseFixture)
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

    }
}
