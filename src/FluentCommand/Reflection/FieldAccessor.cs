using System.Reflection;

namespace FluentCommand.Reflection;

/// <summary>
/// Provides an accessor for <see cref="FieldInfo"/> members, enabling efficient get and set operations
/// on fields using compiled delegates. Inherits from <see cref="MemberAccessor"/> to provide
/// reflection-based access and metadata retrieval for fields.
/// </summary>
public class FieldAccessor : MemberAccessor
{
    private readonly Lazy<Func<object, object>> _getter;
    private readonly Lazy<Action<object, object>> _setter;

    /// <summary>
    /// Initializes a new instance of the <see cref="FieldAccessor"/> class using the specified <see cref="FieldInfo"/>.
    /// </summary>
    /// <param name="fieldInfo">The <see cref="FieldInfo"/> instance to use for this accessor.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="fieldInfo"/> is <c>null</c>.</exception>
    public FieldAccessor(FieldInfo fieldInfo) : base(fieldInfo)
    {
        if (fieldInfo == null)
            throw new ArgumentNullException(nameof(fieldInfo));

        Name = fieldInfo.Name;
        MemberType = fieldInfo.FieldType;

        _getter = new Lazy<Func<object, object>>(() => ExpressionFactory.CreateGet(fieldInfo));
        HasGetter = true;

        bool isReadonly = !fieldInfo.IsInitOnly && !fieldInfo.IsLiteral;
        if (!isReadonly)
            _setter = new Lazy<Action<object, object>>(() => ExpressionFactory.CreateSet(fieldInfo));

        HasSetter = !isReadonly;
    }

    /// <summary>
    /// Gets the <see cref="Type"/> of the field.
    /// </summary>
    /// <value>The <see cref="Type"/> representing the field's data type.</value>
    public override Type MemberType { get; }

    /// <summary>
    /// Gets the name of the field.
    /// </summary>
    /// <value>The name of the field.</value>
    public override string Name { get; }

    /// <summary>
    /// Gets a value indicating whether this field has a getter.
    /// </summary>
    /// <value><c>true</c> if this field has a getter; otherwise, <c>false</c>.</value>
    public override bool HasGetter { get; }

    /// <summary>
    /// Gets a value indicating whether this field has a setter.
    /// </summary>
    /// <value><c>true</c> if this field has a setter; otherwise, <c>false</c>.</value>
    public override bool HasSetter { get; }

    /// <summary>
    /// Returns the value of the field for the specified object instance.
    /// </summary>
    /// <param name="instance">The object whose field value will be returned.</param>
    /// <returns>The value of the field for the given <paramref name="instance"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="instance"/> is <c>null</c>.</exception>
    /// <exception cref="InvalidOperationException">Thrown if the field does not have a getter.</exception>
    public override object GetValue(object instance)
    {
        if (instance == null)
            throw new ArgumentNullException(nameof(instance));

        if (_getter == null || !HasGetter)
            throw new InvalidOperationException($"Field '{Name}' does not have a getter.");

        var get = _getter.Value;
        if (get == null)
            throw new InvalidOperationException($"Field '{Name}' does not have a getter.");

        return get(instance);
    }

    /// <summary>
    /// Sets the value of the field for the specified object instance.
    /// </summary>
    /// <param name="instance">The object whose field value will be set.</param>
    /// <param name="value">The new value for this field.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="instance"/> is <c>null</c>.</exception>
    /// <exception cref="InvalidOperationException">Thrown if the field does not have a setter.</exception>
    public override void SetValue(object instance, object value)
    {
        if (instance == null)
            throw new ArgumentNullException(nameof(instance));

        if (_setter == null || !HasSetter)
            throw new InvalidOperationException($"Field '{Name}' does not have a setter.");

        var set = _setter.Value;
        if (set == null)
            throw new InvalidOperationException($"Field '{Name}' does not have a setter.");

        set(instance, value);
    }
}
