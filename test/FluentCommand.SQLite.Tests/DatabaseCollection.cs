[assembly: CollectionBehavior(CollectionBehavior.CollectionPerAssembly, DisableTestParallelization = true)]

namespace FluentCommand.SQLite.Tests;

[CollectionDefinition(DatabaseCollection.CollectionName)]
public class DatabaseCollection : ICollectionFixture<DatabaseFixture>
{
    public const string CollectionName = "DatabaseCollection";
}
