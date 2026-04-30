using System.Data.Common;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace FluentCommand.SqlServer.Tests;

public class DataInterceptorTests : DatabaseTestBase
{
    public DataInterceptorTests(DatabaseFixture databaseFixture) : base(databaseFixture)
    {
    }

    [Fact]
    public void WhenConnectionOpened_ConnectionInterceptorCalledOnce()
    {
        var config = Services.GetRequiredService<IDataConfiguration>();
        var interceptor = new TrackingConnectionInterceptor();

        using var session = new DataSession(config.CreateConnection(), interceptors: [interceptor]);

        session.EnsureConnection();
        session.ReleaseConnection();

        interceptor.ConnectionOpenedCount.Should().Be(1);
        interceptor.ConnectionOpenedAsyncCount.Should().Be(0);
        interceptor.ConnectionClosingCount.Should().Be(1);
        interceptor.ConnectionClosingAsyncCount.Should().Be(0);
    }

    [Fact]
    public async Task WhenConnectionOpenedAsync_ConnectionInterceptorCalledOnce()
    {
        var config = Services.GetRequiredService<IDataConfiguration>();
        var interceptor = new TrackingConnectionInterceptor();

        await using var session = new DataSession(config.CreateConnection(), interceptors: [interceptor]);

        await session.EnsureConnectionAsync(TestCancellation);
        await session.ReleaseConnectionAsync();

        interceptor.ConnectionOpenedAsyncCount.Should().Be(1);
        interceptor.ConnectionOpenedCount.Should().Be(0);
        interceptor.ConnectionClosingAsyncCount.Should().Be(1);
        interceptor.ConnectionClosingCount.Should().Be(0);
    }

    [Fact]
    public void WhenEnsureConnectionCalledTwice_ConnectionInterceptorCalledOnce()
    {
        var config = Services.GetRequiredService<IDataConfiguration>();
        var interceptor = new TrackingConnectionInterceptor();

        using var session = new DataSession(config.CreateConnection(), interceptors: [interceptor]);

        session.EnsureConnection();
        session.EnsureConnection();
        session.ReleaseConnection();
        session.ReleaseConnection();

        interceptor.ConnectionOpenedCount.Should().Be(1);
        interceptor.ConnectionClosingCount.Should().Be(1);
    }

    [Fact]
    public void WhenCommandExecuted_CommandInterceptorCalledOnce()
    {
        var config = Services.GetRequiredService<IDataConfiguration>();
        var interceptor = new TrackingCommandInterceptor();

        using var session = new DataSession(config.CreateConnection(), interceptors: [interceptor]);

        var result = session
            .Sql("SELECT 1")
            .QueryValue<int>();

        result.Should().Be(1);
        interceptor.CommandExecutingCount.Should().Be(1);
        interceptor.CommandExecutingAsyncCount.Should().Be(0);
    }

    [Fact]
    public async Task WhenCommandExecutedAsync_CommandInterceptorCalledOnce()
    {
        var config = Services.GetRequiredService<IDataConfiguration>();
        var interceptor = new TrackingCommandInterceptor();

        await using var session = new DataSession(config.CreateConnection(), interceptors: [interceptor]);

        var result = await session
            .Sql("SELECT 1")
            .QueryValueAsync<int>(TestCancellation);

        result.Should().Be(1);
        interceptor.CommandExecutingAsyncCount.Should().Be(1);
        interceptor.CommandExecutingCount.Should().Be(0);
    }

    [Fact]
    public async Task WhenMultipleCommandsExecuted_CommandInterceptorCalledForEach()
    {
        var config = Services.GetRequiredService<IDataConfiguration>();
        var interceptor = new TrackingCommandInterceptor();

        await using var session = new DataSession(config.CreateConnection(), interceptors: [interceptor]);

        await session.Sql("SELECT 1").QueryValueAsync<int>(TestCancellation);
        await session.Sql("SELECT 2").QueryValueAsync<int>(TestCancellation);

        interceptor.CommandExecutingAsyncCount.Should().Be(2);
    }

    [Fact]
    public void WhenBothInterceptorTypesRegistered_BothAreCalledOnCommandExecution()
    {
        var config = Services.GetRequiredService<IDataConfiguration>();
        var connectionInterceptor = new TrackingConnectionInterceptor();
        var commandInterceptor = new TrackingCommandInterceptor();

        using var session = new DataSession(config.CreateConnection(), interceptors: [connectionInterceptor, commandInterceptor]);

        var result = session.Sql("SELECT 1").QueryValue<int>();

        result.Should().Be(1);
        connectionInterceptor.ConnectionOpenedCount.Should().Be(1);
        commandInterceptor.CommandExecutingCount.Should().Be(1);
    }

    [Fact]
    public void WhenCreatedWithInterceptors_SessionExposesAllInterceptors()
    {
        var config = Services.GetRequiredService<IDataConfiguration>();
        var connectionInterceptor = new TrackingConnectionInterceptor();
        var commandInterceptor = new TrackingCommandInterceptor();

        using var session = new DataSession(config.CreateConnection(), interceptors: [connectionInterceptor, commandInterceptor]);

        session.Interceptors.Should().HaveCount(2);
        session.Interceptors.Should().Contain(connectionInterceptor);
        session.Interceptors.Should().Contain(commandInterceptor);
    }

    [Fact]
    public void WhenCreatedWithNoInterceptors_SessionExposesEmptyList()
    {
        var config = Services.GetRequiredService<IDataConfiguration>();

        using var session = new DataSession(config.CreateConnection());

        session.Interceptors.Should().BeEmpty();
    }

    [Fact]
    public void WhenSqlPrintExecuted_PrintMessageIsLogged()
    {
        var config = Services.GetRequiredService<IDataConfiguration>();
        var logger = new TrackingLogger<MessageInterceptor>();
        var interceptor = new MessageInterceptor(logger);

        using var session = new DataSession(config.CreateConnection(), interceptors: [interceptor]);

        session.Sql("PRINT 'fluent command print message'; SELECT 1").QueryValue<int>().Should().Be(1);

        logger.Messages.Should().Contain(m => m.Contains("fluent command print message", StringComparison.Ordinal));
    }


    private sealed class TrackingConnectionInterceptor : IDataConnectionInterceptor
    {
        public int ConnectionOpenedCount { get; private set; }
        public int ConnectionOpenedAsyncCount { get; private set; }
        public int ConnectionClosingCount { get; private set; }
        public int ConnectionClosingAsyncCount { get; private set; }

        public void ConnectionOpened(DbConnection connection, IDataSession session)
            => ConnectionOpenedCount++;

        public Task ConnectionOpenedAsync(DbConnection connection, IDataSession session, CancellationToken cancellationToken = default)
        {
            ConnectionOpenedAsyncCount++;
            return Task.CompletedTask;
        }

        public void ConnectionClosing(DbConnection connection, IDataSession session)
            => ConnectionClosingCount++;

        public Task ConnectionClosingAsync(DbConnection connection, IDataSession session, CancellationToken cancellationToken = default)
        {
            ConnectionClosingAsyncCount++;
            return Task.CompletedTask;
        }
    }

    private sealed class TrackingCommandInterceptor : IDataCommandInterceptor
    {
        public int CommandExecutingCount { get; private set; }
        public int CommandExecutingAsyncCount { get; private set; }

        public void CommandExecuting(DbCommand command, IDataSession session)
            => CommandExecutingCount++;

        public Task CommandExecutingAsync(DbCommand command, IDataSession session, CancellationToken cancellationToken = default)
        {
            CommandExecutingAsyncCount++;
            return Task.CompletedTask;
        }
    }

    private sealed class TrackingLogger<T> : ILogger<T>
    {
        public List<string> Messages { get; } = [];

        public IDisposable? BeginScope<TState>(TState state)
            where TState : notnull
            => null;

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception? exception,
            Func<TState, Exception?, string> formatter)
        {
            Messages.Add(formatter(state, exception));
        }
    }
}
