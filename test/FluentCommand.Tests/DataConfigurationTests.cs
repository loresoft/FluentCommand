using Microsoft.Data.SqlClient;
using Microsoft.Extensions.DependencyInjection;
using System.Data;

namespace FluentCommand.Tests;

public class DataConfigurationTests
{
    [Fact]
    public void CreateSessionShouldApplyConfiguredCommandTimeout()
    {
        // Arrange
        var configuration = new DataConfiguration(
            SqlClientFactory.Instance,
            "Server=(local);Database=FluentCommand;Integrated Security=true;TrustServerCertificate=true;",
            commandTimeout: 47);

        using var session = configuration.CreateSession();

        // Act
        var command = session.Sql("SELECT 1");

        // Assert
        command.Command.CommandTimeout.Should().Be(47);
    }

    [Fact]
    public void CommandTimeoutShouldOverrideConfiguredCommandTimeout()
    {
        // Arrange
        var configuration = new DataConfiguration(
            SqlClientFactory.Instance,
            "Server=(local);Database=FluentCommand;Integrated Security=true;TrustServerCertificate=true;",
            commandTimeout: 47);

        using var session = configuration.CreateSession();

        // Act
        var command = session.Sql("SELECT 1").CommandTimeout(12);

        // Assert
        command.Command.CommandTimeout.Should().Be(12);
    }

    [Fact]
    public void UseCommandTimeoutShouldRegisterConfiguredCommandTimeout()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton<IDataQueryLogger, NullDataQueryLogger>();
        services.AddFluentCommand(builder => builder
            .UseConnectionString("Server=(local);Database=FluentCommand;Integrated Security=true;TrustServerCertificate=true;")
            .AddProviderFactory(SqlClientFactory.Instance)
            .UseCommandTimeout(47));

        using var provider = services.BuildServiceProvider();

        // Act
        var configuration = provider.GetRequiredService<IDataConfiguration>();
        using var session = configuration.CreateSession();
        var command = session.StoredProcedure("dbo.TestProcedure");

        // Assert
        configuration.CommandTimeout.Should().Be(47);
        command.Command.CommandTimeout.Should().Be(47);
    }

    [Fact]
    public void UseCommandTimeoutWithTimeSpanShouldRegisterConfiguredCommandTimeout()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton<IDataQueryLogger, NullDataQueryLogger>();
        services.AddFluentCommand(builder => builder
            .UseConnectionString("Server=(local);Database=FluentCommand;Integrated Security=true;TrustServerCertificate=true;")
            .AddProviderFactory(SqlClientFactory.Instance)
            .UseCommandTimeout(TimeSpan.FromSeconds(47)));

        using var provider = services.BuildServiceProvider();

        // Act
        var configuration = provider.GetRequiredService<IDataConfiguration>();
        using var session = configuration.CreateSession();
        var command = session.Sql("SELECT 1");

        // Assert
        configuration.CommandTimeout.Should().Be(47);
        command.Command.CommandTimeout.Should().Be(47);
    }

    private sealed class NullDataQueryLogger : IDataQueryLogger
    {
        public void LogCommand(IDbCommand command, TimeSpan duration, Exception? exception = null, object? state = null)
        {
        }
    }
}
