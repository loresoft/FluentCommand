using System;

using Npgsql;

using Xunit;
using Xunit.Abstractions;

namespace FluentCommand.PostgreSQL.Tests;

[Collection(DatabaseCollection.CollectionName)]
public abstract class DatabaseTestBase : IDisposable
{
    protected DatabaseTestBase(ITestOutputHelper output, DatabaseFixture databaseFixture)
    {
        Output = output;
        Fixture = databaseFixture;
    }


    public ITestOutputHelper Output { get; }

    public DatabaseFixture Fixture { get; }


    protected IDataConfiguration GetConfiguration()
    {
        var dataConfiguration = new DataConfiguration(
            NpgsqlFactory.Instance,
            Fixture.ConnectionString,
            logger: Output.WriteLine);

        return dataConfiguration;
    }

    public void Dispose()
    {
        Fixture?.Report(Output);
    }
}
