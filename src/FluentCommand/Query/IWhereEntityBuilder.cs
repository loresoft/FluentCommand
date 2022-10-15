using System.Linq.Expressions;

namespace FluentCommand.Query;

public interface IWhereEntityBuilder<TEntity, TBuilder>
    where TEntity : class
{
    TBuilder Where<TValue>(
        Expression<Func<TEntity, TValue>> property,
        TValue parameterValue,
        FilterOperators filterOperator = FilterOperators.Equal);

    TBuilder Where<TValue>(
        Expression<Func<TEntity, TValue>> property,
        TValue parameterValue,
        string tableAlias,
        FilterOperators filterOperator = FilterOperators.Equal);


    TBuilder WhereIf<TValue>(
        Expression<Func<TEntity, TValue>> property,
        TValue parameterValue,
        FilterOperators filterOperator = FilterOperators.Equal,
        Func<string, TValue, bool> condition = null);

    TBuilder WhereIf<TValue>(
        Expression<Func<TEntity, TValue>> property,
        TValue parameterValue,
        string tableAlias,
        FilterOperators filterOperator = FilterOperators.Equal,
        Func<string, TValue, bool> condition = null);

    TBuilder WhereAnd(Action<LogicalEntityBuilder<TEntity>> builder);

    TBuilder WhereOr(Action<LogicalEntityBuilder<TEntity>> builder);
}
