using System.Reflection;

namespace FluentCommand.Reflection;

/// <summary>
/// Provides an accessor for <see cref="PropertyInfo"/>, enabling efficient dynamic get and set operations
/// on properties using compiled delegates. Inherits from <see cref="MemberAccessor"/> to provide
/// reflection-based access and metadata retrieval for properties.
/// </summary>
public class PropertyAccessor : MemberAccessor
{
    private readonly Lazy<Func<object, object>> _getter;
    private readonly Lazy<Action<object, object>> _setter;

    /// <summary>
    /// Initializes a new instance of the <see cref="PropertyAccessor"/> class using the specified <see cref="PropertyInfo"/>.
    /// </summary>
    /// <param name="propertyInfo">The <see cref="PropertyInfo"/> instance to use for this accessor.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="propertyInfo"/> is <c>null</c>.</exception>
    public PropertyAccessor(PropertyInfo propertyInfo) : base(propertyInfo)
    {
        if (propertyInfo == null)
            throw new ArgumentNullException(nameof(propertyInfo));

        Name = propertyInfo.Name;
        MemberType = propertyInfo.PropertyType;

        HasGetter = propertyInfo.CanRead;
        _getter = new Lazy<Func<object, object>>(() => ExpressionFactory.CreateGet(propertyInfo));

        HasSetter = propertyInfo.CanWrite;
        _setter = new Lazy<Action<object, object>>(() => ExpressionFactory.CreateSet(propertyInfo));
    }

    /// <summary>
    /// Gets the <see cref="Type"/> of the property.
    /// </summary>
    /// <value>The <see cref="Type"/> representing the property's data type.</value>
    public override Type MemberType { get; }

    /// <summary>
    /// Gets the name of the property.
    /// </summary>
    /// <value>The name of the property.</value>
    public override string Name { get; }

    /// <summary>
    /// Gets a value indicating whether this property has a getter.
    /// </summary>
    /// <value><c>true</c> if this property has a getter; otherwise, <c>false</c>.</value>
    public override bool HasGetter { get; }

    /// <summary>
    /// Gets a value indicating whether this property has a setter.
    /// </summary>
    /// <value><c>true</c> if this property has a setter; otherwise, <c>false</c>.</value>
    public override bool HasSetter { get; }

    /// <summary>
    /// Gets the value of the property for the specified object instance.
    /// </summary>
    /// <param name="instance">The object whose property value will be returned.</param>
    /// <returns>The value of the property for the given <paramref name="instance"/>.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the property does not have a getter.</exception>
    public override object GetValue(object instance)
    {
        if (_getter == null || !HasGetter)
            throw new InvalidOperationException($"Property '{Name}' does not have a getter.");

        var get = _getter.Value;
        if (get == null)
            throw new InvalidOperationException($"Property '{Name}' does not have a getter.");

        return get(instance);
    }

    /// <summary>
    /// Sets the value of the property for the specified object instance.
    /// </summary>
    /// <param name="instance">The object whose property value will be set.</param>
    /// <param name="value">The new value for this property.</param>
    /// <exception cref="InvalidOperationException">Thrown if the property does not have a setter.</exception>
    public override void SetValue(object instance, object value)
    {
        if (_setter == null || !HasSetter)
            throw new InvalidOperationException($"Property '{Name}' does not have a setter.");

        var set = _setter.Value;
        if (set == null)
            throw new InvalidOperationException($"Property '{Name}' does not have a setter.");

        set(instance, value);
    }
}
