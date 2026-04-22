using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using FluentCommand.Handlers;

namespace FluentCommand.Tests.Models;

[Table(nameof(User))]
public class User
{
    public Guid Id { get; set; }

    public string EmailAddress { get; set; } = null!;

    public string DisplayName { get; set; } = null!;

    public string FirstName { get; set; } = null!;

    public string LastName { get; set; } = null!;

    public string PasswordHash { get; set; } = null!;

    public DateTimeOffset Created { get; set; }
    public string CreatedBy { get; set; } = null!;
    public DateTimeOffset Updated { get; set; }
    public string UpdatedBy { get; set; } = null!;

    [ConcurrencyCheck]
    [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
    [DataFieldConverter(typeof(ConcurrencyTokenHandler))]
    public ConcurrencyToken RowVersion { get; set; }
}
