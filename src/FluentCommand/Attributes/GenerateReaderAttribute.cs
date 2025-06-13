using System.Diagnostics;

namespace FluentCommand.Attributes;

/// <summary>
/// Specifies that a source generator should create a data reader for the given type.
/// </summary>
/// <remarks>
/// Apply this attribute to an assembly, class, interface, or module to indicate that a data reader should be generated
/// for the specified <see cref="Type"/>. This is typically used in conjunction with source generators.
/// </remarks>
/// <example>
/// <code>
/// [assembly: GenerateReader(typeof(Product))]
/// </code>
/// </example>
/// <param name="type">
/// The <see cref="Type"/> for which to generate a data reader. Must not be <c>null</c>.
/// </param>
[Conditional("FLUENTCOMMAND_GENERATOR")]
[AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Class | AttributeTargets.Interface | AttributeTargets.Module, AllowMultiple = true)]
public class GenerateReaderAttribute(Type type) : Attribute
{
    /// <summary>
    /// Gets the type for which the data reader will be generated.
    /// </summary>
    public Type Type { get; } = type ?? throw new ArgumentNullException(nameof(type));
}
