using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Exporters;
using BenchmarkDotNet.Exporters.Csv;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Order;
using BenchmarkDotNet.Running;

using Microsoft.Data.SqlClient;

using static System.Console;

namespace FluentCommand.Performance;

internal static class Program
{
    static void Main(string[] args)
    {
        WriteLine("Using ConnectionString: " + BenchmarkBase.ConnectionString);
        EnsureDatabase();
        WriteLine("Database setup complete.");

        var benchmarkSwitcher = new BenchmarkSwitcher(typeof(BenchmarkBase).Assembly);

        var config = ManualConfig.CreateMinimumViable()
            .AddExporter(CsvExporter.Default)
            .AddExporter(MarkdownExporter.GitHub)
            .AddDiagnoser(MemoryDiagnoser.Default)
            .AddJob(Job.ShortRun
                .WithLaunchCount(1)
                .WithWarmupCount(2)
                .WithUnrollFactor(500)
                .WithIterationCount(10)
            )
            .WithOption(ConfigOptions.JoinSummary, true)
            .WithOrderer(new DefaultOrderer(SummaryOrderPolicy.FastestToSlowest));

        benchmarkSwitcher.Run(args, config);
    }

    private static void EnsureDatabase()
    {
        using var connection = new SqlConnection(BenchmarkBase.ConnectionString);
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = @"
If (Object_Id('Post') Is Null)
Begin
    Create Table Post
    (
        Id int identity primary key, 
        [Text] varchar(max) not null, 
        CreationDate datetime not null, 
        LastChangeDate datetime not null,
        Counter1 int,
        Counter2 int,
        Counter3 int,
        Counter4 int,
        Counter5 int,
        Counter6 int,
        Counter7 int,
        Counter8 int,
        Counter9 int
    );
       
    Set NoCount On;
    Declare @i int = 0;

    While @i <= 5001
    Begin
        Insert Post ([Text],CreationDate, LastChangeDate) values (replicate('x', 2000), GETDATE(), GETDATE());
        Set @i = @i + 1;
    End
End
";
        command.Connection = connection;
        command.ExecuteNonQuery();
    }
}
