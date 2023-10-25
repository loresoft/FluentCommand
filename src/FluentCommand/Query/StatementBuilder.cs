using System.Runtime.CompilerServices;

using FluentCommand.Extensions;
using FluentCommand.Query.Generators;

namespace FluentCommand.Query;

/// <summary>
/// Query statement builder
/// </summary>
public class StatementBuilder : StatementBuilder<StatementBuilder>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="StatementBuilder"/> class.
    /// </summary>
    /// <param name="queryGenerator">The query generator.</param>
    /// <param name="parameters">The query parameters.</param>
    public StatementBuilder(IQueryGenerator queryGenerator, List<QueryParameter> parameters)
        : base(queryGenerator, parameters)
    {
    }

    /// <summary>
    /// Gets or sets the query sql statement.
    /// </summary>
    /// <value>
    /// The query sql statement.
    /// </value>
    protected string Statement { get; set; }

    /// <summary>
    /// Sets the query statement.
    /// </summary>
    /// <param name="queryStatement">The query statement.</param>
    /// <returns>
    /// The same builder so that multiple calls can be chained.
    /// </returns>
    public StatementBuilder Query(string queryStatement)
    {
        Statement = queryStatement;
        return this;
    }

    /// <summary>
    /// Adds a query parameter the specified name and value.
    /// </summary>
    /// <typeparam name="TValue">The type of the value.</typeparam>
    /// <param name="name">The parameter name.</param>
    /// <param name="value">The parameter value.</param>
    /// <returns>
    /// The same builder so that multiple calls can be chained.
    /// </returns>
    public StatementBuilder Parameter<TValue>(string name, TValue value)
    {
        var queryParam = new QueryParameter(name, value, typeof(TValue));
        Parameters.Add(queryParam);

        return this;
    }

    /// <inheritdoc />
    public override QueryStatement BuildStatement()
    {
        return new QueryStatement(Statement, Parameters);
    }
}

/// <summary>
/// Base class for query statement 
/// </summary>
/// <typeparam name="TBuilder">The type of the builder.</typeparam>
public abstract class StatementBuilder<TBuilder> : IStatementBuilder, IQueryBuilder
    where TBuilder : StatementBuilder<TBuilder>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="StatementBuilder{TBuilder}"/> class.
    /// </summary>
    /// <param name="queryGenerator">The query generator.</param>
    /// <param name="parameters">The query parameters.</param>
    /// <exception cref="System.ArgumentNullException">queryGenerator or parameters is null</exception>
    protected StatementBuilder(IQueryGenerator queryGenerator, List<QueryParameter> parameters)
    {
        QueryGenerator = queryGenerator ?? throw new ArgumentNullException(nameof(queryGenerator));
        Parameters = parameters ?? throw new ArgumentNullException(nameof(parameters));
    }


    /// <summary>
    /// Gets the query generator.
    /// </summary>
    /// <value>
    /// The query generator.
    /// </value>
    protected IQueryGenerator QueryGenerator { get; }

    /// <summary>
    /// Gets the query parameters.
    /// </summary>
    /// <value>
    /// The query parameters.
    /// </value>
    protected List<QueryParameter> Parameters { get; }

    /// <summary>
    /// Gets or sets the query comment expressions.
    /// </summary>
    /// <value>
    /// The query comment expressions.
    /// </value>
    protected List<string> CommentExpressions { get; set; } = new();


    /// <summary>
    /// Tags the query with specified comment, caller name, file and line number.
    /// </summary>
    /// <param name="comment">The query comment.</param>
    /// <param name="memberName">The caller member name.</param>
    /// <param name="sourceFilePath">The caller source file path.</param>
    /// <param name="sourceLineNumber">The caller source line number.</param>
    /// <returns></returns>
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

    /// <summary>
    /// Adds the specified comment to the query.
    /// </summary>
    /// <param name="comment">The query comment.</param>
    /// <returns>
    /// The same builder so that multiple calls can be chained.
    /// </returns>
    public TBuilder Comment(string comment)
    {
        if (comment.IsNullOrWhiteSpace())
            return (TBuilder)this;

        var commentExpression = QueryGenerator.CommentExpression(comment);

        CommentExpressions.Add(commentExpression);

        return (TBuilder)this;
    }


    /// <inheritdoc />
    public abstract QueryStatement BuildStatement();

    /// <summary>
    /// Gets the next unique parameter name.
    /// </summary>
    /// <returns>The next unique parameter name</returns>
    protected string NextParameter() => $"@p{Parameters.Count:0000}";


    /// <inheritdoc />
    IQueryGenerator IQueryBuilder.QueryGenerator => QueryGenerator;

    /// <inheritdoc />
    List<QueryParameter> IQueryBuilder.Parameters => Parameters;
}
