namespace FluentCommand.Query.Generators;
public interface IQueryGenerator
{
    string BuildDelete(
        string tableClause,
        IReadOnlyCollection<string> outputClause,
        IReadOnlyCollection<string> fromClause,
        IReadOnlyCollection<string> whereClause,
        IReadOnlyCollection<string> commentExpression);

    string BuildInsert(
        string tableClause,
        IReadOnlyCollection<string> columnExpression,
        IReadOnlyCollection<string> outputClause,
        IReadOnlyCollection<string> valueExpression,
        IReadOnlyCollection<string> commentExpression);

    string BuildSelect(
        IReadOnlyCollection<string> selectClause,
        IReadOnlyCollection<string> fromClause,
        IReadOnlyCollection<string> whereClause,
        IReadOnlyCollection<string> orderByClause,
        IReadOnlyCollection<string> groupByClause,
        IReadOnlyCollection<string> limitClause,
        IReadOnlyCollection<string> commentExpression);

    string BuildUpdate(
        string tableClause,
        IReadOnlyCollection<string> updateClause,
        IReadOnlyCollection<string> outputClause,
        IReadOnlyCollection<string> fromClause,
        IReadOnlyCollection<string> whereClause,
        IReadOnlyCollection<string> commentExpression);

    string BuildWhere(IReadOnlyCollection<string> whereClause);

    string CommentClause(string comment);

    string AggregateClause(
        AggregateFunctions aggregate,
        string columnName,
        string columnPrefix = null,
        string columnAlias = null);

    string FromClause(
        string tableName,
        string tableSchema = null,
        string tableAlias = null);

    string LimitClause(int offset, int size);

    string LogicalClause(
        IReadOnlyCollection<string> whereClause,
        LogicalOperators logicalOperator);

    string OrderClause(
        string columnName,
        string columnPrefix = null,
        SortDirections sortDirection = SortDirections.Ascending);

    string SelectClause(
        string columnName,
        string columnPrefix = null,
        string columnAlias = null);

    string UpdateClause(
        string columnName,
        string paramterName);

    string WhereClause(
        string columnName,
        string parameterName,
        FilterOperators filterOperator = FilterOperators.Equal);

    string GroupClause(string columnName, string columnPrefix = null);
}
