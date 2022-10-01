using System.Data;

namespace FluentCommand;

/// <summary>
/// An <see langword="interface"/> defining a data query operations.
/// </summary>
public interface IDataQuery : IDisposable
{
    /// <summary>
    /// Executes the command against the connection and converts the results to <typeparamref name="TEntity" /> objects.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    /// <param name="factory">The <see langword="delegate"/> factory to convert the <see cref="T:System.Data.IDataReader" /> to <typeparamref name="TEntity" />.</param>
    /// <returns>
    /// An <see cref="T:System.Collections.Generic.IEnumerable`1" /> of <typeparamref name="TEntity" /> objects.
    /// </returns>
    IEnumerable<TEntity> Query<TEntity>(Func<IDataReader, TEntity> factory);

    /// <summary>
    /// Executes the query and returns the first row in the result as a <typeparamref name="TEntity" /> object.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    /// <param name="factory">The <see langword="delegate"/> factory to convert the <see cref="T:System.Data.IDataReader" /> to <typeparamref name="TEntity" />.</param>
    /// <returns>
    /// A instance of <typeparamref name="TEntity" /> if row exists; otherwise null.
    /// </returns>
    TEntity QuerySingle<TEntity>(Func<IDataReader, TEntity> factory);

    /// <summary>
    /// Executes the query and returns the first column of the first row in the result set returned by the query. All other columns and rows are ignored.
    /// </summary>
    /// <typeparam name="TValue">The type of the value.</typeparam>
    /// <param name="convert">The <see langword="delegate"/> to convert the value..</param>
    /// <returns>
    /// The value of the first column of the first row in the result set.
    /// </returns>
    TValue QueryValue<TValue>(Func<object, TValue> convert);

    /// <summary>
    /// Executes the command against the connection and converts the results to a <see cref="DataTable"/>.
    /// </summary>
    /// <returns>A <see cref="DataTable"/> of the results.</returns>
    DataTable QueryTable();
}
