using System.Linq.Expressions;
using System.Reflection;

namespace FluentCommand.Reflection;

/// <summary>
/// Provides factory methods for creating delegates to dynamically invoke methods, constructors, properties, and fields using expression trees.
/// </summary>
internal static class ExpressionFactory
{
    /// <summary>
    /// Creates a delegate that invokes the specified method with the given instance and parameters.
    /// </summary>
    /// <param name="methodInfo">The method to invoke.</param>
    /// <returns>
    /// A <see cref="Func{T, TResult}"/> that takes an instance and an array of parameters, and returns the result of the method invocation.
    /// For void methods, returns <c>null</c>.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="methodInfo"/> is <c>null</c>.</exception>
    public static Func<object, object[], object> CreateMethod(MethodInfo methodInfo)
    {
        if (methodInfo == null)
            throw new ArgumentNullException(nameof(methodInfo));

        // parameters to execute
        var instanceParameter = Expression.Parameter(typeof(object), "instance");
        var parametersParameter = Expression.Parameter(typeof(object[]), "parameters");

        // build parameter list
        var parameterExpressions = new List<Expression>();
        var paramInfos = methodInfo.GetParameters();
        for (int i = 0; i < paramInfos.Length; i++)
        {
            // (Ti)parameters[i]
            var valueObj = Expression.ArrayIndex(parametersParameter, Expression.Constant(i));

            Type parameterType = paramInfos[i].ParameterType;
            if (parameterType.IsByRef)
                parameterType = parameterType.GetElementType();

            var valueCast = Expression.Convert(valueObj, parameterType);

            parameterExpressions.Add(valueCast);
        }

        // non-instance for static method, or ((TInstance)instance)
        var instanceCast = methodInfo.IsStatic ? null : Expression.Convert(instanceParameter, methodInfo.DeclaringType);

        // static invoke or ((TInstance)instance).Method
        var methodCall = Expression.Call(instanceCast, methodInfo, parameterExpressions);

        // ((TInstance)instance).Method((T0)parameters[0], (T1)parameters[1], ...)
        if (methodCall.Type == typeof(void))
        {
            var lambda = Expression.Lambda<Action<object, object[]>>(methodCall, instanceParameter, parametersParameter);
            var execute = lambda.Compile();

            return (instance, parameters) =>
            {
                execute(instance, parameters);
                return null;
            };
        }
        else
        {
            var castMethodCall = Expression.Convert(methodCall, typeof(object));
            var lambda = Expression.Lambda<Func<object, object[], object>>(castMethodCall, instanceParameter, parametersParameter);

            return lambda.Compile();
        }
    }

    /// <summary>
    /// Creates a delegate that constructs an instance of the specified type using its parameterless constructor.
    /// </summary>
    /// <param name="type">The type to instantiate.</param>
    /// <returns>A <see cref="Func{TResult}"/> that creates an instance of the specified type.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="type"/> is <c>null</c>.</exception>
    /// <exception cref="ArgumentException">Thrown if the type does not have a parameterless constructor.</exception>
    public static Func<object> CreateConstructor(Type type)
    {
        if (type == null)
            throw new ArgumentNullException(nameof(type));

        var typeInfo = type.GetTypeInfo();

        var constructorInfo = typeInfo.GetConstructor(Type.EmptyTypes);
        if (constructorInfo == null)
            throw new ArgumentException("Could not find constructor for type.", nameof(type));

        var instanceCreate = Expression.New(constructorInfo);

        var instanceCreateCast = typeInfo.IsValueType
            ? Expression.Convert(instanceCreate, typeof(object))
            : Expression.TypeAs(instanceCreate, typeof(object));

        var lambda = Expression.Lambda<Func<object>>(instanceCreateCast);

        return lambda.Compile();
    }

    /// <summary>
    /// Creates a delegate that gets the value of the specified property from an instance.
    /// </summary>
    /// <param name="propertyInfo">The property to get the value from.</param>
    /// <returns>
    /// A <see cref="Func{T, TResult}"/> that takes an instance and returns the property value as <c>object</c>,
    /// or <c>null</c> if the property is not readable.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="propertyInfo"/> is <c>null</c>.</exception>
    public static Func<object, object> CreateGet(PropertyInfo propertyInfo)
    {
        if (propertyInfo == null)
            throw new ArgumentNullException(nameof(propertyInfo));

        if (!propertyInfo.CanRead)
            return null;

        var instance = Expression.Parameter(typeof(object), "instance");
        var declaringType = propertyInfo.DeclaringType;
        var getMethod = propertyInfo.GetGetMethod(true);

        var instanceCast = CreateCast(instance, declaringType, getMethod.IsStatic);

        var call = Expression.Call(instanceCast, getMethod);
        var valueCast = Expression.TypeAs(call, typeof(object));

        var lambda = Expression.Lambda<Func<object, object>>(valueCast, instance);
        return lambda.Compile();
    }

    /// <summary>
    /// Creates a delegate that gets the value of the specified field from an instance.
    /// </summary>
    /// <param name="fieldInfo">The field to get the value from.</param>
    /// <returns>
    /// A <see cref="Func{T, TResult}"/> that takes an instance and returns the field value as <c>object</c>.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="fieldInfo"/> is <c>null</c>.</exception>
    public static Func<object, object> CreateGet(FieldInfo fieldInfo)
    {
        if (fieldInfo == null)
            throw new ArgumentNullException(nameof(fieldInfo));

        var instance = Expression.Parameter(typeof(object), "instance");
        var declaringType = fieldInfo.DeclaringType;

        var instanceCast = CreateCast(instance, declaringType, fieldInfo.IsStatic);

        var fieldAccess = Expression.Field(instanceCast, fieldInfo);
        var valueCast = Expression.TypeAs(fieldAccess, typeof(object));

        var lambda = Expression.Lambda<Func<object, object>>(valueCast, instance);
        return lambda.Compile();
    }

    /// <summary>
    /// Creates a delegate that sets the value of the specified property on an instance.
    /// </summary>
    /// <param name="propertyInfo">The property to set the value on.</param>
    /// <returns>
    /// An <see cref="Action{T1, T2}"/> that takes an instance and a value to set,
    /// or <c>null</c> if the property is not writable.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="propertyInfo"/> is <c>null</c>.</exception>
    public static Action<object, object> CreateSet(PropertyInfo propertyInfo)
    {
        if (propertyInfo == null)
            throw new ArgumentNullException(nameof(propertyInfo));

        if (!propertyInfo.CanWrite)
            return null;

        var instance = Expression.Parameter(typeof(object), "instance");
        var value = Expression.Parameter(typeof(object), "value");

        var declaringType = propertyInfo.DeclaringType;
        var propertyType = propertyInfo.PropertyType;
        var setMethod = propertyInfo.GetSetMethod(true);

        var instanceCast = CreateCast(instance, declaringType, setMethod.IsStatic);
        var valueCast = CreateCast(value, propertyType, false);

        var call = Expression.Call(instanceCast, setMethod, valueCast);
        var parameters = new[] { instance, value };

        var lambda = Expression.Lambda<Action<object, object>>(call, parameters);
        return lambda.Compile();
    }

    /// <summary>
    /// Creates a delegate that sets the value of the specified field on an instance.
    /// </summary>
    /// <param name="fieldInfo">The field to set the value on.</param>
    /// <returns>
    /// An <see cref="Action{T1, T2}"/> that takes an instance and a value to set.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="fieldInfo"/> is <c>null</c>.</exception>
    public static Action<object, object> CreateSet(FieldInfo fieldInfo)
    {
        if (fieldInfo == null)
            throw new ArgumentNullException(nameof(fieldInfo));

        var instance = Expression.Parameter(typeof(object), "instance");
        var value = Expression.Parameter(typeof(object), "value");

        var declaringType = fieldInfo.DeclaringType;
        var fieldType = fieldInfo.FieldType;

        var instanceCast = CreateCast(instance, declaringType, fieldInfo.IsStatic);
        var valueCast = CreateCast(value, fieldType, false);

        var member = Expression.Field(instanceCast, fieldInfo);
        var assign = Expression.Assign(member, valueCast);

        var parameters = new[] { instance, value };

        var lambda = Expression.Lambda<Action<object, object>>(assign, parameters);
        return lambda.Compile();
    }

    /// <summary>
    /// Creates a cast expression for the given parameter and type, handling static and value types appropriately.
    /// </summary>
    /// <param name="instance">The parameter expression representing the instance or value.</param>
    /// <param name="declaringType">The type to cast to.</param>
    /// <param name="isStatic">Indicates whether the member is static.</param>
    /// <returns>
    /// A <see cref="UnaryExpression"/> representing the cast, or <c>null</c> if the member is static.
    /// </returns>
    private static UnaryExpression CreateCast(ParameterExpression instance, Type declaringType, bool isStatic)
    {
        if (isStatic)
            return null;

        // value as T is slightly faster than (T)value, so if it's not a value type, use that
        if (declaringType.GetTypeInfo().IsValueType)
            return Expression.Convert(instance, declaringType);
        else
            return Expression.TypeAs(instance, declaringType);
    }
}
