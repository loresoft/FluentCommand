namespace FluentCommand.Query;

/// <summary>
/// Specifies the available filter operators for use in SQL WHERE clauses.
/// </summary>
public enum FilterOperators
{
    /// <summary>
    /// Checks if the column value starts with the specified value.
    /// Corresponds to the SQL <c>LIKE 'value%'</c> pattern.
    /// </summary>
    StartsWith,

    /// <summary>
    /// Checks if the column value ends with the specified value.
    /// Corresponds to the SQL <c>LIKE '%value'</c> pattern.
    /// </summary>
    EndsWith,

    /// <summary>
    /// Checks if the column value contains the specified value.
    /// Corresponds to the SQL <c>LIKE '%value%'</c> pattern.
    /// </summary>
    Contains,

    /// <summary>
    /// Checks if the column value is equal to the specified value.
    /// Corresponds to the SQL <c>=</c> operator.
    /// </summary>
    Equal,

    /// <summary>
    /// Checks if the column value is not equal to the specified value.
    /// Corresponds to the SQL <c>&lt;&gt;</c> or <c>!=</c> operator.
    /// </summary>
    NotEqual,

    /// <summary>
    /// Checks if the column value is less than the specified value.
    /// Corresponds to the SQL <c>&lt;</c> operator.
    /// </summary>
    LessThan,

    /// <summary>
    /// Checks if the column value is less than or equal to the specified value.
    /// Corresponds to the SQL <c>&lt;=</c> operator.
    /// </summary>
    LessThanOrEqual,

    /// <summary>
    /// Checks if the column value is greater than the specified value.
    /// Corresponds to the SQL <c>&gt;</c> operator.
    /// </summary>
    GreaterThan,

    /// <summary>
    /// Checks if the column value is greater than or equal to the specified value.
    /// Corresponds to the SQL <c>&gt;=</c> operator.
    /// </summary>
    GreaterThanOrEqual,

    /// <summary>
    /// Checks if the column value is null.
    /// Corresponds to the SQL <c>IS NULL</c> operator.
    /// </summary>
    IsNull,

    /// <summary>
    /// Checks if the column value is not null.
    /// Corresponds to the SQL <c>IS NOT NULL</c> operator.
    /// </summary>
    IsNotNull,

    /// <summary>
    /// Checks if the column value is in the specified set of values.
    /// Corresponds to the SQL <c>IN</c> operator.
    /// </summary>
    In
}
