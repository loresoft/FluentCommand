using Bogus;

using FluentCommand.Bulk;
using FluentCommand.Entities;

using Microsoft.Extensions.DependencyInjection;

namespace FluentCommand.SqlServer.Tests;

public class DataBulkCopyTests : DatabaseTestBase
{
    public DataBulkCopyTests(ITestOutputHelper output, DatabaseFixture databaseFixture) : base(output, databaseFixture)
    {
    }

    [Fact]
    public void WriteServerAutoMap()
    {
        var generator = CreateGenerator();
        var users = generator.Generate(100);

        using var session = Services.GetRequiredService<IDataSession>();
        {
            session.Should().NotBeNull();

            session.BulkCopy("[User]")
                .AutoMap()
                .Ignore("RowVersion")
                .Ignore("Audits")
                .Ignore("AssignedTasks")
                .Ignore("CreatedTasks")
                .Ignore("Roles")
                .WriteToServer(users);
        }
    }


    [Fact]
    public void WriteServerManualMap()
    {
        var generator = CreateGenerator();
        var users = generator.Generate(100);

        using var session = Services.GetRequiredService<IDataSession>();
        session.Should().NotBeNull();

        session.BulkCopy("[User]")
            .Mapping("Id", "Id")
            .Mapping("EmailAddress", "EmailAddress")
            .Mapping("IsEmailAddressConfirmed", "IsEmailAddressConfirmed")
            .Mapping("FirstName", "FirstName")
            .Mapping("LastName", "LastName")
            .Mapping("DisplayName", "DisplayName")
            .Mapping("PasswordHash", "PasswordHash")
            .Mapping("ResetHash", "ResetHash")
            .Mapping("InviteHash", "InviteHash")
            .Mapping("AccessFailedCount", "AccessFailedCount")
            .Mapping("LockoutEnabled", "LockoutEnabled")
            .Mapping("LockoutEnd", "LockoutEnd")
            .Mapping("LastLogin", "LastLogin")
            .Mapping("IsDeleted", "IsDeleted")
            .Mapping("Created", "Created")
            .Mapping("CreatedBy", "CreatedBy")
            .Mapping("Updated", "Updated")
            .Mapping("UpdatedBy", "UpdatedBy")
            .WriteToServer(users);
    }


    [Fact]
    public void WriteServerStrongMap()
    {
        var generator = CreateGenerator();
        var users = generator.Generate(100);

        using var session = Services.GetRequiredService<IDataSession>();
        session.Should().NotBeNull();

        session.BulkCopy("[User]")
            .Mapping<User>(map =>
            {
                map.Mapping(u => u.Id, "Id");
                map.Mapping(u => u.EmailAddress, "EmailAddress");
            })
            .Mapping("IsEmailAddressConfirmed", "IsEmailAddressConfirmed")
            .Mapping("FirstName", "FirstName")
            .Mapping("LastName", "LastName")
            .Mapping("DisplayName", "DisplayName")
            .Mapping("PasswordHash", "PasswordHash")
            .Mapping("ResetHash", "ResetHash")
            .Mapping("InviteHash", "InviteHash")
            .Mapping("AccessFailedCount", "AccessFailedCount")
            .Mapping("LockoutEnabled", "LockoutEnabled")
            .Mapping("LockoutEnd", "LockoutEnd")
            .Mapping("LastLogin", "LastLogin")
            .Mapping("IsDeleted", "IsDeleted")
            .Mapping("Created", "Created")
            .Mapping("CreatedBy", "CreatedBy")
            .Mapping("Updated", "Updated")
            .Mapping("UpdatedBy", "UpdatedBy")
            .WriteToServer(users);
    }


    private static Faker<User> CreateGenerator()
    {
        Faker<User> fakerUser = new Faker<User>()
            .RuleFor(u => u.Id, f => Guid.NewGuid())
            .RuleFor(u => u.FirstName, (f, u) => f.Name.FirstName())
            .RuleFor(u => u.LastName, (f, u) => f.Name.LastName())
            .RuleFor(u => u.DisplayName, (f, u) => $"{u.FirstName} {u.LastName}")
            .RuleFor(u => u.EmailAddress, (f, u) => f.Internet.Email(u.FirstName, u.LastName, uniqueSuffix: $"+{DateTime.Now.Ticks}"))
            .RuleFor(u => u.IsEmailAddressConfirmed, f => true)
            .RuleFor(u => u.PasswordHash, f => f.Random.AlphaNumeric(10))
            .RuleFor(u => u.ResetHash, f => f.Random.AlphaNumeric(10))
            .RuleFor(u => u.InviteHash, f => f.Random.AlphaNumeric(10))
            .RuleFor(u => u.Created, f => f.Date.Past())
            .RuleFor(u => u.CreatedBy, "UnitTest")
            .RuleFor(u => u.Updated, DateTime.Now)
            .RuleFor(u => u.UpdatedBy, "UnitTest");

        return fakerUser;
    }
}
