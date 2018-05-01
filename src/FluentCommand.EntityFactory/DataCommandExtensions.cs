using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FluentCommand
{
    /// <summary>
    /// Extension methods for <see cref="IDataCommand"/>
    /// </summary>
    public static class DataCommandExtensions
    {
        /// <summary>
        /// Executes the command against the connection and converts the results to dynamic objects.
        /// </summary>
        /// <param name="dataQuery">The <see cref="IDataQuery"/> for this extension method.</param>
        /// <returns>
        /// An <see cref="T:System.Collections.Generic.IEnumerable`1" /> of dynamic objects.
        /// </returns>
        public static IEnumerable<dynamic> Query(this IDataQuery dataQuery)
        {
            return dataQuery.Query(ReaderFactory.DynamicFactory);
        }

        /// <summary>
        /// Executes the command against the connection and converts the results to <typeparamref name="TEntity" /> objects.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <param name="dataQuery">The <see cref="IDataQuery"/> for this extension method.</param>
        /// <returns>
        /// An <see cref="T:System.Collections.Generic.IEnumerable`1" /> of <typeparamref name="TEntity" /> objects.
        /// </returns>
        public static IEnumerable<TEntity> Query<TEntity>(this IDataQuery dataQuery)
            where TEntity : class, new()
        {
            return dataQuery.Query(ReaderFactory.EntityFactory<TEntity>);
        }


        /// <summary>
        /// Executes the query and returns the first row in the result as a dynamic object.
        /// </summary>
        /// <param name="dataQuery">The <see cref="IDataQuery"/> for this extension method.</param>
        /// <returns>
        /// A instance of a dynamic object if row exists; otherwise null.
        /// </returns>
        public static dynamic QuerySingle(this IDataQuery dataQuery)
        {
            return dataQuery.QuerySingle(ReaderFactory.DynamicFactory);
        }

        /// <summary>
        /// Executes the query and returns the first row in the result as a <typeparamref name="TEntity" /> object.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <param name="dataQuery">The <see cref="IDataQuery"/> for this extension method.</param>
        /// <returns>
        /// A instance of <typeparamref name="TEntity" /> if row exists; otherwise null.
        /// </returns>
        public static TEntity QuerySingle<TEntity>(this IDataQuery dataQuery)
            where TEntity : class, new()
        {
            return dataQuery.QuerySingle(ReaderFactory.EntityFactory<TEntity>);
        }

    }
}
