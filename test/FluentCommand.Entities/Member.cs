using System.ComponentModel.DataAnnotations.Schema;

namespace FluentCommand.Entities;

[Table("member_user", Schema = "dbo")]
public class Member
{
    [Column("Id")]
    public Guid Id { get; set; }

    [Column("email_address")]
    public string EmailAddress { get; set; }

    [Column("display_name")]
    public string DisplayName { get; set; }

    [Column("first_name")]
    public string FirstName { get; set; }

    [Column("last_name")]
    public string LastName { get; set; }
}
