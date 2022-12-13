using System.Data;

namespace FluentCommand;


/// <summary>
/// An interface defining parameter type handling
/// </summary>
public interface IDataParameterHandler
{
    /// <summary>
    /// Gets the type of the value to handle.
    /// </summary>
    /// <value>
    /// The type of the value to handle.
    /// </value>
    Type ValueType { get; }

    /// <summary>
    /// Read the value from the specified <paramref name="parameter"/>.
    /// </summary>
    /// <param name="parameter">The parameter to read the value from.</param>
    /// <returns>The value read from the <paramref name="parameter"/></returns>
    object ReadValue(IDbDataParameter parameter);

    /// <summary>
    /// Set the value on the specified <paramref name="parameter"/>.
    /// </summary>
    /// <param name="parameter">The parameter to be updated.</param>
    /// <param name="value">The value to set.</param>
    void SetValue(IDbDataParameter parameter, object value);
}
