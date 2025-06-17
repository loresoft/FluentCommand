namespace FluentCommand.Query.Generators;

/// <summary>
/// Defines methods for generating SQL query statements and expressions for various SQL operations.
/// </summary>
public interface IQueryGenerator
{
    /// <summary>
    /// Builds a SQL DELETE statement from the specified <see cref="DeleteStatement"/>.
    /// </summary>
    /// <param name="deleteStatement">The <see cref="DeleteStatement"/> containing the DELETE statement configuration.</param>
    /// <returns>A SQL DELETE statement string.</returns>
    string BuildDelete(DeleteStatement deleteStatement);

    /// <summary>
    /// Builds a SQL INSERT statement from the specified <see cref="InsertStatement"/>.
    /// </summary>
    /// <param name="insertStatement">The <see cref="InsertStatement"/> containing the INSERT statement configuration.</param>
    /// <returns>A SQL INSERT statement string.</returns>
    string BuildInsert(InsertStatement insertStatement);

    /// <summary>
    /// Builds a SQL SELECT statement from the specified <see cref="SelectStatement"/>.
    /// </summary>
    /// <param name="selectStatement">The <see cref="SelectStatement"/> containing the SELECT statement configuration.</param>
    /// <returns>A SQL SELECT statement string.</returns>
    string BuildSelect(SelectStatement selectStatement);

    /// <summary>
    /// Builds a SQL UPDATE statement from the specified <see cref="UpdateStatement"/>.
    /// </summary>
    /// <param name="updateStatement">The <see cref="UpdateStatement"/> containing the UPDATE statement configuration.</param>
    /// <returns>A SQL UPDATE statement string.</returns>
    string BuildUpdate(UpdateStatement updateStatement);

    /// <summary>
    /// Builds a SQL WHERE clause from the specified collection of <see cref="WhereExpression"/> objects.
    /// </summary>
    /// <param name="whereExpressions">A collection of <see cref="WhereExpression"/> objects representing WHERE conditions.</param>
    /// <returns>A SQL WHERE clause string.</returns>
    string BuildWhere(IReadOnlyCollection<WhereExpression> whereExpressions);

    /// <summary>
    /// Builds a SQL ORDER BY clause from the specified collection of <see cref="SortExpression"/> objects.
    /// </summary>
    /// <param name="sortExpressions">A collection of <see cref="SortExpression"/> objects representing sort conditions.</param>
    /// <returns>A SQL ORDER BY clause string.</returns>
    string BuildOrder(IReadOnlyCollection<SortExpression> sortExpressions);

    /// <summary>
    /// Builds a SQL comment expression.
    /// </summary>
    /// <param name="comment">The comment text.</param>
    /// <returns>A SQL comment string.</returns>
    string CommentExpression(string comment);

    /// <summary>
    /// Builds a SQL aggregate expression (e.g., COUNT, SUM) from the specified <see cref="AggregateExpression"/>.
    /// </summary>
    /// <param name="aggregateExpression">The <see cref="AggregateExpression"/> representing the aggregate function.</param>
    /// <returns>A SQL aggregate expression string.</returns>
    string AggregateExpression(AggregateExpression aggregateExpression);

    /// <summary>
    /// Builds a SQL table expression from the specified <see cref="TableExpression"/>.
    /// </summary>
    /// <param name="tableExpression">The <see cref="TableExpression"/> representing the table.</param>
    /// <returns>A SQL table expression string.</returns>
    string TableExpression(TableExpression tableExpression);

    /// <summary>
    /// Builds a SQL LIMIT/OFFSET expression from the specified <see cref="LimitExpression"/>.
    /// </summary>
    /// <param name="limitExpression">The <see cref="LimitExpression"/> representing the limit and offset.</param>
    /// <returns>A SQL LIMIT/OFFSET expression string.</returns>
    string LimitExpression(LimitExpression limitExpression);

    /// <summary>
    /// Builds a logical SQL expression (e.g., AND/OR group) from the specified WHERE expressions and logical operator.
    /// </summary>
    /// <param name="whereExpressions">A collection of <see cref="WhereExpression"/> objects representing conditions.</param>
    /// <param name="logicalOperator">The <see cref="LogicalOperators"/> value to combine the expressions.</param>
    /// <returns>A logical SQL expression string.</returns>
    string LogicalExpression(IReadOnlyCollection<WhereExpression> whereExpressions, LogicalOperators logicalOperator);

    /// <summary>
    /// Builds a SQL sort expression from the specified <see cref="SortExpression"/>.
    /// </summary>
    /// <param name="sortExpression">The <see cref="SortExpression"/> representing the sort condition.</param>
    /// <returns>A SQL sort expression string.</returns>
    string SortExpression(SortExpression sortExpression);

    /// <summary>
    /// Builds a SQL column expression from the specified <see cref="ColumnExpression"/>.
    /// </summary>
    /// <param name="columnExpression">The <see cref="ColumnExpression"/> representing the column.</param>
    /// <returns>A SQL column expression string.</returns>
    string ColumnExpression(ColumnExpression columnExpression);

    /// <summary>
    /// Builds a SQL update expression from the specified <see cref="UpdateExpression"/>.
    /// </summary>
    /// <param name="updateExpression">The <see cref="UpdateExpression"/> representing the update operation.</param>
    /// <returns>A SQL update expression string.</returns>
    string UpdateExpression(UpdateExpression updateExpression);

    /// <summary>
    /// Builds a SQL WHERE expression from the specified <see cref="WhereExpression"/>.
    /// </summary>
    /// <param name="whereExpression">The <see cref="WhereExpression"/> representing the condition.</param>
    /// <returns>A SQL WHERE expression string.</returns>
    string WhereExpression(WhereExpression whereExpression);

    /// <summary>
    /// Builds a SQL GROUP BY expression from the specified <see cref="GroupExpression"/>.
    /// </summary>
    /// <param name="groupExpression">The <see cref="GroupExpression"/> representing the group by condition.</param>
    /// <returns>A SQL GROUP BY expression string.</returns>
    string GroupExpression(GroupExpression groupExpression);

    /// <summary>
    /// Builds a SQL JOIN expression from the specified <see cref="JoinExpression"/>.
    /// </summary>
    /// <param name="joinExpression">The <see cref="JoinExpression"/> representing the join operation.</param>
    /// <returns>A SQL JOIN expression string.</returns>
    string JoinExpression(JoinExpression joinExpression);
}
