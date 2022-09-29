using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;

using FluentCommand.Query.Generators;

namespace FluentCommand;

public abstract class StatementBuilder<TBuilder> : IStatementBuilder
    where TBuilder : StatementBuilder<TBuilder>
{
    protected StatementBuilder(IQueryGenerator queryGenerator, List<QueryParameter> parameters)
    {
        QueryGenerator = queryGenerator ?? throw new System.ArgumentNullException(nameof(queryGenerator));
        Parameters = parameters ?? throw new System.ArgumentNullException(nameof(parameters));
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

        var commentExpression = QueryGenerator.CommentClause(commentMember);

        CommentExpressions.Add(commentExpression);

        return (TBuilder)this;
    }

    public abstract QueryStatement BuildStatement();

    protected string NextParameter() => $"@p{Parameters.Count:0000}";
}
