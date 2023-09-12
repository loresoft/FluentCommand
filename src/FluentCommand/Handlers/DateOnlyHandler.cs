#if NET6_0_OR_GREATER
using System.Data;

using FluentCommand;

namespace FluentCommand.Handlers;

/// <summary>
/// A data type handler for <see cref="DateOnly"/>
/// </summary>
/// <seealso cref="DateOnly" />
public class DateOnlyHandler : IDataParameterHandler, IDataFieldConverter<DateOnly>
{
    /// <inheritdoc />
    public Type ValueType { get; } = typeof(DateOnly);

    /// <inheritdoc />
    public DateOnly ReadValue(IDataRecord dataRecord, int fieldIndex)
    {
        var value = dataRecord.GetValue(fieldIndex);

        return value switch
        {
            DateOnly dateOnly => dateOnly,
            DateTime dateTime => DateOnly.FromDateTime(dateTime),
            _ => default
        };
    }

    /// <inheritdoc />
    public object ReadValue(IDbDataParameter parameter)
    {
        return parameter.Value switch
        {
            DateOnly dateOnly => dateOnly,
            DateTime dateTime => DateOnly.FromDateTime(dateTime),
            _ => default
        };
    }

    /// <inheritdoc />
    public void SetValue(IDbDataParameter parameter, object value)
    {
        parameter.DbType = DbType.Date;
        parameter.Value = value switch
        {
            DateOnly dateOnly => dateOnly.ToDateTime(new TimeOnly(0, 0)),
            null => DBNull.Value,
            _ => value
        };
    }
}
#endif
