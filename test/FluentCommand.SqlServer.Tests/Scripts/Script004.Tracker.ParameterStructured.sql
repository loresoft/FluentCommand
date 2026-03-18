-- Types
IF TYPE_ID('[dbo].[IdListType]') IS NULL
BEGIN
    CREATE TYPE [dbo].[IdListType] AS TABLE
    (
        [Id] UNIQUEIDENTIFIER NOT NULL
        PRIMARY KEY ([Id])
    );
END

GO

-- Stored Procedures
IF OBJECT_ID('[dbo].[UserListByIds]') IS NULL
BEGIN
    EXEC('CREATE PROCEDURE [dbo].[UserListByIds] AS SET NOCOUNT ON;')
END

GO

ALTER PROCEDURE [dbo].[UserListByIds]
    @IdList [dbo].[IdListType] READONLY,
    @Total BIGINT OUT
AS
BEGIN
    SET NOCOUNT ON;

    SET @Total = (
        SELECT COUNT(*)
        FROM [dbo].[User] AS u
        INNER JOIN @IdList AS i ON u.[Id] = i.[Id]
    );

    SELECT
        u.[Id],
        u.[EmailAddress],
        u.[IsEmailAddressConfirmed],
        u.[DisplayName],
        u.[FirstName],
        u.[LastName],
        u.[Created],
        u.[Updated]
    FROM [dbo].[User] AS u
    INNER JOIN @IdList AS i ON u.[Id] = i.[Id]
    ORDER BY u.[EmailAddress];

    SET NOCOUNT OFF;
END

GO

IF OBJECT_ID('[dbo].[UserImportStructured]') IS NULL
BEGIN
    EXEC('CREATE PROCEDURE [dbo].[UserImportStructured] AS SET NOCOUNT ON;')
END

GO

ALTER PROCEDURE [dbo].[UserImportStructured]
    @UserTable [dbo].[UserImportType] READONLY
AS
BEGIN
    SET NOCOUNT ON;

    MERGE [dbo].[User] WITH (ROWLOCK) AS D
    USING @UserTable AS S
        ON D.[EmailAddress] = S.[EmailAddress]
    WHEN MATCHED THEN
        UPDATE SET
            D.[DisplayName] = S.[DisplayName],
            D.[FirstName] = S.[FirstName],
            D.[LastName] = S.[LastName],
            D.[Updated] = sysutcdatetime()
    WHEN NOT MATCHED THEN
        INSERT
        (
            [EmailAddress],
            [DisplayName],
            [FirstName],
            [LastName]
        )
        VALUES
        (
            S.[EmailAddress],
            S.[DisplayName],
            S.[FirstName],
            S.[LastName]
        )
    OUTPUT
        INSERTED.[Id],
        INSERTED.[EmailAddress],
        INSERTED.[DisplayName],
        INSERTED.[FirstName],
        INSERTED.[LastName],
        $action AS [MergeAction];

    SET NOCOUNT OFF;
END
