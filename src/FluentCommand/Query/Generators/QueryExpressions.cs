namespace FluentCommand.Query.Generators;

public record TableExpression(
    string TableName,
    string TableSchema = null,
    string TableAlias = null,
    bool IsRaw = false);

public record ColumnExpression(
    string ColumnName,
    string TableAlias = null,
    string ColumnAlias = null,
    bool IsRaw = false);

public record UpdateExpression(
    string ColumnName,
    string ParameterName,
    string TableAlias = null,
    bool IsRaw = false)
    : ColumnExpression(ColumnName, TableAlias, null, IsRaw);

public record SortExpression(
    string ColumnName,
    string TableAlias = null,
    SortDirections SortDirection = SortDirections.Ascending,
    bool IsRaw = false)
    : ColumnExpression(ColumnName, TableAlias, null, IsRaw);

public record GroupExpression(
    string ColumnName,
    string TableAlias = null,
    bool IsRaw = false)
    : ColumnExpression(ColumnName, TableAlias, null, IsRaw);

public record AggergateExpression(
    AggregateFunctions Aggregate,
    string ColumnName,
    string TableAlias = null,
    string ColumnAlias = null,
    bool IsRaw = false)
    : ColumnExpression(ColumnName, TableAlias, ColumnAlias, IsRaw);

public record WhereExpression(
    string ColumnName,
    string ParameterName = null,
    string TableAlias = null,
    FilterOperators FilterOperator = FilterOperators.Equal,
    bool IsRaw = false)
    : ColumnExpression(ColumnName, TableAlias, null, IsRaw);


public record LimitExpression(
    int Offset,
    int Size);

public record JoinExpression(
    string LeftTableAlias = null,
    string LeftColumnName = null,
    string RightTableName = null,
    string RightTableSchema = null,
    string RightTableAlias = null,
    string RightColumnName = null,
    JoinTypes JoinType = JoinTypes.Inner);

public record SelectStatement(
    IReadOnlyCollection<ColumnExpression> SelectExpressions,
    IReadOnlyCollection<TableExpression> FromExpressions,
    IReadOnlyCollection<JoinExpression> JoinExpressions,
    IReadOnlyCollection<WhereExpression> WhereExpressions,
    IReadOnlyCollection<SortExpression> SortExpressions,
    IReadOnlyCollection<GroupExpression> GroupExpressions,
    IReadOnlyCollection<LimitExpression> LimitExpressions,
    IReadOnlyCollection<string> CommentExpressions);

public record InsertStatement(
    TableExpression TableExpression,
    IReadOnlyCollection<ColumnExpression> ColumnExpressions,
    IReadOnlyCollection<ColumnExpression> OutputExpressions,
    IReadOnlyCollection<string> ValueExpressions,
    IReadOnlyCollection<string> CommentExpressions);

public record UpdateStatement(
    TableExpression TableExpression,
    IReadOnlyCollection<UpdateExpression> UpdateExpressions,
    IReadOnlyCollection<ColumnExpression> OutputExpressions,
    IReadOnlyCollection<TableExpression> FromExpressions,
    IReadOnlyCollection<JoinExpression> JoinExpressions,
    IReadOnlyCollection<WhereExpression> WhereExpressions,
    IReadOnlyCollection<string> CommentExpressions);

public record DeleteStatement(
    TableExpression TableExpression,
    IReadOnlyCollection<ColumnExpression> OutputExpressions,
    IReadOnlyCollection<TableExpression> FromExpressions,
    IReadOnlyCollection<JoinExpression> JoinExpressions,
    IReadOnlyCollection<WhereExpression> WhereExpressions,
    IReadOnlyCollection<string> CommentExpressions);
