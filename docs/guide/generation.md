# Source Generator

The project supports generating a DbDataReader from a class via an attribute.  Add the `TableAttribute` to a class to generate the needed extension methods.

```c#
[Table("Status", Schema = "dbo")]
public class Status
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; }
    public DateTimeOffset Created { get; set; }
    public string CreatedBy { get; set; }
    public DateTimeOffset Updated { get; set; }
    public string UpdatedBy { get; set; }

    [ConcurrencyCheck]
    [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
    [DataFieldConverter(typeof(ConcurrencyTokenHandler))]
    public ConcurrencyToken RowVersion { get; set; }

    [NotMapped]
    public virtual ICollection<Task> Tasks { get; set; } = new List<Task>();
}
```

Extension methods are generated to materialize data command to entities

```c#
string email = "kara.thrace@battlestar.com";
string sql = "select * from [User] where EmailAddress = @EmailAddress";
var session = configuration.CreateSession();
var user = await session
    .Sql(sql)
    .Parameter("@EmailAddress", email)
    .QuerySingleAsync<User>();
```
