using System;
using System.Collections.Generic;
using System.Linq.Expressions;

using FluentCommand.Extensions;
using FluentCommand.Query.Generators;
using FluentCommand.Reflection;

namespace FluentCommand.Query;

public class DeleteEntityBuilder<TEntity> : DeleteBuilder<DeleteEntityBuilder<TEntity>>
    where TEntity : class
{
    private static readonly TypeAccessor _typeAccessor = TypeAccessor.GetAccessor<TEntity>();

    public DeleteEntityBuilder(IQueryGenerator queryGenerator, Dictionary<string, object> parameters, LogicalOperators logicalOperator = LogicalOperators.And)
        : base(queryGenerator, parameters, logicalOperator)
    {
    }

    public DeleteEntityBuilder<TEntity> Output<TValue>(Expression<Func<TEntity, TValue>> property, string prefix = "DELETED", string alias = null)
    {
        var propertyAccessor = _typeAccessor.FindProperty(property);
        return Output(propertyAccessor.Column, prefix, alias);
    }

    public DeleteEntityBuilder<TEntity> Where<TValue>(
        Expression<Func<TEntity, TValue>> property,
        TValue parameterValue,
        FilterOperators filterOperator = FilterOperators.Equal)
    {
        var propertyAccessor = _typeAccessor.FindProperty(property);

        return Where(propertyAccessor.Column, parameterValue, filterOperator);
    }

    public DeleteEntityBuilder<TEntity> Where(Action<LogicalEntityBuilder<TEntity>> builder)
    {
        var innerBuilder = new LogicalEntityBuilder<TEntity>(QueryGenerator, Parameters, CommentExpressions, LogicalOperators.Or);

        builder(innerBuilder);

        var statement = innerBuilder.BuildStatement();
        WhereClause.Add(statement.Statement);

        return this;
    }

    public override QueryStatement BuildStatement()
    {
        // add table and schema from attribute if not set
        if (TableClause.IsNullOrWhiteSpace())
            Table(_typeAccessor.TableName, _typeAccessor.TableSchema);

        return base.BuildStatement();
    }
}
