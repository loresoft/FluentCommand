namespace FluentCommand.Query.Generators;

/// <summary>
/// Represents a table reference in a SQL statement, including optional schema, alias, and raw SQL support.
/// </summary>
/// <param name="TableName">The name of the table.</param>
/// <param name="TableSchema">The schema of the table (optional).</param>
/// <param name="TableAlias">The alias for the table (optional).</param>
/// <param name="IsRaw">Indicates if the table expression is a raw SQL fragment.</param>
public record TableExpression(
    string TableName,
    string TableSchema = null,
    string TableAlias = null,
    bool IsRaw = false);

/// <summary>
/// Represents a column reference in a SQL statement, including optional table alias, column alias, and raw SQL support.
/// </summary>
/// <param name="ColumnName">The name of the column.</param>
/// <param name="TableAlias">The alias of the table (optional).</param>
/// <param name="ColumnAlias">The alias for the column (optional).</param>
/// <param name="IsRaw">Indicates if the column expression is a raw SQL fragment.</param>
public record ColumnExpression(
    string ColumnName,
    string TableAlias = null,
    string ColumnAlias = null,
    bool IsRaw = false);

/// <summary>
/// Represents an update assignment in a SQL UPDATE statement.
/// </summary>
/// <param name="ColumnName">The name of the column to update.</param>
/// <param name="ParameterName">The parameter name for the value to assign.</param>
/// <param name="TableAlias">The alias of the table (optional).</param>
/// <param name="IsRaw">Indicates if the update expression is a raw SQL fragment.</param>
public record UpdateExpression(
    string ColumnName,
    string ParameterName,
    string TableAlias = null,
    bool IsRaw = false)
    : ColumnExpression(ColumnName, TableAlias, null, IsRaw);

/// <summary>
/// Represents a sort (ORDER BY) expression in a SQL statement.
/// </summary>
/// <param name="ColumnName">The name of the column to sort by.</param>
/// <param name="TableAlias">The alias of the table (optional).</param>
/// <param name="SortDirection">The sort direction (ascending or descending).</param>
/// <param name="IsRaw">Indicates if the sort expression is a raw SQL fragment.</param>
public record SortExpression(
    string ColumnName,
    string TableAlias = null,
    SortDirections SortDirection = SortDirections.Ascending,
    bool IsRaw = false)
    : ColumnExpression(ColumnName, TableAlias, null, IsRaw);

/// <summary>
/// Represents a group by expression in a SQL statement.
/// </summary>
/// <param name="ColumnName">The name of the column to group by.</param>
/// <param name="TableAlias">The alias of the table (optional).</param>
/// <param name="IsRaw">Indicates if the group expression is a raw SQL fragment.</param>
public record GroupExpression(
    string ColumnName,
    string TableAlias = null,
    bool IsRaw = false)
    : ColumnExpression(ColumnName, TableAlias, null, IsRaw);

/// <summary>
/// Represents an aggregate function expression (e.g., COUNT, SUM) in a SQL statement.
/// </summary>
/// <param name="Aggregate">The aggregate function to use.</param>
/// <param name="ColumnName">The name of the column to aggregate.</param>
/// <param name="TableAlias">The alias of the table (optional).</param>
/// <param name="ColumnAlias">The alias for the result column (optional).</param>
/// <param name="IsRaw">Indicates if the aggregate expression is a raw SQL fragment.</param>
public record AggregateExpression(
    AggregateFunctions Aggregate,
    string ColumnName,
    string TableAlias = null,
    string ColumnAlias = null,
    bool IsRaw = false)
    : ColumnExpression(ColumnName, TableAlias, ColumnAlias, IsRaw);

/// <summary>
/// Represents a WHERE clause condition in a SQL statement.
/// </summary>
/// <param name="ColumnName">The name of the column to filter on.</param>
/// <param name="ParameterName">The parameter name for the value to compare (optional).</param>
/// <param name="TableAlias">The alias of the table (optional).</param>
/// <param name="FilterOperator">The filter operator to use (e.g., Equal, In, IsNull).</param>
/// <param name="IsRaw">Indicates if the where expression is a raw SQL fragment.</param>
public record WhereExpression(
    string ColumnName,
    string ParameterName = null,
    string TableAlias = null,
    FilterOperators FilterOperator = FilterOperators.Equal,
    bool IsRaw = false)
    : ColumnExpression(ColumnName, TableAlias, null, IsRaw);

/// <summary>
/// Represents a LIMIT/OFFSET clause in a SQL statement.
/// </summary>
/// <param name="Offset">The number of rows to skip before starting to return rows.</param>
/// <param name="Size">The maximum number of rows to return.</param>
public record LimitExpression(
    int Offset,
    int Size);

/// <summary>
/// Represents a JOIN clause in a SQL statement, including join type and join columns.
/// </summary>
/// <param name="LeftTableAlias">The alias of the left table in the join.</param>
/// <param name="LeftColumnName">The column name from the left table to join on.</param>
/// <param name="RightTableName">The name of the right table in the join.</param>
/// <param name="RightTableSchema">The schema of the right table (optional).</param>
/// <param name="RightTableAlias">The alias of the right table in the join.</param>
/// <param name="RightColumnName">The column name from the right table to join on.</param>
/// <param name="JoinType">The type of join (e.g., Inner, Left, Right).</param>
public record JoinExpression(
    string LeftTableAlias = null,
    string LeftColumnName = null,
    string RightTableName = null,
    string RightTableSchema = null,
    string RightTableAlias = null,
    string RightColumnName = null,
    JoinTypes JoinType = JoinTypes.Inner);

/// <summary>
/// Represents a complete SQL SELECT statement, including all clauses and expressions.
/// </summary>
/// <param name="SelectExpressions">The columns or expressions to select.</param>
/// <param name="FromExpressions">The tables to select from.</param>
/// <param name="JoinExpressions">The join clauses.</param>
/// <param name="WhereExpressions">The WHERE clause conditions.</param>
/// <param name="SortExpressions">The ORDER BY clause expressions.</param>
/// <param name="GroupExpressions">The GROUP BY clause expressions.</param>
/// <param name="LimitExpressions">The LIMIT/OFFSET clause expressions.</param>
/// <param name="CommentExpressions">The comment expressions to include in the statement.</param>
public record SelectStatement(
    IReadOnlyCollection<ColumnExpression> SelectExpressions,
    IReadOnlyCollection<TableExpression> FromExpressions,
    IReadOnlyCollection<JoinExpression> JoinExpressions,
    IReadOnlyCollection<WhereExpression> WhereExpressions,
    IReadOnlyCollection<SortExpression> SortExpressions,
    IReadOnlyCollection<GroupExpression> GroupExpressions,
    IReadOnlyCollection<LimitExpression> LimitExpressions,
    IReadOnlyCollection<string> CommentExpressions);

/// <summary>
/// Represents a complete SQL INSERT statement, including table, columns, values, output, and comments.
/// </summary>
/// <param name="TableExpression">The table to insert into.</param>
/// <param name="ColumnExpressions">The columns to insert values into.</param>
/// <param name="OutputExpressions">The columns to return/output after insert (optional).</param>
/// <param name="ValueExpressions">The values to insert.</param>
/// <param name="CommentExpressions">The comment expressions to include in the statement.</param>
public record InsertStatement(
    TableExpression TableExpression,
    IReadOnlyCollection<ColumnExpression> ColumnExpressions,
    IReadOnlyCollection<ColumnExpression> OutputExpressions,
    IReadOnlyCollection<string> ValueExpressions,
    IReadOnlyCollection<string> CommentExpressions);

/// <summary>
/// Represents a complete SQL UPDATE statement, including table, assignments, output, joins, where, and comments.
/// </summary>
/// <param name="TableExpression">The table to update.</param>
/// <param name="UpdateExpressions">The column assignments for the update.</param>
/// <param name="OutputExpressions">The columns to return/output after update (optional).</param>
/// <param name="FromExpressions">The FROM clause tables (optional).</param>
/// <param name="JoinExpressions">The join clauses (optional).</param>
/// <param name="WhereExpressions">The WHERE clause conditions (optional).</param>
/// <param name="CommentExpressions">The comment expressions to include in the statement.</param>
public record UpdateStatement(
    TableExpression TableExpression,
    IReadOnlyCollection<UpdateExpression> UpdateExpressions,
    IReadOnlyCollection<ColumnExpression> OutputExpressions,
    IReadOnlyCollection<TableExpression> FromExpressions,
    IReadOnlyCollection<JoinExpression> JoinExpressions,
    IReadOnlyCollection<WhereExpression> WhereExpressions,
    IReadOnlyCollection<string> CommentExpressions);

/// <summary>
/// Represents a complete SQL DELETE statement, including table, output, joins, where, and comments.
/// </summary>
/// <param name="TableExpression">The table to delete from.</param>
/// <param name="OutputExpressions">The columns to return/output after delete (optional).</param>
/// <param name="FromExpressions">The FROM clause tables (optional).</param>
/// <param name="JoinExpressions">The join clauses (optional).</param>
/// <param name="WhereExpressions">The WHERE clause conditions (optional).</param>
/// <param name="CommentExpressions">The comment expressions to include in the statement.</param>
public record DeleteStatement(
    TableExpression TableExpression,
    IReadOnlyCollection<ColumnExpression> OutputExpressions,
    IReadOnlyCollection<TableExpression> FromExpressions,
    IReadOnlyCollection<JoinExpression> JoinExpressions,
    IReadOnlyCollection<WhereExpression> WhereExpressions,
    IReadOnlyCollection<string> CommentExpressions);
