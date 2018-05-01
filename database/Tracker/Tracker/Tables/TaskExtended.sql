CREATE TABLE [dbo].[TaskExtended]
(
    [TaskId] UNIQUEIDENTIFIER NOT NULL,

    [UserAgent] NVARCHAR(MAX) NULL,
    [Browser] NVARCHAR(256) NULL,
    [OperatingSystem] NVARCHAR(256) NULL,

    [Created] DATETIMEOFFSET NOT NULL CONSTRAINT [DF_TaskExtended_Created] DEFAULT (SYSUTCDATETIME()),
    [CreatedBy] NVARCHAR(100) NULL,
    [Updated] DATETIMEOFFSET NOT NULL CONSTRAINT [DF_TaskExtended_Updated] DEFAULT (SYSUTCDATETIME()),
    [UpdatedBy] NVARCHAR(100) NULL,
    [RowVersion] ROWVERSION NOT NULL,

    CONSTRAINT [PK_TaskExtended] PRIMARY KEY NONCLUSTERED ([TaskId] ASC),
    CONSTRAINT [FK_TaskExtended_Task_TaskId] FOREIGN KEY ([TaskId]) REFERENCES [dbo].[Task]([Id]),
)
