namespace FluentCommand.Query.Generators;

public interface IQueryGenerator
{
    string BuildDelete(DeleteStatement deleteStatement);

    string BuildInsert(InsertStatement insertStatement);

    string BuildSelect(SelectStatement selectStatement);

    string BuildUpdate(UpdateStatement updateStatement);


    string BuildWhere(IReadOnlyCollection<WhereExpression> whereExpressions);

    string BuildOrder(IReadOnlyCollection<SortExpression> sortExpressions);


    string CommentExpression(string comment);

    string AggregateExpression(AggergateExpression aggergateExpression);

    string TableExpression(TableExpression tableExpression);

    string LimitExpression(LimitExpression limitExpression);

    string LogicalExpression(IReadOnlyCollection<WhereExpression> whereExpressions, LogicalOperators logicalOperator);

    string SortExpression(SortExpression sortExpression);

    string ColumnExpression(ColumnExpression columnExpression);

    string UpdateExpression(UpdateExpression updateExpression);

    string WhereExpression(WhereExpression whereExpression);

    string GroupExpression(GroupExpression groupExpression);

    string JoinExpression(JoinExpression joinExpression);
}
