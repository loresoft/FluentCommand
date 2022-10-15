using FluentCommand.Extensions;
using FluentCommand.Query.Generators;

namespace FluentCommand.Query;

public class TemporalBuilder : StatementBuilder<TemporalBuilder>
{
    private TableExpression _from;
    private string _temporal;

    public TemporalBuilder(IQueryGenerator queryGenerator, List<QueryParameter> parameters)
        : base(queryGenerator, parameters)
    {
    }

    public TemporalBuilder From(
        string tableName,
        string tableSchema = null,
        string tableAlias = null)
    {
        _from = new TableExpression(tableName, tableSchema, tableAlias);

        return this;
    }

    public TemporalBuilder All()
    {
        _temporal = "FOR SYSTEM_TIME ALL";
        return this;
    }

    public TemporalBuilder AsOf(DateTime utcPointInTime)
    {
        var paramName = NextParameter();
        var queryParameter = new QueryParameter(paramName, utcPointInTime, typeof(DateTime));

        _temporal = $"FOR SYSTEM_TIME AS OF {paramName}";
        Parameters.Add(queryParameter);

        return this;
    }

    public TemporalBuilder Between(DateTime utcStart, DateTime utcEnd)
    {
        var nameStart = NextParameter();
        var paramStart = new QueryParameter(nameStart, utcStart, typeof(DateTime));

        var nameEnd = NextParameter();
        var paramEnd = new QueryParameter(nameEnd, utcEnd, typeof(DateTime));

        _temporal = $"FOR SYSTEM_TIME BETWEEN {nameStart} AND {nameEnd}";

        Parameters.Add(paramStart);
        Parameters.Add(paramEnd);

        return this;
    }

    public TemporalBuilder ContainedIn(DateTime utcStart, DateTime utcEnd)
    {
        //FOR SYSTEM_TIME CONTAINED IN (<start_date_time>, <end_date_time>)

        var nameStart = NextParameter();
        var paramStart = new QueryParameter(nameStart, utcStart, typeof(DateTime));

        var nameEnd = NextParameter();
        var paramEnd = new QueryParameter(nameEnd, utcEnd, typeof(DateTime));

        _temporal = $"FOR SYSTEM_TIME CONTAINED IN ({nameStart}, {nameEnd})";

        Parameters.Add(paramStart);
        Parameters.Add(paramEnd);


        return this;
    }

    public TemporalBuilder FromTo(DateTime utcStart, DateTime utcEnd)
    {
        var nameStart = NextParameter();
        var paramStart = new QueryParameter(nameStart, utcStart, typeof(DateTime));

        var nameEnd = NextParameter();
        var paramEnd = new QueryParameter(nameEnd, utcEnd, typeof(DateTime));

        _temporal = $"FOR SYSTEM_TIME FROM {nameStart} AND {nameEnd}";

        Parameters.Add(paramStart);
        Parameters.Add(paramEnd);

        return this;
    }


    public override QueryStatement BuildStatement()
    {
        var fromTable = QueryGenerator.TableExpression(_from);

        var statement = _temporal.HasValue() ? $"{fromTable} {_temporal}" : fromTable;

        return new QueryStatement(statement, Parameters);
    }
}
