using System;
using System.Collections.Generic;
using System.Data;
using System.Dynamic;
using FluentCommand.Reflection;

namespace FluentCommand
{
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
                var fieldType = reader.GetFieldType(i);

                var coerceValue = ReflectionHelper.CoerceValue(memberAccessor.MemberType, fieldType, value);
                memberAccessor.SetValue(entity, coerceValue);
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
}