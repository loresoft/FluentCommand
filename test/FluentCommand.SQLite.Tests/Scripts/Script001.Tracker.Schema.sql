-- Tables
CREATE TABLE IF NOT EXISTS "Audit" (
    "Id" integer NOT NULL PRIMARY KEY AUTOINCREMENT NOT NULL,
    "Date" datetime NOT NULL,
    "UserId" int NULL,
    "TaskId" int NULL,
    "Content" text NOT NULL,
    "Username" nvarchar(50) NOT NULL,
    "Created" datetimeoffset NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "CreatedBy" nvarchar(100) NULL,
    "Updated" datetimeoffset NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "UpdatedBy" nvarchar(100) NULL,
    "RowVersion" varbinary(8) NULL
);

CREATE TABLE IF NOT EXISTS "Priority" (
    "Id" int NOT NULL,
    "Name" nvarchar(100) NOT NULL,
    "Description" nvarchar(255) NULL,
    "DisplayOrder" int NOT NULL DEFAULT (0),
    "IsActive" bit NOT NULL DEFAULT (1),
    "Created" datetimeoffset NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "CreatedBy" nvarchar(100) NULL,
    "Updated" datetimeoffset NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "UpdatedBy" nvarchar(100) NULL,
    "RowVersion" varbinary(8) NULL,
    CONSTRAINT "PK_Priority" PRIMARY KEY ("Id")
);

CREATE TABLE IF NOT EXISTS "Role" (
    "Id" uniqueidentifier NOT NULL,
    "Name" nvarchar(256) NOT NULL,
    "Description" text NULL,
    "Created" datetimeoffset NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "CreatedBy" nvarchar(100) NULL,
    "Updated" datetimeoffset NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "UpdatedBy" nvarchar(100) NULL,
    "RowVersion" varbinary(8) NULL,
    CONSTRAINT "PK_Role" PRIMARY KEY ("Id")
);

CREATE TABLE IF NOT EXISTS "Status" (
    "Id" int NOT NULL,
    "Name" nvarchar(100) NOT NULL,
    "Description" nvarchar(255) NULL,
    "DisplayOrder" int NOT NULL DEFAULT (0),
    "IsActive" bit NOT NULL DEFAULT (1),
    "Created" datetimeoffset NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "CreatedBy" nvarchar(100) NULL,
    "Updated" datetimeoffset NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "UpdatedBy" nvarchar(100) NULL,
    "RowVersion" varbinary(8) NULL,
    CONSTRAINT "PK_Status" PRIMARY KEY ("Id")
);

CREATE TABLE IF NOT EXISTS "Task" (
    "Id" uniqueidentifier NOT NULL,
    "StatusId" int NOT NULL,
    "PriorityId" int NULL,
    "Title" nvarchar(255) NOT NULL,
    "Description" text NULL,
    "StartDate" datetimeoffset NULL,
    "DueDate" datetimeoffset NULL,
    "CompleteDate" datetimeoffset NULL,
    "AssignedId" uniqueidentifier NULL,
    "Created" datetimeoffset NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "CreatedBy" nvarchar(100) NULL,
    "Updated" datetimeoffset NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "UpdatedBy" nvarchar(100) NULL,
    "RowVersion" varbinary(8) NULL,
    CONSTRAINT "PK_Task" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_Task_Priority_PriorityId" FOREIGN KEY ("PriorityId") REFERENCES "Priority" ("Id"),
    CONSTRAINT "FK_Task_Status_StatusId" FOREIGN KEY ("StatusId") REFERENCES "Status" ("Id"),
    CONSTRAINT "FK_Task_User_AssignedId" FOREIGN KEY ("AssignedId") REFERENCES "User" ("Id")
);

CREATE TABLE IF NOT EXISTS "TaskExtended" (
    "TaskId" uniqueidentifier NOT NULL,
    "UserAgent" text NULL,
    "Browser" nvarchar(256) NULL,
    "OperatingSystem" nvarchar(256) NULL,
    "Created" datetimeoffset NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "CreatedBy" nvarchar(100) NULL,
    "Updated" datetimeoffset NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "UpdatedBy" nvarchar(100) NULL,
    "RowVersion" varbinary(8) NULL,
    CONSTRAINT "PK_TaskExtended" PRIMARY KEY ("TaskId"),
    CONSTRAINT "FK_TaskExtended_Task_TaskId" FOREIGN KEY ("TaskId") REFERENCES "Task" ("Id")
);

CREATE TABLE IF NOT EXISTS "User" (
    "Id" uniqueidentifier NOT NULL,
    "EmailAddress" nvarchar(256) NOT NULL,
    "IsEmailAddressConfirmed" bit NOT NULL DEFAULT (0),
    "DisplayName" nvarchar(256) NOT NULL,
    "PasswordHash" text NULL,
    "ResetHash" text NULL,
    "InviteHash" text NULL,
    "AccessFailedCount" int NOT NULL DEFAULT (0),
    "LockoutEnabled" bit NOT NULL DEFAULT (0),
    "LockoutEnd" datetimeoffset NULL,
    "LastLogin" datetimeoffset NULL,
    "IsDeleted" bit NOT NULL DEFAULT (0),
    "Created" datetimeoffset NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "CreatedBy" nvarchar(100) NULL,
    "Updated" datetimeoffset NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "UpdatedBy" nvarchar(100) NULL,
    "RowVersion" varbinary(8) NULL,
    CONSTRAINT "PK_User" PRIMARY KEY ("Id")
);

CREATE TABLE IF NOT EXISTS "UserLogin" (
    "Id" uniqueidentifier NOT NULL,
    "EmailAddress" nvarchar(256) NOT NULL,
    "UserId" uniqueidentifier NULL,
    "UserAgent" text NULL,
    "Browser" nvarchar(256) NULL,
    "OperatingSystem" nvarchar(256) NULL,
    "DeviceFamily" nvarchar(256) NULL,
    "DeviceBrand" nvarchar(256) NULL,
    "DeviceModel" nvarchar(256) NULL,
    "IpAddress" nvarchar(50) NULL,
    "IsSuccessful" bit NOT NULL DEFAULT (0),
    "FailureMessage" nvarchar(256) NULL,
    "Created" datetimeoffset NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "CreatedBy" nvarchar(100) NULL,
    "Updated" datetimeoffset NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "UpdatedBy" nvarchar(100) NULL,
    "RowVersion" varbinary(8) NULL,
    CONSTRAINT "PK_UserLogin" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_UserLogin_User_UserId" FOREIGN KEY ("UserId") REFERENCES "User" ("Id")
);

CREATE TABLE IF NOT EXISTS "UserRole" (
    "UserId" uniqueidentifier NOT NULL,
    "RoleId" uniqueidentifier NOT NULL,
    CONSTRAINT "PK_UserRole" PRIMARY KEY ("UserId", "RoleId"),
    CONSTRAINT "FK_UserRole_Role_RoleId" FOREIGN KEY ("RoleId") REFERENCES "Role" ("Id"),
    CONSTRAINT "FK_UserRole_User_UserId" FOREIGN KEY ("UserId") REFERENCES "User" ("Id")
);

-- Indexes
CREATE UNIQUE INDEX IF NOT EXISTS "UX_Role_Name"
ON "Role" ("Name");

CREATE INDEX IF NOT EXISTS "IX_Task_AssignedId"
ON "Task" ("AssignedId");

CREATE INDEX IF NOT EXISTS "IX_Task_PriorityId"
ON "Task" ("PriorityId");

CREATE INDEX IF NOT EXISTS "IX_Task_StatusId"
ON "Task" ("StatusId");

CREATE UNIQUE INDEX IF NOT EXISTS "UX_User_EmailAddress"
ON "User" ("EmailAddress");

CREATE INDEX IF NOT EXISTS "IX_UserLogin_EmailAddress"
ON "UserLogin" ("EmailAddress");

CREATE INDEX IF NOT EXISTS "IX_UserLogin_UserId"
ON "UserLogin" ("UserId");


