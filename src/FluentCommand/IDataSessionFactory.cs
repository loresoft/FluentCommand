using System.Data.Common;

namespace FluentCommand;

/// <summary>
/// An interface for creating <see cref="IDataSession"/> instances
/// </summary>
public interface IDataSessionFactory
{
    /// <summary>
    /// Creates a new data session from this database configuration
    /// </summary>
    /// <param name="connectionString">The connection string to use for the session.  If <paramref name="connectionString"/> is <c>null</c>, <see cref="ConnectionString"/> will be used.</param>
    /// <returns>
    /// A new <see cref="IDataSession" /> instance.
    /// </returns>
    IDataSession CreateSession(string connectionString = null);

    /// <summary>
    /// Creates a new data session from this database configuration
    /// </summary>
    /// <param name="transaction">The transaction to create the session with.</param>
    /// <returns>
    /// A new <see cref="IDataSession" /> instance.
    /// </returns>
    IDataSession CreateSession(DbTransaction transaction);
}

/// <summary>
/// The data session factory by discriminator.  Used to register multiple instances of IDataSessionFactory.
/// </summary>
/// <typeparam name="TDiscriminator">The type of the discriminator.</typeparam>
public interface IDataSessionFactory<TDiscriminator> : IDataSessionFactory
{

}
