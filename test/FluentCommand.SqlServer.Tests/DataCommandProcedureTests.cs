using System.Data;

using Bogus;

using FluentCommand.Entities;

using Microsoft.Extensions.DependencyInjection;

namespace FluentCommand.SqlServer.Tests;

public class DataCommandProcedureTests : DatabaseTestBase
{
    public DataCommandProcedureTests(ITestOutputHelper output, DatabaseFixture databaseFixture) : base(output, databaseFixture)
    {
    }

    [Fact]
    public void ProcedureQueryParameterOut()
    {
        long total = -1;

        var email = "%@battlestar.com";

        using var session = Services.GetRequiredService<IDataSession>();
        session.Should().NotBeNull();

        var users = session
            .StoredProcedure("[dbo].[UserListByEmailAddress]")
            .Parameter("@EmailAddress", email)
            .Parameter("@Offset", 0)
            .Parameter("@Size", 10)
            .Parameter<long>(parameter => parameter
                .Name("@Total")
                .Type(DbType.Int64)
                .Output(v => total = v)
                .Direction(ParameterDirection.Output)
            )
            .Query<User>()
            .ToList();


        users.Should().NotBeEmpty();
        total.Should().Be(6);
    }

    [Fact]
    public void ProcedureExecuteUpsert()
    {
        int errorCode = -1;

        var userId = Guid.NewGuid();
        var username = "test." + DateTime.Now.Ticks;
        var email = username + "@email.com";

        using var session = Services.GetRequiredService<IDataSession>();
        session.Should().NotBeNull();

        var user = session
            .StoredProcedure("[dbo].[UserUpsert]")
            .Parameter("@Id", userId)
            .Parameter("@EmailAddress", email)
            .Parameter("@IsEmailAddressConfirmed", true)
            .Parameter("@DisplayName", "Unit Test")
            .Parameter("@PasswordHash", "T@est" + DateTime.Now.Ticks)
            .Parameter<string>("@ResetHash", null)
            .Parameter<string>("@InviteHash", null)
            .Parameter("@AccessFailedCount", 0)
            .Parameter("@LockoutEnabled", false)
            .Parameter("@IsDeleted", false)
            .Return<int>(p => errorCode = p)
            .QuerySingle<User>();

        errorCode.Should().Be(0);

        user.Should().NotBeNull();
        user.Id.Should().Be(userId);
        user.Created.Should().NotBe(default);
        user.Updated.Should().NotBe(default);
    }

    [Fact]
    public void ProcedureExecuteReturn()
    {
        int result = -1;
        long total = -1;

        var email = "william.adama@battlestar.com";

        using var session = Services.GetRequiredService<IDataSession>();
        session.Should().NotBeNull();

        result = session
            .StoredProcedure("[dbo].[UserCountByEmailAddress]")
            .Parameter("@EmailAddress", email)
            .Return<long>(p => total = p)
            .Execute();

        result.Should().Be(-1);
        total.Should().Be(1);
    }

    [Fact]
    public void ProcedureExecuteTransaction()
    {
        using var session = Services.GetRequiredService<IDataSession>();
        session.Should().NotBeNull();

        using var transaction = session.BeginTransaction(IsolationLevel.Unspecified);
        transaction.Should().NotBeNull();

        int errorCode = -1;

        var userId = Guid.NewGuid();
        var username = "test." + DateTime.Now.Ticks;
        var email = username + "@email.com";

        var user = session.StoredProcedure("[dbo].[UserUpsert]")
            .Parameter("@Id", userId)
            .Parameter("@EmailAddress", email)
            .Parameter("@IsEmailAddressConfirmed", true)
            .Parameter("@DisplayName", "Unit Test")
            .Parameter("@PasswordHash", "T@est" + DateTime.Now.Ticks)
            .Parameter<string>("@ResetHash", null)
            .Parameter<string>("@InviteHash", null)
            .Parameter("@AccessFailedCount", 0)
            .Parameter("@LockoutEnabled", false)
            .Parameter("@IsDeleted", false)
            .Return<int>(p => errorCode = p)
            .QuerySingle<User>();

        errorCode.Should().Be(0);

        user.Should().NotBeNull();
        user.Id.Should().Be(userId);
        user.Created.Should().NotBe(default(DateTimeOffset));
        user.Updated.Should().NotBe(default(DateTimeOffset));

        transaction.Commit();
    }

    [Fact]
    public void ProcedureExecuteMerge()
    {
        var fakerUser = new Faker<UserImport>()
            .RuleFor(u => u.FirstName, (f, u) => f.Name.FirstName())
            .RuleFor(u => u.LastName, (f, u) => f.Name.LastName())
            .RuleFor(u => u.DisplayName, (f, u) => $"{u.FirstName} {u.LastName}")
            .RuleFor(u => u.EmailAddress, (f, u) => f.Internet.Email(u.FirstName, u.LastName, uniqueSuffix: $"+{DateTime.Now.Ticks}"));

        var users = fakerUser.Generate(100);

        using var session = Services.GetRequiredService<IDataSession>();
        session.Should().NotBeNull();

        var result = session
            .StoredProcedure("[dbo].[ImportUsers]")
            .SqlParameter("@userTable", users)
            .Execute();

        result.Should().Be(-1);
    }

}
