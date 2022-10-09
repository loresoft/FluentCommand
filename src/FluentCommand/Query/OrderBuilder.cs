using FluentCommand.Extensions;
using FluentCommand.Query.Generators;

namespace FluentCommand.Query;

public class OrderBuilder : OrderBuilder<OrderBuilder>
{
    public OrderBuilder(IQueryGenerator queryGenerator, List<QueryParameter> parameters)
        : base(queryGenerator, parameters)
    {
    }

    public override QueryStatement BuildStatement()
    {
        var statement = QueryGenerator.BuildOrder(
            orderClause: OrderByClause
        );

        return new QueryStatement(statement, Parameters);
    }
}

public abstract class OrderBuilder<TBuilder> : StatementBuilder<TBuilder>
    where TBuilder : OrderBuilder<TBuilder>
{
    protected OrderBuilder(IQueryGenerator queryGenerator, List<QueryParameter> parameters)
        : base(queryGenerator, parameters)
    {
    }

    protected HashSet<string> OrderByClause { get; } = new();


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
        var orderClause = QueryGenerator.OrderClause(columnName, tableAlias, sortDirection);

        OrderByClause.Add(orderClause);

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
            OrderByClause.Add(sortExpression);

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
            OrderByClause.Add(sortExpression);

        return (TBuilder)this;
    }
}
