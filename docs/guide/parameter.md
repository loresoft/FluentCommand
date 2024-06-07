# Parameters

## Calling a stored procedure with an out parameter

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
    .Query<User>()
    .ToList();
```

## SQL query with a parameter

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

## Executing an Upsert stored procedure

```csharp
int errorCode = -1;

var userId = Guid.NewGuid();
var username = "test." + DateTime.Now.Ticks;
var email = username + "@email.com";

using var session = Services.GetRequiredService<IDataSession>();

var user = session
    .StoredProcedure("[dbo].[UserUpsert]")
    .Parameter("@Id", userId)
    .Parameter("@EmailAddress", email)
    .Parameter("@IsEmailAddressConfirmed", true)
    .Parameter("@DisplayName", "Unit Test")
    .Parameter("@PasswordHash", "T@est" + DateTime.Now.Ticks)
    .Parameter<string>("@ResetHash", null)
    .Parameter<string>("@InviteHash", null)
    .Parameter("@AccessFailedCount", 0)
    .Parameter("@LockoutEnabled", false)
    .Parameter("@IsDeleted", false)
    .Return<int>(p => errorCode = p)
    .QuerySingle<User>();
```

## Executing a stored procedure with a return parameter

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
