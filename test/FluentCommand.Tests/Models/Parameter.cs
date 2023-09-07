using System.Data;

using FluentCommand.Internal;

namespace FluentCommand.Tests.Models;

public class Parameter
{
    public string Name { get; set; }
    public object Value { get; set; }
    public DbType DbType { get; set; }

    public override int GetHashCode()
    {
        return HashCode.Seed
            .Combine(Name)
            .Combine(Value)
            .Combine(DbType);
    }
}
