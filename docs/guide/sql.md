# SQL Query Builder

## Basic select query

```csharp
await using var session = Services.GetRequiredService<IDataSession>();

string email = "kara.thrace@battlestar.com";

var user = await session
    .Sql(builder => builder
        .Select<User>()
        .Where(p => p.EmailAddress, email)
        .OrderBy(p => p.EmailAddress)
        .Limit(0, 10)
    )
    .QuerySingleAsync(r => new User
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

Output

```sql
SELECT *
FROM [User]
WHERE ([EmailAddress] = @p0000)
ORDER BY [EmailAddress] ASC
OFFSET 0 ROWS FETCH NEXT 10 ROWS ONLY;
```

## Select with OR where clause

```csharp
await using var session = Services.GetRequiredService<IDataSession>();

string email = "@battlestar.com";

var users = await session
    .Sql(builder => builder
        .Select<User>()
        .Where(p => p.EmailAddress, email, FilterOperators.Contains)
        .WhereOr(o => o
            .Where(p => p.IsDeleted, true, FilterOperators.NotEqual)
            .Where(p => p.IsEmailAddressConfirmed, true)
        )
        .OrderBy(p => p.Updated)
        .Limit(0, 10)
    )
    .QueryAsync<User>();
```

Output

```sql
SELECT *
FROM [User]
WHERE ([EmailAddress] LIKE '%' + @p0000 + '%' AND ([IsDeleted] != @p0001 OR [IsEmailAddressConfirmed] = @p0002))
ORDER BY [Updated] ASC
OFFSET 0 ROWS FETCH NEXT 10 ROWS ONLY;
```

## Select Join clause

```csharp
await using var session = Services.GetRequiredService<IDataSession>();

string email = "@battlestar.com";

var users = await session
    .Sql(builder => builder
        .Select<User>()
        .Column(p => p.DisplayName, "u")
        .Column(p => p.EmailAddress, "u")
        .Column<Role>(p => p.Name, "r", "RoleName")
        .From(tableAlias: "u")
        .Join<UserRole>(j => j
            .Left(u => u.Id, "u")
            .Right(u => u.UserId, "ur")
        )
        .Join<UserRole, Role>(j => j
            .Left(u => u.RoleId, "ur")
            .Right(u => u.Id, "r")
        )
        .Where(p => p.EmailAddress, email, "u", FilterOperators.Contains)
        .OrderBy(p => p.Updated, "r")
        .Limit(0, 10)
    )
    .QueryAsync<User>();
```

Output

```sql
SELECT [u].[DisplayName], [u].[EmailAddress], [r].[Name] AS [RoleName]
FROM [User] AS [u]
INNER JOIN [UserRole] AS [ur] ON [u].[Id] = [ur].[UserId]
INNER JOIN [Role] AS [r] ON [ur].[RoleId] = [r].[Id]
WHERE ([u].[EmailAddress] LIKE '%' + @p0000 + '%')
ORDER BY [r].[Updated] ASC
OFFSET 0 ROWS FETCH NEXT 10 ROWS ONLY;
```

## Select Count

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

Output

```sql
SELECT COUNT(*)
FROM [User]
WHERE ([EmailAddress] = @p0000);
```

## Select aggregate sum

```csharp
await using var session = Services.GetRequiredService<IDataSession>();

var count = await session
    .Sql(builder => builder
        .Select<Status>()
        .Aggregate(p => p.DisplayOrder, AggregateFunctions.Sum)
        .GroupBy(p => p.IsActive)
    )
    .QueryValueAsync<int>();
```

Output

```sql
SELECT SUM([DisplayOrder])
FROM [dbo].[Status]
GROUP BY [IsActive];
```

## Select IN clause

```csharp
await using var session = Services.GetRequiredService<IDataSession>();

var values = new[] { 1, 2, 3 };

var results = await session
    .Sql(builder => builder
        .Select<Status>()
        .WhereIn(p => p.Id, values)
        .Tag()
    )
    .QueryAsync<Status>();
```

Output

```sql
/* Caller; SqlQueryInEntityAsync() in DataQueryTests.cs:line 177 */
SELECT *
FROM [dbo].[Status]
WHERE ([Id] IN (@p0000,@p0001,@p0002));
```

## Complex multiple statements

```csharp
await using var session = Services.GetRequiredService<IDataSession>();

var values = new[] { 1, 2, 3 }.ToDelimitedString();

var results = await session
    .Sql(builder =>
    {
        builder
            .Statement()
            .Query("CREATE TABLE #identifiers (Id int);");

        builder
            .Statement()
            .Query("INSERT INTO #identifiers (Id) SELECT CONVERT(int, value) FROM STRING_SPLIT(@Identifiers, @Separator);")
            .Parameter("@Identifiers", values)
            .Parameter("@Separator", ",");

        builder
            .Select<Status>()
            .Tag()
            .From(tableAlias: "s")
            .Join(j => j
                .Left("Id", "s")
                .Right("Id", "#identifiers", null, "i")
            );
    })
    .QueryAsync<Status>();
```

Output

```sql
CREATE TABLE #identifiers (Id int);
INSERT INTO #identifiers (Id) SELECT CONVERT(int, value) FROM STRING_SPLIT(@Identifiers, @Separator);
/* Caller; SqlQueryInComplexEntityAsync() in DataQueryTests.cs:line 210 */
SELECT *
FROM [dbo].[Status] AS [s]
INNER JOIN [#identifiers] AS [i] ON [s].[Id] = [i].[Id];
```

## Insert values query

```csharp
await using var session = Services.GetRequiredService<IDataSession>();

var id = Guid.NewGuid();

var userId = await session
    .Sql(builder => builder
        .Insert<User>()
        .Value(p => p.Id, id)
        .Value(p => p.EmailAddress, $"{id}@email.com")
        .Value(p => p.DisplayName, "Last, First")
        .Value(p => p.FirstName, "First")
        .Value(p => p.LastName, "Last")
        .Output(p => p.Id)
        .Tag()
    )
    .QueryValueAsync<Guid>();
```

Output

```sql
/* Caller; SqlInsertValueQuery() in DataQueryTests.cs:line 243 */
INSERT INTO [User] ([Id], [EmailAddress], [DisplayName], [FirstName], [LastName])
OUTPUT [INSERTED].[Id]
VALUES (@p0000, @p0001, @p0002, @p0003, @p0004);
```

## Insert entity

```csharp
await using var session = Services.GetRequiredService<IDataSession>();

var id = Guid.NewGuid();
var user = new User
{
    Id = id,
    EmailAddress = $"{id}@email.com",
    DisplayName = "Last, First",
    FirstName = "First",
    LastName = "Last",
    Created = DateTimeOffset.Now,
    Updated = DateTimeOffset.Now
};

var userId = await session
    .Sql(builder => builder
        .Insert<User>()
        .Values(user)
        .Output(p => p.Id)
        .Tag()
    )
    .QueryValueAsync<Guid>();
```

Output

```sql
/* Caller; SqlInsertEntityQuery() in DataQueryTests.cs:line 273 */
INSERT INTO [User] ([Id], [EmailAddress], [IsEmailAddressConfirmed], [DisplayName], [FirstName], [LastName], [PasswordHash], [ResetHash], [InviteHash], [AccessFailedCount], [LockoutEnabled], [LockoutEnd], [LastLogin], [IsDeleted], [Created], [CreatedBy], [Updated], [UpdatedBy])
OUTPUT [INSERTED].[Id]
VALUES (@p0000, @p0001, @p0002, @p0003, @p0004, @p0005, @p0006, @p0007, @p0008, @p0009, @p0010, @p0011, @p0012, @p0013, @p0014, @p0015, @p0016, @p0017);
```

## Update value

```csharp
await using var session = Services.GetRequiredService<IDataSession>();

var id = Guid.NewGuid();

var updateId = await session
    .Sql(builder => builder
        .Update<User>()
        .Value(p => p.DisplayName, "Updated")
        .Output(p => p.Id)
        .Where(p => p.Id, id)
        .Tag()
    )
    .QueryValueAsync<Guid>();
```

Output

```sql
/* Caller; SqlInsertUpdateDeleteEntityQuery() in DataQueryTests.cs:line 327 */
UPDATE [User]
SET [DisplayName] = @p0000
OUTPUT [INSERTED].[Id]
WHERE ([Id] = @p0001);
```
