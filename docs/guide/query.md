# Query Methods

## Execute or ExecuteAsync

Executes the command against a connection and returns the number of rows affected.

```csharp
int result = -1;
long total = -1;

var email = "william.adama@battlestar.com";

using var session = Services.GetRequiredService<IDataSession>();

result = session
    .StoredProcedure("[dbo].[UserCountByEmailAddress]")
    .Parameter("@EmailAddress", email)
    .Return<long>(p => total = p)
    .Execute();
```


## Query or QueryAsync

Executes the command against the connection and converts the results to a list of objects.

```csharp
long total = -1;
var email = "%@battlestar.com";

using var session = Services.GetRequiredService<IDataSession>();

var users = session
    .StoredProcedure("[dbo].[UserListByEmailAddress]")
    .Parameter("@EmailAddress", email)
    .Parameter("@Offset", 0)
    .Parameter("@Size", 10)
    .Parameter<long>(parameter => parameter
        .Name("@Total")
        .Type(DbType.Int64)
        .Output(v => total = v)
        .Direction(ParameterDirection.Output)
    )
    .Query<User>() // using source generated factor
    .ToList();
```

## QuerySingle or QuerySingleAsync

Executes the query and returns the first row in the result as an object.

```csharp
var session = Services.GetRequiredService<IDataSession>();

var email = "kara.thrace@battlestar.com";
var sql = "select * from [User] where EmailAddress = @EmailAddress";

var user = session.Sql(sql)
    .Parameter("@EmailAddress", email)
    .QuerySingle(r => new User
    {
        Id = r.GetGuid("Id"),
        EmailAddress = r.GetString("EmailAddress"),
        IsEmailAddressConfirmed = r.GetBoolean("IsEmailAddressConfirmed"),
        DisplayName = r.GetString("DisplayName"),
        PasswordHash = r.GetString("PasswordHash"),
        ResetHash = r.GetString("ResetHash"),
        InviteHash = r.GetString("InviteHash"),
        AccessFailedCount = r.GetInt32("AccessFailedCount"),
        LockoutEnabled = r.GetBoolean("LockoutEnabled"),
        LockoutEnd = r.GetDateTimeOffsetNull("LockoutEnd"),
        LastLogin = r.GetDateTimeOffsetNull("LastLogin"),
        IsDeleted = r.GetBoolean("IsDeleted"),
        Created = r.GetDateTimeOffset("Created"),
        CreatedBy = r.GetString("CreatedBy"),
        Updated = r.GetDateTimeOffset("Updated"),
        UpdatedBy = r.GetString("UpdatedBy"),
        RowVersion = r.GetBytes("RowVersion"),
    });
```

## QueryValue or QueryValueAsync

Executes the query and returns the first column of the first row in the result set returned by the query asynchronously. All other columns and rows are ignored.

```csharp
await using var session = Services.GetRequiredService<IDataSession>();

string email = "@battlestar.com";

var count = await session
    .Sql(builder => builder
        .Select<User>()
        .Count()
        .Where(p => p.EmailAddress, email, FilterOperators.Contains)
    )
    .QueryValueAsync<int>();
```

## QueryMultiple or QueryMultipleAsync

Executes the command against the connection and sends the results for reading multiple results sets

```csharp
string email = "kara.thrace@battlestar.com";
string sql = "select * from [User] where EmailAddress = @EmailAddress; " +
             "select * from [Role]; " +
             "select * from [Priority]; ";

User user = null;
List<Role> roles = null;
List<Priority> priorities = null;

await using var session = Services.GetRequiredService<IDataSession>();

await session.Sql(sql)
    .Parameter("@EmailAddress", email)
    .QueryMultipleAsync(async q =>
    {
        user = await q.QuerySingleAsync<User>();
        roles = (await q.QueryAsync<Role>()).ToList();
        priorities = (await q.QueryAsync<Priority>()).ToList();
    });
```
