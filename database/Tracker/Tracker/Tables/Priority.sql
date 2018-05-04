CREATE TABLE [dbo].[Priority]
(
    [Id] INT NOT NULL,
    [Name] NVARCHAR(100)  NOT NULL,
    [Description] NVARCHAR(255)  NULL,

    [DisplayOrder] INT NOT NULL CONSTRAINT [DF_Priority_DisplayOrder] DEFAULT (0),

    [IsActive] BIT NOT NULL CONSTRAINT [DF_Priority_IsActive] DEFAULT (1),

    [Created] DATETIMEOFFSET NOT NULL CONSTRAINT [DF_Priority_Created] DEFAULT (SYSUTCDATETIME()),
    [CreatedBy] NVARCHAR(100) NULL,
    [Updated] DATETIMEOFFSET NOT NULL CONSTRAINT [DF_Priority_Updated] DEFAULT (SYSUTCDATETIME()),
    [UpdatedBy] NVARCHAR(100) NULL,
    [RowVersion] ROWVERSION NOT NULL,

    CONSTRAINT [PK_Priority] PRIMARY KEY CLUSTERED ([Id] ASC), 
)
