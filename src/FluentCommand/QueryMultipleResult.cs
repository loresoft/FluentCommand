using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;
using FluentCommand.Extensions;

namespace FluentCommand
{
    /// <summary>
    /// Query class to wrap multiple results.
    /// </summary>
    internal class QueryMultipleResult : DisposableBase, IDataQuery, IDataQueryAsync
    {
        private readonly DbDataReader _reader;
        private int _readCount;

        /// <summary>
        /// Initializes a new instance of the <see cref="QueryMultipleResult"/> class.
        /// </summary>
        /// <param name="reader">The reader.</param>
        internal QueryMultipleResult(DbDataReader reader)
        {
            _readCount = 0;
            _reader = reader;
        }
        
        /// <summary>
        /// Executes the command against the connection and converts the results to <typeparamref name="TEntity" /> objects.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <param name="factory">The <see langword="delegate" /> factory to convert the <see cref="T:System.Data.IDataReader" /> to <typeparamref name="TEntity" />.</param>
        /// <returns>
        /// An <see cref="T:System.Collections.Generic.IEnumerable`1" /> of <typeparamref name="TEntity" /> objects.
        /// </returns>
        public IEnumerable<TEntity> Query<TEntity>(Func<IDataReader, TEntity> factory)
        {
            NextResult();

            var results = new List<TEntity>();
            while (_reader.Read())
            {
                var entity = factory(_reader);
                results.Add(entity);
            }

            return results;
        }

        /// <summary>
        /// Executes the command against the connection and converts the results to <typeparamref name="TEntity" /> objects asynchronously.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <param name="factory">The <see langword="delegate" /> factory to convert the <see cref="T:System.Data.IDataReader" /> to <typeparamref name="TEntity" />.</param>
        /// <param name="cancellationToken">The cancellation instruction.</param>
        /// <returns>
        /// An <see cref="T:System.Collections.Generic.IEnumerable`1" /> of <typeparamref name="TEntity" /> objects.
        /// </returns>
        public async Task<IEnumerable<TEntity>> QueryAsync<TEntity>(Func<IDataReader, TEntity> factory, CancellationToken cancellationToken = default(CancellationToken))
        {
            await NextResultAsync(cancellationToken).ConfigureAwait(false);

            var results = new List<TEntity>();
            while (await _reader.ReadAsync(cancellationToken).ConfigureAwait(false))
            {
                var entity = factory(_reader);
                results.Add(entity);
            }

            return results;
        }


        /// <summary>
        /// Executes the query and returns the first row in the result as a <typeparamref name="TEntity" /> object.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <param name="factory">The <see langword="delegate" /> factory to convert the <see cref="T:System.Data.IDataReader" /> to <typeparamref name="TEntity" />.</param>
        /// <returns>
        /// A instance of <typeparamref name="TEntity" /> if row exists; otherwise null.
        /// </returns>
        public TEntity QuerySingle<TEntity>(Func<IDataReader, TEntity> factory)
        {
            NextResult();

            var result = _reader.Read()
                ? factory(_reader)
                : default(TEntity);

            return result;
        }

        /// <summary>
        /// Executes the query and returns the first row in the result as a <typeparamref name="TEntity" /> object asynchronously.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <param name="factory">The <see langword="delegate" /> factory to convert the <see cref="T:System.Data.IDataReader" /> to <typeparamref name="TEntity" />.</param>
        /// <param name="cancellationToken">The cancellation instruction.</param>
        /// <returns>
        /// A instance of <typeparamref name="TEntity" /> if row exists; otherwise null.
        /// </returns>
        public async Task<TEntity> QuerySingleAsync<TEntity>(Func<IDataReader, TEntity> factory, CancellationToken cancellationToken = default(CancellationToken))
        {
            await NextResultAsync(cancellationToken).ConfigureAwait(false);

            var result = await _reader.ReadAsync(cancellationToken).ConfigureAwait(false)
                ? factory(_reader)
                : default(TEntity);

            return result;
        }


        /// <summary>
        /// Executes the query and returns the first column of the first row in the result set returned by the query. All other columns and rows are ignored.
        /// </summary>
        /// <typeparam name="TValue">The type of the value.</typeparam>
        /// <param name="convert">The <see langword="delegate" /> to convert the value..</param>
        /// <returns>
        /// The value of the first column of the first row in the result set.
        /// </returns>
        public TValue QueryValue<TValue>(Func<object, TValue> convert)
        {
            NextResult();

            var result = _reader.Read()
                ? _reader.GetValue(0)
                : default(TValue);

            var value = result.ConvertValue(convert);

            return value;
        }

        /// <summary>
        /// Executes the query and returns the first column of the first row in the result set returned by the query asynchronously. All other columns and rows are ignored.
        /// </summary>
        /// <typeparam name="TValue">The type of the value.</typeparam>
        /// <param name="convert">The <see langword="delegate" /> to convert the value..</param>
        /// <param name="cancellationToken">The cancellation instruction.</param>
        /// <returns>
        /// The value of the first column of the first row in the result set.
        /// </returns>
        public async Task<TValue> QueryValueAsync<TValue>(Func<object, TValue> convert, CancellationToken cancellationToken = default(CancellationToken))
        {
            await NextResultAsync(cancellationToken).ConfigureAwait(false);

            var result = await _reader.ReadAsync(cancellationToken).ConfigureAwait(false)
                ? _reader.GetValue(0)
                : default(TValue);

            var value = result.ConvertValue(convert);

            return value;
        }


        /// <summary>
        /// Executes the command against the connection and converts the results to a <see cref="DataTable" />.
        /// </summary>
        /// <returns>
        /// A <see cref="DataTable" /> of the results.
        /// </returns>
        public DataTable QueryTable()
        {
            NextResult();

            var dataTable = new DataTable();
            dataTable.Load(_reader);

            return dataTable;
        }

        /// <summary>
        /// Executes the command against the connection and converts the results to a <see cref="DataTable" /> asynchronously.
        /// </summary>
        /// <param name="cancellationToken">The cancellation instruction.</param>
        /// <returns>
        /// A <see cref="DataTable" /> of the results.
        /// </returns>
        public async Task<DataTable> QueryTableAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            await NextResultAsync(cancellationToken).ConfigureAwait(false);

            var dataTable = new DataTable();
            dataTable.Load(_reader);

            return dataTable;
        }


        private void NextResult()
        {
            if (_readCount > 0)
            {
                bool hasNextResult = _reader.NextResult();
                if (!hasNextResult)
                    throw new InvalidOperationException("The data reader could not advance to the next result.");
            }

            _readCount++;
        }

        private async Task NextResultAsync(CancellationToken cancellationToken)
        {
            if (_readCount > 0)
            {
                bool hasNextResult = await _reader.NextResultAsync(cancellationToken).ConfigureAwait(false);
                if (!hasNextResult)
                    throw new InvalidOperationException("The data reader could not advance to the next result.");
            }

            _readCount++;
        }

    }
}