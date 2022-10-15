using System.Linq.Expressions;

using FluentCommand.Query.Generators;
using FluentCommand.Reflection;

namespace FluentCommand.Query;

public class JoinEntityBuilder<TLeft, TRight> : JoinBuilder<JoinEntityBuilder<TLeft, TRight>>
    where TLeft : class
    where TRight : class
{
    private static readonly TypeAccessor _leftAccessor = TypeAccessor.GetAccessor<TLeft>();
    private static readonly TypeAccessor _rightAccessor = TypeAccessor.GetAccessor<TRight>();

    public JoinEntityBuilder(IQueryGenerator queryGenerator, List<QueryParameter> parameters)
        : base(queryGenerator, parameters)
    {
    }

    public JoinEntityBuilder<TLeft, TRight> Left<TValue>(
        Expression<Func<TLeft, TValue>> property,
        string tableAlias)
    {
        var propertyAccessor = _leftAccessor.FindProperty(property);


        return Left(propertyAccessor.Column, tableAlias);
    }

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
