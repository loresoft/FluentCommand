using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Runtime.Caching;
using FluentAssertions;
using FluentCommand.Extensions;
using FluentCommand.Entities;
using Xunit;

namespace FluentCommand.SqlServer.Tests
{

    public class DataCommandTests
    {

        [Fact]
        public void SqlQuerySingleEntity()
        {

            string email = "kara.thrace@battlestar.com";
            string sql = "select * from [User] where EmailAddress = @EmailAddress";

            User user;
            using (var session = new DataSession("Tracker").Log(Console.WriteLine))
            {
                session.Should().NotBeNull();

                user = session.Sql(sql)
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
                        PasswordHash = r.GetString("PasswordHash"),
                        PasswordSalt = r.GetString("PasswordSalt"),
                        Comment = r.GetString("Comment"),
                        IsApproved = r.GetBoolean("IsApproved"),
                        LastLoginDate = r.GetDateTime("LastLoginDate"),
                        LastActivityDate = r.GetDateTime("LastActivityDate"),
                        LastPasswordChangeDate = r.GetDateTime("LastPasswordChangeDate"),
                        AvatarType = r.GetString("AvatarType"),
                    });
            }

            user.Should().NotBeNull();
            user.EmailAddress.Should().Be(email);
        }


        //[Fact]
        //public void SqlQuerySingleEntityFactory()
        //{
        //    var session = new DataSession("Tracker").Log(Console.WriteLine);
        //    session.Should().NotBeNull();

        //    string email = "kara.thrace@battlestar.com";
        //    string sql = "select * from [User] where EmailAddress = @EmailAddress";

        //    var user = session.Sql(sql)
        //        .Parameter("@EmailAddress", email)
        //        .QuerySingle<User>();

        //    user.Should().NotBeNull();
        //    user.EmailAddress.Should().Be(email);
        //}


        //[Fact]
        //public void SqlQuerySingleEntityFactoryCache()
        //{
        //    var session = new DataSession("Tracker").Log(Console.WriteLine);
        //    session.Should().NotBeNull();

        //    string email = "kara.thrace@battlestar.com";
        //    string sql = "select * from [User] where EmailAddress = @EmailAddress";

        //    var policy = new CacheItemPolicy { SlidingExpiration = TimeSpan.FromMinutes(5) };

        //    var user = session.Sql(sql)
        //        .Parameter("@EmailAddress", email)
        //        .UseCache(policy)
        //        .QuerySingle<User>();

        //    user.Should().NotBeNull();
        //    user.EmailAddress.Should().Be(email);

        //    var cachedUser = session.Sql(sql)
        //        .Parameter("@EmailAddress", email)
        //        .UseCache(policy)
        //        .QuerySingle<User>();

        //    cachedUser.Should().NotBeNull();
        //    cachedUser.EmailAddress.Should().Be(email);

        //}


        [Fact]
        public void SqlQuerySingleEntityDynamic()
        {
            var session = new DataSession("Tracker").Log(Console.WriteLine);
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
            var session = new DataSession("Tracker").Log(Console.WriteLine);
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
            var session = new DataSession("Tracker").Log(Console.WriteLine);
            session.Should().NotBeNull();

            string email = "%@battlestar.com";
            string sql = "select * from [User] where EmailAddress like @EmailAddress";

            IEnumerable<dynamic> users = session.Sql(sql)
                .Parameter("@EmailAddress", email)
                .Query();

            users.Should().NotBeNull();
            users.Should().NotBeEmpty();
        }


        //[Fact]
        //public void SqlQueryEntityDynamicCache()
        //{
        //    var session = new DataSession("Tracker").Log(Console.WriteLine);

        //    session.Should().NotBeNull();

        //    string email = "%@battlestar.com";
        //    string sql = "select * from [User] where EmailAddress like @EmailAddress";

        //    var policy = new CacheItemPolicy { SlidingExpiration = TimeSpan.FromMinutes(5) };

        //    var users = session
        //        .Sql(sql)
        //        .Parameter("@EmailAddress", email)
        //        .UseCache(policy)
        //        .Query()
        //        .ToList();

        //    users.Should().NotBeNull();
        //    users.Should().NotBeEmpty();

        //    var cachedUsers = session
        //        .Sql(sql)
        //        .Parameter("@EmailAddress", email)
        //        .UseCache(policy)
        //        .Query()
        //        .ToList();

        //    cachedUsers.Should().NotBeNull();
        //    cachedUsers.Should().NotBeEmpty();
        //}


        //[Fact]
        //public void SqlQueryEntityFactory()
        //{
        //    var session = new DataSession("Tracker").Log(Console.WriteLine);
        //    session.Log(Console.WriteLine);
        //    session.Should().NotBeNull();

        //    string email = "%@battlestar.com";
        //    string sql = "select * from [User] where EmailAddress like @EmailAddress";

        //    var users = session.Sql(sql)
        //        .Parameter("@EmailAddress", email)
        //        .Query<User>();

        //    users.Should().NotBeNull();
        //    users.Should().NotBeEmpty();
        //}


        [Fact]
        public void SqlQueryTable()
        {
            var session = new DataSession("Tracker").Log(Console.WriteLine);
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
            var session = new DataSession("Tracker").Log(Console.WriteLine);
            session.Should().NotBeNull();

            string email = "%@battlestar.com";
            string sql = "select Count(*) from [User] where EmailAddress like @EmailAddress";

            var count = session.Sql(sql)
                .Parameter("@EmailAddress", email)
                .QueryValue<int>();

            count.Should().BeGreaterThan(0);
        }


        //[Fact]
        //public void SqlReader()
        //{
        //    var session = new DataSession("Tracker").Log(Console.WriteLine);
        //    session.Should().NotBeNull();

        //    string email = "%@battlestar.com";
        //    string sql = "select * from [User] where EmailAddress like @EmailAddress";

        //    var users = new List<dynamic>();

        //    session.Sql(sql)
        //        .Parameter("@EmailAddress", email)
        //        .Read(reader =>
        //        {
        //            while (reader.Read())
        //            {
        //                var user = DataFactory.DynamicFactory(reader);
        //                users.Add(user);
        //            }
        //        });

        //    users.Should().NotBeNull();
        //    users.Should().NotBeEmpty();
        //}


        //[Fact]
        //public void SqlQueryMultiple()
        //{

        //    string email = "kara.thrace@battlestar.com";
        //    string sql = "select * from [User] where EmailAddress = @EmailAddress; " +
        //                 "select * from [Role]; " +
        //                 "select * from [Priority]; ";

        //    User user = null;
        //    List<Role> roles = null;
        //    List<Priority> priorities = null;

        //    using (var session = new DataSession("Tracker").Log(Console.WriteLine))
        //    {
        //        session.Should().NotBeNull();
        //        session.Sql(sql)
        //            .Parameter("@EmailAddress", email)
        //            .QueryMultiple(q =>
        //            {
        //                user = q.QuerySingle<User>();
        //                roles = q.Query<Role>().ToList();
        //                priorities = q.Query<Priority>().ToList();
        //            });
        //    }

        //    user.Should().NotBeNull();
        //    user.EmailAddress.Should().NotBeEmpty();

        //    roles.Should().NotBeNull();
        //    roles.Should().NotBeEmpty();

        //    priorities.Should().NotBeNull();
        //    priorities.Should().NotBeEmpty();

        //}
    }
}
