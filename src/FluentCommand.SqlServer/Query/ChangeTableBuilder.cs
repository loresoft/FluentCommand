using FluentCommand.Extensions;
using FluentCommand.Query.Generators;
using FluentCommand.Reflection;

namespace FluentCommand.Query;

public class ChangeTableBuilder : StatementBuilder<ChangeTableBuilder>
{
    private TableExpression _fromTable;
    private QueryParameter _parameter;

    public ChangeTableBuilder(IQueryGenerator queryGenerator, List<QueryParameter> parameters) : base(queryGenerator, parameters)
    {
    }

    public ChangeTableBuilder From(
        string tableName,
        string tableSchema = null,
        string tableAlias = null)
    {
        _fromTable = new TableExpression(tableName, tableSchema, tableAlias);

        return this;
    }

    public ChangeTableBuilder From<TEntity>(
        string tableAlias = null)
    {
        var typeAccessor = TypeAccessor.GetAccessor<TEntity>();

        _fromTable = new TableExpression(typeAccessor.TableName, typeAccessor.TableSchema, tableAlias);

        return this;
    }

    public ChangeTableBuilder LastVersion(long lastVersion)
    {
        var name = NextParameter();
        _parameter = new QueryParameter(name, lastVersion, typeof(long));

        Parameters.Add(_parameter);

        return this;
    }

    public override QueryStatement BuildStatement()
    {
        if (_parameter == null)
            return new QueryStatement(QueryGenerator.TableExpression(_fromTable), Parameters);

        var table = QueryGenerator.TableExpression(new TableExpression(_fromTable.TableName, _fromTable.TableSchema));

        var statement = $"CHANGETABLE (CHANGES {table}, {_parameter.Name})";

        if (_fromTable.TableAlias.HasValue())
            statement += $" AS [{_fromTable.TableAlias}]";

        return new QueryStatement(statement, Parameters);
    }
}
