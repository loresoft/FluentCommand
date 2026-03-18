using System.Data;

using FluentCommand.Entities;

using Microsoft.Extensions.DependencyInjection;

namespace FluentCommand.SqlServer.Tests;

public class ParameterStructuredTests : DatabaseTestBase
{
    public ParameterStructuredTests(DatabaseFixture databaseFixture) : base(databaseFixture)
    {
    }

    [Fact]
    public void ParameterStructuredWithEntityQueryReturnsMatchingUsers()
    {
        var ids = new List<IdItem>
        {
            new() { Id = Guid.Parse("83507c95-0744-e811-bd87-f8633fc30ac7") }, // William Adama
            new() { Id = Guid.Parse("38da04bb-0744-e811-bd87-f8633fc30ac7") }, // Kara Thrace
        };

        long total = -1;

        using var session = Services.GetRequiredService<IDataSession>();
        session.Should().NotBeNull();

        var users = session
            .StoredProcedure("[dbo].[UserListByIds]")
            .ParameterStructured("@IdList", ids)
            .Parameter<long>(p => p
                .Name("@Total")
                .Type(DbType.Int64)
                .Output(v => total = v)
                .Direction(ParameterDirection.Output)
            )
            .Query<User>()
            .ToList();

        users.Should().HaveCount(2);
        total.Should().Be(2);

        users.Select(u => u.EmailAddress)
            .Should()
            .Contain("william.adama@battlestar.com")
            .And
            .Contain("kara.thrace@battlestar.com");
    }

    [Fact]
    public void ParameterStructuredWithEntityExecutesMerge()
    {
        var email = $"structured.test.{DateTime.UtcNow.Ticks}@test.com";

        var imports = new List<UserImport>
        {
            new()
            {
                EmailAddress = email,
                DisplayName = "Structured Test",
                FirstName = "Structured",
                LastName = "Test",
            }
        };

        using var session = Services.GetRequiredService<IDataSession>();
        session.Should().NotBeNull();

        var result = session
            .StoredProcedure("[dbo].[UserImportStructured]")
            .ParameterStructured("@UserTable", imports)
            .Execute();

        result.Should().Be(-1);

        // verify the user was inserted
        var user = session
            .Sql("SELECT * FROM [dbo].[User] WHERE [EmailAddress] = @EmailAddress")
            .Parameter("@EmailAddress", email)
            .QuerySingle<User>();

        user.Should().NotBeNull();
        user.DisplayName.Should().Be("Structured Test");
    }

    [Fact]
    public void ParameterStructuredWithDataTableQueryReturnsMatchingUsers()
    {
        var dataTable = new DataTable();
        dataTable.Columns.Add("Id", typeof(Guid));
        dataTable.Rows.Add(Guid.Parse("490312a6-0744-e811-bd87-f8633fc30ac7")); // Laura Roslin
        dataTable.Rows.Add(Guid.Parse("589d67c6-0744-e811-bd87-f8633fc30ac7")); // Lee Adama

        long total = -1;

        using var session = Services.GetRequiredService<IDataSession>();
        session.Should().NotBeNull();

        var users = session
            .StoredProcedure("[dbo].[UserListByIds]")
            .ParameterStructured("@IdList", dataTable)
            .Parameter<long>(p => p
                .Name("@Total")
                .Type(DbType.Int64)
                .Output(v => total = v)
                .Direction(ParameterDirection.Output)
            )
            .Query<User>()
            .ToList();

        users.Should().HaveCount(2);
        total.Should().Be(2);

        users.Select(u => u.EmailAddress)
            .Should()
            .Contain("laura.roslin@battlestar.com")
            .And
            .Contain("lee.adama@battlestar.com");
    }

    [Fact]
    public void ParameterStructuredWithEmptyDataTableReturnsNoResults()
    {
        var dataTable = new DataTable();
        dataTable.Columns.Add("Id", typeof(Guid));

        long total = -1;

        using var session = Services.GetRequiredService<IDataSession>();
        session.Should().NotBeNull();

        var users = session
            .StoredProcedure("[dbo].[UserListByIds]")
            .ParameterStructured("@IdList", dataTable)
            .Parameter<long>(p => p
                .Name("@Total")
                .Type(DbType.Int64)
                .Output(v => total = v)
                .Direction(ParameterDirection.Output)
            )
            .Query<User>()
            .ToList();

        users.Should().BeEmpty();
        total.Should().Be(0);
    }
}
