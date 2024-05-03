using System.Data;

namespace FluentCommand.Tests.Models;

public class Command
{
    public string Text { get; set; }

    public CommandType CommandType { get; set; }

    public List<Parameter> Parameters { get; set; } = new List<Parameter>();

    public override int GetHashCode()
    {
        return FluentCommand.Internal.HashCode.Seed
            .Combine(Text)
            .Combine(CommandType)
            .CombineAll(Parameters)
            .GetHashCode();
    }
}
