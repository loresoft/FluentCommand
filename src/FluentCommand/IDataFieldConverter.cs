using System.Data;

namespace FluentCommand;

/// <summary>
/// Interface defining how to read a field value
/// </summary>
/// <typeparam name="TValue">The type of the value.</typeparam>
public interface IDataFieldConverter<out TValue>
{
    /// <summary>
    /// Read the value from the specified <paramref name="dataRecord"/>.
    /// </summary>
    /// <param name="dataRecord">The data record to read the value from.</param>
    /// <param name="fieldIndex">Index of the field.</param>
    /// <returns>The value read from the <paramref name="dataRecord"/></returns>
    public TValue ReadValue(IDataRecord dataRecord, int fieldIndex);

}
