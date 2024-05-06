using FluentCommand.Entities;
using FluentCommand.Query;
using FluentCommand.Query.Generators;

namespace FluentCommand.Tests.Query;

public class UpdateBuilderTest
{
    [Fact]
    public async System.Threading.Tasks.Task UpdateEntityValueWithOutput()
    {
        var sqlProvider = new SqlServerGenerator();
        var parameters = new List<QueryParameter>();

        var builder = new UpdateEntityBuilder<Status>(sqlProvider, parameters)
            .Value(p => p.Name, "test")
            .Value(p => p.Description, "test")
            .Value(p => p.DisplayOrder, 10)
            .Value(p => p.Created, System.DateTimeOffset.UtcNow)
            .Value(p => p.Updated, System.DateTimeOffset.UtcNow)
            .Output(p => p.Id)
            .Where(p => p.Id, 1);

        var queryStatement = builder.BuildStatement();

        var sql = queryStatement.Statement;

        await Verifier
            .Verify(sql)
            .UseDirectory("Snapshots")
            .ScrubLinesContaining("/* Caller;");
    }

    [Fact]
    public async System.Threading.Tasks.Task UpdateEntityValueWithJoin()
    {
        var sqlProvider = new SqlServerGenerator();
        var parameters = new List<QueryParameter>();

        var builder = new UpdateEntityBuilder<Entities.Task>(sqlProvider, parameters)
            .Value(p => p.Description, "test")
            .Value(p => p.Updated, System.DateTimeOffset.UtcNow)
            .Output(p => p.Id)
            .From(tableAlias: "t")
            .Join<Priority>(p => p
                .Left(p => p.PriorityId, "t")
                .Right(p => p.Id, "p")
            )
            .Where<Priority, int>(p => p.Id, 4, "p", FilterOperators.GreaterThanOrEqual);

        var queryStatement = builder.BuildStatement();

        var sql = queryStatement.Statement;

        await Verifier
            .Verify(sql)
            .UseDirectory("Snapshots")
            .ScrubLinesContaining("/* Caller;");
    }
}
