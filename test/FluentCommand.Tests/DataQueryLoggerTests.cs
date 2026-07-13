using System.Data;
using System.Diagnostics.CodeAnalysis;

using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;

namespace FluentCommand.Tests;

public class DataQueryLoggerTests
{
    [Fact]
    public void ConstructorShouldRejectInvalidMaxQueryLogLength()
    {
        // Arrange
        var logger = new CaptureLogger<DataQueryLogger>(LogLevel.Debug);

        // Act
        var action = () => new DataQueryLogger(logger, 0, 0);

        // Assert
        action.Should().Throw<ArgumentOutOfRangeException>()
            .WithParameterName("maxQueryLength");
    }

    [Fact]
    public void LogCommandShouldNotFormatCommandWhenLevelDisabled()
    {
        // Arrange
        var logger = new CaptureLogger<DataQueryLogger>(LogLevel.Information);
        var queryLogger = new DataQueryLogger(logger);
        var command = new StubDbCommand
        {
            CommandText = "select 1",
            CommandType = CommandType.Text,
            CommandTimeout = 30,
        };

        // Act
        queryLogger.LogCommand(command, TimeSpan.FromMilliseconds(12));

        // Assert
        command.CommandTextReadCount.Should().Be(0);
        command.ParametersReadCount.Should().Be(0);
        logger.Entries.Should().BeEmpty();
    }

    [Fact]
    public void LogCommandShouldWriteDebugEntryWhenCommandSucceeds()
    {
        // Arrange
        var logger = new CaptureLogger<DataQueryLogger>(LogLevel.Debug);
        var queryLogger = new DataQueryLogger(logger);
        using var command = CreateCommand();

        // Act
        queryLogger.LogCommand(command, TimeSpan.FromMilliseconds(12));

        // Assert
        var entry = logger.Entries.Should().ContainSingle().Subject;
        entry.LogLevel.Should().Be(LogLevel.Debug);
        entry.Exception.Should().BeNull();
        entry.Message.Should().Contain($"[CommandType='Text', CommandTimeout='30']{Environment.NewLine}select 1{Environment.NewLine}-- @Id: Input Int32(Size=0; Precision=0; Scale=0) [42]");
    }

    [Fact]
    public void LogCommandShouldWriteErrorEntryWhenCommandFails()
    {
        // Arrange
        var logger = new CaptureLogger<DataQueryLogger>(LogLevel.Error);
        var queryLogger = new DataQueryLogger(logger);
        using var command = CreateCommand();
        var exception = new InvalidOperationException("Query failed.");

        // Act
        queryLogger.LogCommand(command, TimeSpan.FromMilliseconds(12), exception);

        // Assert
        var entry = logger.Entries.Should().ContainSingle().Subject;
        entry.LogLevel.Should().Be(LogLevel.Error);
        entry.Exception.Should().BeSameAs(exception);
        entry.Message.Should().Contain("Error Executing DbCommand");
    }


    [Fact]
    public void LogCommandShouldNotAddBlankLineBetweenCommandTextAndParameters()
    {
        // Arrange
        var logger = new CaptureLogger<DataQueryLogger>(LogLevel.Debug);
        var queryLogger = new DataQueryLogger(logger);
        using var command = new SqlCommand("SELECT 1;\r\n\r\n");
        command.Parameters.Add(new SqlParameter("@p0000", SqlDbType.Int) { Value = 1 });

        // Act
        queryLogger.LogCommand(command, TimeSpan.FromMilliseconds(1));

        // Assert
        logger.Message.Should().Contain($"SELECT 1;{Environment.NewLine}-- @p0000:");
        logger.Message.Should().NotContain($"SELECT 1;{Environment.NewLine}{Environment.NewLine}-- @p0000:");
    }

    [Fact]
    public void LogCommandShouldNotAddTrailingBlankLineWhenCommandHasNoParameters()
    {
        // Arrange
        var logger = new CaptureLogger<DataQueryLogger>(LogLevel.Debug);
        var queryLogger = new DataQueryLogger(logger);
        using var command = new SqlCommand("\r\nSELECT 1;\r\n");

        // Act
        queryLogger.LogCommand(command, TimeSpan.FromMilliseconds(1));

        // Assert
        logger.Message.Should().EndWith("SELECT 1;");
    }

    [Fact]
    public void LogCommandShouldOmitLargeCommandText()
    {
        // Arrange
        var logger = new CaptureLogger<DataQueryLogger>(LogLevel.Debug);
        var queryLogger = new DataQueryLogger(logger, 64, 64);
        var command = new StubDbCommand
        {
            CommandText = new string('x', 128),
            CommandType = CommandType.Text,
            CommandTimeout = 30,
        };

        // Act
        queryLogger.LogCommand(command, TimeSpan.FromMilliseconds(12));

        // Assert
        var entry = logger.Entries.Should().ContainSingle().Subject;
        entry.Message.Should().NotContain(new string('x', 64));
    }

    [Fact]
    public void LogCommandShouldOmitLargeParameterValue()
    {
        // Arrange
        var logger = new CaptureLogger<DataQueryLogger>(LogLevel.Debug);
        var queryLogger = new DataQueryLogger(logger, 24, 24);
        using var command = new SqlCommand("select 1");
        command.Parameters.Add(new SqlParameter("@Value", SqlDbType.NVarChar, 128) { Value = new string('x', 128) });

        // Act
        queryLogger.LogCommand(command, TimeSpan.FromMilliseconds(12));

        // Assert
        var entry = logger.Entries.Should().ContainSingle().Subject;
        entry.Message.Should().Contain($"{Environment.NewLine}select 1{Environment.NewLine}-- @Value: Input String(Size=128; Precision=0; Scale=0) [xxxxxxxxxxxxxxxxxxxxx...]");
        entry.Message.Should().NotContain(new string('x', 96));
    }


    private static SqlCommand CreateCommand()
    {
        var command = new SqlCommand($"{Environment.NewLine}select 1{Environment.NewLine}")
        {
            CommandTimeout = 30,
        };
        command.Parameters.Add(new SqlParameter("@Id", SqlDbType.Int) { Value = 42 });

        return command;
    }

    private sealed class CaptureLogger<T>(LogLevel minimumLogLevel) : ILogger<T>
    {
        public string Message { get; private set; } = string.Empty;

        public List<LogEntry> Entries { get; } = [];

        public IDisposable? BeginScope<TState>(TState state)
            where TState : notnull
            => NullScope.Instance;

        public bool IsEnabled(LogLevel logLevel) => minimumLogLevel != LogLevel.None && logLevel >= minimumLogLevel;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            if (!IsEnabled(logLevel))
                return;

            Message = formatter(state, exception);
            Entries.Add(new LogEntry(logLevel, Message, exception));
        }
    }

    private sealed record LogEntry(LogLevel LogLevel, string Message, Exception? Exception);

    private sealed class NullScope : IDisposable
    {
        public static readonly NullScope Instance = new();

        public void Dispose()
        {
        }
    }

    private sealed class StubDbCommand : IDbCommand
    {
        private string _commandText = string.Empty;

        public int CommandTextReadCount { get; private set; }

        public int ParametersReadCount { get; private set; }

        [AllowNull]
        public string CommandText
        {
            get
            {
                CommandTextReadCount++;
                return _commandText;
            }
            set => _commandText = value ?? string.Empty;
        }

        public int CommandTimeout { get; set; }

        public CommandType CommandType { get; set; }

        public IDbConnection? Connection { get; set; }

        public IDataParameterCollection Parameters
        {
            get
            {
                ParametersReadCount++;
                throw new InvalidOperationException("Parameters should not be read when logging is disabled.");
            }
        }

        public IDbTransaction? Transaction { get; set; }

        public UpdateRowSource UpdatedRowSource { get; set; }

        public void Cancel()
        {
        }

        public IDbDataParameter CreateParameter() => throw new NotSupportedException();

        public void Dispose()
        {
        }

        public int ExecuteNonQuery() => throw new NotSupportedException();

        public IDataReader ExecuteReader() => throw new NotSupportedException();

        public IDataReader ExecuteReader(CommandBehavior behavior) => throw new NotSupportedException();

        public object? ExecuteScalar() => throw new NotSupportedException();

        public void Prepare()
        {
        }
    }

}
