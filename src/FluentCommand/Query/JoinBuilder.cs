using FluentCommand.Query.Generators;

namespace FluentCommand.Query;

public class JoinBuilder : JoinBuilder<JoinBuilder>
{
    public JoinBuilder(IQueryGenerator queryGenerator, List<QueryParameter> parameters)
        : base(queryGenerator, parameters)
    {
    }
}

public class JoinBuilder<TBuilder> : StatementBuilder<TBuilder>
    where TBuilder : JoinBuilder<TBuilder>
{
    public JoinBuilder(IQueryGenerator queryGenerator, List<QueryParameter> parameters)
        : base(queryGenerator, parameters)
    {
    }

    protected JoinExpression JoinExpression { get; set; } = new();

    public TBuilder Left(
        string columnName,
        string tableAlias)
    {
        JoinExpression = JoinExpression with
        {
            LeftColumnName = columnName,
            LeftTableAlias = tableAlias
        };

        return (TBuilder)this;
    }

    public TBuilder Right(
        string columnName,
        string tableName,
        string tableSchema,
        string tableAlias)
    {
        JoinExpression = JoinExpression with
        {
            RightColumnName = columnName,
            RightTableName = tableName,
            RightTableSchema = tableSchema,
            RightTableAlias = tableAlias ?? tableName
        };

        return (TBuilder)this;
    }

    public TBuilder Type(JoinTypes joinType)
    {
        JoinExpression = JoinExpression with
        {
            JoinType = joinType
        };

        return (TBuilder)this;
    }

    public virtual JoinExpression BuildExpression()
    {
        return JoinExpression;
    }

    public override QueryStatement BuildStatement()
    {
        var joinClause = QueryGenerator.JoinExpression(JoinExpression);

        return new QueryStatement(joinClause, Parameters);
    }
}
