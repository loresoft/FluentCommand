using System.Collections.Concurrent;
using System.Data.Common;

using FluentCommand.Extensions;
using FluentCommand.Handlers;

namespace FluentCommand;

public static class DataParameterHandlers
{
    private static readonly ConcurrentDictionary<Type, IDataParameterHandler> _dataTypeHandlers;

    static DataParameterHandlers()
    {
        _dataTypeHandlers = new ConcurrentDictionary<Type, IDataParameterHandler>();
        _dataTypeHandlers.TryAdd(typeof(ConcurrencyToken), new ConcurrencyTokenHandler());

#if !NETSTANDARD2_0
        // once ADO supports DateOnly & TimeOnly, this can be removed
        _dataTypeHandlers.TryAdd(typeof(DateOnly), new DateOnlyHandler());
        _dataTypeHandlers.TryAdd(typeof(TimeOnly), new TimeOnlyHandler());
#endif
    }

    public static void AddTypeHandler<THandler>()
        where THandler : IDataParameterHandler, new()
    {
        var handler = new THandler();
        AddTypeHandler(handler);
    }

    public static void AddTypeHandler<THandler>(THandler handler)
        where THandler : IDataParameterHandler
    {
        _dataTypeHandlers.TryAdd(handler.ValueType, handler);
    }

    public static IDataParameterHandler GetTypeHandler(Type type)
    {
        var underlyingType = type.GetUnderlyingType();
        return _dataTypeHandlers.TryGetValue(underlyingType, out var handler) ? handler : null;
    }

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
