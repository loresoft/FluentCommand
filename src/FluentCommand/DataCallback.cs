using System.Data.Common;

using FluentCommand.Extensions;

namespace FluentCommand;

internal class DataCallback
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DataCallback"/> class.
    /// </summary>
    /// <param name="type">The type.</param>
    /// <param name="parameter">The parameter.</param>
    /// <param name="callback">The callback.</param>
    public DataCallback(Type type, DbParameter parameter, Delegate callback)
    {
        Type = type ?? throw new ArgumentNullException(nameof(type));
        Parameter = parameter ?? throw new ArgumentNullException(nameof(parameter));
        Callback = callback ?? throw new ArgumentNullException(nameof(callback));
    }

    /// <summary>
    /// Gets or sets the type of the call back value.
    /// </summary>
    public Type Type { get; }
    /// <summary>
    /// Gets or sets the callback <see langword="delegate"/>.
    /// </summary>
    public Delegate Callback { get; }
    /// <summary>
    /// Gets or sets the parameter associated with the callback.
    /// </summary>
    public DbParameter Parameter { get; }

    /// <summary>
    /// Invokes the <see cref="Callback"/> with the <see cref="Parameter"/> value.
    /// </summary>
    public void Invoke()
    {
        var handler = DataParameterHandlers.GetTypeHandler(Type);

        var value = handler?.ReadValue(Parameter) ?? Parameter.Value;
        if (value == DBNull.Value)
            value = Type.Default();

        Callback.DynamicInvoke(value);
    }
}
