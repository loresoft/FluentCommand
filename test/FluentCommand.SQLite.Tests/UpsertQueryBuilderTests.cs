using FluentCommand.Entities;
using FluentCommand.Extensions;

using Microsoft.Extensions.DependencyInjection;

namespace FluentCommand.SQLite.Tests;

public class UpsertQueryBuilderTests : DatabaseTestBase
{
    public UpsertQueryBuilderTests(DatabaseFixture databaseFixture) : base(databaseFixture)
    {
    }

    [Fact]
    public void UpsertInsertsThenUpdatesStatus()
    {
        using var session = Services.GetRequiredService<IDataSession>();
        session.Should().NotBeNull();

        const int id = 99001;

        try
        {
            session
                .Sql(q => q
                    .Upsert()
                    .Into("Status")
                    .Key("Id")
                    .Value("Id", id)
                    .Value("Name", "Upsert Insert")
                    .Value("Description", "Inserted by upsert")
                    .Value("DisplayOrder", 1)
                    .Value("Created", DateTimeOffset.UtcNow)
                    .Value("Updated", DateTimeOffset.UtcNow)
                )
                .Execute();

            session
                .Sql(q => q
                    .Upsert()
                    .Into("Status")
                    .Key("Id")
                    .Value("Id", id)
                    .Value("Name", "Upsert Update")
                    .Value("Description", "Updated by upsert")
                    .Value("DisplayOrder", 2)
                    .Value("Created", DateTimeOffset.UtcNow)
                    .Value("Updated", DateTimeOffset.UtcNow)
                )
                .Execute();

            var status = session
                .Sql("SELECT \"Name\", \"Description\", \"DisplayOrder\" FROM \"Status\" WHERE \"Id\" = @Id")
                .Parameter("@Id", id)
                .QuerySingle(r => new
                {
                    Name = r.GetString("Name"),
                    Description = r.GetString("Description"),
                    DisplayOrder = r.GetInt32("DisplayOrder")
                });

            status.Should().NotBeNull();
            status!.Name.Should().Be("Upsert Update");
            status.Description.Should().Be("Updated by upsert");
            status.DisplayOrder.Should().Be(2);
        }
        finally
        {
            session
                .Sql("DELETE FROM \"Status\" WHERE \"Id\" = @Id")
                .Parameter("@Id", id)
                .Execute();
        }
    }

    [Fact]
    public void UpsertEntityInsertsThenUpdatesStatus()
    {
        using var session = Services.GetRequiredService<IDataSession>();
        session.Should().NotBeNull();

        const int id = 99002;

        try
        {
            var insertStatus = new Status
            {
                Id = id,
                Name = "Upsert Entity Insert",
                Description = "Inserted by typed upsert",
                DisplayOrder = 1,
                IsActive = true,
                Created = DateTimeOffset.UtcNow,
                Updated = DateTimeOffset.UtcNow
            };

            session
                .Sql(q => q
                    .Upsert<Status>()
                    .Key(p => p.Id)
                    .Values(insertStatus)
                )
                .Execute();

            var updateStatus = new Status
            {
                Id = id,
                Name = "Upsert Entity Update",
                Description = "Updated by typed upsert",
                DisplayOrder = 2,
                IsActive = true,
                Created = insertStatus.Created,
                Updated = DateTimeOffset.UtcNow
            };

            session
                .Sql(q => q
                    .Upsert<Status>()
                    .Key(p => p.Id)
                    .Values(updateStatus)
                )
                .Execute();

            var status = session
                .Sql("SELECT \"Name\", \"Description\", \"DisplayOrder\" FROM \"Status\" WHERE \"Id\" = @Id")
                .Parameter("@Id", id)
                .QuerySingle(r => new
                {
                    Name = r.GetString("Name"),
                    Description = r.GetString("Description"),
                    DisplayOrder = r.GetInt32("DisplayOrder")
                });

            status.Should().NotBeNull();
            status!.Name.Should().Be("Upsert Entity Update");
            status.Description.Should().Be("Updated by typed upsert");
            status.DisplayOrder.Should().Be(2);
        }
        finally
        {
            session
                .Sql("DELETE FROM \"Status\" WHERE \"Id\" = @Id")
                .Parameter("@Id", id)
                .Execute();
        }
    }

    [Fact]
    public void UpsertEntityOutputUsesKeyAttribute()
    {
        using var session = Services.GetRequiredService<IDataSession>();
        session.Should().NotBeNull();

        const int id = 99003;

        try
        {
            var insertStatus = new StatusWithKey
            {
                Id = id,
                Name = "Upsert Output Insert",
                Description = "Inserted by output upsert",
                DisplayOrder = 1,
                IsActive = true,
                Created = DateTimeOffset.UtcNow,
                Updated = DateTimeOffset.UtcNow
            };

            var insertedId = session
                .Sql(q => q
                    .Upsert<StatusWithKey>()
                    .Values(insertStatus)
                    .Output(p => p.Id)
                )
                .QueryValue<int>();

            var updateStatus = new StatusWithKey
            {
                Id = id,
                Name = "Upsert Output Update",
                Description = "Updated by output upsert",
                DisplayOrder = 2,
                IsActive = true,
                Created = insertStatus.Created,
                Updated = DateTimeOffset.UtcNow
            };

            var updatedId = session
                .Sql(q => q
                    .Upsert<StatusWithKey>()
                    .Values(updateStatus)
                    .Output(p => p.Id)
                )
                .QueryValue<int>();

            insertedId.Should().Be(id);
            updatedId.Should().Be(id);

            var status = session
                .Sql("SELECT \"Name\", \"Description\", \"DisplayOrder\" FROM \"Status\" WHERE \"Id\" = @Id")
                .Parameter("@Id", id)
                .QuerySingle(r => new
                {
                    Name = r.GetString("Name"),
                    Description = r.GetString("Description"),
                    DisplayOrder = r.GetInt32("DisplayOrder")
                });

            status.Should().NotBeNull();
            status!.Name.Should().Be("Upsert Output Update");
            status.Description.Should().Be("Updated by output upsert");
            status.DisplayOrder.Should().Be(2);
        }
        finally
        {
            session
                .Sql("DELETE FROM \"Status\" WHERE \"Id\" = @Id")
                .Parameter("@Id", id)
                .Execute();
        }
    }
}
