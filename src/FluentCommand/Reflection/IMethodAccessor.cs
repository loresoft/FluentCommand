using System.Reflection;

namespace FluentCommand.Reflection;

/// <summary>
/// Defines an interface for accessing and invoking methods via reflection, providing metadata and dynamic invocation capabilities.
/// </summary>
public interface IMethodAccessor
{
    /// <summary>
    /// Gets the <see cref="MethodInfo"/> associated with this accessor.
    /// </summary>
    /// <value>The <see cref="MethodInfo"/> representing the method.</value>
    MethodInfo MethodInfo { get; }

    /// <summary>
    /// Gets the name of the method.
    /// </summary>
    /// <value>The name of the method.</value>
    string Name { get; }

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
    object Invoke(object instance, params object[] arguments);
}
