using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

using FluentCommand.Query.Generators;
using FluentCommand.Reflection;

namespace FluentCommand;

public class InsertEntityBuilder<TEntity> : InsertBuilder<InsertEntityBuilder<TEntity>>
    where TEntity : class
{
    private static readonly TypeAccessor _typeAccessor = TypeAccessor.GetAccessor<TEntity>();

    public InsertEntityBuilder(IQueryGenerator queryGenerator, Dictionary<string, object> parameters)
        : base(queryGenerator, parameters)
    {
    }

    public InsertEntityBuilder<TEntity> Value<TValue>(Expression<Func<TEntity, TValue>> property, TValue parameterValue)
    {
        var propertyAccessor = _typeAccessor.FindProperty(property);
        return Value(propertyAccessor.Column, parameterValue);
    }

    public InsertEntityBuilder<TEntity> Values(TEntity entity, IEnumerable<string> columnNames = null)
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

    public InsertEntityBuilder<TEntity> Output<TValue>(Expression<Func<TEntity, TValue>> property, string prefix = "INSERTED", string alias = null)
    {
        var propertyAccessor = _typeAccessor.FindProperty(property);
        return Output(propertyAccessor.Column, prefix, alias);
    }

    public override QueryStatement BuildStatement()
    {
        // add table and schema from attribute if not set
        if (IntoClause.Count == 0)
            Into(_typeAccessor.TableName, _typeAccessor.TableSchema);

        return base.BuildStatement();
    }
}
