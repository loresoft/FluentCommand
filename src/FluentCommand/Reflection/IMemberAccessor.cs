namespace FluentCommand.Reflection;

/// <summary>
/// Defines an interface for late binding member accessors, enabling dynamic get and set operations
/// on object members (properties or fields) at runtime. Inherits metadata and mapping details from <see cref="IMemberInformation"/>.
/// </summary>
public interface IMemberAccessor : IMemberInformation
{
    /// <summary>
    /// Gets the value of the member for the specified object instance.
    /// </summary>
    /// <param name="instance">The object instance whose member value will be retrieved.</param>
    /// <returns>
    /// The value of the member for the given <paramref name="instance"/>.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="instance"/> is <c>null</c>.</exception>
    /// <exception cref="InvalidOperationException">Thrown if the member does not have a getter.</exception>
    object GetValue(object instance);

    /// <summary>
    /// Sets the value of the member for the specified object instance.
    /// </summary>
    /// <param name="instance">The object instance whose member value will be set.</param>
    /// <param name="value">The new value to assign to the member.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="instance"/> is <c>null</c>.</exception>
    /// <exception cref="InvalidOperationException">Thrown if the member does not have a setter.</exception>
    void SetValue(object instance, object value);
}
