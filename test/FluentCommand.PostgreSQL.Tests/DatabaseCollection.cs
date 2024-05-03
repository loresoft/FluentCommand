namespace FluentCommand.PostgreSQL.Tests;

[CollectionDefinition(DatabaseCollection.CollectionName)]
public class DatabaseCollection : ICollectionFixture<DatabaseFixture>
{
    public const string CollectionName = "DatabaseCollection";
}
