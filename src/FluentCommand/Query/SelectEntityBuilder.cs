using System;
using System.Collections.Generic;
using System.Linq.Expressions;

using FluentCommand.Query.Generators;
using FluentCommand.Reflection;

namespace FluentCommand;

public class SelectEntityBuilder<TEntity> : SelectBuilder<SelectEntityBuilder<TEntity>>
    where TEntity : class
{
    private static readonly TypeAccessor _typeAccessor = TypeAccessor.GetAccessor<TEntity>();

    public SelectEntityBuilder(IQueryGenerator queryGenerator, List<QueryParameter> parameters, LogicalOperators logicalOperator = LogicalOperators.And)
        : base(queryGenerator, parameters, logicalOperator)
    {
    }

    public SelectEntityBuilder<TEntity> Column<TValue>(
        Expression<Func<TEntity, TValue>> property,
        string prefix = null,
        string alias = null)
    {
        var propertyAccessor = _typeAccessor.FindProperty(property);

        return Column(propertyAccessor.Column, prefix, alias);
    }

    public SelectEntityBuilder<TEntity> Count<TValue>(
        Expression<Func<TEntity, TValue>> property,
        string prefix = null,
        string alias = null)
    {
        var propertyAccessor = _typeAccessor.FindProperty(property);

        return Count(propertyAccessor.Column, prefix, alias);
    }

    public SelectEntityBuilder<TEntity> Aggregate<TValue>(
        Expression<Func<TEntity, TValue>> property,
        AggregateFunctions function,
        string prefix = null,
        string alias = null)
    {
        var propertyAccessor = _typeAccessor.FindProperty(property);

        return Aggregate(function, propertyAccessor.Column, prefix, alias);
    }


    public SelectEntityBuilder<TEntity> Where<TValue>(
        Expression<Func<TEntity, TValue>> property,
        TValue parameterValue,
        FilterOperators filterOperator = FilterOperators.Equal)
    {
        var propertyAccessor = _typeAccessor.FindProperty(property);

        return Where(propertyAccessor.Column, parameterValue, filterOperator);
    }

    public SelectEntityBuilder<TEntity> Where(Action<LogicalEntityBuilder<TEntity>> builder)
    {
        var innerBuilder = new LogicalEntityBuilder<TEntity>(QueryGenerator, Parameters, CommentExpressions, LogicalOperators.Or);

        builder(innerBuilder);

        var statement = innerBuilder.BuildStatement();
        WhereClause.Add(statement.Statement);

        return this;
    }


    public SelectEntityBuilder<TEntity> OrderBy<TValue>(
        Expression<Func<TEntity, TValue>> property,
        SortDirections sortDirection = SortDirections.Ascending)
    {
        var propertyAccessor = _typeAccessor.FindProperty(property);

        return OrderBy(propertyAccessor.Column, null, sortDirection);
    }

    public SelectEntityBuilder<TEntity> OrderBy<TValue>(
        Expression<Func<TEntity, TValue>> property,
        string prefix,
        SortDirections sortDirection = SortDirections.Ascending)
    {
        var propertyAccessor = _typeAccessor.FindProperty(property);

        return OrderBy(propertyAccessor.Column, prefix, sortDirection);
    }

    public SelectEntityBuilder<TEntity> GroupBy<TValue>(
        Expression<Func<TEntity, TValue>> property,
        string prefix = null)
    {
        var propertyAccessor = _typeAccessor.FindProperty(property);

        return GroupBy(propertyAccessor.Column, prefix);
    }

    public override QueryStatement BuildStatement()
    {
        // add table and schema from attribute if not set
        if (FromClause.Count == 0)
            From(_typeAccessor.TableName, _typeAccessor.TableSchema);

        return base.BuildStatement();
    }
}
