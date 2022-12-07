using System.Collections.Concurrent;

namespace FluentCommand.Handlers;

public static class DataTypeHandlers
{
    private static readonly ConcurrentDictionary<Type, IDataTypeHandler> _dataTypeHandlers;

    static DataTypeHandlers()
    {
        _dataTypeHandlers = new ConcurrentDictionary<Type, IDataTypeHandler>();
        _dataTypeHandlers.TryAdd(typeof(ConcurrencyToken), new ConcurrencyTokenHandler());
        // once ADO supports DateOnly & TimeOnly, this can be removed

        _dataTypeHandlers.TryAdd(typeof(DateOnly), new DateOnlyHandler());
        _dataTypeHandlers.TryAdd(typeof(TimeOnly), new TimeOnlyHandler());
    }

    public static void AddTypeHandler<THandler>()
        where THandler : IDataTypeHandler, new()
    {
        var handler = new THandler();
        AddTypeHandler(handler);
    }

    public static void AddTypeHandler<THandler>(THandler handler)
        where THandler : IDataTypeHandler
    {
        _dataTypeHandlers.TryAdd(handler.ValueType, handler);
    }

    public static IDataTypeHandler<TValue> GetTypeHandler<TValue>(Type valueType)
    {
        if (_dataTypeHandlers.TryGetValue(valueType, out var handler))
            return handler as IDataTypeHandler<TValue>;

        return null;
    }
}
