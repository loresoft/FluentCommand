using FluentCommand.Attributes;
using FluentCommand.Entities;

[assembly: GenerateReader(typeof(Brand))]

namespace FluentCommand.Entities;

public class Brand
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
}
