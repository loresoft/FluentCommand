CREATE TABLE IF NOT EXISTS [Audit] (
    [Id] integer NOT NULL PRIMARY KEY AUTOINCREMENT,
    [Date] datetime NOT NULL,
    [UserId] int,
    [TaskId] int,
    [Content] varchar NOT NULL,
    [Username] nvarchar(50) NOT NULL,
    [CreatedDate] datetime NOT NULL,
    CONSTRAINT [FK_Audit_Task] FOREIGN KEY ([TaskId]) REFERENCES [Task] ([Id])
    CONSTRAINT [FK_Audit_User] FOREIGN KEY ([UserId]) REFERENCES [User] ([Id])
);

CREATE TABLE IF NOT EXISTS [Priority] (
    [Id] int NOT NULL PRIMARY KEY,
    [Name] nvarchar(50) NOT NULL,
    [Order] int NOT NULL,
    [Description] nvarchar(200),
    [CreatedDate] datetime NOT NULL,
    [ModifiedDate] datetime NOT NULL
);

CREATE TABLE IF NOT EXISTS [Role] (
    [Id] integer NOT NULL PRIMARY KEY AUTOINCREMENT,
    [Name] nvarchar(50) NOT NULL,
    [Description] nvarchar(150),
    [CreatedDate] datetime NOT NULL,
    [ModifiedDate] datetime NOT NULL
);

CREATE TABLE IF NOT EXISTS [Status] (
    [Id] integer NOT NULL PRIMARY KEY AUTOINCREMENT,
    [Name] nvarchar(50) NOT NULL,
    [Description] nvarchar(150),
    [Order] int NOT NULL,
    [CreatedDate] datetime NOT NULL,
    [ModifiedDate] datetime NOT NULL
);

CREATE TABLE IF NOT EXISTS [Task] (
    [Id] integer NOT NULL PRIMARY KEY AUTOINCREMENT,
    [StatusId] int NOT NULL,
    [PriorityId] int,
    [CreatedId] int NOT NULL,
    [Summary] nvarchar(255) NOT NULL,
    [Details] nvarchar(2000),
    [StartDate] datetime,
    [DueDate] datetime,
    [CompleteDate] datetime,
    [AssignedId] int,
    [CreatedDate] datetime NOT NULL,
    [ModifiedDate] datetime NOT NULL,
    [LastModifiedBy] nvarchar(50),
    CONSTRAINT [FK_Task_Priority] FOREIGN KEY ([PriorityId]) REFERENCES [Priority] ([Id])
    CONSTRAINT [FK_Task_Status] FOREIGN KEY ([StatusId]) REFERENCES [Status] ([Id])
    CONSTRAINT [FK_Task_User_Assigned] FOREIGN KEY ([AssignedId]) REFERENCES [User] ([Id])
    CONSTRAINT [FK_Task_User_Created] FOREIGN KEY ([CreatedId]) REFERENCES [User] ([Id])
);

CREATE INDEX IF NOT EXISTS [IX_Task]
ON [Task]
([AssignedId], [StatusId]);

CREATE TABLE IF NOT EXISTS [TaskExtended] (
    [TaskId] int NOT NULL PRIMARY KEY,
    [Browser] nvarchar(200),
    [OS] nvarchar(150),
    [CreatedDate] datetime NOT NULL,
    [ModifiedDate] datetime NOT NULL,
    CONSTRAINT [FK_TaskExtended_Task] FOREIGN KEY ([TaskId]) REFERENCES [Task] ([Id])
);

CREATE TABLE IF NOT EXISTS [User] (
    [Id] integer NOT NULL PRIMARY KEY AUTOINCREMENT,
    [EmailAddress] nvarchar(250) NOT NULL,
    [FirstName] nvarchar(200),
    [LastName] nvarchar(200),
    [Avatar] varbinary,
    [CreatedDate] datetime NOT NULL,
    [ModifiedDate] datetime NOT NULL,
    [PasswordHash] char(86) NOT NULL,
    [PasswordSalt] char(5) NOT NULL,
    [Comment] text,
    [IsApproved] bit NOT NULL,
    [LastLoginDate] datetime,
    [LastActivityDate] datetime NOT NULL,
    [LastPasswordChangeDate] datetime,
    [AvatarType] nvarchar(150)
);

CREATE UNIQUE INDEX IF NOT EXISTS [IX_User]
ON [User]
([EmailAddress]);

CREATE TABLE IF NOT EXISTS [UserRole] (
    [UserId] int NOT NULL,
    [RoleId] int NOT NULL,
    CONSTRAINT [PK_UserRole] PRIMARY KEY ([UserId], [RoleId])
    CONSTRAINT [FK_UserRole_Role] FOREIGN KEY ([RoleId]) REFERENCES [Role] ([Id])
    CONSTRAINT [FK_UserRole_User] FOREIGN KEY ([UserId]) REFERENCES [User] ([Id])
);

