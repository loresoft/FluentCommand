using System.Data;

namespace FluentCommand.Tests.Models;

public class Parameter
{
    public string Name { get; set; }
    public object Value { get; set; }
    public DbType DbType { get; set; }

    public override int GetHashCode()
    {
        return FluentCommand.Internal.HashCode.Seed
            .Combine(Name)
            .Combine(Value)
            .Combine(DbType);
    }
}
