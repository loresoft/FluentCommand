using System.Data;

using Dapper;

using static Dapper.SqlMapper;

namespace FluentCommand;

/// <summary>
/// A class with data reader factory methods.
/// </summary>
public static class ReaderFactory
{
    /// <summary>
    /// A factory for creating TEntity objects from the current row in the <see cref="IDataReader" />.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    /// <param name="reader">The open <see cref="IDataReader" /> to get the object from.</param>
    /// <returns>A TEntity object having property names set that match the field names in the <see cref="IDataReader" />.</returns>
    public static TEntity EntityFactory<TEntity>(IDataReader reader)
        where TEntity : class, new()
    {
        // parser is cached in dapper, ok to repeated calls
        var parser = reader.GetRowParser<TEntity>();
        return parser(reader);
    }

    /// <summary>
    /// A factory for creating dynamic objects from the current row in the <see cref="IDataReader" />.
    /// </summary>
    /// <param name="reader">The open <see cref="IDataReader" /> to get the object from.</param>
    /// <returns>A dynamic object having property names set that match the field names in the <see cref="IDataReader" />.</returns>
    public static dynamic DynamicFactory(IDataReader reader)
    {
        // parser is cached in dapper, ok to repeated calls
        var parser = reader.GetRowParser<dynamic>();
        return parser(reader);
    }
}
