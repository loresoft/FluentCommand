using System.Data.Common;

using FluentCommand.Query.Generators;

namespace FluentCommand;

/// <summary>
/// An interface for database configuration
/// </summary>
public interface IDataConfiguration : IDataSessionFactory
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
    /// Gets the data command query logger.
    /// </summary>
    /// <value>
    /// The data command query logger.
    /// </value>
    IDataQueryLogger QueryLogger { get; }

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
    /// Creates a new <see cref="DbConnection" /> instance from this database configuration.
    /// </summary>
    /// <returns>
    /// <param name="connectionString">The connection string to use for the session.  If <paramref name="connectionString"/> is <c>null</c>, <see cref="ConnectionString"/> will be used.</param>
    /// A new <see cref="DbConnection" /> instance.
    /// </returns>
    DbConnection CreateConnection(string connectionString = null);
}

/// <summary>
/// The database configuration by discriminator.  Used to register multiple instances of IDataConfiguration.
/// </summary>
/// <typeparam name="TDiscriminator">The type of the discriminator.</typeparam>
public interface IDataConfiguration<TDiscriminator> : IDataConfiguration, IDataSessionFactory<TDiscriminator>
{

}
