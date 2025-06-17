namespace FluentCommand.Query;

/// <summary>
/// Specifies the sort direction to use in SQL ORDER BY clauses.
/// </summary>
public enum SortDirections
{
    /// <summary>
    /// Sorts the results in ascending order (e.g., A to Z, 0 to 9).
    /// Corresponds to the SQL <c>ASC</c> keyword.
    /// </summary>
    Ascending,

    /// <summary>
    /// Sorts the results in descending order (e.g., Z to A, 9 to 0).
    /// Corresponds to the SQL <c>DESC</c> keyword.
    /// </summary>
    Descending
}
