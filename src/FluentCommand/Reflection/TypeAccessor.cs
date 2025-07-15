using System.Collections.Concurrent;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq.Expressions;
using System.Reflection;

namespace FluentCommand.Reflection;

/// <summary>
/// Provides efficient access to type reflection information, including dynamic creation, property, field, and method accessors,
/// and metadata such as table mapping. Caches accessors for performance and supports late-bound operations on types.
/// </summary>
public class TypeAccessor
{
    private static readonly ConcurrentDictionary<Type, TypeAccessor> _typeCache = new();
    private readonly ConcurrentDictionary<string, IMemberAccessor> _memberCache = new();
    private readonly ConcurrentDictionary<int, IMethodAccessor> _methodCache = new();
    private readonly ConcurrentDictionary<int, IEnumerable<IMemberAccessor>> _propertyCache = new();

    private readonly Lazy<Func<object>> _constructor;
    private readonly Lazy<TableAttribute> _tableAttribute;

    /// <summary>
    /// Initializes a new instance of the <see cref="TypeAccessor"/> class for the specified <see cref="Type"/>.
    /// </summary>
    /// <param name="type">The <see cref="Type"/> this accessor is for.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="type"/> is <c>null</c>.</exception>
    public TypeAccessor(Type type)
    {
        if (type == null)
            throw new ArgumentNullException(nameof(type));

        Type = type;
        _constructor = new Lazy<Func<object>>(() => ExpressionFactory.CreateConstructor(Type));
        _tableAttribute = new Lazy<TableAttribute>(() => type.GetCustomAttribute<TableAttribute>(true));
    }

    /// <summary>
    /// Gets the <see cref="Type"/> this accessor is for.
    /// </summary>
    /// <value>The <see cref="Type"/> this accessor is for.</value>
    public Type Type { get; }

    /// <summary>
    /// Gets the name of the type.
    /// </summary>
    /// <value>The name of the type.</value>
    public string Name => Type.Name;

    /// <summary>
    /// Gets the name of the table the class is mapped to, as specified by the <see cref="TableAttribute"/>.
    /// If not specified, returns the type name.
    /// </summary>
    /// <value>The name of the mapped table.</value>
    public string TableName => _tableAttribute.Value?.Name ?? Type.Name;

    /// <summary>
    /// Gets the schema of the table the class is mapped to, as specified by the <see cref="TableAttribute"/>.
    /// </summary>
    /// <value>The schema of the mapped table, or <c>null</c> if not specified.</value>
    public string TableSchema => _tableAttribute.Value?.Schema;

    /// <summary>
    /// Creates a new instance of the type represented by this accessor using the default constructor.
    /// </summary>
    /// <returns>A new instance of the type.</returns>
    /// <exception cref="InvalidOperationException">Thrown if a parameterless constructor is not found.</exception>
    public object Create()
    {
        var constructor = _constructor.Value;
        if (constructor == null)
            throw new InvalidOperationException($"Could not find constructor for '{Type.Name}'.");

        return constructor.Invoke();
    }

    #region Method

    /// <summary>
    /// Finds a method with the specified <paramref name="name"/> and no parameters.
    /// </summary>
    /// <param name="name">The name of the method.</param>
    /// <returns>An <see cref="IMethodAccessor"/> for the method, or <c>null</c> if not found.</returns>
    public IMethodAccessor FindMethod(string name)
    {
        return FindMethod(name, Type.EmptyTypes);
    }

    /// <summary>
    /// Finds a method with the specified <paramref name="name"/> and parameter types.
    /// </summary>
    /// <param name="name">The name of the method.</param>
    /// <param name="parameterTypes">The method parameter types.</param>
    /// <returns>An <see cref="IMethodAccessor"/> for the method, or <c>null</c> if not found.</returns>
    public IMethodAccessor FindMethod(string name, params Type[] parameterTypes)
    {
        return FindMethod(name, parameterTypes, BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance);
    }

    /// <summary>
    /// Finds a method with the specified <paramref name="name"/>, parameter types, and binding flags.
    /// </summary>
    /// <param name="name">The name of the method.</param>
    /// <param name="parameterTypes">The method parameter types.</param>
    /// <param name="flags">The binding flags to use for the search.</param>
    /// <returns>An <see cref="IMethodAccessor"/> for the method, or <c>null</c> if not found.</returns>
    public IMethodAccessor FindMethod(string name, Type[] parameterTypes, BindingFlags flags)
    {
        int key = MethodAccessor.GetKey(name, parameterTypes);
        return _methodCache.GetOrAdd(key, n => CreateMethodAccessor(name, parameterTypes, flags));
    }

    private IMethodAccessor CreateMethodAccessor(string name, Type[] parameters, BindingFlags flags)
    {
        var info = FindMethod(Type, name, parameters, flags);
        return info == null ? null : CreateAccessor(info);
    }

    private static MethodInfo FindMethod(Type type, string name, Type[] parameterTypes, BindingFlags flags)
    {
        if (type == null)
            throw new ArgumentNullException(nameof(type));
        if (name == null)
            throw new ArgumentNullException(nameof(name));

        if (parameterTypes == null)
            parameterTypes = Type.EmptyTypes;

        var typeInfo = type.GetTypeInfo();

        //first try full match
        var methodInfo = typeInfo.GetMethod(name, parameterTypes);
        if (methodInfo != null)
            return methodInfo;

        // next, get all that match by name
        var methodsByName = typeInfo.GetMethods(flags)
          .Where(m => m.Name == name)
          .ToList();

        if (methodsByName.Count == 0)
            return null;

        // if only one matches name, return it
        if (methodsByName.Count == 1)
            return methodsByName.FirstOrDefault();

        // next, get all methods that match param count
        var methodsByParamCount = methodsByName
            .Where(m => m.GetParameters().Length == parameterTypes.Length)
            .ToList();

        // if only one matches with same param count, return it
        if (methodsByParamCount.Count == 1)
            return methodsByParamCount.FirstOrDefault();

        // still no match, make best guess by greatest matching param types
        MethodInfo current = methodsByParamCount.FirstOrDefault();
        int matchCount = 0;

        foreach (var info in methodsByParamCount)
        {
            var paramTypes = info.GetParameters()
                .Select(p => p.ParameterType)
                .ToArray();

            // unsure which way IsAssignableFrom should be checked?
            int count = paramTypes
                .Select(t => t.GetTypeInfo())
                .Where((t, i) => t.IsAssignableFrom(parameterTypes[i]))
                .Count();

            if (count <= matchCount)
                continue;

            current = info;
            matchCount = count;
        }

        return current;
    }

    private static IMethodAccessor CreateAccessor(MethodInfo methodInfo)
    {
        return methodInfo == null ? null : new MethodAccessor(methodInfo);
    }
    #endregion

    #region Find

    /// <summary>
    /// Searches for the public property or field with the specified name.
    /// </summary>
    /// <param name="name">The name of the property or field to find.</param>
    /// <returns>An <see cref="IMemberAccessor"/> for the property or field if found; otherwise, <c>null</c>.</returns>
    public IMemberAccessor Find(string name)
    {
        return Find(name, BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy);
    }

    /// <summary>
    /// Searches for the specified property or field using the specified binding flags.
    /// </summary>
    /// <param name="name">The name of the property or field to find.</param>
    /// <param name="flags">A bitmask comprised of one or more <see cref="BindingFlags"/> that specify how the search is conducted.</param>
    /// <returns>An <see cref="IMemberAccessor"/> for the property or field if found; otherwise, <c>null</c>.</returns>
    public IMemberAccessor Find(string name, BindingFlags flags)
    {
        return _memberCache.GetOrAdd(name, n => CreateAccessor(n, flags));
    }

    private IMemberAccessor CreateAccessor(string name, BindingFlags flags)
    {
        // first try property
        var property = FindProperty(Type, name, flags);
        if (property != null)
            return CreateAccessor(property);

        // next try field
        var field = FindField(Type, name, flags);
        if (field != null)
            return CreateAccessor(field);

        return null;
    }
    #endregion

    #region Column
    /// <summary>
    /// Searches for the public property with the specified column name.
    /// </summary>
    /// <param name="name">The name of the property or field to find.</param>
    /// <returns>An <see cref="IMemberAccessor"/> instance for the property or field if found; otherwise <c>null</c>.</returns>
    public IMemberAccessor FindColumn(string name)
    {
        return FindColumn(name, BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy);
    }

    /// <summary>
    /// Searches for the property with the specified column name and binding flags, using <see cref="ColumnAttribute"/> if present.
    /// </summary>
    /// <param name="name">The name of the property or column to find.</param>
    /// <param name="flags">A bitmask comprised of one or more <see cref="BindingFlags"/> that specify how the search is conducted.</param>
    /// <returns>An <see cref="IMemberAccessor"/> for the property if found; otherwise, <c>null</c>.</returns>
    public IMemberAccessor FindColumn(string name, BindingFlags flags)
    {
        return _memberCache.GetOrAdd(name, n => CreateColumnAccessor(n, flags));
    }

    private IMemberAccessor CreateColumnAccessor(string name, BindingFlags flags)
    {
        var typeInfo = Type.GetTypeInfo();

        foreach (var p in typeInfo.GetProperties(flags))
        {
            // try ColumnAttribute
            var columnAttribute = p.GetCustomAttribute<System.ComponentModel.DataAnnotations.Schema.ColumnAttribute>();
            if (columnAttribute != null && name.Equals(columnAttribute.Name, StringComparison.OrdinalIgnoreCase))
                return CreateAccessor(p);

            if (p.Name.Equals(name, StringComparison.OrdinalIgnoreCase))
                return CreateAccessor(p);
        }

        return null;
    }
    #endregion

    #region Property
    /// <summary>
    /// Searches for the property using a property expression.
    /// </summary>
    /// <typeparam name="T">The object type containing the property specified in the expression.</typeparam>
    /// <param name="propertyExpression">The property expression (e.g. <c>p =&gt; p.PropertyName</c>).</param>
    /// <returns>An <see cref="IMemberAccessor"/> for the property if found; otherwise, <c>null</c>.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="propertyExpression"/> is <c>null</c>.</exception>
    /// <exception cref="ArgumentException">Thrown if the expression is not a valid property access expression.</exception>
    public IMemberAccessor FindProperty<T>(Expression<Func<T>> propertyExpression)
    {
        if (propertyExpression == null)
            throw new ArgumentNullException(nameof(propertyExpression));

        if (propertyExpression.Body is UnaryExpression unaryExpression)
            return FindProperty(unaryExpression.Operand as MemberExpression);
        else
            return FindProperty(propertyExpression.Body as MemberExpression);
    }

    /// <summary>
    /// Searches for the property using a property expression.
    /// </summary>
    /// <typeparam name="TSource">The object type containing the property specified in the expression.</typeparam>
    /// <typeparam name="TValue">The type of the value.</typeparam>
    /// <param name="propertyExpression">The property expression (e.g. <c>p =&gt; p.PropertyName</c>).</param>
    /// <returns>An <see cref="IMemberAccessor"/> for the property if found; otherwise, <c>null</c>.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="propertyExpression"/> is <c>null</c>.</exception>
    /// <exception cref="ArgumentException">Thrown if the expression is not a valid property access expression.</exception>
    public IMemberAccessor FindProperty<TSource, TValue>(Expression<Func<TSource, TValue>> propertyExpression)
    {
        if (propertyExpression == null)
            throw new ArgumentNullException(nameof(propertyExpression));

        if (propertyExpression.Body is UnaryExpression unaryExpression)
            return FindProperty(unaryExpression.Operand as MemberExpression);
        else
            return FindProperty(propertyExpression.Body as MemberExpression);
    }

    /// <summary>
    /// Searches for the public property with the specified name.
    /// </summary>
    /// <param name="name">The name of the property to find.</param>
    /// <returns>An <see cref="IMemberAccessor"/> for the property if found; otherwise, <c>null</c>.</returns>
    public IMemberAccessor FindProperty(string name)
    {
        return FindProperty(name, BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy);
    }

    /// <summary>
    /// Searches for the property with the specified name and binding flags.
    /// </summary>
    /// <param name="name">The name of the property to find.</param>
    /// <param name="flags">A bitmask comprised of one or more <see cref="BindingFlags"/> that specify how the search is conducted.</param>
    /// <returns>An <see cref="IMemberAccessor"/> for the property if found; otherwise, <c>null</c>.</returns>
    public IMemberAccessor FindProperty(string name, BindingFlags flags)
    {
        return _memberCache.GetOrAdd(name, n => CreatePropertyAccessor(n, flags));
    }

    /// <summary>
    /// Gets the property member accessors for the type.
    /// </summary>
    /// <returns>An <see cref="IEnumerable{IMemberAccessor}"/> for the type's properties.</returns>
    public IEnumerable<IMemberAccessor> GetProperties()
    {
        return GetProperties(BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance);
    }

    /// <summary>
    /// Gets the property member accessors for the type using the specified binding flags.
    /// </summary>
    /// <param name="flags">A bitmask comprised of one or more <see cref="BindingFlags"/> that specify how the search is conducted.</param>
    /// <returns>An <see cref="IEnumerable{IMemberAccessor}"/> for the type's properties.</returns>
    public IEnumerable<IMemberAccessor> GetProperties(BindingFlags flags)
    {
        return _propertyCache.GetOrAdd((int)flags, k =>
        {
            var typeInfo = Type.GetTypeInfo();
            var properties = typeInfo.GetProperties(flags);
            return properties.Select(GetAccessor);
        });
    }


    private IMemberAccessor GetAccessor(PropertyInfo propertyInfo)
    {
        if (propertyInfo == null)
            throw new ArgumentNullException(nameof(propertyInfo));

        return _memberCache.GetOrAdd(propertyInfo.Name, n => CreateAccessor(propertyInfo));
    }

    private IMemberAccessor CreatePropertyAccessor(string name, BindingFlags flags)
    {
        var info = FindProperty(Type, name, flags);
        return info == null ? null : CreateAccessor(info);
    }

    private IMemberAccessor FindProperty(MemberExpression memberExpression)
    {
        if (memberExpression == null)
            throw new ArgumentException("The expression is not a member access expression.", nameof(memberExpression));

        var property = memberExpression.Member as PropertyInfo;
        if (property == null)
            throw new ArgumentException("The member access expression does not access a property.", nameof(memberExpression));

        // find by name because we can't trust the PropertyInfo here as it could be from an interface or inherited class
        return FindProperty(property.Name);
    }

    private static PropertyInfo FindProperty(Type type, string name, BindingFlags flags)
    {
        if (type == null)
            throw new ArgumentNullException(nameof(type));

        if (name == null)
            throw new ArgumentNullException(nameof(name));

        var typeInfo = type.GetTypeInfo();
        // first try GetProperty
        var property = typeInfo.GetProperty(name, flags);
        if (property != null)
            return property;

        // if not found, search while ignoring case
        property = typeInfo
            .GetProperties(flags)
            .FirstOrDefault(p => p.Name.Equals(name, StringComparison.OrdinalIgnoreCase));

        return property;
    }

    private static IMemberAccessor CreateAccessor(PropertyInfo propertyInfo)
    {
        return propertyInfo == null ? null : new PropertyAccessor(propertyInfo);
    }
    #endregion

    #region Field
    /// <summary>
    /// Searches for the specified field with the specified name.
    /// </summary>
    /// <param name="name">The name of the field to find.</param>
    /// <returns>
    /// An <see cref="IMemberAccessor"/> instance for the field if found; otherwise <c>null</c>.
    /// </returns>
    public IMemberAccessor FindField(string name)
    {
        return FindField(name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
    }

    /// <summary>
    /// Searches for the field with the specified name and binding flags.
    /// </summary>
    /// <param name="name">The name of the field to find.</param>
    /// <param name="flags">A bitmask comprised of one or more <see cref="BindingFlags"/> that specify how the search is conducted.</param>
    /// <returns>An <see cref="IMemberAccessor"/> for the field if found; otherwise, <c>null</c>.</returns>
    public IMemberAccessor FindField(string name, BindingFlags flags)
    {
        return _memberCache.GetOrAdd(name, n => CreateFieldAccessor(n, flags));
    }

    private IMemberAccessor CreateFieldAccessor(string name, BindingFlags flags)
    {
        var info = FindField(Type, name, flags);
        return info == null ? null : CreateAccessor(info);
    }

    private static FieldInfo FindField(Type type, string name, BindingFlags flags)
    {
        if (type == null)
            throw new ArgumentNullException(nameof(type));

        if (name == null)
            throw new ArgumentNullException(nameof(name));

        // first try GetField
        var typeInfo = type.GetTypeInfo();
        var field = typeInfo.GetField(name, flags);
        if (field != null)
            return field;

        // if not found, search while ignoring case
        return typeInfo
            .GetFields(flags)
            .FirstOrDefault(f => f.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
    }

    private static IMemberAccessor CreateAccessor(FieldInfo fieldInfo)
    {
        return fieldInfo == null ? null : new FieldAccessor(fieldInfo);
    }
    #endregion


    /// <summary>
    /// Gets the <see cref="TypeAccessor"/> for the specified Type.
    /// </summary>
    /// <typeparam name="T">The Type to get the accessor for.</typeparam>
    /// <returns></returns>
    public static TypeAccessor GetAccessor<T>()
    {
        return GetAccessor(typeof(T));
    }

    /// <summary>
    /// Gets the <see cref="TypeAccessor"/> for the specified <see cref="Type"/>.
    /// </summary>
    /// <param name="type">The type to get the accessor for.</param>
    /// <returns>The <see cref="TypeAccessor"/> for the specified type.</returns>
    public static TypeAccessor GetAccessor(Type type)
    {
        return _typeCache.GetOrAdd(type, t => new TypeAccessor(t));
    }
}
