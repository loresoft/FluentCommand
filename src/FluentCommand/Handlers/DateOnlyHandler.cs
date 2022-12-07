#if !NETSTANDARD2_0
using System.Data;

namespace FluentCommand.Handlers;

/// <summary>
/// A data type handler for <see cref="DateOnly"/>
/// </summary>
/// <seealso cref="DateOnly" />
public class DateOnlyHandler : IDataTypeHandler<DateOnly>
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
    public DateOnly ReadValue(IDbDataParameter parameter)
    {
        return parameter.Value switch
        {
            DateOnly dateOnly => dateOnly,
            DateTime dateTime => DateOnly.FromDateTime(dateTime),
            _ => default
        };
    }

    /// <inheritdoc />
    public void SetValue(IDbDataParameter parameter, DateOnly value)
    {
        parameter.DbType = DbType.Date;
        parameter.Value = value.ToDateTime(new TimeOnly(0, 0));
    }
}
#endif
