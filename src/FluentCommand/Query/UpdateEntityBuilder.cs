using System.Linq.Expressions;

using FluentCommand.Extensions;
using FluentCommand.Query.Generators;
using FluentCommand.Reflection;

namespace FluentCommand.Query;

public class UpdateEntityBuilder<TEntity>
    : UpdateBuilder<UpdateEntityBuilder<TEntity>>, IWhereEntityBuilder<TEntity, UpdateEntityBuilder<TEntity>>
    where TEntity : class
{
    private static readonly TypeAccessor _typeAccessor = TypeAccessor.GetAccessor<TEntity>();

    public UpdateEntityBuilder(
        IQueryGenerator queryGenerator,
        List<QueryParameter> parameters,
        LogicalOperators logicalOperator = LogicalOperators.And)
        : base(queryGenerator, parameters, logicalOperator)
    {
    }

    public UpdateEntityBuilder<TEntity> Value<TValue>(
        Expression<Func<TEntity, TValue>> property,
        TValue parameterValue)
    {
        var propertyAccessor = _typeAccessor.FindProperty(property);
        return Value(propertyAccessor?.Column, parameterValue);
    }

    public UpdateEntityBuilder<TEntity> ValueIf<TValue>(
        Expression<Func<TEntity, TValue>> property,
        TValue parameterValue,
        Func<string, TValue, bool> condition)
    {
        var propertyAccessor = _typeAccessor.FindProperty(property);
        return ValueIf(propertyAccessor?.Column, parameterValue, condition);
    }

    public UpdateEntityBuilder<TEntity> Values(
        TEntity entity,
        IEnumerable<string> columnNames = null)
    {
        if (entity is null)
            throw new ArgumentNullException(nameof(entity));

        var properties = _typeAccessor.GetProperties();
        var columnSet = new HashSet<string>(columnNames ?? Enumerable.Empty<string>());

        foreach (var property in properties)
        {
            if (columnSet.Count > 0 && !columnSet.Contains(property.Column))
                continue;

            if (property.IsNotMapped || property.IsDatabaseGenerated)
                continue;

            // include the type to prevent issues with null
            Value(property.Column, property.GetValue(entity), property.MemberType);
        }

        return this;
    }


    public UpdateEntityBuilder<TEntity> Output<TValue>(
        Expression<Func<TEntity, TValue>> property,
        string tableAlias = "INSERTED",
        string columnAlias = null)
    {
        var propertyAccessor = _typeAccessor.FindProperty(property);
        return Output(propertyAccessor?.Column, tableAlias, columnAlias);
    }

    public UpdateEntityBuilder<TEntity> OutputIf<TValue>(
        Expression<Func<TEntity, TValue>> property,
        string tableAlias = "INSERTED",
        string columnAlias = null,
        Func<string, bool> condition = null)
    {
        var propertyAccessor = _typeAccessor.FindProperty(property);
        return OutputIf(propertyAccessor?.Column, tableAlias, columnAlias, condition);
    }

    public override UpdateEntityBuilder<TEntity> From(
        string tableName = null,
        string tableSchema = null,
        string tableAlias = null)
    {
        return base.From(
            tableName ?? _typeAccessor.TableName,
            tableSchema ?? _typeAccessor.TableSchema,
            tableAlias);
    }

    public UpdateEntityBuilder<TEntity> Join<TRight>(Action<JoinEntityBuilder<TEntity, TRight>> builder)
        where TRight : class
    {
        var innerBuilder = new JoinEntityBuilder<TEntity, TRight>(QueryGenerator, Parameters);
        builder(innerBuilder);

        JoinExpressions.Add(innerBuilder.BuildExpression());

        return this;
    }

    public UpdateEntityBuilder<TEntity> Join<TLeft, TRight>(Action<JoinEntityBuilder<TLeft, TRight>> builder)
        where TLeft : class
        where TRight : class
    {
        var innerBuilder = new JoinEntityBuilder<TLeft, TRight>(QueryGenerator, Parameters);
        builder(innerBuilder);

        JoinExpressions.Add(innerBuilder.BuildExpression());

        return this;
    }


    public UpdateEntityBuilder<TEntity> Where<TValue>(
        Expression<Func<TEntity, TValue>> property,
        TValue parameterValue,
        FilterOperators filterOperator = FilterOperators.Equal)
    {
        return Where<TValue>(property, parameterValue, null, filterOperator);
    }

    public UpdateEntityBuilder<TEntity> Where<TValue>(
        Expression<Func<TEntity, TValue>> property,
        TValue parameterValue,
        string tableAlias,
        FilterOperators filterOperator = FilterOperators.Equal)
    {
        var propertyAccessor = _typeAccessor.FindProperty(property);

        return Where(propertyAccessor.Column, parameterValue, tableAlias, filterOperator);
    }

    public UpdateEntityBuilder<TEntity> Where<TModel, TValue>(
        Expression<Func<TModel, TValue>> property,
        TValue parameterValue,
        string tableAlias,
        FilterOperators filterOperator = FilterOperators.Equal)
    {
        var typeAccessor = TypeAccessor.GetAccessor<TModel>();
        var propertyAccessor = typeAccessor.FindProperty(property);

        return Where(propertyAccessor.Column, parameterValue, tableAlias, filterOperator);
    }

    public UpdateEntityBuilder<TEntity> WhereIf<TValue>(
        Expression<Func<TEntity, TValue>> property,
        TValue parameterValue,
        FilterOperators filterOperator = FilterOperators.Equal,
        Func<string, TValue, bool> condition = null)
    {
        return WhereIf(property, parameterValue, null, filterOperator, condition);
    }

    public UpdateEntityBuilder<TEntity> WhereIf<TValue>(
        Expression<Func<TEntity, TValue>> property,
        TValue parameterValue,
        string tableAlias,
        FilterOperators filterOperator = FilterOperators.Equal,
        Func<string, TValue, bool> condition = null)
    {
        var propertyAccessor = _typeAccessor.FindProperty(property);

        return WhereIf(propertyAccessor.Column, parameterValue, tableAlias, filterOperator, condition);
    }

    public UpdateEntityBuilder<TEntity> WhereIn<TValue>(
        Expression<Func<TEntity, TValue>> property,
        IEnumerable<TValue> parameterValues,
        string tableAlias)
    {
        var propertyAccessor = _typeAccessor.FindProperty(property);

        return WhereIn(propertyAccessor?.Column, parameterValues, tableAlias);
    }

    public UpdateEntityBuilder<TEntity> WhereInIf<TValue>(
        Expression<Func<TEntity, TValue>> property,
        IEnumerable<TValue> parameterValues,
        Func<string, IEnumerable<TValue>, bool> condition = null)
    {
        var propertyAccessor = _typeAccessor.FindProperty(property);

        return WhereInIf(propertyAccessor?.Column, parameterValues, condition);
    }

    public UpdateEntityBuilder<TEntity> WhereInIf<TValue>(
        Expression<Func<TEntity, TValue>> property,
        IEnumerable<TValue> parameterValues,
        string tableAlias,
        Func<string, IEnumerable<TValue>, bool> condition = null)
    {
        var propertyAccessor = _typeAccessor.FindProperty(property);

        return WhereInIf(propertyAccessor?.Column, parameterValues, tableAlias, condition);
    }

    public UpdateEntityBuilder<TEntity> WhereOr(Action<LogicalEntityBuilder<TEntity>> builder)
    {
        var innerBuilder = new LogicalEntityBuilder<TEntity>(QueryGenerator, Parameters, LogicalOperators.Or);

        builder(innerBuilder);

        var statement = innerBuilder.BuildStatement();

        if (statement != null && statement.Statement.HasValue())
            WhereExpressions.Add(new WhereExpression(statement.Statement, IsRaw: true));

        return this;
    }

    public UpdateEntityBuilder<TEntity> WhereAnd(Action<LogicalEntityBuilder<TEntity>> builder)
    {
        var innerBuilder = new LogicalEntityBuilder<TEntity>(QueryGenerator, Parameters, LogicalOperators.And);

        builder(innerBuilder);

        var statement = innerBuilder.BuildStatement();

        if (statement != null && statement.Statement.HasValue())
            WhereExpressions.Add(new WhereExpression(statement.Statement, IsRaw: true));

        return this;
    }


    public override QueryStatement BuildStatement()
    {
        // add table and schema from attribute if not set
        if (TableExpression == null)
            Table(_typeAccessor.TableName, _typeAccessor.TableSchema);

        return base.BuildStatement();
    }
}
