namespace FluentCommand.Query;

/// <summary>
/// Specifies the available aggregate functions for use in SQL queries.
/// </summary>
public enum AggregateFunctions
{
    /// <summary>
    /// Calculates the average value of a numeric column.
    /// Corresponds to the SQL <c>AVG</c> function.
    /// </summary>
    Average,

    /// <summary>
    /// Counts the number of rows or non-null values in a column.
    /// Corresponds to the SQL <c>COUNT</c> function.
    /// </summary>
    Count,

    /// <summary>
    /// Returns the maximum value from a column.
    /// Corresponds to the SQL <c>MAX</c> function.
    /// </summary>
    Max,

    /// <summary>
    /// Returns the minimum value from a column.
    /// Corresponds to the SQL <c>MIN</c> function.
    /// </summary>
    Min,

    /// <summary>
    /// Calculates the sum of all values in a numeric column.
    /// Corresponds to the SQL <c>SUM</c> function.
    /// </summary>
    Sum,
}
