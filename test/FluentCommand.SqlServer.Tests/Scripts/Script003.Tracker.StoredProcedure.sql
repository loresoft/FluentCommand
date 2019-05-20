
IF OBJECT_ID('[dbo].[UserUpsert]') IS NULL
BEGIN
    EXEC('CREATE PROCEDURE [dbo].[UserUpsert] AS SET NOCOUNT ON;')
END

GO

ALTER PROCEDURE [dbo].[UserUpsert]
    @Id UNIQUEIDENTIFIER,
    @EmailAddress NVARCHAR(256),
    @IsEmailAddressConfirmed BIT,
    @DisplayName NVARCHAR(256),
    @PasswordHash NVARCHAR(MAX) = NULL,
    @ResetHash NVARCHAR(MAX) = NULL,
    @InviteHash NVARCHAR(MAX) = NULL,
    @AccessFailedCount INT,
    @LockoutEnabled BIT,
    @LockoutEnd DATETIMEOFFSET = NULL,
    @LastLogin DATETIMEOFFSET = NULL,
    @IsDeleted BIT
AS
BEGIN
    SET NOCOUNT ON;

    MERGE INTO [dbo].[User] AS t
    USING
    (
        SELECT
            @Id,
            @EmailAddress,
            @IsEmailAddressConfirmed,
            @DisplayName,
            @PasswordHash,
            @ResetHash,
            @InviteHash,
            @AccessFailedCount,
            @LockoutEnabled,
            @LockoutEnd,
            @LastLogin,
            @IsDeleted
    )
    AS s
    (
        [Id], 
        [EmailAddress], 
        [IsEmailAddressConfirmed], 
        [DisplayName], 
        [PasswordHash], 
        [ResetHash], 
        [InviteHash], 
        [AccessFailedCount], 
        [LockoutEnabled], 
        [LockoutEnd], 
        [LastLogin], 
        [IsDeleted]
    )
    ON
    (
        t.[Id] = s.[Id]
    )
    WHEN NOT MATCHED BY TARGET THEN 
        INSERT
        (
            [Id], 
            [EmailAddress], 
            [IsEmailAddressConfirmed], 
            [DisplayName], 
            [PasswordHash], 
            [ResetHash], 
            [InviteHash], 
            [AccessFailedCount], 
            [LockoutEnabled], 
            [LockoutEnd], 
            [LastLogin], 
            [IsDeleted]
        )
        VALUES
        (
            s.[Id], 
            s.[EmailAddress], 
            s.[IsEmailAddressConfirmed], 
            s.[DisplayName], 
            s.[PasswordHash], 
            s.[ResetHash], 
            s.[InviteHash], 
            s.[AccessFailedCount], 
            s.[LockoutEnabled], 
            s.[LockoutEnd], 
            s.[LastLogin], 
            s.[IsDeleted]
        )
    WHEN MATCHED THEN
        UPDATE SET
            t.[EmailAddress] = s.[EmailAddress], 
            t.[IsEmailAddressConfirmed] = s.[IsEmailAddressConfirmed], 
            t.[DisplayName] = s.[DisplayName], 
            t.[PasswordHash] = s.[PasswordHash], 
            t.[ResetHash] = s.[ResetHash], 
            t.[InviteHash] = s.[InviteHash], 
            t.[AccessFailedCount] = s.[AccessFailedCount], 
            t.[LockoutEnabled] = s.[LockoutEnabled], 
            t.[LockoutEnd] = s.[LockoutEnd], 
            t.[LastLogin] = s.[LastLogin], 
            t.[IsDeleted] = s.[IsDeleted],
            t.[Updated] = sysutcdatetime()
    OUTPUT
        INSERTED.[Id], 
        INSERTED.[EmailAddress], 
        INSERTED.[IsEmailAddressConfirmed], 
        INSERTED.[DisplayName], 
        INSERTED.[PasswordHash], 
        INSERTED.[ResetHash], 
        INSERTED.[InviteHash], 
        INSERTED.[AccessFailedCount], 
        INSERTED.[LockoutEnabled], 
        INSERTED.[LockoutEnd], 
        INSERTED.[LastLogin], 
        INSERTED.[IsDeleted], 
        INSERTED.[Created], 
        INSERTED.[CreatedBy], 
        INSERTED.[Updated], 
        INSERTED.[UpdatedBy], 
        INSERTED.[RowVersion],
        $action AS [ActionType];

    SET NOCOUNT OFF;
END

GO

IF OBJECT_ID('[dbo].[UserListByEmailAddress]') IS NULL
BEGIN
    EXEC('CREATE PROCEDURE [dbo].[UserListByEmailAddress] AS SET NOCOUNT ON;')
END

GO
 
ALTER PROCEDURE [dbo].[UserListByEmailAddress]
    @EmailAddress NVARCHAR(256),    
    @Offset INT = 0,
    @Size INT = 100,
    @Total BIGINT OUT
AS
BEGIN
    SET NOCOUNT ON;

    SET @Total = (
        SELECT COUNT(t.[Id])
        FROM [dbo].[User] AS t
        WHERE t.EmailAddress LIKE @EmailAddress
    );

    SELECT 
        t.[Id], 
        t.[EmailAddress], 
        t.[IsEmailAddressConfirmed], 
        t.[DisplayName], 
        t.[PasswordHash], 
        t.[ResetHash], 
        t.[InviteHash], 
        t.[AccessFailedCount], 
        t.[LockoutEnabled], 
        t.[LockoutEnd], 
        t.[LastLogin], 
        t.[IsDeleted], 
        t.[Created], 
        t.[CreatedBy], 
        t.[Updated], 
        t.[UpdatedBy]
    FROM [dbo].[User] AS t
    WHERE t.EmailAddress LIKE @EmailAddress
    ORDER BY t.[Id]
    OFFSET @Offset ROWS
    FETCH NEXT @Size ROWS ONLY;

    SET NOCOUNT OFF;
END

GO

IF OBJECT_ID('[dbo].[UserCountByEmailAddress]') IS NULL
BEGIN
    EXEC('CREATE PROCEDURE [dbo].[UserCountByEmailAddress] AS SET NOCOUNT ON;')
END

GO

ALTER PROCEDURE [dbo].[UserCountByEmailAddress]
    @EmailAddress NVARCHAR(256)
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @Total BIGINT;

    SET @Total = (
        SELECT COUNT(t.[Id])
        FROM [dbo].[User] AS t
        WHERE t.EmailAddress = @EmailAddress
    );

    RETURN @Total;

    SET NOCOUNT OFF;
END

GO 

IF OBJECT_ID('[dbo].[ImportUsers]') IS NULL
BEGIN
    EXEC('CREATE PROCEDURE [dbo].[ImportUsers] AS SET NOCOUNT ON;')
END

GO

ALTER PROCEDURE [dbo].[ImportUsers]
    @userTable [dbo].[UserImportType] READONLY
AS
BEGIN
    SET NOCOUNT ON;

    MERGE [dbo].[User] WITH (ROWLOCK) AS D
    USING @userTable AS S
        ON D.[EmailAddress] = S.[EmailAddress]
    WHEN MATCHED THEN
        UPDATE SET
            D.[DisplayName] = S.[DisplayName]
    WHEN NOT MATCHED THEN
        INSERT
        (
            [EmailAddress],
            [DisplayName]
        )
        VALUES
        (
            S.[EmailAddress],
            S.[DisplayName]
        );

    SET NOCOUNT OFF;
END
