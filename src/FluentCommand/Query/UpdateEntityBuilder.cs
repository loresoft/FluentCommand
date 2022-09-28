using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

using FluentCommand.Extensions;
using FluentCommand.Query.Generators;
using FluentCommand.Reflection;

namespace FluentCommand;

public class UpdateEntityBuilder<TEntity> : UpdateBuilder<UpdateEntityBuilder<TEntity>>
    where TEntity : class
{
    private static readonly TypeAccessor _typeAccessor = TypeAccessor.GetAccessor<TEntity>();

    public UpdateEntityBuilder(IQueryGenerator queryGenerator, Dictionary<string, object> parameters, LogicalOperators logicalOperator = LogicalOperators.And)
        : base(queryGenerator, parameters, logicalOperator)
    {
    }

    public UpdateEntityBuilder<TEntity> Value<TValue>(Expression<Func<TEntity, TValue>> property, TValue parameterValue)
    {
        var propertyAccessor = _typeAccessor.FindProperty(property);
        return Value(propertyAccessor.Column, parameterValue);
    }

    public UpdateEntityBuilder<TEntity> Values(TEntity entity, IEnumerable<string> columnNames = null)
    {
        if (entity is null)
            throw new ArgumentNullException(nameof(entity));

        var properties = _typeAccessor.GetProperties();
        var columnSet = new HashSet<string>(columnNames ?? Enumerable.Empty<string>());

        foreach (var property in properties)
        {
            if (columnSet.Count > 0 && !columnSet.Contains(property.Name))
                continue;

            Value(property.Column, property.GetValue(entity));
        }

        return this;
    }

    public UpdateEntityBuilder<TEntity> Output<TValue>(Expression<Func<TEntity, TValue>> property, string prefix = "INSERTED", string alias = null)
    {
        var propertyAccessor = _typeAccessor.FindProperty(property);
        return Output(propertyAccessor.Column, prefix, alias);
    }

    public UpdateEntityBuilder<TEntity> Where<TValue>(
        Expression<Func<TEntity, TValue>> property,
        TValue parameterValue,
        FilterOperators filterOperator = FilterOperators.Equal)
    {
        var propertyAccessor = _typeAccessor.FindProperty(property);

        return Where(propertyAccessor.Column, parameterValue, filterOperator);
    }

    public UpdateEntityBuilder<TEntity> Where(Action<LogicalEntityBuilder<TEntity>> builder)
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
