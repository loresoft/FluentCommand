CREATE TABLE [dbo].[UserLogin]
(
    [Id] UNIQUEIDENTIFIER NOT NULL CONSTRAINT [DF_UserLogin_Id] DEFAULT (NEWSEQUENTIALID()),

    [EmailAddress] NVARCHAR(256) NOT NULL,
    [UserId] UNIQUEIDENTIFIER NULL,

    [UserAgent] NVARCHAR(MAX) NULL,
    [Browser] NVARCHAR(256) NULL,
    [OperatingSystem] NVARCHAR(256) NULL,
    [DeviceFamily] NVARCHAR(256) NULL,
    [DeviceBrand] NVARCHAR(256) NULL,
    [DeviceModel] NVARCHAR(256) NULL,

    [IpAddress] NVARCHAR(50) NULL,

    [IsSuccessful] BIT NOT NULL CONSTRAINT [DF_UserLogin_IsSuccessful] DEFAULT (0),
    [FailureMessage] NVARCHAR(256) NULL,

    [Created] DATETIMEOFFSET NOT NULL CONSTRAINT [DF_UserLogin_Created] DEFAULT (SYSUTCDATETIME()),
    [CreatedBy] NVARCHAR(100) NULL,
    [Updated] DATETIMEOFFSET NOT NULL CONSTRAINT [DF_UserLogin_Updated] DEFAULT (SYSUTCDATETIME()),
    [UpdatedBy] NVARCHAR(100) NULL,
    [RowVersion] ROWVERSION NOT NULL,

    CONSTRAINT [PK_UserLogin] PRIMARY KEY NONCLUSTERED ([Id] ASC),
    CONSTRAINT [FK_UserLogin_User_UserId] FOREIGN KEY ([UserId]) REFERENCES [User]([Id]),
)

GO

CREATE INDEX [IX_UserLogin_EmailAddress]
ON [dbo].[UserLogin] ([EmailAddress])

GO

CREATE INDEX [IX_UserLogin_UserId]
ON [dbo].[UserLogin] ([UserId])

