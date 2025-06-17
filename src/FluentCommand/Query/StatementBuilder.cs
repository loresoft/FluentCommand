using System.Runtime.CompilerServices;

using FluentCommand.Extensions;
using FluentCommand.Query.Generators;

namespace FluentCommand.Query;

/// <summary>
/// Provides a builder for constructing raw SQL query statements and parameters.
/// </summary>
public class StatementBuilder : StatementBuilder<StatementBuilder>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="StatementBuilder"/> class.
    /// </summary>
    /// <param name="queryGenerator">The <see cref="IQueryGenerator"/> used to generate SQL expressions and comments.</param>
    /// <param name="parameters">The initial list of <see cref="QueryParameter"/> objects for the query.</param>
    public StatementBuilder(IQueryGenerator queryGenerator, List<QueryParameter> parameters)
        : base(queryGenerator, parameters)
    {
    }

    /// <summary>
    /// Gets or sets the SQL query statement to be executed.
    /// </summary>
    /// <value>
    /// The SQL query statement as a <see cref="string"/>.
    /// </value>
    protected string Statement { get; set; }

    /// <summary>
    /// Sets the SQL query statement for this builder.
    /// </summary>
    /// <param name="queryStatement">The SQL query statement as a <see cref="string"/>.</param>
    /// <returns>
    /// The current <see cref="StatementBuilder"/> instance for method chaining.
    /// </returns>
    public StatementBuilder Query(string queryStatement)
    {
        Statement = queryStatement;
        return this;
    }

    /// <summary>
    /// Adds a query parameter with the specified name and value to the builder.
    /// </summary>
    /// <typeparam name="TValue">The type of the parameter value.</typeparam>
    /// <param name="name">The name of the parameter.</param>
    /// <param name="value">The value of the parameter.</param>
    /// <returns>
    /// The current <see cref="StatementBuilder"/> instance for method chaining.
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
/// Provides a base class for building SQL query statements, supporting tagging, comments, and parameter management.
/// </summary>
/// <typeparam name="TBuilder">The type of the builder for fluent chaining.</typeparam>
public abstract class StatementBuilder<TBuilder> : IStatementBuilder, IQueryBuilder
    where TBuilder : StatementBuilder<TBuilder>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="StatementBuilder{TBuilder}"/> class.
    /// </summary>
    /// <param name="queryGenerator">The <see cref="IQueryGenerator"/> used to generate SQL expressions and comments.</param>
    /// <param name="parameters">The initial list of <see cref="QueryParameter"/> objects for the query.</param>
    /// <exception cref="System.ArgumentNullException">
    /// Thrown if <paramref name="queryGenerator"/> or <paramref name="parameters"/> is <c>null</c>.
    /// </exception>
    protected StatementBuilder(IQueryGenerator queryGenerator, List<QueryParameter> parameters)
    {
        QueryGenerator = queryGenerator ?? throw new ArgumentNullException(nameof(queryGenerator));
        Parameters = parameters ?? throw new ArgumentNullException(nameof(parameters));
    }

    /// <summary>
    /// Gets the <see cref="IQueryGenerator"/> used to generate SQL expressions and comments.
    /// </summary>
    /// <value>
    /// The <see cref="IQueryGenerator"/> instance.
    /// </value>
    protected IQueryGenerator QueryGenerator { get; }

    /// <summary>
    /// Gets the list of <see cref="QueryParameter"/> objects used in the query.
    /// </summary>
    /// <value>
    /// The list of query parameters.
    /// </value>
    protected List<QueryParameter> Parameters { get; }

    /// <summary>
    /// Gets or sets the list of comment expressions to be included in the query.
    /// </summary>
    /// <value>
    /// The list of comment expressions.
    /// </value>
    protected List<string> CommentExpressions { get; set; } = new();

    /// <summary>
    /// Tags the query with a comment that includes the specified text, caller member name, source file, and line number.
    /// </summary>
    /// <param name="comment">The comment text to include in the tag. Defaults to "Caller".</param>
    /// <param name="memberName">The name of the calling member. Automatically provided by the compiler.</param>
    /// <param name="sourceFilePath">The source file path of the caller. Automatically provided by the compiler.</param>
    /// <param name="sourceLineNumber">The line number in the source file. Automatically provided by the compiler.</param>
    /// <returns>
    /// The current builder instance for method chaining.
    /// </returns>
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
    /// Adds a custom comment to the query.
    /// </summary>
    /// <param name="comment">The comment text to add.</param>
    /// <returns>
    /// The current builder instance for method chaining.
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
    /// Generates the next unique parameter name for use in the query.
    /// </summary>
    /// <returns>
    /// A unique parameter name as a <see cref="string"/> (e.g., "@p0001").
    /// </returns>
    protected string NextParameter() => $"@p{Parameters.Count:0000}";

    /// <inheritdoc />
    IQueryGenerator IQueryBuilder.QueryGenerator => QueryGenerator;

    /// <inheritdoc />
    List<QueryParameter> IQueryBuilder.Parameters => Parameters;
}
