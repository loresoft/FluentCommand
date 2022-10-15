namespace FluentCommand.Query;

public interface IQueryStatement
{
    IReadOnlyCollection<QueryParameter> Parameters { get; }
    string Statement { get; }
}
