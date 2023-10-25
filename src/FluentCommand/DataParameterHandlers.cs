using System.Collections.Concurrent;
using System.Data.Common;

using FluentCommand.Extensions;
using FluentCommand.Handlers;

namespace FluentCommand;

/// <summary>
/// 
/// </summary>
public static class DataParameterHandlers
{
    private static readonly ConcurrentDictionary<Type, IDataParameterHandler> _dataTypeHandlers;

    static DataParameterHandlers()
    {
        _dataTypeHandlers = new ConcurrentDictionary<Type, IDataParameterHandler>();
        _dataTypeHandlers.TryAdd(typeof(ConcurrencyToken), new ConcurrencyTokenHandler());

#if NET6_0_OR_GREATER
        // once ADO supports DateOnly & TimeOnly, this can be removed
        _dataTypeHandlers.TryAdd(typeof(DateOnly), new DateOnlyHandler());
        _dataTypeHandlers.TryAdd(typeof(TimeOnly), new TimeOnlyHandler());
#endif
    }

    /// <summary>
    /// Adds the data type handler.
    /// </summary>
    /// <typeparam name="THandler">The type of the handler.</typeparam>
    public static void AddTypeHandler<THandler>()
        where THandler : IDataParameterHandler, new()
    {
        var handler = new THandler();
        AddTypeHandler(handler);
    }

    /// <summary>
    /// Adds the data type handler.
    /// </summary>
    /// <typeparam name="THandler">The type of the handler.</typeparam>
    /// <param name="handler">The handler.</param>
    public static void AddTypeHandler<THandler>(THandler handler)
        where THandler : IDataParameterHandler
    {
        _dataTypeHandlers.TryAdd(handler.ValueType, handler);
    }

    /// <summary>
    /// Gets the type handler for the specified <paramref name="type"/>.
    /// </summary>
    /// <param name="type">The type to get a handler for.</param>
    /// <returns>The <see cref="IDataParameterHandler"/> for the specified type; otherwise null if not found</returns>
    public static IDataParameterHandler GetTypeHandler(Type type)
    {
        var underlyingType = type.GetUnderlyingType();
        return _dataTypeHandlers.TryGetValue(underlyingType, out var handler) ? handler : null;
    }


    /// <summary>
    /// Sets the Value and DbType on the specified <paramref name="parameter"/> using the registered type handlers.
    /// </summary>
    /// <param name="parameter">The parameter to set the value on.</param>
    /// <param name="value">The value to set.</param>
    /// <param name="type">The data type to use.</param>
    public static void SetValue(DbParameter parameter, object value, Type type)
    {
        var valueType = type.GetUnderlyingType();
        var handler = GetTypeHandler(valueType);

        value ??= DBNull.Value;

        if (handler != null)
        {
            handler.SetValue(parameter, value);
            return;
        }

        parameter.Value = value;
        parameter.DbType = valueType.ToDbType();
    }
}
