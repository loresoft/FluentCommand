using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics;
using System.Reflection;

namespace FluentCommand.Reflection;

/// <summary>
/// Provides a base implementation for member accessors, enabling reflection-based access and metadata retrieval
/// for entity members (properties or fields), including support for data annotation attributes and database mapping.
/// </summary>
[DebuggerDisplay("Name: {Name}")]
public abstract class MemberAccessor : IMemberAccessor, IEquatable<IMemberAccessor>
{
    private readonly Lazy<ColumnAttribute> _columnAttribute;
    private readonly Lazy<KeyAttribute> _keyAttribute;
    private readonly Lazy<NotMappedAttribute> _notMappedAttribute;
    private readonly Lazy<DatabaseGeneratedAttribute> _databaseGeneratedAttribute;
    private readonly Lazy<ConcurrencyCheckAttribute> _concurrencyCheckAttribute;
    private readonly Lazy<ForeignKeyAttribute> _foreignKeyAttribute;
    private readonly Lazy<RequiredAttribute> _requiredAttribute;
    private readonly Lazy<DisplayAttribute> _displayAttribute;
    private readonly Lazy<DisplayFormatAttribute> _displayFormatAttribute;


    /// <summary>
    /// Initializes a new instance of the <see cref="MemberAccessor"/> class using the specified <see cref="MemberInfo"/>.
    /// </summary>
    /// <param name="memberInfo">The reflection metadata for the member to be accessed.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="memberInfo"/> is <c>null</c>.</exception>
    protected MemberAccessor(MemberInfo memberInfo)
    {
        MemberInfo = memberInfo ?? throw new ArgumentNullException(nameof(memberInfo));

        _columnAttribute = new Lazy<ColumnAttribute>(() => MemberInfo.GetCustomAttribute<ColumnAttribute>(true));
        _keyAttribute = new Lazy<KeyAttribute>(() => MemberInfo.GetCustomAttribute<KeyAttribute>(true));
        _notMappedAttribute = new Lazy<NotMappedAttribute>(() => MemberInfo.GetCustomAttribute<NotMappedAttribute>(true));
        _databaseGeneratedAttribute = new Lazy<DatabaseGeneratedAttribute>(() => MemberInfo.GetCustomAttribute<DatabaseGeneratedAttribute>(true));
        _concurrencyCheckAttribute = new Lazy<ConcurrencyCheckAttribute>(() => MemberInfo.GetCustomAttribute<ConcurrencyCheckAttribute>(true));
        _foreignKeyAttribute = new Lazy<ForeignKeyAttribute>(() => MemberInfo.GetCustomAttribute<ForeignKeyAttribute>(true));
        _requiredAttribute = new Lazy<RequiredAttribute>(() => MemberInfo.GetCustomAttribute<RequiredAttribute>(true));
        _displayAttribute = new Lazy<DisplayAttribute>(() => MemberInfo.GetCustomAttribute<DisplayAttribute>(true));
        _displayFormatAttribute = new Lazy<DisplayFormatAttribute>(() => MemberInfo.GetCustomAttribute<DisplayFormatAttribute>(true));
    }

    /// <summary>
    /// Gets the <see cref="Type"/> of the member (property or field).
    /// </summary>
    /// <value>The <see cref="Type"/> representing the member's data type.</value>
    public abstract Type MemberType { get; }

    /// <summary>
    /// Gets the <see cref="MemberInfo"/> instance for the accessor, providing reflection metadata.
    /// </summary>
    /// <value>The <see cref="MemberInfo"/> for the member.</value>
    public MemberInfo MemberInfo { get; }

    /// <summary>
    /// Gets the name of the member as defined in the entity.
    /// </summary>
    /// <value>The member's name.</value>
    public abstract string Name { get; }

    /// <summary>
    /// Gets a value indicating whether this member has a getter method.
    /// </summary>
    /// <value><c>true</c> if the member has a getter; otherwise, <c>false</c>.</value>
    public abstract bool HasGetter { get; }

    /// <summary>
    /// Gets a value indicating whether this member has a setter method.
    /// </summary>
    /// <value><c>true</c> if the member has a setter; otherwise, <c>false</c>.</value>
    public abstract bool HasSetter { get; }

    /// <summary>
    /// Gets the name of the database column that the member is mapped to.
    /// This value is determined by the <see cref="ColumnAttribute.Name"/> property if specified,
    /// otherwise it defaults to the member name.
    /// </summary>
    /// <value>The mapped database column name.</value>
    public string Column => _columnAttribute.Value?.Name ?? Name;

    /// <summary>
    /// Gets the database provider-specific data type of the column the member is mapped to.
    /// This value is determined by the <see cref="ColumnAttribute.TypeName"/> property if specified.
    /// </summary>
    /// <value>The provider-specific column data type.</value>
    public string ColumnType => _columnAttribute.Value?.TypeName;

    /// <summary>
    /// Gets the zero-based order of the column the member is mapped to in the database.
    /// This value is determined by the <see cref="ColumnAttribute.Order"/> property if specified.
    /// </summary>
    /// <value>The zero-based column order, or <c>null</c> if not specified.</value>
    public int? ColumnOrder => _columnAttribute.Value?.Order;

    /// <summary>
    /// Gets a value indicating whether this member is the primary key for the entity.
    /// This is determined by the presence of the <see cref="KeyAttribute"/>.
    /// </summary>
    /// <value><c>true</c> if this member is a primary key; otherwise, <c>false</c>.</value>
    public bool IsKey => _keyAttribute.Value != null;

    /// <summary>
    /// Gets a value indicating whether this member should be excluded from database mapping.
    /// This is determined by the presence of the <see cref="NotMappedAttribute"/>.
    /// </summary>
    /// <value><c>true</c> if this member is not mapped to a database column; otherwise, <c>false</c>.</value>
    public bool IsNotMapped => _notMappedAttribute.Value != null;

    /// <summary>
    /// Gets a value indicating whether this member participates in optimistic concurrency checks.
    /// This is determined by the presence of the <see cref="ConcurrencyCheckAttribute"/>.
    /// </summary>
    /// <value><c>true</c> if this member is used for concurrency checking; otherwise, <c>false</c>.</value>
    public bool IsConcurrencyCheck => _concurrencyCheckAttribute.Value != null;

    /// <summary>
    /// Gets a value indicating whether this member's value is generated by the database.
    /// This is determined by the presence of the <see cref="DatabaseGeneratedAttribute"/> and its option.
    /// </summary>
    /// <value><c>true</c> if the value is generated by the database; otherwise, <c>false</c>.</value>
    public bool IsDatabaseGenerated => _databaseGeneratedAttribute.Value != null
        && _databaseGeneratedAttribute.Value.DatabaseGeneratedOption != DatabaseGeneratedOption.None;

    /// <summary>
    /// Gets the name of the associated navigation property or foreign key(s) for this member.
    /// This value is determined by the <see cref="ForeignKeyAttribute.Name"/> property if specified.
    /// </summary>
    /// <value>The name of the navigation property or foreign key(s), or <c>null</c> if not applicable.</value>
    public string ForeignKey => _foreignKeyAttribute.Value?.Name;

    /// <summary>
    /// Gets a value indicating whether this member is required (non-nullable or marked as required).
    /// This is determined by the presence of the <see cref="RequiredAttribute"/>.
    /// </summary>
    /// <value><c>true</c> if the member is required; otherwise, <c>false</c>.</value>
    public bool IsRequired => _requiredAttribute.Value != null;

    /// <summary>
    /// Gets the display name of the member, typically used for UI or reporting purposes.
    /// This value is determined by the <see cref="DisplayAttribute.Name"/> property if specified; otherwise, the member name.
    /// </summary>
    /// <value>The display name of the member.</value>
    public string DisplayName => _displayAttribute.Value?.Name ?? Name;

    /// <summary>
    /// Gets the format string used to display the member's value, if specified.
    /// This value is determined by the <see cref="DisplayFormatAttribute.DataFormatString"/> property if present.
    /// </summary>
    /// <value>
    /// The data format string for display formatting, or <c>null</c> if not specified.
    /// </value>
    public string DataFormatString => _displayFormatAttribute.Value?.DataFormatString;

    /// <summary>
    /// Returns the value of the member for the specified object instance.
    /// </summary>
    /// <param name="instance">The object whose member value will be returned.</param>
    /// <returns>The value of the member for the given <paramref name="instance"/>.</returns>
    public abstract object GetValue(object instance);

    /// <summary>
    /// Sets the value of the member for the specified object instance.
    /// </summary>
    /// <param name="instance">The object whose member value will be set.</param>
    /// <param name="value">The new value to assign to the member.</param>
    public abstract void SetValue(object instance, object value);


    /// <summary>
    /// Determines whether the specified <see cref="IMemberAccessor"/> is equal to this instance.
    /// </summary>
    /// <param name="other">The <see cref="IMemberAccessor"/> to compare with this instance.</param>
    /// <returns><c>true</c> if the specified accessor is equal to this instance; otherwise, <c>false</c>.</returns>
    public bool Equals(IMemberAccessor other)
    {
        if (ReferenceEquals(null, other))
            return false;
        if (ReferenceEquals(this, other))
            return true;

        return Equals(other.MemberInfo, MemberInfo);
    }

    /// <summary>
    /// Determines whether the specified <see cref="object"/> is equal to this instance.
    /// </summary>
    /// <param name="obj">The <see cref="object"/> to compare with this instance.</param>
    /// <returns><c>true</c> if the specified object is equal to this instance; otherwise, <c>false</c>.</returns>
    public override bool Equals(object obj)
    {
        if (ReferenceEquals(null, obj))
            return false;
        if (ReferenceEquals(this, obj))
            return true;
        if (obj.GetType() != typeof(MemberAccessor))
            return false;

        return Equals((MemberAccessor)obj);
    }

    /// <summary>
    /// Returns a hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table.
    /// </summary>
    /// <returns>A hash code for this instance.</returns>
    public override int GetHashCode()
    {
        return MemberInfo.GetHashCode();
    }
}
