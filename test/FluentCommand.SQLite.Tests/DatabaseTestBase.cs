using XUnit.Hosting;

namespace FluentCommand.SQLite.Tests;

[Collection(DatabaseCollection.CollectionName)]
public abstract class DatabaseTestBase : TestHostBase<DatabaseFixture>
{
    protected DatabaseTestBase(DatabaseFixture databaseFixture)
        : base(databaseFixture)
    {
    }
}
