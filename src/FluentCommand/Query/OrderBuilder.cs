using FluentCommand.Extensions;
using FluentCommand.Query.Generators;

namespace FluentCommand.Query;

public class OrderBuilder : OrderBuilder<OrderBuilder>
{
    public OrderBuilder(IQueryGenerator queryGenerator, List<QueryParameter> parameters)
        : base(queryGenerator, parameters)
    {
    }
}

public abstract class OrderBuilder<TBuilder> : StatementBuilder<TBuilder>
    where TBuilder : OrderBuilder<TBuilder>
{
    protected OrderBuilder(IQueryGenerator queryGenerator, List<QueryParameter> parameters)
        : base(queryGenerator, parameters)
    {
    }

    protected HashSet<SortExpression> SortExpressions { get; } = new();


    public TBuilder OrderBy(
        string columnName,
        SortDirections sortDirection = SortDirections.Ascending)
    {
        return OrderBy(columnName, null, sortDirection);
    }

    public TBuilder OrderBy(
        string columnName,
        string tableAlias,
        SortDirections sortDirection = SortDirections.Ascending)
    {
        var orderClause = new SortExpression(columnName, tableAlias, sortDirection);

        SortExpressions.Add(orderClause);

        return (TBuilder)this;
    }

    public TBuilder OrderByIf(
        string columnName,
        string tableAlias = null,
        SortDirections sortDirection = SortDirections.Ascending,
        Func<string, bool> condition = null)
    {
        if (condition != null && !condition(columnName))
            return (TBuilder)this;

        return OrderBy(columnName, tableAlias, sortDirection);
    }

    public TBuilder OrderByRaw(string sortExpression)
    {
        if (sortExpression.HasValue())
            SortExpressions.Add(new SortExpression(sortExpression, IsRaw: true));

        return (TBuilder)this;
    }

    public TBuilder OrderByRawIf(string sortExpression, Func<string, bool> condition = null)
    {
        if (condition != null && !condition(sortExpression))
            return (TBuilder)this;

        return OrderByRaw(sortExpression);
    }

    public TBuilder OrderByRaw(IEnumerable<string> sortExpressions)
    {
        if (sortExpressions is null)
            throw new ArgumentNullException(nameof(sortExpressions));

        foreach (var sortExpression in sortExpressions)
            OrderByRaw(sortExpression);

        return (TBuilder)this;
    }

    public override QueryStatement BuildStatement()
    {
        if (SortExpressions == null || SortExpressions.Count == 0)
            return null;

        var statement = QueryGenerator.BuildOrder(SortExpressions);

        return new QueryStatement(statement, Parameters);
    }
}
