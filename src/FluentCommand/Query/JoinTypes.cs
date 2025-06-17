namespace FluentCommand.Query;

/// <summary>
/// Specifies the types of SQL JOIN operations available for combining tables in a query.
/// </summary>
public enum JoinTypes
{
    /// <summary>
    /// Represents an inner join, which returns only the rows with matching values in both tables.
    /// Corresponds to the SQL <c>INNER JOIN</c> operation.
    /// </summary>
    Inner,

    /// <summary>
    /// Represents a left outer join, which returns all rows from the left table and the matched rows from the right table.
    /// Unmatched rows from the right table will contain nulls.
    /// Corresponds to the SQL <c>LEFT OUTER JOIN</c> operation.
    /// </summary>
    Left,

    /// <summary>
    /// Represents a right outer join, which returns all rows from the right table and the matched rows from the left table.
    /// Unmatched rows from the left table will contain nulls.
    /// Corresponds to the SQL <c>RIGHT OUTER JOIN</c> operation.
    /// </summary>
    Right
}
