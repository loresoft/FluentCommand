namespace FluentCommand;

/// <summary>
/// Attribute to enable source generation of data reader factory
/// </summary>
/// <seealso cref="System.Attribute" />
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
public class GenerateDataReaderAttribute : Attribute
{

}
