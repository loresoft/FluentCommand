using System.ComponentModel.DataAnnotations.Schema;

using FluentCommand.Entities;
using FluentCommand.Query;
using FluentCommand.Query.Generators;

namespace FluentCommand.Tests.Query;

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

    [Fact]
    public async System.Threading.Tasks.Task SelectEntityTemporalBuilder()
    {
        var sqlProvider = new SqlServerGenerator();
        var parameters = new List<QueryParameter>();

        var builder = new SelectEntityBuilder<Status>(sqlProvider, parameters, LogicalOperators.And)
            .Tag()
            .Column(p => p.Id)
            .Column(p => p.Name)
            .Temporal(t => t.AsOf(DateTime.Now))
            .Where(p => p.Id, 1)
            .OrderBy(p => p.Name);

        var queryStatement = builder.BuildStatement();

        var sql = queryStatement.Statement;

        await Verifier.Verify(sql).UseDirectory("Snapshots");
    }

    [Fact]
    public async System.Threading.Tasks.Task SelectEntityChangeTableBuilder()
    {
        var sqlProvider = new SqlServerGenerator();
        var parameters = new List<QueryParameter>();

        var builder = new SelectEntityBuilder<User>(sqlProvider, parameters)
            .Columns("t")
            .ChangeTable(t => t
                .From<User>("c")
                .LastVersion(0)
            )
            .Join<User>(join => join
                .Left(p => p.Id, "c")
                .Right(p => p.Id, "t")
            );

        var queryStatement = builder.BuildStatement();

        var sql = queryStatement.Statement;

        await Verifier.Verify(sql).UseDirectory("Snapshots");
    }

    [Fact]
    public async System.Threading.Tasks.Task SelectEntityWhereIn()
    {
        var sqlProvider = new SqlServerGenerator();
        var parameters = new List<QueryParameter>();

        var builder = new SelectEntityBuilder<Status>(sqlProvider, parameters, LogicalOperators.And)
            .Tag()
            .Column(p => p.Id)
            .Column(p => p.Name)
            .WhereIn(p => p.Id, new[] { 1, 2 })
            .OrderBy(p => p.Name);

        var queryStatement = builder.BuildStatement();

        var sql = queryStatement.Statement;

        await Verifier.Verify(sql).UseDirectory("Snapshots");
    }
    [Fact]
    public async System.Threading.Tasks.Task SelectEntityWhereInDouble()
    {
        var sqlProvider = new SqlServerGenerator();
        var parameters = new List<QueryParameter>();

        var builder = new SelectEntityBuilder<Status>(sqlProvider, parameters, LogicalOperators.And)
            .Tag()
            .Column(p => p.Id)
            .Column(p => p.Name)
            .WhereIn(p => p.Id, new[] { 1, 2 })
            .WhereIn(p => p.Name, new[] { "jim", "bob" })
            .OrderBy(p => p.Name);

        var queryStatement = builder.BuildStatement();

        var sql = queryStatement.Statement;

        await Verifier.Verify(sql).UseDirectory("Snapshots");
    }
    [Fact]
    public async System.Threading.Tasks.Task SelectEntityWhereInIf()
    {
        var sqlProvider = new SqlServerGenerator();
        var parameters = new List<QueryParameter>();

        var builder = new SelectEntityBuilder<Status>(sqlProvider, parameters, LogicalOperators.And)
            .Tag()
            .Column(p => p.Id)
            .Column(p => p.Name)
            .WhereInIf(p => p.Id, new[] { 1, 2 }, (c, v) => true)
            .OrderBy(p => p.Name);

        var queryStatement = builder.BuildStatement();

        var sql = queryStatement.Statement;

        await Verifier.Verify(sql).UseDirectory("Snapshots");
    }

    [Fact]
    public async System.Threading.Tasks.Task SelectEntityJoinBuilder()
    {
        var sqlProvider = new SqlServerGenerator();
        var parameters = new List<QueryParameter>();

        var builder = new SelectEntityBuilder<Entities.Task>(sqlProvider, parameters, LogicalOperators.And)
            .Tag()
            .Column(p => p.Id, "t")
            .Column(p => p.Description, "t")
            .Column(p => p.DueDate, "t")
            .Column<User>(p => p.DisplayName, "u")
            .Column<User>(p => p.EmailAddress, "u", "Email")
            .Column<Status>(p => p.Name, "s", "Status")
            .From(tableAlias: "t")
            .Join<Status>(j => j
                .Left(p => p.StatusId, "t")
                .Right(p => p.Id, "s")
            )
            .Join<User>(j => j
                .Left(p => p.AssignedId, "t")
                .Right(p => p.Id, "u")
                .Type(JoinTypes.Left)
            )
            .Where(p => p.PriorityId, 1, "t")
            .Where<User, string>(p => p.EmailAddress, "@email.com", "u", FilterOperators.NotEqual)
            .OrderBy(p => p.PriorityId, "t");

        var queryStatement = builder.BuildStatement();

        var sql = queryStatement.Statement;

        await Verifier.Verify(sql).UseDirectory("Snapshots");
    }

    [Fact]
    public async System.Threading.Tasks.Task SelectEntityFilterBuilder()
    {
        var sqlProvider = new SqlServerGenerator();
        var parameters = new List<QueryParameter>();

        var builder = new SelectEntityBuilder<User>(sqlProvider, parameters)
            .Columns("t", p => p.Name != nameof(User.PasswordHash));

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
