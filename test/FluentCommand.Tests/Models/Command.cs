using System.Collections.Generic;
using System.Data;

using FluentCommand.Internal;

namespace FluentCommand.Tests.Models;

public class Command
{
    public string Text { get; set; }

    public CommandType CommandType { get; set; }

    public List<Parameter> Parameters { get; set; } = new List<Parameter>();

    public override int GetHashCode()
    {
        return HashCode.Seed
            .Combine(Text)
            .Combine(CommandType)
            .CombineAll(Parameters)
            .GetHashCode();
    }
}
