using System.ComponentModel;
using System.Linq;

using BenchmarkDotNet.Attributes;

using Dapper;

using FluentCommand.Entities;
using FluentCommand.Extensions;

namespace FluentCommand.Performance;

[Description("Fluent")]
public class FluentCommandBenchmarks : BenchmarkBase
{
    [GlobalSetup]
    public void Setup()
    {
        BaseSetup();
    }

    [Benchmark(Description = "Query<T> (Factory)")]
    public Post QueryFactory()
    {
        var i = NextIndex();

        var session = new DataSession(Connection, false);
        var sql = "select * from Post where Id = @Id";

        return session.Sql(sql)
            .Parameter("@Id", i)
            .Query(r => new Post
            {
                Id = r.GetInt32("Id"),
                Text = r.GetStringNull("Text"),
                CreationDate = r.GetDateTime("CreationDate"),
                LastChangeDate = r.GetDateTime("LastChangeDate"),
                Counter1 = r.GetInt32Null("Counter1"),
                Counter2 = r.GetInt32Null("Counter2"),
                Counter3 = r.GetInt32Null("Counter3"),
                Counter4 = r.GetInt32Null("Counter4"),
                Counter5 = r.GetInt32Null("Counter5"),
                Counter6 = r.GetInt32Null("Counter6"),
                Counter7 = r.GetInt32Null("Counter7"),
                Counter8 = r.GetInt32Null("Counter8"),
                Counter9 = r.GetInt32Null("Counter9"),
            })
            .First();
    }

    [Benchmark(Description = "QueryPost (Generated)")]
    public Post QueryPost()
    {
        var i = NextIndex();

        var session = new DataSession(Connection, false);
        var sql = "select * from Post where Id = @Id";

        return session.Sql(sql)
            .Parameter("@Id", i)
            .Query<Post>()
            .First();
    }

    [Benchmark(Description = "QuerySingle<T> (Factory)")]
    public Post QuerySingleFactory()
    {
        var i = NextIndex();

        var session = new DataSession(Connection, false);
        var sql = "select * from Post where Id = @Id";

        return session.Sql(sql)
            .Parameter("@Id", i)
            .QuerySingle(r => new Post
            {
                Id = r.GetInt32("Id"),
                Text = r.GetStringNull("Text"),
                CreationDate = r.GetDateTime("CreationDate"),
                LastChangeDate = r.GetDateTime("LastChangeDate"),
                Counter1 = r.GetInt32Null("Counter1"),
                Counter2 = r.GetInt32Null("Counter2"),
                Counter3 = r.GetInt32Null("Counter3"),
                Counter4 = r.GetInt32Null("Counter4"),
                Counter5 = r.GetInt32Null("Counter5"),
                Counter6 = r.GetInt32Null("Counter6"),
                Counter7 = r.GetInt32Null("Counter7"),
                Counter8 = r.GetInt32Null("Counter8"),
                Counter9 = r.GetInt32Null("Counter9"),
            });
    }

    [Benchmark(Description = "QuerySinglePost (Generated)")]
    public Post QuerySinglePost()
    {
        var i = NextIndex();

        var session = new DataSession(Connection, false);
        var sql = "select * from Post where Id = @Id";

        return session.Sql(sql)
            .Parameter("@Id", i)
            .QuerySingle<Post>();
    }
}
