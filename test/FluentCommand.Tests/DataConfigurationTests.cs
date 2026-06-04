using Microsoft.Data.SqlClient;
using Microsoft.Extensions.DependencyInjection;
using System.Collections;
using System.Data;
using System.Data.Common;
using System.Text.Json;

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
    public void UseJsonSerializerOptionsShouldRegisterConfiguredOptions()
    {
        // Arrange
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        var services = new ServiceCollection();
        services.AddSingleton<IDataQueryLogger, NullDataQueryLogger>();
        services.AddFluentCommand(builder => builder
            .UseConnectionString("Server=(local);Database=FluentCommand;Integrated Security=true;TrustServerCertificate=true;")
            .AddProviderFactory(SqlClientFactory.Instance)
            .UseJsonSerializerOptions(options));

        using var provider = services.BuildServiceProvider();

        // Act
        var configuration = provider.GetRequiredService<IDataConfiguration>();
        using var session = configuration.CreateSession();

        // Assert
        configuration.JsonSerializerOptions.Should().BeSameAs(options);
        session.JsonSerializerOptions.Should().BeSameAs(options);
    }

    [Fact]
    public void UseJsonSerializerOptionsFactoryShouldResolveConfiguredOptions()
    {
        // Arrange
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        var services = new ServiceCollection();
        services.AddSingleton(options);
        services.AddSingleton<IDataQueryLogger, NullDataQueryLogger>();
        services.AddFluentCommand(builder => builder
            .UseConnectionString("Server=(local);Database=FluentCommand;Integrated Security=true;TrustServerCertificate=true;")
            .AddProviderFactory(SqlClientFactory.Instance)
            .UseJsonSerializerOptions(sp => sp.GetRequiredService<JsonSerializerOptions>()));

        using var provider = services.BuildServiceProvider();

        // Act
        var configuration = provider.GetRequiredService<IDataConfiguration>();

        // Assert
        configuration.JsonSerializerOptions.Should().BeSameAs(options);
    }

    [Fact]
    public void ReadShouldPassContextDataReaderWhenJsonSerializerOptionsConfigured()
    {
        // Arrange
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        using var session = new DataSession(
            new StubDbConnection(new StubDbDataReader()),
            jsonSerializerOptions: options);

        IDataReader? captured = null;

        // Act
        session.Sql("SELECT 1").Read(reader => captured = reader);

        // Assert
        captured.Should().BeAssignableTo<IDataReaderContext>();
        ((IDataReaderContext)captured!).JsonSerializerOptions.Should().BeSameAs(options);
    }

    [Fact]
    public void ParameterJsonShouldUseConfiguredJsonSerializerOptions()
    {
        // Arrange
        var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        var value = new JsonOptionsModel("Json Options", 42);
        using var session = new DataSession(
            new StubDbConnection(new StubDbDataReader()),
            jsonSerializerOptions: options);

        // Act
        var command = session.Sql("SELECT 1")
            .ParameterJson("@Data", value);

        // Assert
        command.Command.Parameters["@Data"].Value.Should().Be(JsonSerializer.Serialize(value, options));
    }

    [Fact]
    public void ParameterJsonShouldPreferExplicitJsonSerializerOptions()
    {
        // Arrange
        var configuredOptions = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        var explicitOptions = new JsonSerializerOptions { PropertyNamingPolicy = null };
        var value = new JsonOptionsModel("Json Options", 42);
        using var session = new DataSession(
            new StubDbConnection(new StubDbDataReader()),
            jsonSerializerOptions: configuredOptions);

        // Act
        var command = session.Sql("SELECT 1")
            .ParameterJson("@Data", value, explicitOptions);

        // Assert
        command.Command.Parameters["@Data"].Value.Should().Be(JsonSerializer.Serialize(value, explicitOptions));
    }

    [Fact]
    public void SqlBuilderValueJsonShouldUseConfiguredJsonSerializerOptions()
    {
        // Arrange
        var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        var value = new JsonOptionsModel("Json Options", 42);
        using var session = new DataSession(
            new StubDbConnection(new StubDbDataReader()),
            jsonSerializerOptions: options);

        // Act
        var command = session.Sql(query => query
            .Insert()
            .Into("JsonLog")
            .ValueJson("Data", value));

        // Assert
        command.Command.Parameters["@p0000"].Value.Should().Be(JsonSerializer.Serialize(value, options));
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

    private sealed record JsonOptionsModel(string Name, int Count);

#pragma warning disable CS8764, CS8765 // Test stubs mirror framework override signatures across target annotations.

    private sealed class StubDbConnection(DbDataReader reader) : DbConnection
    {
        public override string? ConnectionString { get; set; } = "Server=(local);Database=FluentCommand;";

        public override string Database => "FluentCommand";

        public override string DataSource => "Stub";

        public override string ServerVersion => "1.0";

        private ConnectionState _state = ConnectionState.Closed;

        public override ConnectionState State => _state;

        public override void ChangeDatabase(string databaseName)
        {
        }

        public override void Close() => _state = ConnectionState.Closed;

        public override void Open() => _state = ConnectionState.Open;

        protected override DbTransaction BeginDbTransaction(IsolationLevel isolationLevel) => throw new NotSupportedException();

        protected override DbCommand CreateDbCommand() => new StubDbCommand(this, reader);
    }

    private sealed class StubDbCommand(DbConnection connection, DbDataReader reader) : DbCommand
    {
        public override string? CommandText { get; set; } = string.Empty;

        public override int CommandTimeout { get; set; }

        public override CommandType CommandType { get; set; }

        public override bool DesignTimeVisible { get; set; }

        public override UpdateRowSource UpdatedRowSource { get; set; }

        protected override DbConnection? DbConnection { get; set; } = connection;

        protected override DbParameterCollection DbParameterCollection { get; } = new StubDbParameterCollection();

        protected override DbTransaction? DbTransaction { get; set; }

        public override void Cancel()
        {
        }

        public override int ExecuteNonQuery() => 0;

        public override object? ExecuteScalar() => null;

        public override void Prepare()
        {
        }

        protected override DbParameter CreateDbParameter() => new StubDbParameter();

        protected override DbDataReader ExecuteDbDataReader(CommandBehavior behavior) => reader;
    }

    private sealed class StubDbDataReader : DbDataReader
    {
        public override object this[int ordinal] => throw new NotSupportedException();

        public override object this[string name] => throw new NotSupportedException();

        public override int Depth => 0;

        public override int FieldCount => 0;

        public override bool HasRows => false;

        public override bool IsClosed => false;

        public override int RecordsAffected => 0;

        public override bool GetBoolean(int ordinal) => throw new NotSupportedException();

        public override byte GetByte(int ordinal) => throw new NotSupportedException();

        public override long GetBytes(int ordinal, long dataOffset, byte[]? buffer, int bufferOffset, int length) => throw new NotSupportedException();

        public override char GetChar(int ordinal) => throw new NotSupportedException();

        public override long GetChars(int ordinal, long dataOffset, char[]? buffer, int bufferOffset, int length) => throw new NotSupportedException();

        public override string GetDataTypeName(int ordinal) => throw new NotSupportedException();

        public override DateTime GetDateTime(int ordinal) => throw new NotSupportedException();

        public override decimal GetDecimal(int ordinal) => throw new NotSupportedException();

        public override double GetDouble(int ordinal) => throw new NotSupportedException();

        public override Type GetFieldType(int ordinal) => throw new NotSupportedException();

        public override float GetFloat(int ordinal) => throw new NotSupportedException();

        public override Guid GetGuid(int ordinal) => throw new NotSupportedException();

        public override short GetInt16(int ordinal) => throw new NotSupportedException();

        public override int GetInt32(int ordinal) => throw new NotSupportedException();

        public override long GetInt64(int ordinal) => throw new NotSupportedException();

        public override string GetName(int ordinal) => throw new NotSupportedException();

        public override int GetOrdinal(string name) => throw new NotSupportedException();

        public override string GetString(int ordinal) => throw new NotSupportedException();

        public override object GetValue(int ordinal) => throw new NotSupportedException();

        public override int GetValues(object[] values) => 0;

        public override bool IsDBNull(int ordinal) => throw new NotSupportedException();

        public override bool NextResult() => false;

        public override bool Read() => false;

        public override IEnumerator<object> GetEnumerator() => Enumerable.Empty<object>().GetEnumerator();
    }

    private sealed class StubDbParameterCollection : DbParameterCollection
    {
        private readonly List<DbParameter> _parameters = [];

        public override int Count => _parameters.Count;

        public override object SyncRoot { get; } = new();

        public override int Add(object value)
        {
            _parameters.Add((DbParameter)value);
            return _parameters.Count - 1;
        }

        public override void AddRange(Array values)
        {
            foreach (var value in values)
                Add(value!);
        }

        public override void Clear() => _parameters.Clear();

        public override bool Contains(object value) => _parameters.Contains((DbParameter)value);

        public override bool Contains(string value) => _parameters.Any(p => p.ParameterName == value);

        public override void CopyTo(Array array, int index) => _parameters.ToArray().CopyTo(array, index);

        public override IEnumerator GetEnumerator() => _parameters.GetEnumerator();

        public override int IndexOf(object value) => _parameters.IndexOf((DbParameter)value);

        public override int IndexOf(string parameterName) => _parameters.FindIndex(p => p.ParameterName == parameterName);

        public override void Insert(int index, object value) => _parameters.Insert(index, (DbParameter)value);

        public override void Remove(object value) => _parameters.Remove((DbParameter)value);

        public override void RemoveAt(int index) => _parameters.RemoveAt(index);

        public override void RemoveAt(string parameterName)
        {
            var index = IndexOf(parameterName);
            if (index >= 0)
                RemoveAt(index);
        }

        protected override DbParameter GetParameter(int index) => _parameters[index];

        protected override DbParameter GetParameter(string parameterName) => _parameters[IndexOf(parameterName)];

        protected override void SetParameter(int index, DbParameter value) => _parameters[index] = value;

        protected override void SetParameter(string parameterName, DbParameter value)
        {
            var index = IndexOf(parameterName);
            if (index >= 0)
                _parameters[index] = value;
            else
                _parameters.Add(value);
        }
    }

    private sealed class StubDbParameter : DbParameter
    {
        public override DbType DbType { get; set; }

        public override ParameterDirection Direction { get; set; }

        public override bool IsNullable { get; set; }

        public override string? ParameterName { get; set; } = string.Empty;

        public override int Size { get; set; }

        public override string? SourceColumn { get; set; } = string.Empty;

        public override bool SourceColumnNullMapping { get; set; }

        public override object? Value { get; set; }

        public override void ResetDbType()
        {
        }
    }

#pragma warning restore CS8764, CS8765
}
