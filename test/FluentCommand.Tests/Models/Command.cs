using System.Collections.Generic;

using FluentCommand.Internal;

namespace FluentCommand.Tests.Models;

public class Command
{
    public string Text { get; set; }
    public List<Parameter> Parameters { get; set; } = new List<Parameter>();

    public override int GetHashCode()
    {
        return HashCode.Seed
            .Combine(Text)
            .CombineAll(Parameters)
            .GetHashCode();
    }
}
