using System.Text.Json;

using FluentCommand.Entities;
using FluentCommand.Query;
using FluentCommand.Query.Generators;

namespace FluentCommand.Tests.Query;

public class InsertBuilderTest
{
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
}
