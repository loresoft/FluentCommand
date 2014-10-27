using System;
using System.Collections.Generic;
using System.Data;

namespace FluentCommand
{
    /// <summary>
    /// Query class to wrap multiple results.
    /// </summary>
    internal class QueryMultipleResult : DisposableBase, IDataQuery
    {
        private readonly IDataReader _reader;
        private int _readCount;

        /// <summary>
        /// Initializes a new instance of the <see cref="QueryMultipleResult"/> class.
        /// </summary>
        /// <param name="reader">The reader.</param>
        internal QueryMultipleResult(IDataReader reader)
        {
            _readCount = 0;
            _reader = reader;
        }


        /// <summary>
        /// Executes the command against the connection and converts the results to dynamic objects.
        /// </summary>
        /// <returns>
        /// An <see cref="T:System.Collections.Generic.IEnumerable`1" /> of dynamic objects.
        /// </returns>
        public IEnumerable<dynamic> Query()
        {
            return Query(DataFactory.DynamicFactory);
        }

        /// <summary>
        /// Executes the command against the connection and converts the results to <typeparamref name="TEntity" /> objects.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <returns>
        /// An <see cref="T:System.Collections.Generic.IEnumerable`1" /> of <typeparamref name="TEntity" /> objects.
        /// </returns>
        public IEnumerable<TEntity> Query<TEntity>() where TEntity : class, new()
        {
            return Query(DataFactory.EntityFactory<TEntity>);
        }

        /// <summary>
        /// Executes the command against the connection and converts the results to <typeparamref name="TEntity" /> objects.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <param name="factory">The <see langword="delegate" /> factory to convert the <see cref="T:System.Data.IDataReader" /> to <typeparamref name="TEntity" />.</param>
        /// <returns>
        /// An <see cref="T:System.Collections.Generic.IEnumerable`1" /> of <typeparamref name="TEntity" /> objects.
        /// </returns>
        public IEnumerable<TEntity> Query<TEntity>(Func<IDataReader, TEntity> factory) where TEntity : class
        {
            NextResult();

            var results = new List<TEntity>();
            while (_reader.Read())
                results.Add(factory(_reader));

            return results;
        }


        /// <summary>
        /// Executes the query and returns the first row in the result as a dynamic object.
        /// </summary>
        /// <returns>
        /// A instance of a dynamic object if row exists; otherwise null.
        /// </returns>
        public dynamic QuerySingle()
        {
            return QuerySingle(DataFactory.DynamicFactory);
        }

        /// <summary>
        /// Executes the query and returns the first row in the result as a <typeparamref name="TEntity" /> object.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <returns>
        /// A instance of <typeparamref name="TEntity" /> if row exists; otherwise null.
        /// </returns>
        public TEntity QuerySingle<TEntity>() where TEntity : class, new()
        {
            return QuerySingle(DataFactory.EntityFactory<TEntity>);
        }

        /// <summary>
        /// Executes the query and returns the first row in the result as a <typeparamref name="TEntity" /> object.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <param name="factory">The <see langword="delegate" /> factory to convert the <see cref="T:System.Data.IDataReader" /> to <typeparamref name="TEntity" />.</param>
        /// <returns>
        /// A instance of <typeparamref name="TEntity" /> if row exists; otherwise null.
        /// </returns>
        public TEntity QuerySingle<TEntity>(Func<IDataReader, TEntity> factory) where TEntity : class
        {
            NextResult();

            var result = _reader.Read()
                ? factory(_reader)
                : default(TEntity);

            return result;
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


        private void NextResult()
        {
            if (_readCount > 0)
            {
                if (!_reader.NextResult())
                    throw new InvalidOperationException("The data reader could not advance to the next result.");
            }

            _readCount++;
        }
    }
}