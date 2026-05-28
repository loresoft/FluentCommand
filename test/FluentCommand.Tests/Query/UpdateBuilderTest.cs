using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

using FluentCommand.Entities;
using FluentCommand.Query;
using FluentCommand.Query.Generators;

namespace FluentCommand.Tests.Query;

public class UpdateBuilderTest
{
    [Fact]
    public void UpdateValueWithEnumAddsUnderlyingValueAndType()
    {
        var sqlProvider = new SqlServerGenerator();
        var parameters = new List<QueryParameter>();

        var builder = new UpdateBuilder(sqlProvider, parameters)
            .Table("EnumLog")
            .Value("Status", BuilderStatus.Active);

        var queryStatement = builder.BuildStatement();
        var parameter = queryStatement!.Parameters.Single();

        parameter.Value.Should().Be((short)BuilderStatus.Active);
        parameter.Type.Should().Be(typeof(short));
    }

    [Fact]
    public void UpdateEntityValueWithEnumAddsUnderlyingValueAndType()
    {
        var sqlProvider = new SqlServerGenerator();
        var parameters = new List<QueryParameter>();

        var builder = new UpdateEntityBuilder<EnumLog>(sqlProvider, parameters)
            .Value(p => p.Status, BuilderStatus.Active);

        var queryStatement = builder.BuildStatement();
        var parameter = queryStatement!.Parameters.Single();

        parameter.Value.Should().Be((short)BuilderStatus.Active);
        parameter.Type.Should().Be(typeof(short));
    }

    [Fact]
    public void UpdateValueJsonWithTypeInfoAddsJsonStringParameter()
    {
        var sqlProvider = new SqlServerGenerator();
        var parameters = new List<QueryParameter>();
        var options = JsonSerializerOptions.Default;
        var typeInfo = (JsonTypeInfo<ValueJsonModel>)options.GetTypeInfo(typeof(ValueJsonModel));
        var value = new ValueJsonModel("Json TypeInfo", 42);

        var builder = new UpdateBuilder(sqlProvider, parameters)
            .Table("JsonLog")
            .ValueJson("Data", value, typeInfo)
            .Where("Id", 1);

        var queryStatement = builder.BuildStatement();
        var parameter = queryStatement!.Parameters.First();

        parameter.Value.Should().Be(JsonSerializer.Serialize(value, typeInfo));
        parameter.Type.Should().Be(typeof(string));
    }

    [Fact]
    public void UpdateEntityValueJsonWithTypeInfoAddsJsonStringParameter()
    {
        var sqlProvider = new SqlServerGenerator();
        var parameters = new List<QueryParameter>();
        var options = JsonSerializerOptions.Default;
        var typeInfo = (JsonTypeInfo<ValueJsonModel>)options.GetTypeInfo(typeof(ValueJsonModel));
        var value = new ValueJsonModel("Json TypeInfo", 42);

        var builder = new UpdateEntityBuilder<JsonLog>(sqlProvider, parameters)
            .ValueJson(p => p.Data, value, typeInfo)
            .Where(p => p.Id, 1);

        var queryStatement = builder.BuildStatement();
        var parameter = queryStatement!.Parameters.First();

        parameter.Value.Should().Be(JsonSerializer.Serialize(value, typeInfo));
        parameter.Type.Should().Be(typeof(string));
    }

    [Fact]
    public async System.Threading.Tasks.Task UpdateEntityValueWithOutput()
    {
        var sqlProvider = new SqlServerGenerator();
        var parameters = new List<QueryParameter>();

        var builder = new UpdateEntityBuilder<Status>(sqlProvider, parameters)
            .Value(p => p.Name, "test")
            .Value(p => p.Description, "test")
            .Value(p => p.DisplayOrder, 10)
            .Value(p => p.Created, System.DateTimeOffset.UtcNow)
            .Value(p => p.Updated, System.DateTimeOffset.UtcNow)
            .Output(p => p.Id)
            .Where(p => p.Id, 1);

        var queryStatement = builder.BuildStatement();

        var sql = queryStatement!.Statement;

        await Verifier
            .Verify(sql)
            .UseDirectory("Snapshots")
            .ScrubLinesContaining("/* Caller;");
    }

    [Fact]
    public async System.Threading.Tasks.Task UpdateEntityValueWithJoin()
    {
        var sqlProvider = new SqlServerGenerator();
        var parameters = new List<QueryParameter>();

        var builder = new UpdateEntityBuilder<Entities.Task>(sqlProvider, parameters)
            .Value(p => p.Description, "test")
            .Value(p => p.Updated, System.DateTimeOffset.UtcNow)
            .Output(p => p.Id)
            .From(tableAlias: "t")
            .Join<Priority>(p => p
                .Left(p => p.PriorityId, "t")
                .Right(p => p.Id, "p")
            )
            .Where<Priority, int>(p => p.Id, 4, "p", FilterOperators.GreaterThanOrEqual);

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
        public int Id { get; set; }

        public ValueJsonModel Data { get; set; } = null!;
    }

    private enum BuilderStatus : short
    {
        Inactive = 0,
        Active = 1
    }

    private sealed class EnumLog
    {
        public BuilderStatus Status { get; set; }
    }
}
