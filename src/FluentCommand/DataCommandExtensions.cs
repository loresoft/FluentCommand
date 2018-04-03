using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using FluentCommand.Extensions;

namespace FluentCommand
{
    /// <summary>
    /// Extension methods for <see cref="IDataCommand"/>
    /// </summary>
    public static class DataCommandExtensions
    {
        /// <summary>
        /// Adds the parameters to the underlying command.
        /// </summary>
        /// <param name="dataCommand">The <see cref="IDataCommand"/> for this extension method.</param>
        /// <param name="parameters">The <see cref="T:IEnumerable`1" /> of <see cref="T:DbParameter" />.</param>
        /// <returns>
        /// A fluent <see langword="interface" /> to the data command.
        /// </returns>
        public static IDataCommand Parameter(this IDataCommand dataCommand, IEnumerable<DbParameter> parameters)
        {
            foreach (var parameter in parameters)
                dataCommand.Parameter(parameter);

            return dataCommand;
        }

        /// <summary>
        /// Adds a new parameter with the specified <paramref name="name" /> and <paramref name="value" />.
        /// </summary>
        /// <typeparam name="TParameter">The type of the parameter value.</typeparam>
        /// <param name="dataCommand">The <see cref="IDataCommand"/> for this extension method.</param>
        /// <param name="name">The name of the parameter.</param>
        /// <param name="value">The value to be added.</param>
        /// <returns>
        /// A fluent <see langword="interface" /> to the data command.
        /// </returns>
        public static IDataCommand Parameter<TParameter>(this IDataCommand dataCommand, string name, TParameter value)
        {
            // convert to object
            object innerValue = value;

            // handle value type by using actual value
            var valueType = value != null ? value.GetType() : typeof(TParameter);

            var parameter = dataCommand.Command.CreateParameter();
            parameter.ParameterName = name;
            parameter.Value = innerValue ?? DBNull.Value;
            parameter.DbType = valueType.GetUnderlyingType().ToDbType();
            parameter.Direction = ParameterDirection.Input;

            return dataCommand.Parameter(parameter);
        }

        /// <summary>
        /// Adds a new parameter with the <see cref="IDataParameter" /> fluent object.
        /// </summary>
        /// <typeparam name="TParameter"></typeparam>
        /// <param name="dataCommand">The <see cref="IDataCommand"/> for this extension method.</param>
        /// <param name="configurator">The <see langword="delegate" />  to configurator the <see cref="IDataParameter" />.</param>
        /// <returns>
        /// A fluent <see langword="interface" /> to the data command.
        /// </returns>
        public static IDataCommand Parameter<TParameter>(this IDataCommand dataCommand, Action<IDataParameter<TParameter>> configurator)
        {
            var parameter = dataCommand.Command.CreateParameter();

            var dataParameter = new DataParameter<TParameter>(dataCommand, parameter);
            configurator(dataParameter);

            return dataCommand.Parameter(parameter);
        }


        /// <summary>
        /// Adds a new out parameter with the specified <paramref name="name" /> and <paramref name="callback" />.
        /// </summary>
        /// <typeparam name="TParameter">The type of the parameter value.</typeparam>
        /// <param name="dataCommand">The <see cref="IDataCommand"/> for this extension method.</param>
        /// <param name="name">The name of the parameter.</param>
        /// <param name="callback">The callback used to get the out value.</param>
        /// <returns>
        /// A fluent <see langword="interface" /> to the data command.
        /// </returns>
        public static IDataCommand ParameterOut<TParameter>(this IDataCommand dataCommand, string name, Action<TParameter> callback)
        {
            var parameter = dataCommand.Command.CreateParameter();
            parameter.ParameterName = name;
            parameter.DbType = typeof(TParameter).GetUnderlyingType().ToDbType();
            parameter.Direction = ParameterDirection.Output;
            // output parameters must have a size, default to MAX
            parameter.Size = -1;

            dataCommand.RegisterCallback(parameter, callback);
            dataCommand.Parameter(parameter);

            return dataCommand;
        }

        /// <summary>
        /// Adds a new out parameter with the specified <paramref name="name" />, <paramref name="value" /> and <paramref name="callback" />.
        /// </summary>
        /// <typeparam name="TParameter">The type of the parameter value.</typeparam>
        /// <param name="dataCommand">The <see cref="IDataCommand"/> for this extension method.</param>
        /// <param name="name">The name of the parameter.</param>
        /// <param name="value">The value to be added.</param>
        /// <param name="callback">The callback used to get the out value.</param>
        /// <returns>
        /// A fluent <see langword="interface" /> to the data command.
        /// </returns>
        public static IDataCommand ParameterOut<TParameter>(this IDataCommand dataCommand, string name, TParameter value, Action<TParameter> callback)
        {
            object innerValue = value;

            var parameter = dataCommand.Command.CreateParameter();
            parameter.ParameterName = name;
            parameter.Value = innerValue ?? DBNull.Value;
            parameter.DbType = typeof(TParameter).GetUnderlyingType().ToDbType();
            parameter.Direction = ParameterDirection.InputOutput;

            dataCommand.RegisterCallback(parameter, callback);
            dataCommand.Parameter(parameter);

            return dataCommand;
        }


        /// <summary>
        /// Adds a new return parameter with the specified <paramref name="callback" />.
        /// </summary>
        /// <typeparam name="TParameter">The type of the parameter value.</typeparam>
        /// <param name="dataCommand">The <see cref="IDataCommand"/> for this extension method.</param>
        /// <param name="callback">The callback used to get the return value.</param>
        /// <returns>
        /// A fluent <see langword="interface" /> to the data command.
        /// </returns>
        public static IDataCommand Return<TParameter>(this IDataCommand dataCommand, Action<TParameter> callback)
        {
            const string parameterName = "@ReturnValue";

            var parameter = dataCommand.Command.CreateParameter();
            parameter.ParameterName = parameterName;
            parameter.DbType = typeof(TParameter).GetUnderlyingType().ToDbType();
            parameter.Direction = ParameterDirection.ReturnValue;

            dataCommand.RegisterCallback(parameter, callback);
            dataCommand.Parameter(parameter);

            return dataCommand;
        }


        /// <summary>
        /// Executes the command against the connection and converts the results to dynamic objects.
        /// </summary>
        /// <param name="dataQuery">The <see cref="IDataQuery"/> for this extension method.</param>
        /// <returns>
        /// An <see cref="T:System.Collections.Generic.IEnumerable`1" /> of dynamic objects.
        /// </returns>
        public static IEnumerable<dynamic> Query(this IDataQuery dataQuery)
        {
            return dataQuery.Query(DataFactory.DynamicFactory);
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
            return dataQuery.Query(DataFactory.EntityFactory<TEntity>);
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
            return dataQuery.QuerySingle(DataFactory.DynamicFactory);
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
            return dataQuery.QuerySingle(DataFactory.EntityFactory<TEntity>);
        }


        /// <summary>
        /// Executes the query and returns the first column of the first row in the result set returned by the query. All other columns and rows are ignored.
        /// </summary>
        /// <typeparam name="TValue">The type of the value.</typeparam>
        /// <param name="dataQuery">The <see cref="IDataQuery"/> for this extension method.</param>
        /// <returns>
        /// The value of the first column of the first row in the result set.
        /// </returns>
        public static TValue QueryValue<TValue>(this IDataQuery dataQuery)
        {
            return dataQuery.QueryValue<TValue>(null);
        }

    }
}
