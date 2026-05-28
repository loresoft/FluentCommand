using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

using FluentCommand.Entities;
using FluentCommand.Query;

using Microsoft.Extensions.DependencyInjection;

using Task = System.Threading.Tasks.Task;

namespace FluentCommand.SQLite.Tests;

public class JsonTests : DatabaseTestBase
{
    public JsonTests(DatabaseFixture databaseFixture)
        : base(databaseFixture)
    {
    }

    [Fact]
    public void QueryJson()
    {
        var session = Services.GetRequiredService<IDataSession>();
        session.Should().NotBeNull();

        string sql = "select * from \"User\" LIMIT 1000";

        var json = session.Sql(sql)
            .QueryJson();

        json.Should().NotBeNull();
    }

    [Fact]
    public async Task QueryJsonAsync()
    {
        var session = Services.GetRequiredService<IDataSession>();
        session.Should().NotBeNull();

        string sql = "select * from \"User\" LIMIT 1000";

        var json = await session.Sql(sql)
            .QueryJsonAsync(cancellationToken: TestCancellation);

        json.Should().NotBeNull();
    }

    [Fact]
    public async Task QuerySelectAsync()
    {
        var session = Services.GetRequiredService<IDataSession>();
        session.Should().NotBeNull();

        var json = await session
            .Sql(builder => builder
                .Select<Status>()
                .OrderBy(p => p.DisplayOrder)
                .Limit(0, 1000)
            )
            .QueryJsonAsync(cancellationToken: TestCancellation);

        json.Should().NotBeNull();
    }

    [Fact]
    public void ParameterJsonInsertsSerializedValue()
    {
        using var session = Services.GetRequiredService<IDataSession>();
        session.Should().NotBeNull();

        var input = new UserImport
        {
            EmailAddress = "json.param@test.com",
            DisplayName = "Json Param",
            FirstName = "Json",
            LastName = "Param",
        };

        var id = (int)(DateTimeOffset.UtcNow.Ticks % int.MaxValue);

        session.Sql("INSERT INTO \"JsonLog\" (\"Id\", \"Data\") VALUES (@Id, @Data)")
            .Parameter("@Id", id)
            .ParameterJson("@Data", input)
            .Execute();

        var json = session.Sql("SELECT \"Data\" FROM \"JsonLog\" WHERE \"Id\" = @Id LIMIT 1")
            .Parameter("@Id", id)
            .QueryValue<string>();

        json.Should().NotBeNullOrEmpty();

        var result = JsonSerializer.Deserialize<UserImport>(json);
        result.Should().NotBeNull();
        result.EmailAddress.Should().Be(input.EmailAddress);
        result.DisplayName.Should().Be(input.DisplayName);
        result.FirstName.Should().Be(input.FirstName);
        result.LastName.Should().Be(input.LastName);
    }

    [Fact]
    public void ParameterJsonWithOptionsInsertsSerializedValue()
    {
        using var session = Services.GetRequiredService<IDataSession>();
        session.Should().NotBeNull();

        var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

        var input = new UserImport
        {
            EmailAddress = "json.options@test.com",
            DisplayName = "Json Options",
            FirstName = "Json",
            LastName = "Options",
        };

        var id = (int)(DateTimeOffset.UtcNow.Ticks % int.MaxValue);

        session.Sql("INSERT INTO \"JsonLog\" (\"Id\", \"Data\") VALUES (@Id, @Data)")
            .Parameter("@Id", id)
            .ParameterJson("@Data", input, options)
            .Execute();

        var json = session.Sql("SELECT \"Data\" FROM \"JsonLog\" WHERE \"Id\" = @Id LIMIT 1")
            .Parameter("@Id", id)
            .QueryValue<string>();

        json.Should().NotBeNullOrEmpty();
        json.Should().Contain("\"emailAddress\"");

        var result = JsonSerializer.Deserialize<UserImport>(json, options);
        result.Should().NotBeNull();
        result.EmailAddress.Should().Be(input.EmailAddress);
        result.DisplayName.Should().Be(input.DisplayName);
    }

    [Fact]
    public void InsertValueJsonWithOptionsInsertsSerializedValue()
    {
        using var session = Services.GetRequiredService<IDataSession>();
        session.Should().NotBeNull();

        var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

        var input = new UserImport
        {
            EmailAddress = "insert.valuejson.options@test.com",
            DisplayName = "Insert ValueJson Options",
            FirstName = "Insert",
            LastName = "ValueJson",
        };

        var id = (int)(DateTimeOffset.UtcNow.Ticks % int.MaxValue);

        session.Sql(builder => builder
                .Insert()
                .Into("JsonLog")
                .Value("Id", id)
                .ValueJson("Data", input, options)
            )
            .Execute();

        var json = session.Sql("SELECT \"Data\" FROM \"JsonLog\" WHERE \"Id\" = @Id LIMIT 1")
            .Parameter("@Id", id)
            .QueryValue<string>();

        json.Should().NotBeNullOrEmpty();
        json.Should().Contain("\"emailAddress\"");

        var result = JsonSerializer.Deserialize<UserImport>(json, options);
        result.Should().NotBeNull();
        result.EmailAddress.Should().Be(input.EmailAddress);
        result.DisplayName.Should().Be(input.DisplayName);
    }

    [Fact]
    public void ParameterJsonWithTypeInfoInsertsSerializedValue()
    {
        using var session = Services.GetRequiredService<IDataSession>();
        session.Should().NotBeNull();

        var options = JsonSerializerOptions.Default;
        var typeInfo = (JsonTypeInfo<UserImport>)options.GetTypeInfo(typeof(UserImport));

        var input = new UserImport
        {
            EmailAddress = "json.typeinfo@test.com",
            DisplayName = "Json TypeInfo",
            FirstName = "Json",
            LastName = "TypeInfo",
        };

        var id = (int)(DateTimeOffset.UtcNow.Ticks % int.MaxValue);

        session.Sql("INSERT INTO \"JsonLog\" (\"Id\", \"Data\") VALUES (@Id, @Data)")
            .Parameter("@Id", id)
            .ParameterJson("@Data", input, typeInfo)
            .Execute();

        var json = session.Sql("SELECT \"Data\" FROM \"JsonLog\" WHERE \"Id\" = @Id LIMIT 1")
            .Parameter("@Id", id)
            .QueryValue<string>();

        json.Should().NotBeNullOrEmpty();

        var result = JsonSerializer.Deserialize(json, typeInfo);
        result.Should().NotBeNull();
        result.EmailAddress.Should().Be(input.EmailAddress);
        result.DisplayName.Should().Be(input.DisplayName);
    }

    [Fact]
    public void UpdateValueJsonWithTypeInfoUpdatesSerializedValue()
    {
        using var session = Services.GetRequiredService<IDataSession>();
        session.Should().NotBeNull();

        const int id = 99002;

        try
        {
            session.Sql("INSERT INTO \"JsonLog\" (\"Id\", \"Data\") VALUES (@Id, @Data)")
                .Parameter("@Id", id)
                .Parameter("@Data", "{}")
                .Execute();

            var options = JsonSerializerOptions.Default;
            var typeInfo = (JsonTypeInfo<UserImport>)options.GetTypeInfo(typeof(UserImport));

            var input = new UserImport
            {
                EmailAddress = "update.valuejson.typeinfo@test.com",
                DisplayName = "Update ValueJson TypeInfo",
                FirstName = "Update",
                LastName = "ValueJson",
            };

            session.Sql(builder => builder
                    .Update()
                    .Table("JsonLog")
                    .ValueJson("Data", input, typeInfo)
                    .Where("Id", id)
                )
                .Execute();

            var json = session.Sql("SELECT \"Data\" FROM \"JsonLog\" WHERE \"Id\" = @Id LIMIT 1")
                .Parameter("@Id", id)
                .QueryValue<string>();

            json.Should().NotBeNullOrEmpty();

            var result = JsonSerializer.Deserialize(json, typeInfo);
            result.Should().NotBeNull();
            result.EmailAddress.Should().Be(input.EmailAddress);
            result.DisplayName.Should().Be(input.DisplayName);
        }
        finally
        {
            session.Sql("DELETE FROM \"JsonLog\" WHERE \"Id\" = @Id")
                .Parameter("@Id", id)
                .Execute();
        }
    }

    [Fact]
    public void ParameterJsonWithNullInsertsSerializedNull()
    {
        using var session = Services.GetRequiredService<IDataSession>();
        session.Should().NotBeNull();

        UserImport? userImport = null;

        var id = (int)(DateTimeOffset.UtcNow.Ticks % int.MaxValue);

        session.Sql("INSERT INTO \"JsonLog\" (\"Id\", \"Data\") VALUES (@Id, @Data)")
            .Parameter("@Id", id)
            .ParameterJson("@Data", userImport)
            .Execute();

        var json = session.Sql("SELECT \"Data\" FROM \"JsonLog\" WHERE \"Id\" = @Id LIMIT 1")
            .Parameter("@Id", id)
            .QueryValue<string>();

        json.Should().BeNull();
    }

    [Fact]
    public void UpsertValueJsonRoundTripsGeneratedReader()
    {
        using var session = Services.GetRequiredService<IDataSession>();
        session.Should().NotBeNull();

        const int id = 99001;

        var input = new UserImport
        {
            EmailAddress = "upsert.generated.reader@test.com",
            DisplayName = "Upsert Generated Reader",
            FirstName = "Upsert",
            LastName = "Generated",
        };

        try
        {
            session.Sql(builder => builder
                    .Upsert()
                    .Into("JsonLog")
                    .Key("Id")
                    .Value("Id", id)
                    .ValueJson("Data", input)
                )
                .Execute();

            var jsonLog = session.Sql("SELECT \"Id\", \"Data\", \"Created\" FROM \"JsonLog\" WHERE \"Id\" = @Id LIMIT 1")
                .Parameter("@Id", id)
                .QuerySingle<JsonLog>();

            jsonLog.Should().NotBeNull();
            jsonLog!.Id.Should().Be(id);
            jsonLog.Data.Should().NotBeNull();
            jsonLog.Data!.EmailAddress.Should().Be(input.EmailAddress);
            jsonLog.Data.DisplayName.Should().Be(input.DisplayName);
            jsonLog.Data.FirstName.Should().Be(input.FirstName);
            jsonLog.Data.LastName.Should().Be(input.LastName);
        }
        finally
        {
            session.Sql("DELETE FROM \"JsonLog\" WHERE \"Id\" = @Id")
                .Parameter("@Id", id)
                .Execute();
        }
    }
}
