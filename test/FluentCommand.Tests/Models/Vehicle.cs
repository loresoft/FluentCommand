using System.ComponentModel.DataAnnotations.Schema;

namespace FluentCommand.Tests.Models;

[Table(nameof(Vehicle))]
public class Vehicle
{
    public string Name { get; set; } = null!;
    public virtual int Type { get; set; }
}
