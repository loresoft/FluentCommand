using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using FluentAssertions;
using FluentCommand.Extensions;
using FluentCommand.Entities;
using Microsoft.Extensions.Configuration;
using Xunit;
using Xunit.Abstractions;

namespace FluentCommand.SqlServer.Tests
{

    public class DataCommandTests : DataTestBase
    {

        public DataCommandTests(ITestOutputHelper output) : base(output)
        {

        }


        [Fact]
        public void SqlQuerySingleEntity()
        {
            var session = GetConfiguration("Tracker").CreateSession();
            session.Should().NotBeNull();

            string email = "kara.thrace@battlestar.com";
            string sql = "select * from [User] where EmailAddress = @EmailAddress";

            var user = session.Sql(sql)
                .Parameter("@EmailAddress", email)
                .QuerySingle(r => new User
                {
                    Id = r.GetInt32("Id"),
                    EmailAddress = r.GetString("EmailAddress"),
                    FirstName = r.GetString("FirstName"),
                    LastName = r.GetString("LastName"),
                    Avatar = (Byte[])r.GetValue("Avatar"),
                    CreatedDate = r.GetDateTime("CreatedDate"),
                    ModifiedDate = r.GetDateTime("ModifiedDate"),
                    RowVersion = (Byte[])r.GetValue("RowVersion"),
                    PasswordHash = r.GetString("PasswordHash"),
                    PasswordSalt = r.GetString("PasswordSalt"),
                    Comment = r.GetString("Comment"),
                    IsApproved = r.GetBoolean("IsApproved"),
                    LastLoginDate = r.GetDateTime("LastLoginDate"),
                    LastActivityDate = r.GetDateTime("LastActivityDate"),
                    LastPasswordChangeDate = r.GetDateTime("LastPasswordChangeDate"),
                    AvatarType = r.GetString("AvatarType"),
                });

            user.Should().NotBeNull();
            user.EmailAddress.Should().Be(email);
        }


        [Fact]
        public void SqlQuerySingleEntityFactory()
        {
            var session = GetConfiguration("Tracker").CreateSession();
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
            var session = GetConfiguration("Tracker").CreateSession();
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
            var session = GetConfiguration("Tracker").CreateSession();
            session.Should().NotBeNull();

            string email = "kara.thrace@battlestar.com";
            string sql = "select * from [User] where EmailAddress = @EmailAddress";

            dynamic user = session.Sql(sql)
                .Parameter("@EmailAddress", email)
                .QuerySingle();

            Assert.NotNull(user);
            Assert.Equal<string>(user.EmailAddress, email);
        }


        [Fact]
        public void SqlQueryEntity()
        {
            var session = GetConfiguration("Tracker").CreateSession();
            session.Should().NotBeNull();

            string email = "%@battlestar.com";
            string sql = "select * from [User] where EmailAddress like @EmailAddress";

            var users = session.Sql(sql)
                .Parameter("@EmailAddress", email)
                .Query(r => new User
                {
                    Id = r.GetInt32("Id"),
                    EmailAddress = r.GetString("EmailAddress"),
                    FirstName = r.GetString("FirstName"),
                    LastName = r.GetString("LastName"),
                    Avatar = (Byte[])r.GetValue("Avatar"),
                    CreatedDate = r.GetDateTime("CreatedDate"),
                    ModifiedDate = r.GetDateTime("ModifiedDate"),
                    RowVersion = (Byte[])r.GetValue("RowVersion"),
                    PasswordHash = r.GetString("PasswordHash"),
                    PasswordSalt = r.GetString("PasswordSalt"),
                    Comment = r.GetString("Comment"),
                    IsApproved = r.GetBoolean("IsApproved"),
                    LastLoginDate = r.GetDateTime("LastLoginDate"),
                    LastActivityDate = r.GetDateTime("LastActivityDate"),
                    LastPasswordChangeDate = r.GetDateTime("LastPasswordChangeDate"),
                    AvatarType = r.GetString("AvatarType"),
                });

            users.Should().NotBeNull();
            users.Should().NotBeEmpty();
        }


        [Fact]
        public void SqlQueryEntityDynamic()
        {
            var session = GetConfiguration("Tracker").CreateSession();
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
        public void SqlQueryEntityDynamicCache()
        {
            var session = GetConfiguration("Tracker").CreateSession();
            session.Should().NotBeNull();

            string email = "%@battlestar.com";
            string sql = "select * from [User] where EmailAddress like @EmailAddress";

            var users = session
                .Sql(sql)
                .Parameter("@EmailAddress", email)
                .UseCache(TimeSpan.FromMinutes(5))
                .Query()
                .ToList();

            users.Should().NotBeNull();
            users.Should().NotBeEmpty();

            var cachedUsers = session
                .Sql(sql)
                .Parameter("@EmailAddress", email)
                .UseCache(TimeSpan.FromMinutes(5))
                .Query()
                .ToList();

            cachedUsers.Should().NotBeNull();
            cachedUsers.Should().NotBeEmpty();
        }


        [Fact]
        public void SqlQueryEntityFactory()
        {
            var session = GetConfiguration("Tracker").CreateSession();
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
            var session = GetConfiguration("Tracker").CreateSession();
            session.Should().NotBeNull();

            string email = "%@battlestar.com";
            string sql = "select * from [User] where EmailAddress like @EmailAddress";

            var users = session.Sql(sql)
                .Parameter("@EmailAddress", email)
                .QueryTable();

            users.Should().NotBeNull();
        }


        [Fact]
        public void ProcedureExecuteOut()
        {

            Guid userId = Guid.Empty;
            int errorCode = -1;

            var username = "test." + DateTime.Now.Ticks;
            var email = username + "@email.com";

            int result;
            using (var session = GetConfiguration("AspNet").CreateSession())
            {
                session.Should().NotBeNull();
                result = session.StoredProcedure("[dbo].[aspnet_Membership_CreateUser]")
                    .Parameter("@ApplicationName", "/")
                    .Parameter("@UserName", username)
                    .Parameter("@Password", "T@est" + DateTime.Now.Ticks)
                    .Parameter("@Email", email)
                    .Parameter("@PasswordSalt", "test salt")
                    .Parameter<string>("@PasswordQuestion", null)
                    .Parameter<string>("@PasswordAnswer", null)
                    .Parameter("@IsApproved", true)
                    .Parameter("@CurrentTimeUtc", DateTime.UtcNow)
                    .Parameter("@UniqueEmail", 1)
                    .Parameter("@PasswordFormat", 1)
                    .ParameterOut<Guid>("@UserId", p => userId = p)
                    .Return<int>(p => errorCode = p)
                    .Execute();
            }

            result.Should().BeGreaterOrEqualTo(1);
            userId.Should().NotBe(Guid.Empty);
            errorCode.Should().Be(0);
        }


        [Fact]
        public void ProcedureExecuteOutDuplicate()
        {

            Guid? userId = null;
            int errorCode = -1;

            var username = "test." + DateTime.Now.Ticks;
            var email = username + "@email.com";
            int result;

            using (var session = GetConfiguration("AspNet").CreateSession())
            {
                session.Should().NotBeNull();

                result = session.StoredProcedure("[dbo].[aspnet_Membership_CreateUser]")
                    .Parameter("@ApplicationName", "/")
                    .Parameter("@UserName", "paul.welter")
                    .Parameter("@Password", "T@est" + DateTime.Now.Ticks)
                    .Parameter("@Email", email)
                    .Parameter("@PasswordSalt", "test salt")
                    .Parameter<string>("@PasswordQuestion", null)
                    .Parameter<string>("@PasswordAnswer", null)
                    .Parameter("@IsApproved", true)
                    .Parameter("@CurrentTimeUtc", DateTime.UtcNow)
                    .Parameter("@UniqueEmail", 1)
                    .Parameter("@PasswordFormat", 1)
                    .Parameter<Guid?>(parameter => parameter
                        .Name("@UserId")
                        .Type(DbType.Guid)
                        .Output(p => userId = p)
                        .Direction(ParameterDirection.Output)
                    )
                    .Return<int>(p => errorCode = p)
                    .Execute();

                // Duplicate
                result = session.StoredProcedure("[dbo].[aspnet_Membership_CreateUser]")
                    .Parameter("@ApplicationName", "/")
                    .Parameter("@UserName", "paul.welter")
                    .Parameter("@Password", "T@est" + DateTime.Now.Ticks)
                    .Parameter("@Email", email)
                    .Parameter("@PasswordSalt", "test salt")
                    .Parameter<string>("@PasswordQuestion", null)
                    .Parameter<string>("@PasswordAnswer", null)
                    .Parameter("@IsApproved", true)
                    .Parameter("@CurrentTimeUtc", DateTime.UtcNow)
                    .Parameter("@UniqueEmail", 1)
                    .Parameter("@PasswordFormat", 1)
                    .Parameter<Guid?>(parameter => parameter
                        .Name("@UserId")
                        .Type(DbType.Guid)
                        .Output(p => userId = p)
                        .Direction(ParameterDirection.Output)
                    )
                    .Return<int>(p => errorCode = p)
                    .Execute();
            }

            result.Should().Be(-1);
            errorCode.Should().BeGreaterThan(0);

        }


        [Fact]
        public void ProcedureQueryDynamicOut()
        {
            int totalRecords = -1;
            int result = 0;

            Guid userId = Guid.Empty;
            int errorCode = -1;

            var username = "test." + DateTime.Now.Ticks;
            var email = username + "@email.com";

            List<dynamic> results;
            using (var session = GetConfiguration("AspNet").CreateSession())
            {
                session.Should().NotBeNull();

                result = session.StoredProcedure("[dbo].[aspnet_Membership_CreateUser]")
                    .Parameter("@ApplicationName", "/")
                    .Parameter("@UserName", username)
                    .Parameter("@Password", "T@est" + DateTime.Now.Ticks)
                    .Parameter("@Email", email)
                    .Parameter("@PasswordSalt", "test salt")
                    .Parameter<string>("@PasswordQuestion", null)
                    .Parameter<string>("@PasswordAnswer", null)
                    .Parameter("@IsApproved", true)
                    .Parameter("@CurrentTimeUtc", DateTime.UtcNow)
                    .Parameter("@UniqueEmail", 1)
                    .Parameter("@PasswordFormat", 1)
                    .ParameterOut<Guid>("@UserId", p => userId = p)
                    .Return<int>(p => errorCode = p)
                    .Execute();

                results = session.StoredProcedure("[dbo].[aspnet_Membership_FindUsersByEmail]")
                    .Parameter("@ApplicationName", "/")
                    .Parameter("@EmailToMatch", "%@email.com")
                    .Parameter("@PageIndex", 0)
                    .Parameter("@PageSize", 10)
                    .Return<int>(p => totalRecords = p)
                    .Query()
                    .ToList();
            }

            results.Should().NotBeNull();
            results.Count.Should().BeGreaterThan(0);
            totalRecords.Should().BeGreaterThan(0);
        }


        [Fact]
        public void SqlQueryValue()
        {
            var session = GetConfiguration("Tracker").CreateSession();
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
            var session = GetConfiguration("Tracker").CreateSession();
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
                        var user = DataFactory.DynamicFactory(reader);
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

            using (var session = GetConfiguration("Tracker").CreateSession())
            {
                session.Should().NotBeNull();
                session.Sql(sql)
                    .Parameter("@EmailAddress", email)
                    .QueryMultiple(q =>
                    {
                        user = q.QuerySingle<User>();
                        roles = q.Query<Role>().ToList();
                        priorities = q.Query<Priority>().ToList();
                    });
            }

            user.Should().NotBeNull();
            user.EmailAddress.Should().NotBeEmpty();

            roles.Should().NotBeNull();
            roles.Should().NotBeEmpty();

            priorities.Should().NotBeNull();
            priorities.Should().NotBeEmpty();

        }


        [Fact]
        public void ProcedureExecuteTransaction()
        {
            var session = GetConfiguration("AspNet").CreateSession();
            session.Should().NotBeNull();

            var transaction = session.BeginTransaction(IsolationLevel.Unspecified);
            transaction.Should().NotBeNull();

            Guid userId = Guid.Empty;
            int errorCode = -1;

            var username = "test." + DateTime.Now.Ticks;
            var email = username + "@email.com";

            var result = session.StoredProcedure("[dbo].[aspnet_Membership_CreateUser]")
                .Parameter("@ApplicationName", "/")
                .Parameter("@UserName", username)
                .Parameter("@Password", "T@est" + DateTime.Now.Ticks)
                .Parameter("@Email", email)
                .Parameter("@PasswordSalt", "test salt")
                .Parameter<string>("@PasswordQuestion", null)
                .Parameter<string>("@PasswordAnswer", null)
                .Parameter("@IsApproved", true)
                .Parameter("@CurrentTimeUtc", DateTime.UtcNow)
                .Parameter("@UniqueEmail", 1)
                .Parameter("@PasswordFormat", 1)
                .ParameterOut<Guid>("@UserId", p => userId = p)
                .Return<int>(p => errorCode = p)
                .Execute();

            result.Should().BeGreaterOrEqualTo(1);
            userId.Should().NotBe(Guid.Empty);
            errorCode.Should().Be(0);

            transaction.Commit();
        }
    }
}
