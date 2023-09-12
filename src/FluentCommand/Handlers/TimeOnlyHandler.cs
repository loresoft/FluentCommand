#if NET6_0_OR_GREATER
using System.Data;

using FluentCommand;

namespace FluentCommand.Handlers;

/// <summary>
/// A data type handler for <see cref="TimeOnly"/>
/// </summary>
/// <seealso cref="TimeOnly" />
public class TimeOnlyHandler : IDataParameterHandler, IDataFieldConverter<TimeOnly>
{
    /// <inheritdoc />
    public Type ValueType { get; } = typeof(TimeOnly);

    /// <inheritdoc />
    public TimeOnly ReadValue(IDataRecord dataRecord, int fieldIndex)
    {
        var value = dataRecord.GetValue(fieldIndex);

        return value switch
        {
            TimeOnly timeOnly => timeOnly,
            TimeSpan timeSpan => TimeOnly.FromTimeSpan(timeSpan),
            DateTime dateTime => TimeOnly.FromDateTime(dateTime),
            _ => default
        };
    }

    /// <inheritdoc />
    public object ReadValue(IDbDataParameter parameter)
    {
        return parameter.Value switch
        {
            TimeOnly timeOnly => timeOnly,
            TimeSpan timeSpan => TimeOnly.FromTimeSpan(timeSpan),
            DateTime dateTime => TimeOnly.FromDateTime(dateTime),
            _ => default
        };
    }

    /// <inheritdoc />
    public void SetValue(IDbDataParameter parameter, object value)
    {
        parameter.DbType = DbType.Time;
        parameter.Value = value switch
        {
            TimeOnly timeOnly => timeOnly.ToTimeSpan(),
            null => DBNull.Value,
            _ => value
        };
    }
}
#endif
