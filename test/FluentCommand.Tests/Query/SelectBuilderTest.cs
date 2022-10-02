using System.Collections.Generic;

using FluentCommand.Entities;
using FluentCommand.Query;
using FluentCommand.Query.Generators;

using VerifyXunit;

using Xunit;

namespace FluentCommand.Tests.Query;

[UsesVerify]
public class SelectBuilderTest
{
    [Fact]
    public async System.Threading.Tasks.Task SelectEntityNestedWhereBuilder()
    {
        var sqlProvider = new SqlServerGenerator();
        var parameters = new List<QueryParameter>();

        var builder = new SelectEntityBuilder<Status>(sqlProvider, parameters, LogicalOperators.And)
            .Column("Id")
            .Column(p => p.Name)
            .Column("Description")
            .Where(p => p.IsActive, true)
            .Where(b => b
                .Or(o => o
                    .Where("Name", "Test", FilterOperators.Contains)
                    .Where(p => p.Description, "Test", FilterOperators.Contains)
                )
            )
            .OrderBy(p => p.DisplayOrder, SortDirections.Descending)
            .OrderBy("Name");


        var queryStatement = builder.BuildStatement();

        var sql = queryStatement.Statement;

        await Verifier.Verify(sql).UseDirectory("Snapshots");
    }

    [Fact]
    public async System.Threading.Tasks.Task SelectEntityWhereTagBuilder()
    {
        var sqlProvider = new SqlServerGenerator();
        var parameters = new List<QueryParameter>();

        var builder = new SelectEntityBuilder<Status>(sqlProvider, parameters, LogicalOperators.And)
            .Tag("Select Entity Where Tag Builder Query")
            .Column("Id")
            .Column(p => p.Name)
            .Column("Description")
            .Where(p => p.IsActive, true)
            .OrderBy(p => p.DisplayOrder, SortDirections.Descending)
            .OrderBy("Name");


        var queryStatement = builder.BuildStatement();

        var sql = queryStatement.Statement;

        await Verifier.Verify(sql).UseDirectory("Snapshots");
    }

    [Fact]
    public async System.Threading.Tasks.Task SelectEntityWhereLimitBuilder()
    {
        var sqlProvider = new SqlServerGenerator();
        var parameters = new List<QueryParameter>();

        var builder = new SelectEntityBuilder<Status>(sqlProvider, parameters, LogicalOperators.And)
            .Tag()
            .Column("Id")
            .Column(p => p.Name)
            .Column("Description")
            .Where(p => p.IsActive, true)
            .OrderBy(p => p.DisplayOrder, SortDirections.Descending)
            .OrderBy("Name")
            .Limit(50, 25);


        var queryStatement = builder.BuildStatement();

        var sql = queryStatement.Statement;

        await Verifier.Verify(sql).UseDirectory("Snapshots");
    }
}
