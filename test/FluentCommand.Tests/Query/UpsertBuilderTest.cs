using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using FluentCommand.Query;
using FluentCommand.Query.Generators;

namespace FluentCommand.Tests.Query;

public class UpsertBuilderTest
{
    [Fact]
    public async System.Threading.Tasks.Task UpsertExplicitKeySqlServer()
    {
        var sqlProvider = new SqlServerGenerator();
        var parameters = new List<QueryParameter>();

        var builder = new UpsertBuilder(sqlProvider, parameters)
            .Into("Status", "dbo")
            .Key("Id")
            .Value("Id", 1)
            .Value("Name", "test")
            .Value("Description", "test")
            .Output("Id");

        var queryStatement = builder.BuildStatement();

        var sql = queryStatement!.Statement;

        await Verifier
            .Verify(sql)
            .UseDirectory("Snapshots")
            .ScrubLinesContaining("/* Caller;");
    }

    [Fact]
    public async System.Threading.Tasks.Task UpsertExplicitKeyPostgreSql()
    {
        var sqlProvider = new PostgreSqlGenerator();
        var parameters = new List<QueryParameter>();

        var builder = new UpsertBuilder(sqlProvider, parameters)
            .Into("Status", "dbo")
            .Key("Id")
            .Value("Id", 1)
            .Value("Name", "test")
            .Value("Description", "test")
            .Output("Id");

        var queryStatement = builder.BuildStatement();

        var sql = queryStatement!.Statement;

        await Verifier
            .Verify(sql)
            .UseDirectory("Snapshots")
            .ScrubLinesContaining("/* Caller;");
    }

    [Fact]
    public async System.Threading.Tasks.Task UpsertExplicitKeySqlite()
    {
        var sqlProvider = new SqliteGenerator();
        var parameters = new List<QueryParameter>();

        var builder = new UpsertBuilder(sqlProvider, parameters)
            .Into("Status", "dbo")
            .Key("Id")
            .Value("Id", 1)
            .Value("Name", "test")
            .Value("Description", "test")
            .Output("Id");

        var queryStatement = builder.BuildStatement();

        var sql = queryStatement!.Statement;

        await Verifier
            .Verify(sql)
            .UseDirectory("Snapshots")
            .ScrubLinesContaining("/* Caller;");
    }

    [Fact]
    public async System.Threading.Tasks.Task UpsertEntityInferredKeySqlServer()
    {
        var sqlProvider = new SqlServerGenerator();
        var parameters = new List<QueryParameter>();
        var entity = new UpsertStatus
        {
            Id = 1,
            Name = "test",
            Description = "test"
        };

        var builder = new UpsertEntityBuilder<UpsertStatus>(sqlProvider, parameters)
            .Values(entity)
            .Output(p => p.Id);

        var queryStatement = builder.BuildStatement();

        var sql = queryStatement!.Statement;

        await Verifier
            .Verify(sql)
            .UseDirectory("Snapshots")
            .ScrubLinesContaining("/* Caller;");
    }

    [Fact]
    public async System.Threading.Tasks.Task QueryBuilderUpsert()
    {
        var sqlProvider = new SqlServerGenerator();
        var queryParameters = new List<QueryParameter>();
        var queryBuilder = new QueryBuilder(sqlProvider, queryParameters);

        queryBuilder.Upsert()
            .Into("Status", "dbo")
            .Key("Id")
            .Value("Id", 1)
            .Value("Name", "test");

        var queryStatement = queryBuilder.BuildStatement();

        var sql = queryStatement!.Statement;

        await Verifier
            .Verify(sql)
            .UseDirectory("Snapshots")
            .ScrubLinesContaining("/* Caller;");
    }

}

[Table("Status", Schema = "dbo")]
public class UpsertStatus
{
    [Key]
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }
}
