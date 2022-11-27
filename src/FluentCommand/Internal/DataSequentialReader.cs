using System.Data;

namespace FluentCommand.Internal;

public static class DataSequentialReader
{
    public static IEnumerable<TEntity> Query<TEntity>(IDataCommand dataCommand, Func<IDataRecord, TEntity> factory)
    {
        var results = new List<TEntity>();

        dataCommand.Read(reader =>
        {
            while (reader.Read())
            {
                var entity = factory(reader);
                results.Add(entity);
            }
        }, CommandBehavior.SequentialAccess | CommandBehavior.SingleResult);

        return results;
    }

    public static TEntity QuerySingle<TEntity>(IDataCommand dataCommand, Func<IDataRecord, TEntity> factory)
    {
        TEntity result = default;

        dataCommand.Read(reader =>
        {
            if (reader.Read())
                result = factory(reader);
        }, CommandBehavior.SequentialAccess | CommandBehavior.SingleResult | CommandBehavior.SingleRow);

        return result;

    }

    public static async Task<IEnumerable<TEntity>> QueryAsync<TEntity>(IDataCommand dataCommand, Func<IDataRecord, TEntity> factory, CancellationToken cancellationToken = default)
    {
        var results = new List<TEntity>();

        await dataCommand.ReadAsync((reader, token) =>
        {
            while (reader.Read())
            {
                var entity = factory(reader);
                results.Add(entity);
            }
            return Task.CompletedTask;
        }, CommandBehavior.SequentialAccess | CommandBehavior.SingleResult, cancellationToken);


        return results;
    }

    public static async Task<TEntity> QuerySingleAsync<TEntity>(IDataCommand dataCommand, Func<IDataRecord, TEntity> factory, CancellationToken cancellationToken = default)
    {
        TEntity result = default;

        await dataCommand.ReadAsync((reader, token) =>
        {
            if (reader.Read())
                result = factory(reader);

            return Task.CompletedTask;
        }, CommandBehavior.SequentialAccess | CommandBehavior.SingleResult | CommandBehavior.SingleRow, cancellationToken);

        return result;
    }

}
