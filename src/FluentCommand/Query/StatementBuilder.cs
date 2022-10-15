using System.Runtime.CompilerServices;

using FluentCommand.Query.Generators;

namespace FluentCommand.Query;

public class StatementBuilder : StatementBuilder<StatementBuilder>
{
    public StatementBuilder(IQueryGenerator queryGenerator, List<QueryParameter> parameters)
        : base(queryGenerator, parameters)
    {
    }

    protected string Statement { get; set; }

    public StatementBuilder Query(string queryStatement)
    {
        Statement = queryStatement;
        return this;
    }

    public StatementBuilder Parameter<TValue>(string name, TValue value)
    {
        var queryParam = new QueryParameter(name, value, typeof(TValue));
        Parameters.Add(queryParam);

        return this;
    }

    public override QueryStatement BuildStatement()
    {
        return new QueryStatement(Statement, Parameters);
    }
}

public abstract class StatementBuilder<TBuilder> : IStatementBuilder, IQueryBuilder
    where TBuilder : StatementBuilder<TBuilder>
{
    protected StatementBuilder(IQueryGenerator queryGenerator, List<QueryParameter> parameters)
    {
        QueryGenerator = queryGenerator ?? throw new ArgumentNullException(nameof(queryGenerator));
        Parameters = parameters ?? throw new ArgumentNullException(nameof(parameters));
    }


    protected IQueryGenerator QueryGenerator { get; }

    protected List<QueryParameter> Parameters { get; }

    protected List<string> CommentExpressions { get; set; } = new();


    public TBuilder Tag(
        string comment = "Caller",
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string sourceFilePath = "",
        [CallerLineNumber] int sourceLineNumber = 0)
    {
        // augment comment with source file and line number
        var fileName = Path.GetFileName(sourceFilePath);

        var commentMember = $"{comment}; {memberName}() in {fileName}:line {sourceLineNumber}";
        var commentExpression = QueryGenerator.CommentExpression(commentMember);

        CommentExpressions.Add(commentExpression);

        return (TBuilder)this;
    }

    public abstract QueryStatement BuildStatement();

    protected string NextParameter() => $"@p{Parameters.Count:0000}";


    IQueryGenerator IQueryBuilder.QueryGenerator => QueryGenerator;

    List<QueryParameter> IQueryBuilder.Parameters => Parameters;
}
