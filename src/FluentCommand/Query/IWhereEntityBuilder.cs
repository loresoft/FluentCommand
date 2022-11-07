using System.Linq.Expressions;

namespace FluentCommand.Query;

public interface IWhereEntityBuilder<TEntity, out TBuilder>
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

    TBuilder WhereIn<TValue>(
        Expression<Func<TEntity, TValue>> property,
        IEnumerable<TValue> parameterValues,
        string tableAlias);

    TBuilder WhereInIf<TValue>(
        Expression<Func<TEntity, TValue>> property,
        IEnumerable<TValue> parameterValues,
        Func<string, IEnumerable<TValue>, bool> condition = null);

    TBuilder WhereInIf<TValue>(
        Expression<Func<TEntity, TValue>> property,
        IEnumerable<TValue> parameterValues,
        string tableAlias,
        Func<string, IEnumerable<TValue>, bool> condition = null);

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
