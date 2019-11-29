using System;
using System.Linq;
using DataGenerator;
using FluentAssertions;
using FluentCommand.Bulk;
using FluentCommand.Entities;
using FluentCommand.Extensions;
using FluentCommand.SqlServer.Tests;
using Xunit;
using Xunit.Abstractions;

namespace FluentCommand.Batch.Tests
{
    [Collection(BatchDatabaseCollection.CollectionName)]
    public class DataBulkCopyTests : DatabaseTestBase
    {
        public DataBulkCopyTests(ITestOutputHelper output, DatabaseFixture databaseFixture) : base(output, databaseFixture)
        {
        }

        [Fact]
        public void WriteServerAutoMap()
        {
            var generator = Generator.Create(c => c
                .ExcludeName("xunit")
                .Profile<UserProfile>()
            );

            var users = generator.List<User>(100);

            using (var session = GetConfiguration().CreateSession())
            {
                session.Should().NotBeNull();

                session.BulkCopy("[User]")
                    .AutoMap()
                    .Ignore("RowVersion")
                    .Ignore("FirstName")
                    .Ignore("LastName")
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
            var generator = Generator.Create(c => c
                .ExcludeName("xunit")
                .Profile<UserProfile>()
            );

            var users = generator.List<User>(100);

            var session = GetConfiguration().CreateSession();
            session.Should().NotBeNull();

            session.BulkCopy("[User]")
                .Mapping("Id", "Id")
                .Mapping("EmailAddress", "EmailAddress")
                .Mapping("IsEmailAddressConfirmed", "IsEmailAddressConfirmed")
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
            var generator = Generator.Create(c => c
                .ExcludeName("xunit")
                .Profile<UserProfile>()
            );

            var users = generator.List<User>(100);


            var session = GetConfiguration().CreateSession();
            session.Should().NotBeNull();

            session.BulkCopy("[User]")
                .Mapping<User>(map =>
                {
                    map.Mapping(u => u.Id, "Id");
                    map.Mapping(u => u.EmailAddress, "EmailAddress");
                })
                .Mapping("IsEmailAddressConfirmed", "IsEmailAddressConfirmed")
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

    }
}