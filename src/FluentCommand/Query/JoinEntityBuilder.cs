using System.Linq.Expressions;

using FluentCommand.Query.Generators;
using FluentCommand.Reflection;

namespace FluentCommand.Query;

/// <summary>
/// Provides a builder for constructing SQL JOIN clauses between two entity types with fluent, chainable methods.
/// </summary>
/// <typeparam name="TLeft">The entity type of the left join.</typeparam>
/// <typeparam name="TRight">The entity type of the right join.</typeparam>
public class JoinEntityBuilder<TLeft, TRight> : JoinBuilder<JoinEntityBuilder<TLeft, TRight>>
    where TLeft : class
    where TRight : class
{
    private static readonly TypeAccessor _leftAccessor = TypeAccessor.GetAccessor<TLeft>();
    private static readonly TypeAccessor _rightAccessor = TypeAccessor.GetAccessor<TRight>();

    /// <summary>
    /// Initializes a new instance of the <see cref="JoinEntityBuilder{TLeft, TRight}"/> class.
    /// </summary>
    /// <param name="queryGenerator">The <see cref="IQueryGenerator"/> used to generate SQL expressions.</param>
    /// <param name="parameters">The list of <see cref="QueryParameter"/> objects for the query.</param>
    public JoinEntityBuilder(IQueryGenerator queryGenerator, List<QueryParameter> parameters)
        : base(queryGenerator, parameters)
    {
    }

    /// <summary>
    /// Specifies the left property to join on using a strongly-typed property expression.
    /// </summary>
    /// <typeparam name="TValue">The type of the property value.</typeparam>
    /// <param name="property">An expression selecting the property on the left entity to join on.</param>
    /// <param name="tableAlias">The alias of the left table.</param>
    /// <returns>
    /// The same builder instance for method chaining.
    /// </returns>
    public JoinEntityBuilder<TLeft, TRight> Left<TValue>(
        Expression<Func<TLeft, TValue>> property,
        string tableAlias)
    {
        var propertyAccessor = _leftAccessor.FindProperty(property);

        return Left(propertyAccessor.Column, tableAlias);
    }

    /// <summary>
    /// Specifies the right property to join on using a strongly-typed property expression.
    /// The table name and schema are inferred from the right entity type.
    /// </summary>
    /// <typeparam name="TValue">The type of the property value.</typeparam>
    /// <param name="property">An expression selecting the property on the right entity to join on.</param>
    /// <param name="tableAlias">The alias of the right table.</param>
    /// <returns>
    /// The same builder instance for method chaining.
    /// </returns>
    public JoinEntityBuilder<TLeft, TRight> Right<TValue>(
        Expression<Func<TRight, TValue>> property,
        string tableAlias)
    {
        return Right(
            property,
            _rightAccessor.TableName,
            _rightAccessor.TableSchema,
            tableAlias);
    }

    /// <summary>
    /// Specifies the right property to join on using a strongly-typed property expression and explicit table information.
    /// </summary>
    /// <typeparam name="TValue">The type of the property value.</typeparam>
    /// <param name="property">An expression selecting the property on the right entity to join on.</param>
    /// <param name="tableName">The name of the right table. If <c>null</c>, the entity mapping is used.</param>
    /// <param name="tableSchema">The schema of the right table. If <c>null</c>, the entity mapping is used.</param>
    /// <param name="tableAlias">The alias of the right table. If <c>null</c>, the table name is used as the alias.</param>
    /// <returns>
    /// The same builder instance for method chaining.
    /// </returns>
    public JoinEntityBuilder<TLeft, TRight> Right<TValue>(
        Expression<Func<TRight, TValue>> property,
        string tableName,
        string tableSchema,
        string tableAlias)
    {
        var propertyAccessor = _rightAccessor.FindProperty(property);

        return Right(
            propertyAccessor.Column,
            tableName ?? _rightAccessor.TableName,
            tableSchema ?? _rightAccessor.TableSchema,
            tableAlias ?? _rightAccessor.TableName);
    }
}
