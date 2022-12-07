using System.Data;

namespace FluentCommand.Handlers;


/// <summary>
/// An interface defining type handling
/// </summary>
public interface IDataTypeHandler
{
    /// <summary>
    /// Gets the type of the value to handle.
    /// </summary>
    /// <value>
    /// The type of the value to handle.
    /// </value>
    Type ValueType { get; }
}

/// <summary>
/// An interface defining type handling for <typeparamref name="TValue"/>
/// </summary>
/// <typeparam name="TValue">The type of the value.</typeparam>
public interface IDataTypeHandler<TValue> : IDataTypeHandler
{
    /// <summary>
    /// Read the value from the specified <paramref name="dataRecord"/>.
    /// </summary>
    /// <param name="dataRecord">The data record to read the value from.</param>
    /// <param name="fieldIndex">Index of the field.</param>
    /// <returns>The value read from the <paramref name="dataRecord"/></returns>
    TValue ReadValue(IDataRecord dataRecord, int fieldIndex);


    /// <summary>
    /// Read the value from the specified <paramref name="parameter"/>.
    /// </summary>
    /// <param name="parameter">The parameter to read the value from.</param>
    /// <returns>The value read from the <paramref name="parameter"/></returns>
    TValue ReadValue(IDbDataParameter parameter);


    /// <summary>
    /// Set the value on the specified <paramref name="parameter"/>.
    /// </summary>
    /// <param name="parameter">The parameter to be updated.</param>
    /// <param name="value">The value to set.</param>
    void SetValue(IDbDataParameter parameter, TValue value);
}
