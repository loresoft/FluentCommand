using System.Collections.Generic;

using FluentCommand.Entities;
using FluentCommand.Query.Generators;

using VerifyXunit;

using Xunit;

namespace FluentCommand.Tests.Query;

[UsesVerify]
public class UpdateBuilderTest
{
    [Fact]
    public async System.Threading.Tasks.Task UpdateEntityValueWithOutput()
    {
        var sqlProvider = new SqlServerGenerator();
        var parameters = new Dictionary<string, object>();

        var builder = new UpdateEntityBuilder<Status>(sqlProvider, parameters)
            .Value(p => p.Name, "test")
            .Value(p => p.Description, "test")
            .Value(p => p.DisplayOrder, 10)
            .Value(p => p.Created, System.DateTimeOffset.UtcNow)
            .Value(p => p.Updated, System.DateTimeOffset.UtcNow)
            .Output(p => p.Id)
            .Where(p => p.Id, 1);

        var queryStatement = builder.BuildStatement();

        var sql = queryStatement.Statement;

        await Verifier.Verify(sql).UseDirectory("Snapshots");
    }
}
