using System.Linq.Expressions;

using FluentCommand.Query.Generators;
using FluentCommand.Reflection;

namespace FluentCommand.Query;

/// <summary>
/// Join clause builder
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
    /// <param name="queryGenerator">The query generator.</param>
    /// <param name="parameters">The query parameters.</param>
    public JoinEntityBuilder(IQueryGenerator queryGenerator, List<QueryParameter> parameters)
        : base(queryGenerator, parameters)
    {
    }

    /// <summary>
    /// The left property to join on.
    /// </summary>
    /// <typeparam name="TValue">The type of the value.</typeparam>
    /// <param name="property">The property.</param>
    /// <param name="tableAlias">The table alias.</param>
    /// <returns>
    /// The same builder so that multiple calls can be chained.
    /// </returns>
    public JoinEntityBuilder<TLeft, TRight> Left<TValue>(
        Expression<Func<TLeft, TValue>> property,
        string tableAlias)
    {
        var propertyAccessor = _leftAccessor.FindProperty(property);


        return Left(propertyAccessor.Column, tableAlias);
    }

    /// <summary>
    /// The right property to join on.
    /// </summary>
    /// <typeparam name="TValue">The type of the value.</typeparam>
    /// <param name="property">The property.</param>
    /// <param name="tableAlias">The table alias.</param>
    /// <returns>
    /// The same builder so that multiple calls can be chained.
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
    /// The right property to join on.
    /// </summary>
    /// <typeparam name="TValue">The type of the value.</typeparam>
    /// <param name="property">The property.</param>
    /// <param name="tableName">Name of the table.</param>
    /// <param name="tableSchema">The table schema.</param>
    /// <param name="tableAlias">The table alias.</param>
    /// <returns>
    /// The same builder so that multiple calls can be chained.
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
