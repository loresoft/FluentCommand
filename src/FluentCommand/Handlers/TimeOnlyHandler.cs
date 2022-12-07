#if !NETSTANDARD2_0
using System.Data;

namespace FluentCommand.Handlers;

/// <summary>
/// A data type handler for <see cref="TimeOnly"/>
/// </summary>
/// <seealso cref="TimeOnly" />
public class TimeOnlyHandler : IDataTypeHandler<TimeOnly>
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
    public TimeOnly ReadValue(IDbDataParameter parameter)
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
    public void SetValue(IDbDataParameter parameter, TimeOnly value)
    {
        parameter.DbType = DbType.Time;
        parameter.Value = value.ToTimeSpan();
    }
}
#endif
