namespace FluentCommand.Query;

/// <summary>
/// Specifies the logical operators used to combine Boolean expressions in SQL WHERE clauses.
/// </summary>
public enum LogicalOperators
{
    /// <summary>
    /// Logical AND operator. Returns <c>true</c> if all Boolean expressions are <c>true</c>.
    /// Corresponds to the SQL <c>AND</c> operator.
    /// </summary>
    And,

    /// <summary>
    /// Logical OR operator. Returns <c>true</c> if any Boolean expression is <c>true</c>.
    /// Corresponds to the SQL <c>OR</c> operator.
    /// </summary>
    Or
}
