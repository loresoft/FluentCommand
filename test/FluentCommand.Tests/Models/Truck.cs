using System.ComponentModel.DataAnnotations.Schema;

namespace FluentCommand.Tests.Models;

[Table(nameof(Truck))]
public class Truck : Vehicle
{
    public string Color { get; set; } = null!;
    public override int Type { get; set; }
}
