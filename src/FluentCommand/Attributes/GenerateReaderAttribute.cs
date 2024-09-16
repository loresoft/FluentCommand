using System.Diagnostics;

namespace FluentCommand.Attributes;

/// <summary>
/// Source generate a data reader for the specified type
/// </summary>
/// <param name="type">The type to generate a data reader for</param>
[Conditional("FLUENTCOMMAND_GENERATOR")]
[AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Class | AttributeTargets.Interface | AttributeTargets.Module, AllowMultiple = true)]
public class GenerateReaderAttribute(Type type) : Attribute
{
    public Type Type { get; } = type ?? throw new ArgumentNullException(nameof(type));
}
