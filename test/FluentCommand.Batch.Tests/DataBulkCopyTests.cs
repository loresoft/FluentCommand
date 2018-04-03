using System;
using System.Linq;
using FluentAssertions;
using FluentCommand.Bulk;
using FluentCommand.Entities;
using FluentCommand.Extensions;
using Xunit;
using Xunit.Abstractions;

namespace FluentCommand.Batch.Tests
{
    
    public class DataBulkCopyTests : DataTestBase
    {
        public DataBulkCopyTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public void WriteServerAutoMap()
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
                    PasswordHash = r.GetStringNull("PasswordHash"),
                    PasswordSalt = r.GetStringNull("PasswordSalt"),
                    Comment = r.GetStringNull("Comment"),
                    IsApproved = r.GetBoolean("IsApproved"),
                    LastLoginDate = r.GetDateTimeNull("LastLoginDate"),
                    LastActivityDate = r.GetDateTime("LastActivityDate"),
                    LastPasswordChangeDate = r.GetDateTimeNull("LastPasswordChangeDate"),
                    AvatarType = r.GetStringNull("AvatarType"),
                })
                .ToList();

            users.Should().NotBeNull();

            long ticks = DateTime.Now.Ticks;

            foreach (var u in users)
                u.EmailAddress = u.EmailAddress.Replace("@battlestar", "@u" + ticks);

            session.BulkCopy("[User]")
                .AutoMap()
                .Ignore("AvatarType")
                .Ignore("Audits")
                .Ignore("AssignedTasks")
                .Ignore("CreatedTasks")
                .Ignore("Roles")
                .WriteToServer(users);
        }

        
        [Fact]
        public void WriteServer()
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
                    PasswordHash = r.GetStringNull("PasswordHash"),
                    PasswordSalt = r.GetStringNull("PasswordSalt"),
                    Comment = r.GetStringNull("Comment"),
                    IsApproved = r.GetBoolean("IsApproved"),
                    LastLoginDate = r.GetDateTimeNull("LastLoginDate"),
                    LastActivityDate = r.GetDateTime("LastActivityDate"),
                    LastPasswordChangeDate = r.GetDateTimeNull("LastPasswordChangeDate"),
                    AvatarType = r.GetStringNull("AvatarType"),
                })
                .ToList();

            users.Should().NotBeNull();
            users.Should().NotBeEmpty();

            long ticks = DateTime.Now.Ticks;

            foreach (var u in users)
                u.EmailAddress = u.EmailAddress.Replace("@battlestar", "@u" + ticks);

            session.BulkCopy("[User]")
                .Mapping("EmailAddress", "EmailAddress")
                .Mapping("FirstName", "FirstName")
                .Mapping("LastName", "LastName")
                .Mapping("CreatedDate", "CreatedDate")
                .Mapping("ModifiedDate", "ModifiedDate")
                .Mapping("PasswordHash", "PasswordHash")
                .Mapping("PasswordSalt", "PasswordSalt")
                .Mapping("Comment", "Comment")
                .Mapping("IsApproved", "IsApproved")
                .Mapping("LastLoginDate", "LastLoginDate")
                .Mapping("LastActivityDate", "LastActivityDate")
                .Mapping("LastPasswordChangeDate", "LastPasswordChangeDate")
                .Mapping("AvatarType", "AvatarType")
                .WriteToServer(users);
        }

        
        [Fact]
        public void WriteServerMapping()
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
                    PasswordHash = r.GetStringNull("PasswordHash"),
                    PasswordSalt = r.GetStringNull("PasswordSalt"),
                    Comment = r.GetStringNull("Comment"),
                    IsApproved = r.GetBoolean("IsApproved"),
                    LastLoginDate = r.GetDateTimeNull("LastLoginDate"),
                    LastActivityDate = r.GetDateTime("LastActivityDate"),
                    LastPasswordChangeDate = r.GetDateTimeNull("LastPasswordChangeDate"),
                    AvatarType = r.GetStringNull("AvatarType"),
                })
                .ToList();

            users.Should().NotBeNull();
            users.Should().NotBeEmpty();

            long ticks = DateTime.Now.Ticks;

            foreach (var u in users)
            {
                u.EmailAddress = u.EmailAddress.Replace("@battlestar", "@u" + ticks);
                u.CreatedDate = DateTime.Now;
            }

            session.BulkCopy("[User]")
                .Mapping<User>(map =>
                {
                    map.Mapping(u => u.EmailAddress, "EmailAddress");
                    map.Mapping(u => u.FirstName, "FirstName");
                })
                .Mapping("LastName", "LastName")
                .Mapping("CreatedDate", "CreatedDate")
                .Mapping("ModifiedDate", "ModifiedDate")
                .Mapping("PasswordHash", "PasswordHash")
                .Mapping("PasswordSalt", "PasswordSalt")
                .Mapping("Comment", "Comment")
                .Mapping("IsApproved", "IsApproved")
                .Mapping("LastLoginDate", "LastLoginDate")
                .Mapping("LastActivityDate", "LastActivityDate")
                .Mapping("LastPasswordChangeDate", "LastPasswordChangeDate")
                .Mapping("AvatarType", "AvatarType")
                .WriteToServer(users);
        }

    }
}