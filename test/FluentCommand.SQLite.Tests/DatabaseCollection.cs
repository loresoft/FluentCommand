// support capturing console and trace output in xunit v3
[assembly: CaptureConsole]
[assembly: CaptureTrace]
[assembly: CollectionBehavior(CollectionBehavior.CollectionPerAssembly)]
[assembly: Xunit.v3.Parallelization(Mode = Xunit.Sdk.ParallelMode.None)]

namespace FluentCommand.SQLite.Tests;

[CollectionDefinition(CollectionName)]
public class DatabaseCollection : ICollectionFixture<DatabaseFixture>
{
    public const string CollectionName = "DatabaseCollection";
}
