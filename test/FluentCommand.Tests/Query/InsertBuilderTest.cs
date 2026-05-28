using System.Text.Json;

using FluentCommand.Entities;
using FluentCommand.Query;
using FluentCommand.Query.Generators;

namespace FluentCommand.Tests.Query;

public class InsertBuilderTest
{
    [Fact]
    public void InsertValueWithEnumAddsUnderlyingValueAndType()
    {
        var sqlProvider = new SqlServerGenerator();
        var parameters = new List<QueryParameter>();

        var builder = new InsertBuilder(sqlProvider, parameters)
            .Into("EnumLog")
            .Value("Status", BuilderStatus.Active);

        var queryStatement = builder.BuildStatement();
        var parameter = queryStatement!.Parameters.Single();

        parameter.Value.Should().Be((short)BuilderStatus.Active);
        parameter.Type.Should().Be(typeof(short));
    }

    [Fact]
    public void InsertValueWithNullableEnumAddsUnderlyingValueAndType()
    {
        var sqlProvider = new SqlServerGenerator();
        var parameters = new List<QueryParameter>();
        BuilderStatus? value = BuilderStatus.Active;

        var builder = new InsertBuilder(sqlProvider, parameters)
            .Into("EnumLog")
            .Value("Status", value);

        var queryStatement = builder.BuildStatement();
        var parameter = queryStatement!.Parameters.Single();

        parameter.Value.Should().Be((short)BuilderStatus.Active);
        parameter.Type.Should().Be(typeof(short));
    }

    [Fact]
    public void InsertValueWithNullNullableEnumAddsNullWithUnderlyingType()
    {
        var sqlProvider = new SqlServerGenerator();
        var parameters = new List<QueryParameter>();
        BuilderStatus? value = null;

        var builder = new InsertBuilder(sqlProvider, parameters)
            .Into("EnumLog")
            .Value("Status", value);

        var queryStatement = builder.BuildStatement();
        var parameter = queryStatement!.Parameters.Single();

        parameter.Value.Should().BeNull();
        parameter.Type.Should().Be(typeof(short));
    }

    [Fact]
    public void InsertEntityValuesWithEnumAddsUnderlyingValueAndType()
    {
        var sqlProvider = new SqlServerGenerator();
        var parameters = new List<QueryParameter>();
        var entity = new EnumLog { Status = BuilderStatus.Active };

        var builder = new InsertEntityBuilder<EnumLog>(sqlProvider, parameters)
            .Values(entity);

        var queryStatement = builder.BuildStatement();
        var parameter = queryStatement!.Parameters.Single();

        parameter.Value.Should().Be((short)BuilderStatus.Active);
        parameter.Type.Should().Be(typeof(short));
    }

    [Fact]
    public void InsertValueJsonWithOptionsAddsJsonStringParameter()
    {
        var sqlProvider = new SqlServerGenerator();
        var parameters = new List<QueryParameter>();
        var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        var value = new ValueJsonModel("Json Options", 42);

        var builder = new InsertBuilder(sqlProvider, parameters)
            .Into("JsonLog")
            .ValueJson("Data", value, options);

        var queryStatement = builder.BuildStatement();
        var parameter = queryStatement!.Parameters.Single();

        parameter.Value.Should().Be(JsonSerializer.Serialize(value, options));
        parameter.Type.Should().Be(typeof(string));
    }

    [Fact]
    public void InsertEntityValueJsonWithOptionsAddsJsonStringParameter()
    {
        var sqlProvider = new SqlServerGenerator();
        var parameters = new List<QueryParameter>();
        var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        var value = new ValueJsonModel("Json Options", 42);

        var builder = new InsertEntityBuilder<JsonLog>(sqlProvider, parameters)
            .ValueJson(p => p.Data, value, options);

        var queryStatement = builder.BuildStatement();
        var parameter = queryStatement!.Parameters.Single();

        parameter.Value.Should().Be(JsonSerializer.Serialize(value, options));
        parameter.Type.Should().Be(typeof(string));
    }

    [Fact]
    public async System.Threading.Tasks.Task InsertEntityValueWithOutput()
    {
        var sqlProvider = new SqlServerGenerator();
        var parameters = new List<QueryParameter>();

        var builder = new InsertEntityBuilder<Status>(sqlProvider, parameters)
            .Value(p => p.Name, "test")
            .Value(p => p.Description, "test")
            .Value(p => p.DisplayOrder, 10)
            .Value(p => p.Created, System.DateTimeOffset.UtcNow)
            .Value(p => p.Updated, System.DateTimeOffset.UtcNow)
            .Output(p => p.Id);

        var queryStatement = builder.BuildStatement();

        var sql = queryStatement!.Statement;

        await Verifier
            .Verify(sql)
            .UseDirectory("Snapshots")
            .ScrubLinesContaining("/* Caller;");
    }

    private sealed record ValueJsonModel(string Name, int Count);

    private sealed class JsonLog
    {
        public ValueJsonModel Data { get; set; } = null!;
    }

    private enum BuilderStatus : short
    {
        Inactive = 0,
        Active = 1
    }

    private sealed class EnumLog
    {
        public BuilderStatus? Status { get; set; }
    }
}
