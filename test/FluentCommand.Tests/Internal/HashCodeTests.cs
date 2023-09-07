using System;
using System.Data;

using FluentAssertions;

using FluentCommand.Tests.Models;

using Xunit;

using HashCode = FluentCommand.Internal.HashCode;

namespace FluentCommand.Tests.Internal;

public class HashCodeTests
{
    [Fact]
    public void HashCommandConsistent()
    {
        var command = new Command
        {
            Text = "SELECT * FROM dbo.Status WHERE Type = @type OR Name = @name",
            CommandType = CommandType.Text,
            Parameters =
            {
                new Parameter{ Name = "@type", Value = 1, DbType = DbType.Int32 },
                new Parameter{ Name = "@name", Value = "test", DbType = DbType.String },
            }
        };

        var commandHash = command.GetHashCode();

        commandHash.Should().Be(92316680);
    }

    [Fact]
    public void HashStringConsistent()
    {
        var stringHash = HashCode.Seed
            .Combine("This is a test")
            .GetHashCode();

        stringHash.Should().Be(1272545829);
    }
}
