namespace FluentCommand.Entities;

[GenerateDataReader]
public class UserImport
{
    public string EmailAddress { get; set; }
    public string DisplayName { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }

}
