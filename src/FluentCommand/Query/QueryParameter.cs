using System;

namespace FluentCommand.Query;

/// <summary>
/// Represents a parameter used in a SQL query, including its name, value, and type information.
/// </summary>
public class QueryParameter
{
    /// <summary>
    /// Initializes a new instance of the <see cref="QueryParameter"/> class with the specified name, value, and type.
    /// </summary>
    /// <param name="name">The name of the parameter as it appears in the SQL statement (e.g., <c>@Id</c>).</param>
    /// <param name="value">The value to be assigned to the parameter.</param>
    /// <param name="type">The <see cref="Type"/> of the parameter value.</param>
    public QueryParameter(string name, object value, Type type)
    {
        Name = name;
        Value = value;
        Type = type;
    }

    /// <summary>
    /// Gets the name of the parameter as it appears in the SQL statement.
    /// </summary>
    /// <value>
    /// A <see cref="string"/> representing the parameter name (e.g., <c>@Id</c>).
    /// </value>
    public string Name { get; }

    /// <summary>
    /// Gets the value assigned to the parameter.
    /// </summary>
    /// <value>
    /// An <see cref="object"/> containing the parameter value.
    /// </value>
    public object Value { get; }

    /// <summary>
    /// Gets the <see cref="Type"/> of the parameter value.
    /// </summary>
    /// <value>
    /// A <see cref="Type"/> representing the data type of the parameter.
    /// </value>
    public Type Type { get; }
}
