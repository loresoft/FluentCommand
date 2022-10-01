using System.Data.Common;

using FluentCommand.Extensions;

namespace FluentCommand;

internal class DataCallback
{
    /// <summary>
    /// Gets or sets the type of the call back value.
    /// </summary>
    public Type Type { get; set; }
    /// <summary>
    /// Gets or sets the callback <see langword="delegate"/>.
    /// </summary>
    public Delegate Callback { get; set; }
    /// <summary>
    /// Gets or sets the parameter associated with the callback.
    /// </summary>
    public DbParameter Parameter { get; set; }

    /// <summary>
    /// Invokes the <see cref="Callback"/> with the <see cref="Parameter"/> value.
    /// </summary>
    public void Invoke()
    {
        var value = Parameter.Value;
        if (value == DBNull.Value)
            value = Type.Default();

        Callback.DynamicInvoke(value);
    }
}
