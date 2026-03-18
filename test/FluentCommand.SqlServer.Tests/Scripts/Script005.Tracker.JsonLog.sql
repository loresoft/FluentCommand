IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[JsonLog]') AND type in (N'U'))
CREATE TABLE [dbo].[JsonLog] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [Data] nvarchar(MAX) NOT NULL,
    [Created] datetimeoffset NOT NULL DEFAULT (sysutcdatetime()),
    CONSTRAINT [PK_JsonLog] PRIMARY KEY ([Id])
);
