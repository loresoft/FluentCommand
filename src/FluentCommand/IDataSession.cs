using System;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;

namespace FluentCommand
{
    /// <summary>
    /// An <see langword="interface"/> for data sessions.
    /// </summary>
    public interface IDataSession : IDisposable
    {
        /// <summary>
        /// Gets the underlying <see cref="DbConnection"/> for the session.
        /// </summary>
        DbConnection Connection { get; }

        /// <summary>
        /// Gets the underlying <see cref="DbTransaction"/> for the session.
        /// </summary>
        DbTransaction Transaction { get; }

        /// <summary>
        /// Gets the underlying <see cref="T:System.Runtime.Caching.ObjectCache"/> for the session.
        /// </summary>
        System.Runtime.Caching.ObjectCache DataCache { get; }


        /// <summary>
        /// Starts a database transaction with the specified isolation level.
        /// </summary>
        /// <param name="isolationLevel">Specifies the isolation level for the transaction.</param>
        /// <returns>An <see cref="IDbTransaction"/> representing the new transaction.</returns>
        IDbTransaction BeginTransaction(IsolationLevel isolationLevel);

        /// <summary>
        /// Starts a data command with the specified SQL.
        /// </summary>
        /// <param name="sql">The SQL statement.</param>
        /// <returns>A fluent <see langword="interface"/> to a data command.</returns>
        IDataCommand Sql(string sql);

        /// <summary>
        /// Starts a data command with the specified stored procedure name.
        /// </summary>
        /// <param name="storedProcedureName">Name of the stored procedure.</param>
        /// <returns>A fluent <see langword="interface"/> to a data command.</returns>
        IDataCommand StoredProcedure(string storedProcedureName);


        /// <summary>
        /// Writes log messages to the logger <see langword="delegate"/>.
        /// </summary>
        /// <param name="logger">The logger <see langword="delegate"/> to write messages to.</param>
        /// <returns>
        /// A fluent <see langword="interface" /> to a data session.
        /// </returns>
        IDataSession Log(Action<string> logger);

        /// <summary>
        /// Set the underlying <see cref="T:System.Runtime.Caching.ObjectCache"/> used to store cached result.
        /// </summary>
        /// <param name="cache">The <see cref="T:System.Runtime.CachingObjectCache"/> used to store cached results.</param>
        /// <returns>
        /// A fluent <see langword="interface" /> to a data session.
        /// </returns>
        IDataSession Cache(System.Runtime.Caching.ObjectCache cache);
        

        /// <summary>
        /// Ensures the connection is open.
        /// </summary>
        void EnsureConnection();

        /// <summary>
        /// Releases the connection.
        /// </summary>
        void ReleaseConnection();

        /// <summary>
        /// Writes the log <paramref name="message"/>.
        /// </summary>
        /// <param name="message">The message to write.</param>
        void WriteLog(string message);

    }
}