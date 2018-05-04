CREATE TABLE [dbo].[Task]
(
    [Id] UNIQUEIDENTIFIER NOT NULL CONSTRAINT [DF_Task_Id] DEFAULT (NEWSEQUENTIALID()),
    [StatusId] INT NOT NULL,
    [PriorityId] INT NULL,

    [Title] NVARCHAR(255) NOT NULL,
    [Description] NVARCHAR(MAX) NULL,
    
    [StartDate] DATETIMEOFFSET NULL,
    [DueDate] DATETIMEOFFSET NULL,
    [CompleteDate] DATETIMEOFFSET NULL,
    [AssignedId] UNIQUEIDENTIFIER NULL,
    
    [Created] DATETIMEOFFSET NOT NULL CONSTRAINT [DF_Task_Created] DEFAULT (SYSUTCDATETIME()),
    [CreatedBy] NVARCHAR(100) NULL,
    [Updated] DATETIMEOFFSET NOT NULL CONSTRAINT [DF_Task_Updated] DEFAULT (SYSUTCDATETIME()),
    [UpdatedBy] NVARCHAR(100) NULL,
    [RowVersion] ROWVERSION NOT NULL,

    CONSTRAINT [PK_Task] PRIMARY KEY NONCLUSTERED ([Id] ASC), 
    CONSTRAINT [FK_Task_Status_StatusId] FOREIGN KEY ([StatusId]) REFERENCES [dbo].[Status]([Id]),
    CONSTRAINT [FK_Task_Priority_PriorityId] FOREIGN KEY ([PriorityId]) REFERENCES [dbo].[Priority]([Id]),
    CONSTRAINT [FK_Task_User_AssignedId] FOREIGN KEY ([AssignedId]) REFERENCES [dbo].[User]([Id]),
)

GO

CREATE INDEX [IX_Task_StatusId]
ON [dbo].[Task] ([StatusId])

GO

CREATE INDEX [IX_Task_PriorityId]
ON [dbo].[Task] ([PriorityId])

GO

CREATE INDEX [IX_Task_AssignedId]
ON [dbo].[Task] ([AssignedId])

GO