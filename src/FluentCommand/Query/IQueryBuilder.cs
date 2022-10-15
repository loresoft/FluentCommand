using FluentCommand.Query.Generators;

namespace FluentCommand.Query;

public interface IQueryBuilder
{
    IQueryGenerator QueryGenerator { get; }

    List<QueryParameter> Parameters { get; }
}
