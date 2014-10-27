using System;
using System.Collections.Generic;
using System.Data;

namespace FluentCommand
{
    /// <summary>
    /// An <see langword="interface"/> defining a data query operations.
    /// </summary>
    public interface IDataQuery : IDisposable
    {
        /// <summary>
        /// Executes the command against the connection and converts the results to dynamic objects.
        /// </summary>
        /// <returns>An <see cref="T:System.Collections.Generic.IEnumerable`1"/> of dynamic objects.</returns>
        IEnumerable<dynamic> Query();

        /// <summary>
        /// Executes the command against the connection and converts the results to <typeparamref name="TEntity" /> objects.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <returns>
        /// An <see cref="T:System.Collections.Generic.IEnumerable`1" /> of <typeparamref name="TEntity" /> objects.
        /// </returns>
        IEnumerable<TEntity> Query<TEntity>() where TEntity : class, new();

        /// <summary>
        /// Executes the command against the connection and converts the results to <typeparamref name="TEntity" /> objects.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <param name="factory">The <see langword="delegate"/> factory to convert the <see cref="T:System.Data.IDataReader" /> to <typeparamref name="TEntity" />.</param>
        /// <returns>
        /// An <see cref="T:System.Collections.Generic.IEnumerable`1" /> of <typeparamref name="TEntity" /> objects.
        /// </returns>
        IEnumerable<TEntity> Query<TEntity>(Func<IDataReader, TEntity> factory) where TEntity : class;

        /// <summary>
        /// Executes the query and returns the first row in the result as a dynamic object.
        /// </summary>
        /// <returns>A instance of a dynamic object if row exists; otherwise null.</returns>
        dynamic QuerySingle();

        /// <summary>
        /// Executes the query and returns the first row in the result as a <typeparamref name="TEntity" /> object.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <returns>A instance of <typeparamref name="TEntity" /> if row exists; otherwise null.</returns>
        TEntity QuerySingle<TEntity>() where TEntity : class, new();

        /// <summary>
        /// Executes the query and returns the first row in the result as a <typeparamref name="TEntity" /> object.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <param name="factory">The <see langword="delegate"/> factory to convert the <see cref="T:System.Data.IDataReader" /> to <typeparamref name="TEntity" />.</param>
        /// <returns>
        /// A instance of <typeparamref name="TEntity" /> if row exists; otherwise null.
        /// </returns>
        TEntity QuerySingle<TEntity>(Func<IDataReader, TEntity> factory) where TEntity : class;

        /// <summary>
        /// Executes the command against the connection and converts the results to a <see cref="DataTable"/>.
        /// </summary>
        /// <returns>A <see cref="DataTable"/> of the results.</returns>
        DataTable QueryTable();
    }
}