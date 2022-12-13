using System.Data;

using FluentCommand.Extensions;

namespace FluentCommand.Handlers;

/// <summary>
/// A data type handler for <see cref="ConcurrencyToken"/>
/// </summary>
/// <seealso cref="ConcurrencyToken" />
public class ConcurrencyTokenHandler : IDataParameterHandler, IDataFieldConverter<ConcurrencyToken>
{
    /// <inheritdoc />
    public Type ValueType { get; } = typeof(ConcurrencyToken);

    /// <inheritdoc />
    public ConcurrencyToken ReadValue(IDataRecord dataRecord, int fieldIndex)
    {
        if (dataRecord.IsDBNull(fieldIndex))
            return ConcurrencyToken.None;

        var bytes = dataRecord.GetBytes(fieldIndex);

        return new ConcurrencyToken(bytes);
    }

    /// <inheritdoc />
    public object ReadValue(IDbDataParameter parameter)
    {
        return parameter.Value switch
        {
            string textToken => new ConcurrencyToken(textToken),
            byte[] byteToken => new ConcurrencyToken(byteToken),
            _ => ConcurrencyToken.None
        };
    }

    /// <inheritdoc />
    public void SetValue(IDbDataParameter parameter, object value)
    {
        parameter.DbType = DbType.Binary;
        parameter.Value = value switch
        {
            ConcurrencyToken concurrencyToken => concurrencyToken.Value,
            null => DBNull.Value,
            _ => value
        };
    }
}
