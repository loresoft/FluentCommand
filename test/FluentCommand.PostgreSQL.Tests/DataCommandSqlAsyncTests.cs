using FluentCommand.Entities;
using FluentCommand.Extensions;

using Microsoft.Extensions.DependencyInjection;

namespace FluentCommand.PostgreSQL.Tests;

public class DataCommandSqlAsyncTests : DatabaseTestBase
{
    public DataCommandSqlAsyncTests(DatabaseFixture databaseFixture) : base(databaseFixture)
    {
    }

    [Fact]
    public async System.Threading.Tasks.Task SqlQuerySingleEntityAsync()
    {
        var session = Services.GetRequiredService<IDataSession>();
        session.Should().NotBeNull();

        string email = "kara.thrace@battlestar.com";
        string sql = "select * from \"User\" where \"EmailAddress\" = @EmailAddress";

        var user = await session.Sql(sql)
            .Parameter("@EmailAddress", email)
            .QuerySingleAsync(r => new User
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
            }, cancellationToken: Xunit.TestContext.Current.CancellationToken);

        user.Should().NotBeNull();
        user.EmailAddress.Should().Be(email);
    }

    [Fact]
    public async System.Threading.Tasks.Task SqlQuerySingleEntityFactoryAsync()
    {
        var session = Services.GetRequiredService<IDataSession>();
        session.Should().NotBeNull();

        string email = "kara.thrace@battlestar.com";
        string sql = "select * from \"User\" where \"EmailAddress\" = @EmailAddress";

        var user = await session.Sql(sql)
            .Parameter("@EmailAddress", email)
            .QuerySingleAsync<User>(cancellationToken: Xunit.TestContext.Current.CancellationToken);

        user.Should().NotBeNull();
        user.EmailAddress.Should().Be(email);
    }

    [Fact]
    public async System.Threading.Tasks.Task SqlQuerySingleEntityFactoryCacheAsync()
    {
        var session = Services.GetRequiredService<IDataSession>();
        session.Should().NotBeNull();

        string email = "kara.thrace@battlestar.com";
        string sql = "select * from \"User\" where \"EmailAddress\" = @EmailAddress";

        var user = await session.Sql(sql)
            .Parameter("@EmailAddress", email)
            .UseCache(TimeSpan.FromMinutes(5))
            .QuerySingleAsync<User>(cancellationToken: Xunit.TestContext.Current.CancellationToken);

        user.Should().NotBeNull();
        user.EmailAddress.Should().Be(email);

        var cachedUser = await session.Sql(sql)
            .Parameter("@EmailAddress", email)
            .UseCache(TimeSpan.FromMinutes(5))
            .QuerySingleAsync<User>(cancellationToken: Xunit.TestContext.Current.CancellationToken);

        cachedUser.Should().NotBeNull();
        cachedUser.EmailAddress.Should().Be(email);

    }

    [Fact]
    public async System.Threading.Tasks.Task SqlQuerySingleEntityDynamicAsync()
    {
        var session = Services.GetRequiredService<IDataSession>();
        session.Should().NotBeNull();

        string email = "kara.thrace@battlestar.com";
        string sql = "select * from \"User\" where \"EmailAddress\" = @EmailAddress";

        dynamic user = await session.Sql(sql)
            .Parameter("@EmailAddress", email)
            .QuerySingleAsync(cancellationToken: Xunit.TestContext.Current.CancellationToken);

        Assert.NotNull(user);
        Assert.Equal(user.EmailAddress, email);
    }

    [Fact]
    public async System.Threading.Tasks.Task SqlQueryEntityAsync()
    {
        var session = Services.GetRequiredService<IDataSession>();
        session.Should().NotBeNull();

        string email = "%@battlestar.com";
        string sql = "select * from \"User\" where \"EmailAddress\" like @EmailAddress";

        var users = await session.Sql(sql)
            .Parameter("@EmailAddress", email)
            .QueryAsync(r => new User
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
            }, cancellationToken: Xunit.TestContext.Current.CancellationToken);

        users.Should().NotBeNull();
        users.Should().NotBeEmpty();
    }

    [Fact]
    public async System.Threading.Tasks.Task SqlQueryEntityDynamicAsync()
    {
        var session = Services.GetRequiredService<IDataSession>();
        session.Should().NotBeNull();

        string email = "%@battlestar.com";
        string sql = "select * from \"User\" where \"EmailAddress\" like @EmailAddress";

        IEnumerable<dynamic> users = await session.Sql(sql)
            .Parameter("@EmailAddress", email)
            .QueryAsync(cancellationToken: Xunit.TestContext.Current.CancellationToken);

        users.Should().NotBeNull();
        users.Should().NotBeEmpty();
    }

    [Fact]
    public async System.Threading.Tasks.Task SqlQueryEntityDynamicCacheAsync()
    {
        var session = Services.GetRequiredService<IDataSession>();
        session.Should().NotBeNull();

        string email = "%@battlestar.com";
        string sql = "select * from \"User\" where \"EmailAddress\" like @EmailAddress";

        var users = await session
            .Sql(sql)
            .Parameter("@EmailAddress", email)
            .UseCache(TimeSpan.FromMinutes(5))
            .QueryAsync(cancellationToken: Xunit.TestContext.Current.CancellationToken);

        var userList = users.ToList();

        userList.Should().NotBeNull();
        userList.Should().NotBeEmpty();

        var cachedUsers = await session
            .Sql(sql)
            .Parameter("@EmailAddress", email)
            .UseCache(TimeSpan.FromMinutes(5))
            .QueryAsync(cancellationToken: Xunit.TestContext.Current.CancellationToken);

        var cachedList = cachedUsers.ToList();

        cachedList.Should().NotBeNull();
        cachedList.Should().NotBeEmpty();
    }

    [Fact]
    public async System.Threading.Tasks.Task SqlQueryEntityFactoryAsync()
    {
        var session = Services.GetRequiredService<IDataSession>();
        session.Should().NotBeNull();

        string email = "%@battlestar.com";
        string sql = "select * from \"User\" where \"EmailAddress\" like @EmailAddress";

        var users = await session.Sql(sql)
            .Parameter("@EmailAddress", email)
            .QueryAsync<User>(cancellationToken: Xunit.TestContext.Current.CancellationToken);

        users.Should().NotBeNull();
        users.Should().NotBeEmpty();
    }

    [Fact]
    public async System.Threading.Tasks.Task SqlQueryTableAsync()
    {
        var session = Services.GetRequiredService<IDataSession>();
        session.Should().NotBeNull();

        string email = "%@battlestar.com";
        string sql = "select * from \"User\" where \"EmailAddress\" like @EmailAddress";

        var users = await session.Sql(sql)
            .Parameter("@EmailAddress", email)
            .QueryTableAsync(Xunit.TestContext.Current.CancellationToken);

        users.Should().NotBeNull();
    }

    [Fact]
    public async System.Threading.Tasks.Task SqlQueryValueAsync()
    {
        var session = Services.GetRequiredService<IDataSession>();
        session.Should().NotBeNull();

        string email = "%@battlestar.com";
        string sql = "select Count(*) from \"User\" where \"EmailAddress\" like @EmailAddress";

        var count = await session.Sql(sql)
            .Parameter("@EmailAddress", email)
            .QueryValueAsync<int>(cancellationToken: Xunit.TestContext.Current.CancellationToken);

        count.Should().BeGreaterThan(0);
    }

    [Fact]
    public async System.Threading.Tasks.Task SqlReaderAsync()
    {
        var session = Services.GetRequiredService<IDataSession>();
        session.Should().NotBeNull();

        string email = "%@battlestar.com";
        string sql = "select * from \"User\" where \"EmailAddress\" like @EmailAddress";

        var users = new List<dynamic>();

        await session.Sql(sql)
            .Parameter("@EmailAddress", email)
            .ReadAsync((reader, token) =>
            {
                while (reader.Read())
                {
                    var user = DataReaderExtensions.DynamicFactory(reader);
                    users.Add(user);
                }

                return System.Threading.Tasks.Task.CompletedTask;
            }, cancellationToken: Xunit.TestContext.Current.CancellationToken);

        users.Should().NotBeNull();
        users.Should().NotBeEmpty();
    }

    [Fact]
    public async System.Threading.Tasks.Task SqlQueryMultipleAsync()
    {

        string email = "kara.thrace@battlestar.com";
        string sql = "select * from \"User\" where \"EmailAddress\" = @EmailAddress; " +
                     "select * from \"Role\"; " +
                     "select * from \"Priority\"; ";

        User user = null;
        List<Role> roles = null;
        List<Priority> priorities = null;

        using (var session = Services.GetRequiredService<IDataSession>())
        {
            session.Should().NotBeNull();
            await session.Sql(sql)
                .Parameter("@EmailAddress", email)
                .QueryMultipleAsync(async q =>
                {
                    user = await q.QuerySingleAsync<User>();
                    roles = (await q.QueryAsync<Role>()).ToList();
                    priorities = (await q.QueryAsync<Priority>()).ToList();
                }, Xunit.TestContext.Current.CancellationToken);
        }

        user.Should().NotBeNull();
        user.EmailAddress.Should().NotBeEmpty();

        roles.Should().NotBeNull();
        roles.Should().NotBeEmpty();

        priorities.Should().NotBeNull();
        priorities.Should().NotBeEmpty();

    }

}
