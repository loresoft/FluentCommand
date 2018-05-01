using System;
using FluentCommand.SqlServer.Tests;
using Xunit;

namespace FluentCommand.Batch.Tests
{
    [CollectionDefinition(BatchDatabaseCollection.CollectionName)]
    public class BatchDatabaseCollection : ICollectionFixture<DatabaseFixture>
    {
        public const string CollectionName = "BatchDatabaseCollection";
    }
}