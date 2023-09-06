using System.ComponentModel;
using System.Linq;

using BenchmarkDotNet.Attributes;

using Dapper;

using FluentCommand.Entities;

namespace FluentCommand.Performance;

[Description("Dapper")]
public class DapperBenchmarks : BenchmarkBase
{
    [GlobalSetup]
    public void Setup()
    {
        BaseSetup();
    }

    [Benchmark(Description = "Query<T> (buffered)")]
    public Post QueryBuffered()
    {
        var i = NextIndex();
        return Connection.Query<Post>("select * from Post where Id = @Id", new { Id = i }, buffered: true).First();
    }

    [Benchmark(Description = "Query<T> (unbuffered)")]
    public Post QueryUnbuffered()
    {
        var i = NextIndex();
        return Connection.Query<Post>("select * from Post where Id = @Id", new { Id = i }, buffered: false).First();
    }

    [Benchmark(Description = "QueryFirstOrDefault<T>")]
    public Post QueryFirstOrDefault()
    {
        var i = NextIndex();
        return Connection.QueryFirstOrDefault<Post>("select * from Post where Id = @Id", new { Id = i });
    }
}
