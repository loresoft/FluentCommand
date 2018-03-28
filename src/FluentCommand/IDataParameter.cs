using System;
using System.Data;

namespace FluentCommand
{
    /// <summary>
    /// An <see langword="interface"/> for data parameter.
    /// </summary>
    /// <typeparam name="TValue">The type of the parameter value.</typeparam>
    public interface IDataParameter<TValue>
    {
        /// <summary>
        /// Sets the name of the parameter.
        /// </summary>
        /// <param name="parameterName">Name of the parameter.</param>
        /// <returns>A fluent <see langword="interface"/> to a data command parameter.</returns>
        IDataParameter<TValue> Name(string parameterName);

        /// <summary>
        /// Sets the value of the parameter.
        /// </summary>
        /// <param name="value">The value of the parameter.</param>
        /// <returns>A fluent <see langword="interface"/> to a data command parameter.</returns>
        IDataParameter<TValue> Value(TValue value);

        /// <summary>
        /// Sets a value that indicates whether the parameter is input-only, output-only, bidirectional, or a stored procedure return value.
        /// </summary>
        /// <param name="parameterDirection">The parameter direction.</param>
        /// <returns>A fluent <see langword="interface"/> to a data command parameter.</returns>
        IDataParameter<TValue> Direction(ParameterDirection parameterDirection);

        /// <summary>
        /// Sets the <see cref="DbType"/> of the parameter. 
        /// </summary>
        /// <param name="dbType">The <see cref="DbType"/> of the parameter.</param>
        /// <returns>A fluent <see langword="interface"/> to a data command parameter.</returns>
        IDataParameter<TValue> Type(DbType dbType);

        /// <summary>
        /// Sets the the maximum size of the data within the parameter.
        /// </summary>
        /// <param name="size">The maximum size of the data within the parameter.</param>
        /// <returns>A fluent <see langword="interface"/> to a data command parameter.</returns>
        IDataParameter<TValue> Size(int size);

        /// <summary>
        /// Sets the parameter direction to Output and registers the call back to get the value.
        /// </summary>
        /// <param name="callback">The callback used to get the out value.</param>
        /// <returns>A fluent <see langword="interface"/> to a data command parameter.</returns>
        IDataParameter<TValue> Output(Action<TValue> callback);

        /// <summary>
        /// Sets the parameter direction to ReturnValue and registers the call back to get the return value.
        /// </summary>
        /// <param name="callback">The callback used to get the return value.</param>
        /// <returns>A fluent <see langword="interface"/> to a data command parameter.</returns>
        IDataParameter<TValue> Return(Action<TValue> callback);
    }
}