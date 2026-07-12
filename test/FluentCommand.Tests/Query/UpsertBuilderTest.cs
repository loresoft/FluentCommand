using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;

using FluentCommand.Query;
using FluentCommand.Query.Generators;

namespace FluentCommand.Tests.Query;

public class UpsertBuilderTest
{
    [Fact]
    public void UpsertValueWithEnumAddsUnderlyingValueAndType()
    {
        var sqlProvider = new SqlServerGenerator();
        var parameters = new List<QueryParameter>();

        var builder = new UpsertBuilder(sqlProvider, parameters)
            .Into("EnumLog")
            .Key("Id")
            .Value("Id", 1)
            .Value("Status", BuilderStatus.Active);

        var queryStatement = builder.BuildStatement();
        var parameter = queryStatement!.Parameters.Last();

        parameter.Value.Should().Be((short)BuilderStatus.Active);
        parameter.Type.Should().Be(typeof(short));
    }

    [Fact]
    public void UpsertEntityValueIfWithEnumAddsUnderlyingValueAndType()
    {
        var sqlProvider = new SqlServerGenerator();
        var parameters = new List<QueryParameter>();

        var builder = new UpsertEntityBuilder<EnumLog>(sqlProvider, parameters)
            .Key(p => p.Id)
            .Value(p => p.Id, 1)
            .ValueIf(p => p.Status, BuilderStatus.Active, (_, _) => true);

        var queryStatement = builder.BuildStatement();
        var parameter = queryStatement!.Parameters.Last();

        parameter.Value.Should().Be((short)BuilderStatus.Active);
        parameter.Type.Should().Be(typeof(short));
    }

    [Fact]
    public void UpsertValueWithJsonElementAddsRawJsonStringParameter()
    {
        var sqlProvider = new SqlServerGenerator();
        var parameters = new List<QueryParameter>();
        using var document = JsonDocument.Parse("""{"name":"Json Element","count":42}""");

        var builder = new UpsertBuilder(sqlProvider, parameters)
            .Into("JsonLog")
            .Key("Id")
            .Value("Id", 1)
            .Value("Data", document.RootElement);

        var queryStatement = builder.BuildStatement();
        var parameter = queryStatement!.Parameters.Last();

        parameter.Value.Should().Be("""{"name":"Json Element","count":42}""");
        parameter.Type.Should().Be(typeof(string));
    }

    [Fact]
    public void UpsertEntityValuesWithJsonElementAddsRawJsonStringParameter()
    {
        var sqlProvider = new SqlServerGenerator();
        var parameters = new List<QueryParameter>();
        using var document = JsonDocument.Parse("""{"name":"Json Element","count":42}""");
        var entity = new JsonElementLog { Id = 1, Data = document.RootElement };

        var builder = new UpsertEntityBuilder<JsonElementLog>(sqlProvider, parameters)
            .Values(entity);

        var queryStatement = builder.BuildStatement();
        var parameter = queryStatement!.Parameters.Single(p => p.Type == typeof(string));

        parameter.Value.Should().Be("""{"name":"Json Element","count":42}""");
    }

    [Fact]
    public async System.Threading.Tasks.Task UpsertExplicitKeySqlServer()
    {
        var sqlProvider = new SqlServerGenerator();
        var parameters = new List<QueryParameter>();

        var builder = new UpsertBuilder(sqlProvider, parameters)
            .Into("Status", "dbo")
            .Key("Id")
            .Value("Id", 1)
            .Value("Name", "test")
            .Value("Description", "test")
            .Output("Id");

        var queryStatement = builder.BuildStatement();

        var sql = queryStatement!.Statement;

        await Verifier
            .Verify(sql)
            .UseDirectory("Snapshots")
            .ScrubLinesContaining("/* Caller;");
    }

    [Fact]
    public async System.Threading.Tasks.Task UpsertExplicitKeyPostgreSql()
    {
        var sqlProvider = new PostgreSqlGenerator();
        var parameters = new List<QueryParameter>();

        var builder = new UpsertBuilder(sqlProvider, parameters)
            .Into("Status", "dbo")
            .Key("Id")
            .Value("Id", 1)
            .Value("Name", "test")
            .Value("Description", "test")
            .Output("Id");

        var queryStatement = builder.BuildStatement();

        var sql = queryStatement!.Statement;

        await Verifier
            .Verify(sql)
            .UseDirectory("Snapshots")
            .ScrubLinesContaining("/* Caller;");
    }

    [Fact]
    public async System.Threading.Tasks.Task UpsertExplicitKeySqlite()
    {
        var sqlProvider = new SqliteGenerator();
        var parameters = new List<QueryParameter>();

        var builder = new UpsertBuilder(sqlProvider, parameters)
            .Into("Status", "dbo")
            .Key("Id")
            .Value("Id", 1)
            .Value("Name", "test")
            .Value("Description", "test")
            .Output("Id");

        var queryStatement = builder.BuildStatement();

        var sql = queryStatement!.Statement;

        await Verifier
            .Verify(sql)
            .UseDirectory("Snapshots")
            .ScrubLinesContaining("/* Caller;");
    }

    [Fact]
    public async System.Threading.Tasks.Task UpsertEntityInferredKeySqlServer()
    {
        var sqlProvider = new SqlServerGenerator();
        var parameters = new List<QueryParameter>();
        var entity = new UpsertStatus
        {
            Id = 1,
            Name = "test",
            Description = "test"
        };

        var builder = new UpsertEntityBuilder<UpsertStatus>(sqlProvider, parameters)
            .Values(entity)
            .Output(p => p.Id);

        var queryStatement = builder.BuildStatement();

        var sql = queryStatement!.Statement;

        await Verifier
            .Verify(sql)
            .UseDirectory("Snapshots")
            .ScrubLinesContaining("/* Caller;");
    }

    [Fact]
    public async System.Threading.Tasks.Task QueryBuilderUpsert()
    {
        var sqlProvider = new SqlServerGenerator();
        var queryParameters = new List<QueryParameter>();
        var queryBuilder = new QueryBuilder(sqlProvider, queryParameters);

        queryBuilder.Upsert()
            .Into("Status", "dbo")
            .Key("Id")
            .Value("Id", 1)
            .Value("Name", "test");

        var queryStatement = queryBuilder.BuildStatement();

        var sql = queryStatement!.Statement;

        await Verifier
            .Verify(sql)
            .UseDirectory("Snapshots")
            .ScrubLinesContaining("/* Caller;");
    }

    [Fact]
    public void UpsertValueJsonWithNullAddsStringParameterWithNullValue()
    {
        var sqlProvider = new SqlServerGenerator();
        var parameters = new List<QueryParameter>();

        var builder = new UpsertBuilder(sqlProvider, parameters)
            .Into("JsonLog")
            .Key("Id")
            .Value("Id", 1)
            .ValueJson<object>("Data", null);

        var queryStatement = builder.BuildStatement();
        var parameter = queryStatement!.Parameters.Last();

        parameter.Value.Should().BeNull();
        parameter.Type.Should().Be(typeof(string));
    }

    [Fact]
    public void UpsertEntityValueJsonWithOptionsAddsJsonStringParameter()
    {
        var sqlProvider = new SqlServerGenerator();
        var parameters = new List<QueryParameter>();
        var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        var value = new ValueJsonModel("Json Options", 42);

        var builder = new UpsertEntityBuilder<JsonLog>(sqlProvider, parameters)
            .Key(p => p.Id)
            .Value(p => p.Id, 1)
            .ValueJson(p => p.Data, value, options);

        var queryStatement = builder.BuildStatement();
        var parameter = queryStatement!.Parameters.Last();

        parameter.Value.Should().Be(JsonSerializer.Serialize(value, options));
        parameter.Type.Should().Be(typeof(string));
    }

    private sealed class JsonElementLog
    {
        [Key]
        public int Id { get; set; }

        public JsonElement Data { get; set; }
    }

}

public sealed record ValueJsonModel(string Name, int Count);

public enum BuilderStatus : short
{
    Inactive = 0,
    Active = 1
}

public sealed class EnumLog
{
    [Key]
    public int Id { get; set; }

    public BuilderStatus Status { get; set; }
}

public sealed class JsonLog
{
    [Key]
    public int Id { get; set; }

    public ValueJsonModel Data { get; set; } = null!;
}

[Table("Status", Schema = "dbo")]
public class UpsertStatus
{
    [Key]
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }
}
