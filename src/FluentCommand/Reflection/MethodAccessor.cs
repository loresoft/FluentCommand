using System.Diagnostics;
using System.Reflection;

namespace FluentCommand.Reflection;

/// <summary>
/// Provides an accessor for <see cref="MethodInfo"/>, enabling efficient dynamic invocation of methods
/// and access to method metadata. Implements <see cref="IMethodAccessor"/> for late-bound method operations.
/// </summary>
[DebuggerDisplay("Name: {Name}")]
public class MethodAccessor : IMethodAccessor
{
    private readonly Lazy<Func<object, object[], object>> _invoker;

    /// <summary>
    /// Initializes a new instance of the <see cref="MethodAccessor"/> class using the specified <see cref="MethodInfo"/>.
    /// </summary>
    /// <param name="methodInfo">The <see cref="MethodInfo"/> representing the method to access.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="methodInfo"/> is <c>null</c>.</exception>
    public MethodAccessor(MethodInfo methodInfo)
    {
        if (methodInfo == null)
            throw new ArgumentNullException(nameof(methodInfo));

        MethodInfo = methodInfo;
        Name = methodInfo.Name;
        _invoker = new Lazy<Func<object, object[], object>>(() => ExpressionFactory.CreateMethod(MethodInfo));
    }

    /// <summary>
    /// Gets the <see cref="MethodInfo"/> associated with this accessor.
    /// </summary>
    /// <value>The <see cref="MethodInfo"/> representing the method.</value>
    public MethodInfo MethodInfo { get; }

    /// <summary>
    /// Gets the name of the method.
    /// </summary>
    /// <value>The name of the method.</value>
    public string Name { get; }

    /// <summary>
    /// Invokes the method represented by this accessor on the specified <paramref name="instance"/> with the given arguments.
    /// </summary>
    /// <param name="instance">
    /// The object on which to invoke the method. For static methods, this argument is ignored and can be <c>null</c>.
    /// </param>
    /// <param name="arguments">An array of arguments to pass to the method.</param>
    /// <returns>
    /// An <see cref="object"/> containing the return value of the invoked method, or <c>null</c> for methods with <c>void</c> return type.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown if required arguments are <c>null</c>.</exception>
    /// <exception cref="TargetParameterCountException">Thrown if the number of parameters in <paramref name="arguments"/> does not match the method signature.</exception>
    /// <exception cref="TargetInvocationException">Thrown if the invoked method throws an exception.</exception>
    public object Invoke(object instance, params object[] arguments)
    {
        return _invoker.Value.Invoke(instance, arguments);
    }

    /// <summary>
    /// Gets a hash code key for the method using its name and parameter types.
    /// </summary>
    /// <param name="name">The name of the method.</param>
    /// <param name="parameterTypes">The method parameter types.</param>
    /// <returns>
    /// An integer representing the method key, suitable for use in hash-based collections.
    /// </returns>
    internal static int GetKey(string name, IEnumerable<Type> parameterTypes)
    {
        return Internal.HashCode.Seed
            .Combine(name)
            .CombineAll(parameterTypes);
    }
}
