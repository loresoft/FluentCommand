using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

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
            .WhereOr(b => b
                .Where("Name", "Test", FilterOperators.Contains)
                .Where(p => p.Description, "Test", FilterOperators.Contains)
                .WhereAnd(o => o
                    .Where(p => p.IsActive, false)
                    .Where(p => p.DisplayOrder, 0, FilterOperators.GreaterThan)
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

    [Fact]
    public async System.Threading.Tasks.Task SelectEntityAliasWhereTagBuilder()
    {
        var sqlProvider = new SqlServerGenerator();
        var parameters = new List<QueryParameter>();

        var builder = new SelectEntityBuilder<EntityAlias>(sqlProvider, parameters, LogicalOperators.And)
            .Tag()
            .Column(p => p.Id)
            .Column(p => p.Name)
            .Where(p => p.Id, 1)
            .OrderBy(p => p.Name);


        var queryStatement = builder.BuildStatement();

        var sql = queryStatement.Statement;

        await Verifier.Verify(sql).UseDirectory("Snapshots");
    }

    [Fact]
    public async System.Threading.Tasks.Task SelectColumnsAliasWhereTagBuilder()
    {
        var sqlProvider = new SqlServerGenerator();
        var parameters = new List<QueryParameter>();

        var builder = new SelectEntityBuilder<EntityAlias>(sqlProvider, parameters, LogicalOperators.And)
            .Tag()
            .Columns(new string[] { "EntityId", "Name" })
            .Where(p => p.Id, 1)
            .OrderBy(p => p.Name);


        var queryStatement = builder.BuildStatement();

        var sql = queryStatement.Statement;

        await Verifier.Verify(sql).UseDirectory("Snapshots");
    }

    private class EntityAlias
    {
        [Column("EntityId")]
        public int Id { get; set; }

        public string Name { get; set; }
    }
}
