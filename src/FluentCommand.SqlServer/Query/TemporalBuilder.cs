using FluentCommand.Extensions;
using FluentCommand.Query.Generators;
using FluentCommand.Reflection;

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

    public TemporalBuilder From<TEntity>(
        string tableAlias = null)
    {
        var typeAccessor = TypeAccessor.GetAccessor<TEntity>();

        _from = new TableExpression(typeAccessor.TableName, typeAccessor.TableSchema, tableAlias);

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
        if (_temporal.IsNullOrWhiteSpace())
            return new QueryStatement(QueryGenerator.TableExpression(_from), Parameters);

        var table = QueryGenerator.TableExpression(new TableExpression(_from.TableName, _from.TableSchema));

        var statement = $"{table} {_temporal}";

        if (_from.TableAlias.HasValue())
            statement += $" AS [{_from.TableAlias}]";

        return new QueryStatement(statement, Parameters);
    }
}
