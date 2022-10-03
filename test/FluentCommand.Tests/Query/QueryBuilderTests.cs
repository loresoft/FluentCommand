using FluentCommand.Entities;
using FluentCommand.Query;
using FluentCommand.Query.Generators;

using VerifyXunit;

using Xunit;

namespace FluentCommand.Tests.Query;

[UsesVerify]
public class QueryBuilderTests
{
    [Fact]
    public async System.Threading.Tasks.Task QueryBuilderSelect()
    {
        var sqlProvider = new SqlServerGenerator();
        var queryBuilder = new QueryBuilder(sqlProvider);

        queryBuilder.Select<Status>()
            .Column("Id")
            .Column(p => p.Name)
            .Column("Description")
            .Where(p => p.IsActive, true)
            .WhereOr(b => b
                .WhereOr(o => o
                    .Where("Name", "Test", FilterOperators.Contains)
                    .Where(p => p.Description, "Test", FilterOperators.Contains)
                )
            )
            .OrderBy(p => p.DisplayOrder, SortDirections.Descending)
            .OrderBy("Name");

        var queryStatement = queryBuilder.BuildStatement();

        var sql = queryStatement.Statement;

        await Verifier.Verify(sql).UseDirectory("Snapshots");
    }
}

