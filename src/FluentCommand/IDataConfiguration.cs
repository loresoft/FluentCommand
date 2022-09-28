using System;
using System.Data.Common;

using FluentCommand.Query.Generators;

namespace FluentCommand;

/// <summary>
/// An interface for database configuration
/// </summary>
public interface IDataConfiguration
{
    /// <summary>
    /// Gets the database provider factory.
    /// </summary>
    /// <value>
    /// The provider factory.
    /// </value>
    DbProviderFactory ProviderFactory { get; }

    /// <summary>
    /// Gets the database connection string.
    /// </summary>
    /// <value>
    /// The connection string.
    /// </value>
    string ConnectionString { get; }

    /// <summary>
    /// Gets the current logger delegate.
    /// </summary>
    /// <value>
    /// The current logger delegate.
    /// </value>
    Action<string> Logger { get; }

    /// <summary>
    /// Gets the data cache manager.
    /// </summary>
    /// <value>
    /// The data cache manager.
    /// </value>
    IDataCache DataCache { get; }

    /// <summary>
    /// Gets the query generator provider.
    /// </summary>
    /// <value>
    /// The query generator provider.
    /// </value>
    IQueryGenerator QueryGenerator { get; }

    /// <summary>
    /// Creates a new data session from this database configuration
    /// </summary>
    /// <returns>
    /// A new <see cref="IDataSession" /> instance.
    /// </returns>
    IDataSession CreateSession();

    /// <summary>
    /// Creates a new <see cref="DbConnection" /> instance from this database configuration.
    /// </summary>
    /// <returns>
    /// A new <see cref="DbConnection" /> instance.
    /// </returns>
    DbConnection CreateConnection();
}
