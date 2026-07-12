using FluentCommand.Entities;
using FluentCommand.Query;
using FluentCommand.Query.Generators;

namespace FluentCommand.Tests.Query;

public class QueryBuilderTests
{
    [Fact]
    public void SqlServerWhereExpressionBuildsNegatedStringFilters()
    {
        var sqlProvider = new SqlServerGenerator();

        sqlProvider.WhereExpression(new WhereExpression("Name", "@p0", FilterOperator: FilterOperators.NotStartsWith))
            .Should().Be("[Name] NOT LIKE @p0 + '%'");
        sqlProvider.WhereExpression(new WhereExpression("Name", "@p0", FilterOperator: FilterOperators.NotEndsWith))
            .Should().Be("[Name] NOT LIKE '%' + @p0");
        sqlProvider.WhereExpression(new WhereExpression("Name", "@p0", FilterOperator: FilterOperators.NotContains))
            .Should().Be("[Name] NOT LIKE '%' + @p0 + '%'");
    }

    [Fact]
    public void PostgreSqlWhereExpressionBuildsNegatedStringFilters()
    {
        var sqlProvider = new PostgreSqlGenerator();

        sqlProvider.WhereExpression(new WhereExpression("Name", "@p0", FilterOperator: FilterOperators.NotStartsWith))
            .Should().Be("\"Name\" NOT LIKE @p0 || '%'");
        sqlProvider.WhereExpression(new WhereExpression("Name", "@p0", FilterOperator: FilterOperators.NotEndsWith))
            .Should().Be("\"Name\" NOT LIKE '%' || @p0");
        sqlProvider.WhereExpression(new WhereExpression("Name", "@p0", FilterOperator: FilterOperators.NotContains))
            .Should().Be("\"Name\" NOT LIKE '%' || @p0 || '%'");
    }

    [Fact]
    public void SqliteWhereExpressionBuildsNegatedStringFilters()
    {
        var sqlProvider = new SqliteGenerator();

        sqlProvider.WhereExpression(new WhereExpression("Name", "@p0", FilterOperator: FilterOperators.NotStartsWith))
            .Should().Be("\"Name\" NOT LIKE @p0 || '%'");
        sqlProvider.WhereExpression(new WhereExpression("Name", "@p0", FilterOperator: FilterOperators.NotEndsWith))
            .Should().Be("\"Name\" NOT LIKE '%' || @p0");
        sqlProvider.WhereExpression(new WhereExpression("Name", "@p0", FilterOperator: FilterOperators.NotContains))
            .Should().Be("\"Name\" NOT LIKE '%' || @p0 || '%'");
    }

    [Fact]
    public async System.Threading.Tasks.Task QueryBuilderSelect()
    {
        var sqlProvider = new SqlServerGenerator();
        var queryParameters = new List<QueryParameter>();
        var queryBuilder = new QueryBuilder(sqlProvider, queryParameters);

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

        var sql = queryStatement!.Statement;

        await Verifier
            .Verify(sql)
            .UseDirectory("Snapshots")
            .ScrubLinesContaining("/* Caller;");
    }

    [Fact]
    public void QueryFilterBuildsNestedWhereExpression()
    {
        var sqlProvider = new SqlServerGenerator();
        var queryParameters = new List<QueryParameter>();
        var builder = new WhereBuilder(sqlProvider, queryParameters);

        builder.Where(new QueryFilter
        {
            Logic = LogicalOperators.And,
            Filters = new List<QueryFilter>
            {
                new()
                {
                    Name = "Name",
                    Value = "Test",
                    Operator = FilterOperators.Contains,
                },
                new()
                {
                    Logic = LogicalOperators.Or,
                    Filters = new List<QueryFilter>
                    {
                        new()
                        {
                            Name = "IsActive",
                            Value = true,
                        },
                        new()
                        {
                            Name = "Id",
                            Value = new[] { 1, 2 },
                            Operator = FilterOperators.In,
                        },
                    },
                },
            },
        });

        var queryStatement = builder.BuildStatement();

        queryStatement!.Statement.Should().Be("(([Name] LIKE '%' + @p0000 + '%' AND ([IsActive] = @p0001 OR [Id] IN (@p0002,@p0003))))");
        queryParameters.Select(p => p.Value).Should().Equal("Test", true, 1, 2);
        queryParameters.Select(p => p.Type).Should().Equal(typeof(string), typeof(bool), typeof(int), typeof(int));
    }

    [Fact]
    public void QueryFilterBuildsParameterlessWhereExpression()
    {
        var sqlProvider = new SqlServerGenerator();
        var queryParameters = new List<QueryParameter>();
        var builder = new WhereBuilder(sqlProvider, queryParameters);

        builder.Where(new QueryFilter
        {
            Name = "Deleted",
            Operator = FilterOperators.IsNull,
        });

        var queryStatement = builder.BuildStatement();

        queryStatement!.Statement.Should().Be("([Deleted] IS NULL)");
    }
}

