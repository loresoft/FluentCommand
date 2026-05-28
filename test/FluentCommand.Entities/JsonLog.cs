using System.ComponentModel.DataAnnotations.Schema;

using FluentCommand.Attributes;

namespace FluentCommand.Entities;

[Table(nameof(JsonLog))]
public class JsonLog
{
    public int Id { get; set; }

    [JsonColumn]
    public UserImport? Data { get; set; }

    public DateTimeOffset Created { get; set; }
}
