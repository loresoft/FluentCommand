using FluentCommand.Entities;
using FluentCommand.Extensions;

using Microsoft.Data.SqlClient;
using Microsoft.Extensions.DependencyInjection;

using XUnit.Hosting.Logging;

namespace FluentCommand.SqlServer.Tests;

public class DataCommandSqlAsyncTests : DatabaseTestBase
{
    public DataCommandSqlAsyncTests(DatabaseFixture databaseFixture) : base(databaseFixture)
    {
    }

    [Fact]
    public async System.Threading.Tasks.Task SqlQuerySingleEntityAsync()
    {
        await using var session = Services.GetRequiredService<IDataSession>();
        session.Should().NotBeNull();

        string email = "kara.thrace@battlestar.com";
        string sql = "select * from [User] where EmailAddress = @EmailAddress";

        var user = await session
            .Sql(sql)
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
            }, cancellationToken: TestCancellation);

        user.Should().NotBeNull();
        user.EmailAddress.Should().Be(email);
    }

    [Fact]
    public async System.Threading.Tasks.Task SqlQuerySingleEntityFactoryAsync()
    {
        await using var session = Services.GetRequiredService<IDataSession>();
        session.Should().NotBeNull();

        string email = "kara.thrace@battlestar.com";
        string sql = "select * from [User] where EmailAddress = @EmailAddress";

        var user = await session
            .Sql(sql)
            .Parameter("@EmailAddress", email)
            .QuerySingleAsync<User>(cancellationToken: TestCancellation);

        user.Should().NotBeNull();
        user.EmailAddress.Should().Be(email);
    }

    [Fact]
    public async System.Threading.Tasks.Task SqlQuerySingleEntityFactoryCacheAsync()
    {
        await using var session = Services.GetRequiredService<IDataSession>();
        session.Should().NotBeNull();

        string email = "kara.thrace@battlestar.com";
        string sql = "select * from [User] where EmailAddress = @EmailAddress";

        var user = await session
            .Sql(sql)
            .Parameter("@EmailAddress", email)
            .UseCache(TimeSpan.FromMinutes(5))
            .QuerySingleAsync<User>(cancellationToken: TestCancellation);

        user.Should().NotBeNull();
        user.EmailAddress.Should().Be(email);

        var cachedUser = await session
            .Sql(sql)
            .Parameter("@EmailAddress", email)
            .UseCache(TimeSpan.FromMinutes(5))
            .QuerySingleAsync<User>(cancellationToken: TestCancellation);

        cachedUser.Should().NotBeNull();
        cachedUser.EmailAddress.Should().Be(email);

        var memoryLoggerProvider = Services.GetRequiredService<MemoryLoggerProvider>();

        // check logs for cache hit
        var logs = memoryLoggerProvider.Logs();
        var hasHit = logs.Any(l => l.Message.Contains("Cache Hit;"));

        hasHit.Should().BeTrue();
    }

    [Fact]
    public async System.Threading.Tasks.Task SqlQuerySingleEntityDynamicAsync()
    {
        await using var session = Services.GetRequiredService<IDataSession>();
        session.Should().NotBeNull();

        string email = "kara.thrace@battlestar.com";
        string sql = "select * from [User] where EmailAddress = @EmailAddress";

        dynamic user = await session.Sql(sql)
            .Parameter("@EmailAddress", email)
            .QuerySingleAsync(cancellationToken: TestCancellation);

        Assert.NotNull(user);
        Assert.Equal(user.EmailAddress, email);
    }

    [Fact]
    public async System.Threading.Tasks.Task SqlQueryEntityAsync()
    {
        await using var session = Services.GetRequiredService<IDataSession>();
        session.Should().NotBeNull();

        string email = "%@battlestar.com";
        string sql = "select * from [User] where EmailAddress like @EmailAddress";

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
            }, cancellationToken: TestCancellation);

        users.Should().NotBeNull();
        users.Should().NotBeEmpty();
    }

    [Fact]
    public async System.Threading.Tasks.Task SqlQueryEntityErrorAsync()
    {
        await using var session = Services.GetRequiredService<IDataSession>();
        session.Should().NotBeNull();

        string email = "%@battlestar.com";
        string sql = "select * from [Blah].[User] where EmailAddress like @EmailAddress";

        Func<System.Threading.Tasks.Task> action = async () =>
        {
            var users = await session
                .Sql(sql)
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
                }, cancellationToken: TestCancellation);
        };

        await action.Should().ThrowAsync<SqlException>();
    }

    [Fact]
    public async System.Threading.Tasks.Task SqlQueryEntityDynamicAsync()
    {
        await using var session = Services.GetRequiredService<IDataSession>();
        session.Should().NotBeNull();

        string email = "%@battlestar.com";
        string sql = "select * from [User] where EmailAddress like @EmailAddress";

        IEnumerable<dynamic> users = await session
            .Sql(sql)
            .Parameter("@EmailAddress", email)
            .QueryAsync(cancellationToken: TestCancellation);

        users.Should().NotBeNull();
        users.Should().NotBeEmpty();
    }


    [Fact]
    public async System.Threading.Tasks.Task SqlQueryEntityFactoryAsync()
    {
        await using var session = Services.GetRequiredService<IDataSession>();
        session.Should().NotBeNull();

        string email = "%@battlestar.com";
        string sql = "select * from [User] where EmailAddress like @EmailAddress";

        var users = await session.Sql(sql)
            .Parameter("@EmailAddress", email)
            .QueryAsync<User>(cancellationToken: TestCancellation);

        users.Should().NotBeNull();
        users.Should().NotBeEmpty();
    }

    [Fact]
    public async System.Threading.Tasks.Task SqlQueryTableAsync()
    {
        await using var session = Services.GetRequiredService<IDataSession>();
        session.Should().NotBeNull();

        string email = "%@battlestar.com";
        string sql = "select * from [User] where EmailAddress like @EmailAddress";

        var users = await session.Sql(sql)
            .Parameter("@EmailAddress", email)
            .QueryTableAsync(cancellationToken: TestCancellation);

        users.Should().NotBeNull();
    }

    [Fact]
    public async System.Threading.Tasks.Task SqlQueryValueAsync()
    {
        await using var session = Services.GetRequiredService<IDataSession>();
        session.Should().NotBeNull();

        string email = "%@battlestar.com";
        string sql = "select Count(*) from [User] where EmailAddress like @EmailAddress";

        var count = await session.Sql(sql)
            .Parameter("@EmailAddress", email)
            .QueryValueAsync<int>(cancellationToken: TestCancellation);

        count.Should().BeGreaterThan(0);
    }

    [Fact]
    public async System.Threading.Tasks.Task SqlReaderAsync()
    {
        await using var session = Services.GetRequiredService<IDataSession>();
        session.Should().NotBeNull();

        string email = "%@battlestar.com";
        string sql = "select * from [User] where EmailAddress like @EmailAddress";

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
            }, cancellationToken: TestCancellation);

        users.Should().NotBeNull();
        users.Should().NotBeEmpty();
    }

    [Fact]
    public async System.Threading.Tasks.Task SqlQueryMultipleAsync()
    {

        string email = "kara.thrace@battlestar.com";
        string sql = "select * from [User] where EmailAddress = @EmailAddress; " +
                     "select * from [Role]; " +
                     "select * from [Priority]; ";

        User user = null;
        List<Role> roles = null;
        List<Priority> priorities = null;

        await using var session = Services.GetRequiredService<IDataSession>();
        session.Should().NotBeNull();

        await session.Sql(sql)
            .Parameter("@EmailAddress", email)
            .QueryMultipleAsync(async q =>
            {
                user = await q.QuerySingleAsync<User>(cancellationToken: TestCancellation);
                roles = (await q.QueryAsync<Role>(cancellationToken: TestCancellation)).ToList();
                priorities = (await q.QueryAsync<Priority>(cancellationToken: TestCancellation)).ToList();
            }, cancellationToken: TestCancellation);

        user.Should().NotBeNull();
        user.EmailAddress.Should().NotBeEmpty();

        roles.Should().NotBeNull();
        roles.Should().NotBeEmpty();

        priorities.Should().NotBeNull();
        priorities.Should().NotBeEmpty();

    }

}
