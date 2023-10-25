using System;

namespace FluentCommand.Query;

/// <summary>
/// A query parameter
/// </summary>
public class QueryParameter
{
    /// <summary>
    /// Initializes a new instance of the <see cref="QueryParameter"/> class.
    /// </summary>
    /// <param name="name">The parameter name.</param>
    /// <param name="value">The parameter value.</param>
    /// <param name="type">The parameter type.</param>
    public QueryParameter(string name, object value, Type type)
    {
        Name = name;
        Value = value;
        Type = type;
    }

    /// <summary>
    /// Gets the parameter name.
    /// </summary>
    /// <value>
    /// The parameter name.
    /// </value>
    public string Name { get; }

    /// <summary>
    /// Gets the parameter value.
    /// </summary>
    /// <value>
    /// The parameter value.
    /// </value>
    public object Value { get; }

    /// <summary>
    /// Gets the parameter type.
    /// </summary>
    /// <value>
    /// The parameter type.
    /// </value>
    public Type Type { get; }
}
