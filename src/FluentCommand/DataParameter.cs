using System.Data;
using System.Data.Common;

using FluentCommand.Extensions;


namespace FluentCommand;

/// <summary>
/// A fluent class to build a data parameter.
/// </summary>
/// <typeparam name="TValue">The type of the value.</typeparam>
public class DataParameter<TValue> : IDataParameter<TValue>
{
    private readonly IDataCommand _dataCommand;
    private readonly DbParameter _parameter;

    // flags to help parameter property defaults
    private bool _hasValue;
    private bool _hasDirection;
    private bool _hasType;
    private bool _hasSize;

    /// <summary>
    /// Initializes a new instance of the <see cref="DataParameter{TValue}" /> class.
    /// </summary>
    /// <param name="dataCommand">The data command.</param>
    /// <param name="parameter">The parameter.</param>
    internal DataParameter(IDataCommand dataCommand, DbParameter parameter)
    {
        _dataCommand = dataCommand;
        _parameter = parameter;
    }

    /// <summary>
    /// Sets the name of the parameter.
    /// </summary>
    /// <param name="parameterName">Name of the parameter.</param>
    /// <returns>A fluent <see langword="interface"/> to a data command parameter.</returns>
    public IDataParameter<TValue> Name(string parameterName)
    {
        _parameter.ParameterName = parameterName;
        return this;
    }

    /// <summary>
    /// Sets the value of the parameter.
    /// </summary>
    /// <param name="value">The value of the parameter.</param>
    /// <returns>A fluent <see langword="interface"/> to a data command parameter.</returns>
    public IDataParameter<TValue> Value(TValue value)
    {
        object innerValue = value;

        // guess type only if not already set
        if (!_hasType)
        {
            // handle value type by using actual value
            Type valueType = value != null ? value.GetType() : typeof(TValue);

            _parameter.DbType = valueType.GetUnderlyingType().ToDbType();
        }

        _parameter.Value = innerValue ?? DBNull.Value;

        _hasValue = true;

        return this;
    }

    /// <summary>
    /// Sets a value that indicates whether the parameter is input-only, output-only, bidirectional, or a stored procedure return value.
    /// </summary>
    /// <param name="parameterDirection">The parameter direction.</param>
    /// <returns>A fluent <see langword="interface"/> to a data command parameter.</returns>
    public IDataParameter<TValue> Direction(ParameterDirection parameterDirection)
    {
        _parameter.Direction = parameterDirection;
        _hasDirection = true;
        return this;
    }

    /// <summary>
    /// Sets the <see cref="DbType"/> of the parameter. 
    /// </summary>
    /// <param name="dbType">The <see cref="DbType"/> of the parameter.</param>
    /// <returns>A fluent <see langword="interface"/> to a data command parameter.</returns>
    public IDataParameter<TValue> Type(DbType dbType)
    {
        _parameter.DbType = dbType;
        _hasType = true;
        return this;
    }

    /// <summary>
    /// Sets the the maximum size of the data within the parameter.
    /// </summary>
    /// <param name="size">The maximum size of the data within the parameter.</param>
    /// <returns>A fluent <see langword="interface"/> to a data command parameter.</returns>
    public IDataParameter<TValue> Size(int size)
    {
        _parameter.Size = size;
        _hasSize = true;
        return this;
    }

    /// <summary>
    /// Sets the parameter direction to Output and registers the call back to get the value.
    /// </summary>
    /// <param name="callback">The callback used to get the out value.</param>
    /// <returns>A fluent <see langword="interface"/> to a data command parameter.</returns>
    public IDataParameter<TValue> Output(Action<TValue> callback)
    {
        // set direction output only if not already set
        if (!_hasDirection)
            _parameter.Direction = _hasValue ? ParameterDirection.InputOutput : ParameterDirection.Output;

        // output parameters must have a size, default to MAX
        if (!_hasSize)
            _parameter.Size = -1;

        _dataCommand.RegisterCallback(_parameter, callback);

        return this;
    }

    /// <summary>
    /// Sets the parameter direction to ReturnValue and registers the call back to get the return value.
    /// </summary>
    /// <param name="callback">The callback used to get the return value.</param>
    /// <returns>A fluent <see langword="interface"/> to a data command parameter.</returns>
    public IDataParameter<TValue> Return(Action<TValue> callback)
    {
        // force name and direction
        _parameter.ParameterName = "@ReturnValue";
        _parameter.Direction = ParameterDirection.ReturnValue;

        _dataCommand.RegisterCallback(_parameter, callback);

        return this;
    }

}
