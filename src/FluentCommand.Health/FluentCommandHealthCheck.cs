using System.Data.Common;

using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;

namespace FluentCommand.Health;

/// <summary>
/// An <see cref="IHealthCheck"/> implementation that verifies database connectivity
/// by executing a SQL command through a <see cref="IDataSessionFactory"/>.
/// </summary>
public sealed class FluentCommandHealthCheck : IHealthCheck
{
    private readonly IDataSessionFactory _sessionFactory;
    private readonly ILogger<FluentCommandHealthCheck> _logger;

    private readonly string _commandText;

    /// <summary>
    /// Initializes a new instance of <see cref="FluentCommandHealthCheck"/>.
    /// </summary>
    /// <param name="logger">The logger used to record health check errors.</param>
    /// <param name="sessionFactory">The factory used to create database sessions.</param>
    /// <param name="commandText">
    /// The SQL command text to execute as the health check query.
    /// Defaults to <c>SELECT 1</c> when <see langword="null"/>.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="logger"/> or <paramref name="sessionFactory"/> is <see langword="null"/>.
    /// </exception>
    public FluentCommandHealthCheck(
        ILogger<FluentCommandHealthCheck> logger,
        IDataSessionFactory sessionFactory,
        string? commandText = null)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _sessionFactory = sessionFactory ?? throw new ArgumentNullException(nameof(sessionFactory));
        _commandText = commandText ?? "SELECT 1";
    }

    /// <summary>
    /// Runs the health check by opening a database session and executing the configured SQL command.
    /// </summary>
    /// <param name="context">The health check context providing registration metadata.</param>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    /// <returns>
    /// A <see cref="HealthCheckResult"/> indicating <see cref="HealthStatus.Healthy"/> when the query succeeds,
    /// or the <see cref="HealthCheckRegistration.FailureStatus"/> when an exception is thrown.
    /// </returns>
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        string? serverName = null;
        string? databaseName = null;

        try
        {

#if NETCOREAPP3_0_OR_GREATER
            await using var session = _sessionFactory.CreateSession();
#else
            using var session = _sessionFactory.CreateSession();
#endif

            (serverName, databaseName) = ParseConnectionDetails(session.Connection.ConnectionString);

            await session
                .Sql(_commandText)
                .ExecuteAsync(cancellationToken);

            string description = BuildHealthMessage("Connected to", serverName, databaseName);
            return HealthCheckResult.Healthy(description);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error checking SQL health for server '{ServerName}' and database '{DatabaseName}': {Message}",
                serverName, databaseName, ex.Message);

            var description = serverName is null || databaseName is null
                ? "SQL health check failed."
                : BuildHealthMessage("Failed to connect to", serverName, databaseName);

            return new HealthCheckResult(context.Registration.FailureStatus, description, ex);
        }
    }


    /// <summary>
    /// Parses the server name and database name from a connection string.
    /// </summary>
    /// <param name="connectionString">The connection string to parse.</param>
    /// <returns>A tuple containing the <c>ServerName</c> and <c>DatabaseName</c> extracted from the connection string.</returns>
    private static (string? ServerName, string? DatabaseName) ParseConnectionDetails(string connectionString)
    {
        var builder = new DbConnectionStringBuilder
        {
            ConnectionString = connectionString
        };

        return
        (
            ServerName: GetConnectionStringValue(builder, "Server", "Data Source", "Address", "Host", "Network Address"),
            DatabaseName: GetConnectionStringValue(builder, "Database", "Initial Catalog")
        );
    }

    /// <summary>
    /// Retrieves the first non-empty value from a <see cref="DbConnectionStringBuilder"/> matching any of the specified keys.
    /// </summary>
    /// <param name="builder">The connection string builder to query.</param>
    /// <param name="keys">An ordered list of candidate keys to look up.</param>
    /// <returns>The first matching value, or <c>"unknown"</c> if none is found.</returns>
    private static string GetConnectionStringValue(DbConnectionStringBuilder builder, params string[] keys)
    {
        foreach (var key in keys)
        {
            if (!builder.TryGetValue(key, out var value) || value is null)
                continue;

            var text = value.ToString();
            if (!string.IsNullOrWhiteSpace(text))
                return text;
        }

        return "unknown";
    }

    /// <summary>
    /// Builds a human-readable health check message describing the target server and database.
    /// </summary>
    /// <param name="prefix">A verb phrase to begin the message (e.g., <c>"Connected to"</c> or <c>"Failed to connect to"</c>).</param>
    /// <param name="serverName">The name of the server.</param>
    /// <param name="databaseName">The name of the database.</param>
    /// <returns>A formatted message string.</returns>
    private static string BuildHealthMessage(string prefix, string? serverName, string? databaseName)
    {
        return $"{prefix} server '{serverName}' and database '{databaseName}'.";
    }
}
