using System.Data;
using System.Dynamic;

using FluentCommand.Reflection;

namespace FluentCommand;

/// <summary>
/// Extension methods for <see cref="IDataReader"/>
/// </summary>
public static class DataReaderExtensions
{
    /// <summary>
    /// A factory for creating TEntity objects from the current row in the <see cref="IDataReader" />.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    /// <param name="reader">The open <see cref="IDataReader" /> to get the object from.</param>
    /// <returns>A TEntity object having property names set that match the field names in the <see cref="IDataReader" />.</returns>
    [Obsolete("Use generated data reader factory")]
    public static TEntity EntityFactory<TEntity>(this IDataReader reader)
        where TEntity : class, new()
    {
        var entityAccessor = TypeAccessor.GetAccessor<TEntity>();
        var entity = new TEntity();

        for (int i = 0; i < reader.FieldCount; i++)
        {
            if (reader.IsDBNull(i))
                continue;

            var name = reader.GetName(i);

            var memberAccessor = entityAccessor.FindColumn(name);
            if (memberAccessor == null)
                continue;

            var value = reader.GetValue(i);

            memberAccessor.SetValue(entity, value);
        }

        return entity;
    }

    /// <summary>
    /// A factory for creating dynamic objects from the current row in the <see cref="IDataReader" />.
    /// </summary>
    /// <param name="reader">The open <see cref="IDataReader" /> to get the object from.</param>
    /// <returns>A dynamic object having property names set that match the field names in the <see cref="IDataReader" />.</returns>
    public static dynamic DynamicFactory(IDataReader reader)
    {
        dynamic expando = new ExpandoObject();
        var dictionary = expando as IDictionary<string, object>;

        for (int i = 0; i < reader.FieldCount; i++)
            dictionary.Add(reader.GetName(i), reader[i]);

        return expando;
    }
}
