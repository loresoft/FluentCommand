using FluentCommand.Entities;
using FluentCommand.Extensions;

using Microsoft.Data.SqlClient;
using Microsoft.Extensions.DependencyInjection;

namespace FluentCommand.SqlServer.Tests;

public class DataCommandSqlTests : DatabaseTestBase
{
    public DataCommandSqlTests(ITestOutputHelper output, DatabaseFixture databaseFixture) : base(output, databaseFixture)
    {
    }

    [Fact]
    public void SqlQuerySingleEntity()
    {
        var session = Services.GetRequiredService<IDataSession>();
        session.Should().NotBeNull();

        string email = "kara.thrace@battlestar.com";
        string sql = "select * from [User] where EmailAddress = @EmailAddress";

        var user = session.Sql(sql)
            .Parameter("@EmailAddress", email)
            .QuerySingle(r => new User
            {
                Id = r.GetGuid("Id"),
                EmailAddress = r.GetString("EmailAddress"),
                IsEmailAddressConfirmed = r.GetBoolean("IsEmailAddressConfirmed"),
                DisplayName = r.GetString("DisplayName"),
                PasswordHash = r.GetString("PasswordHash"),
                ResetHash = r.GetString("ResetHash"),
                InviteHash = r.GetString("InviteHash"),
                AccessFailedCount = r.GetInt32("AccessFailedCount"),
                LockoutEnabled = r.GetBoolean("LockoutEnabled"),
                LockoutEnd = r.GetDateTimeOffsetNull("LockoutEnd"),
                LastLogin = r.GetDateTimeOffsetNull("LastLogin"),
                IsDeleted = r.GetBoolean("IsDeleted"),
                Created = r.GetDateTimeOffset("Created"),
                CreatedBy = r.GetString("CreatedBy"),
                Updated = r.GetDateTimeOffset("Updated"),
                UpdatedBy = r.GetString("UpdatedBy"),
                RowVersion = r.GetBytes("RowVersion"),
            });

        user.Should().NotBeNull();
        user.EmailAddress.Should().Be(email);
    }

    [Fact]
    public void SqlQuerySingleEntityFactory()
    {
        var session = Services.GetRequiredService<IDataSession>();
        session.Should().NotBeNull();

        string email = "kara.thrace@battlestar.com";
        string sql = "select * from [User] where EmailAddress = @EmailAddress";

        var user = session.Sql(sql)
            .Parameter("@EmailAddress", email)
            .QuerySingle<User>();

        user.Should().NotBeNull();
        user.EmailAddress.Should().Be(email);
    }

    [Fact]
    public void SqlQuerySingleEntityFactoryCache()
    {
        var session = Services.GetRequiredService<IDataSession>();
        session.Should().NotBeNull();

        string email = "kara.thrace@battlestar.com";
        string sql = "select * from [User] where EmailAddress = @EmailAddress";

        var user = session.Sql(sql)
            .Parameter("@EmailAddress", email)
            .UseCache(TimeSpan.FromMinutes(5))
            .QuerySingle<User>();

        user.Should().NotBeNull();
        user.EmailAddress.Should().Be(email);

        var cachedUser = session.Sql(sql)
            .Parameter("@EmailAddress", email)
            .UseCache(TimeSpan.FromMinutes(5))
            .QuerySingle<User>();

        cachedUser.Should().NotBeNull();
        cachedUser.EmailAddress.Should().Be(email);

    }

    [Fact]
    public void SqlQuerySingleEntityDynamic()
    {
        var session = Services.GetRequiredService<IDataSession>();
        session.Should().NotBeNull();

        string email = "kara.thrace@battlestar.com";
        string sql = "select * from [User] where EmailAddress = @EmailAddress";

        dynamic user = session.Sql(sql)
            .Parameter("@EmailAddress", email)
            .QuerySingle();

        Assert.NotNull(user);
        Assert.Equal(user.EmailAddress, email);
    }

    [Fact]
    public void SqlQueryEntity()
    {
        var session = Services.GetRequiredService<IDataSession>();
        session.Should().NotBeNull();

        string email = "%@battlestar.com";
        string sql = "select * from [User] where EmailAddress like @EmailAddress";

        var users = session.Sql(sql)
            .Parameter("@EmailAddress", email)
            .Query(r => new User
            {
                Id = r.GetGuid("Id"),
                EmailAddress = r.GetString("EmailAddress"),
                IsEmailAddressConfirmed = r.GetBoolean("IsEmailAddressConfirmed"),
                DisplayName = r.GetString("DisplayName"),
                PasswordHash = r.GetString("PasswordHash"),
                ResetHash = r.GetString("ResetHash"),
                InviteHash = r.GetString("InviteHash"),
                AccessFailedCount = r.GetInt32("AccessFailedCount"),
                LockoutEnabled = r.GetBoolean("LockoutEnabled"),
                LockoutEnd = r.GetDateTimeOffsetNull("LockoutEnd"),
                LastLogin = r.GetDateTimeOffsetNull("LastLogin"),
                IsDeleted = r.GetBoolean("IsDeleted"),
                Created = r.GetDateTimeOffset("Created"),
                CreatedBy = r.GetString("CreatedBy"),
                Updated = r.GetDateTimeOffset("Updated"),
                UpdatedBy = r.GetString("UpdatedBy"),
                RowVersion = r.GetBytes("RowVersion"),
            });

        users.Should().NotBeNull();
        users.Should().NotBeEmpty();
    }

    [Fact]
    public void SqlQueryEntityError()
    {
        var session = Services.GetRequiredService<IDataSession>();
        session.Should().NotBeNull();

        string email = "%@battlestar.com";
        string sql = "select * from [Blah].[User] where EmailAddress like @EmailAddress";

        Action action = () =>
        {
            var users = session.Sql(sql)
                .Parameter("@EmailAddress", email)
                .Query(r => new User
                {
                    Id = r.GetGuid("Id"),
                    EmailAddress = r.GetString("EmailAddress"),
                    IsEmailAddressConfirmed = r.GetBoolean("IsEmailAddressConfirmed"),
                    DisplayName = r.GetString("DisplayName"),
                    PasswordHash = r.GetString("PasswordHash"),
                    ResetHash = r.GetString("ResetHash"),
                    InviteHash = r.GetString("InviteHash"),
                    AccessFailedCount = r.GetInt32("AccessFailedCount"),
                    LockoutEnabled = r.GetBoolean("LockoutEnabled"),
                    LockoutEnd = r.GetDateTimeOffsetNull("LockoutEnd"),
                    LastLogin = r.GetDateTimeOffsetNull("LastLogin"),
                    IsDeleted = r.GetBoolean("IsDeleted"),
                    Created = r.GetDateTimeOffset("Created"),
                    CreatedBy = r.GetString("CreatedBy"),
                    Updated = r.GetDateTimeOffset("Updated"),
                    UpdatedBy = r.GetString("UpdatedBy"),
                    RowVersion = r.GetBytes("RowVersion"),
                });
        };

        action.Should().Throw<SqlException>();
    }

    [Fact]
    public void SqlQueryEntityDynamic()
    {
        var session = Services.GetRequiredService<IDataSession>();
        session.Should().NotBeNull();

        string email = "%@battlestar.com";
        string sql = "select * from [User] where EmailAddress like @EmailAddress";

        IEnumerable<dynamic> users = session.Sql(sql)
            .Parameter("@EmailAddress", email)
            .Query();

        users.Should().NotBeNull();
        users.Should().NotBeEmpty();
    }

    [Fact]
    public void SqlQueryEntityFactory()
    {
        var session = Services.GetRequiredService<IDataSession>();
        session.Should().NotBeNull();

        string email = "%@battlestar.com";
        string sql = "select * from [User] where EmailAddress like @EmailAddress";

        var users = session.Sql(sql)
            .Parameter("@EmailAddress", email)
            .Query<User>();

        users.Should().NotBeNull();
        users.Should().NotBeEmpty();
    }

    [Fact]
    public void SqlQueryTable()
    {
        var session = Services.GetRequiredService<IDataSession>();
        session.Should().NotBeNull();

        string email = "%@battlestar.com";
        string sql = "select * from [User] where EmailAddress like @EmailAddress";

        var users = session.Sql(sql)
            .Parameter("@EmailAddress", email)
            .QueryTable();

        users.Should().NotBeNull();
    }

    [Fact]
    public void SqlQueryValue()
    {
        var session = Services.GetRequiredService<IDataSession>();
        session.Should().NotBeNull();

        string email = "%@battlestar.com";
        string sql = "select Count(*) from [User] where EmailAddress like @EmailAddress";

        var count = session.Sql(sql)
            .Parameter("@EmailAddress", email)
            .QueryValue<int>();

        count.Should().BeGreaterThan(0);
    }

    [Fact]
    public void SqlReader()
    {
        var session = Services.GetRequiredService<IDataSession>();
        session.Should().NotBeNull();

        string email = "%@battlestar.com";
        string sql = "select * from [User] where EmailAddress like @EmailAddress";

        var users = new List<dynamic>();

        session.Sql(sql)
            .Parameter("@EmailAddress", email)
            .Read(reader =>
            {
                while (reader.Read())
                {
                    var user = DataReaderExtensions.DynamicFactory(reader);
                    users.Add(user);
                }
            });

        users.Should().NotBeNull();
        users.Should().NotBeEmpty();
    }

    [Fact]
    public void SqlQueryMultiple()
    {

        string email = "kara.thrace@battlestar.com";
        string sql = "select * from [User] where EmailAddress = @EmailAddress; " +
                     "select * from [Role]; " +
                     "select * from [Priority]; ";

        User user = null;
        List<Role> roles = null;
        List<Priority> priorities = null;

        using var session = Services.GetRequiredService<IDataSession>();
        session.Should().NotBeNull();

        session.Sql(sql)
            .Parameter("@EmailAddress", email)
            .QueryMultiple(q =>
            {
                user = q.QuerySingle<User>();
                roles = q.Query<Role>().ToList();
                priorities = q.Query<Priority>().ToList();
            });


        user.Should().NotBeNull();
        user.EmailAddress.Should().NotBeEmpty();

        roles.Should().NotBeNull();
        roles.Should().NotBeEmpty();

        priorities.Should().NotBeNull();
        priorities.Should().NotBeEmpty();

    }

    [Fact]
    public void SqlQueryEntityTimeOut()
    {
        var session = Services.GetRequiredService<IDataSession>();
        session.Should().NotBeNull();

        string email = "%@battlestar.com";
        string sql = "select * from [User] where EmailAddress like @EmailAddress";

        var dataCommand = session.Sql(sql);

        dataCommand
            .CommandTimeout(TimeSpan.FromMinutes(5))
            .Parameter("@EmailAddress", email);

        dataCommand.Command.CommandTimeout.Should().Be(300);
    }

}
