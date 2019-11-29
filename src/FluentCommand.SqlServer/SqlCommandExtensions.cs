using System;
using System.Collections.Generic;
using System.Data;
using FluentCommand.Extensions;
using Microsoft.Data.SqlClient;

namespace FluentCommand
{
    /// <summary>
    /// Extension methods for <see cref="IDataCommand"/> specific to Sql Server.
    /// </summary>
    public static class SqlCommandExtensions
    {
        /// <summary>
        /// Adds a new Sql Server structured table-valued parameter with the specified <paramref name="name" /> and <paramref name="data" />.
        /// </summary>
        /// <param name="dataCommand">The <see cref="IDataCommand"/> for this extension method.</param>
        /// <param name="name">The name of the parameter.</param>
        /// <param name="data">The data to be added.</param>
        /// <returns>
        /// A fluent <see langword="interface" /> to the data command.
        /// </returns>
        public static IDataCommand SqlParameter<T>(this IDataCommand dataCommand, string name, IEnumerable<T> data)
        {
            var dataTable = data.ToDataTable<T>();
            return SqlParameter(dataCommand, name, dataTable);
        }

        /// <summary>
        /// Adds a new Sql Server structured table-valued parameter with the specified <paramref name="name" /> and <paramref name="dataTable" />.
        /// </summary>
        /// <param name="dataCommand">The <see cref="IDataCommand"/> for this extension method.</param>
        /// <param name="name">The name of the parameter.</param>
        /// <param name="dataTable">The <see cref="DataTable"/> to be added.</param>
        /// <returns>
        /// A fluent <see langword="interface" /> to the data command.
        /// </returns>
        public static IDataCommand SqlParameter(this IDataCommand dataCommand, string name, DataTable dataTable)
        {
            var parameter = dataCommand.Command.CreateParameter();
            var sqlParameter = parameter as SqlParameter;
            if (sqlParameter == null)
                throw new InvalidOperationException(
                    "SqlParameter only supported by SQL Server.  Make sure DataSession was created with a valid SqlConnection.");

            sqlParameter.ParameterName = name;
            sqlParameter.Value = dataTable;
            sqlParameter.Direction = ParameterDirection.Input;
            sqlParameter.SqlDbType = SqlDbType.Structured;

            return dataCommand.Parameter(sqlParameter);
        }

    }
}
