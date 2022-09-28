using System.Collections.Generic;

using FluentCommand.Entities;
using FluentCommand.Query;
using FluentCommand.Query.Generators;

using VerifyXunit;

using Xunit;

namespace FluentCommand.Tests.Query;

[UsesVerify]
public class DeleteBuilderTest
{
    [Fact]
    public async System.Threading.Tasks.Task DeleteEntityWithOutput()
    {
        var sqlProvider = new SqlServerGenerator();
        var parameters = new Dictionary<string, object>();

        var builder = new DeleteEntityBuilder<Status>(sqlProvider, parameters)
            .Output(p => p.Id)
            .Where(p => p.Id, 1);

        var queryStatement = builder.BuildStatement();

        var sql = queryStatement.Statement;

        await Verifier.Verify(sql).UseDirectory("Snapshots");
    }

    [Fact]
    public async System.Threading.Tasks.Task DeleteEntityWithComment()
    {
        var sqlProvider = new SqlServerGenerator();
        var parameters = new Dictionary<string, object>();

        var builder = new DeleteEntityBuilder<Status>(sqlProvider, parameters)
            .Comment()
            .Output(p => p.Id)
            .Where(p => p.Id, 1);

        var queryStatement = builder.BuildStatement();

        var sql = queryStatement.Statement;

        await Verifier.Verify(sql).UseDirectory("Snapshots");
    }
}
