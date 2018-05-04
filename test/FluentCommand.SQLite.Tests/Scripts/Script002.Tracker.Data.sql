-- Table "Priority" data
REPLACE INTO Priority("Id", "Name", "Description", "DisplayOrder", "IsActive") 
VALUES (1, 'High', 'High Priority', 1, 1);

REPLACE INTO Priority("Id", "Name", "Description", "DisplayOrder", "IsActive") 
VALUES (2, 'Normal', 'Normal Priority', 2, 1);

REPLACE INTO Priority("Id", "Name", "Description", "DisplayOrder", "IsActive") 
VALUES (3, 'Low', 'Low Priority', 3, 1);

-- Table "Role" data
REPLACE INTO Role("Id", "Name", "Description") 
VALUES ('b2d78522-0944-e811-bd87-f8633fc30ac7', 'Administrator', 'Administrator');

REPLACE INTO Role("Id", "Name", "Description") 
VALUES ('b3d78522-0944-e811-bd87-f8633fc30ac7', 'Manager', 'Manager');

REPLACE INTO Role("Id", "Name", "Description") 
VALUES ('acbffa29-0944-e811-bd87-f8633fc30ac7', 'Member', 'Member');

-- Table "Status" data
REPLACE INTO Status("Id", "Name", "Description", "DisplayOrder", "IsActive") 
VALUES (1, 'Not Started', 'Not Starated', 1, 1);

REPLACE INTO Status("Id", "Name", "Description", "DisplayOrder", "IsActive") 
VALUES (2, 'In Progress', 'In Progress', 2, 1);

REPLACE INTO Status("Id", "Name", "Description", "DisplayOrder", "IsActive") 
VALUES (3, 'Completed', 'Completed', 3, 1);

REPLACE INTO Status("Id", "Name", "Description", "DisplayOrder", "IsActive") 
VALUES (4, 'Blocked', 'Blocked', 4, 1);

REPLACE INTO Status("Id", "Name", "Description", "DisplayOrder", "IsActive") 
VALUES (5, 'Deferred', 'Deferred', 5, 1);

REPLACE INTO Status("Id", "Name", "Description", "DisplayOrder", "IsActive") 
VALUES (6, 'Done', 'Done', 6, 1);

-- Table "User" data
REPLACE INTO User("Id", "EmailAddress", "IsEmailAddressConfirmed", "DisplayName", "PasswordHash", "ResetHash", "InviteHash", "AccessFailedCount", "LockoutEnabled", "LockoutEnd", "LastLogin", "IsDeleted") 
VALUES ('83507c95-0744-e811-bd87-f8633fc30ac7', 'william.adama@battlestar.com', 1, 'William Adama', NULL, NULL, NULL, 0, 0, NULL, NULL, 0);

REPLACE INTO User("Id", "EmailAddress", "IsEmailAddressConfirmed", "DisplayName", "PasswordHash", "ResetHash", "InviteHash", "AccessFailedCount", "LockoutEnabled", "LockoutEnd", "LastLogin", "IsDeleted") 
VALUES ('490312a6-0744-e811-bd87-f8633fc30ac7', 'laura.roslin@battlestar.com', 1, 'Laura Roslin', NULL, NULL, NULL, 0, 0, NULL, NULL, 0);

REPLACE INTO User("Id", "EmailAddress", "IsEmailAddressConfirmed", "DisplayName", "PasswordHash", "ResetHash", "InviteHash", "AccessFailedCount", "LockoutEnabled", "LockoutEnd", "LastLogin", "IsDeleted") 
VALUES ('38da04bb-0744-e811-bd87-f8633fc30ac7', 'kara.thrace@battlestar.com', 1, 'Kara Thrace', NULL, NULL, NULL, 0, 0, NULL, NULL, 0);

REPLACE INTO User("Id", "EmailAddress", "IsEmailAddressConfirmed", "DisplayName", "PasswordHash", "ResetHash", "InviteHash", "AccessFailedCount", "LockoutEnabled", "LockoutEnd", "LastLogin", "IsDeleted") 
VALUES ('589d67c6-0744-e811-bd87-f8633fc30ac7', 'lee.adama@battlestar.com', 1, 'Lee Adama', NULL, NULL, NULL, 0, 0, NULL, NULL, 0);

REPLACE INTO User("Id", "EmailAddress", "IsEmailAddressConfirmed", "DisplayName", "PasswordHash", "ResetHash", "InviteHash", "AccessFailedCount", "LockoutEnabled", "LockoutEnd", "LastLogin", "IsDeleted") 
VALUES ('118b84d4-0744-e811-bd87-f8633fc30ac7', 'gaius.baltar@battlestar.com', 1, 'Gaius Baltar', NULL, NULL, NULL, 0, 0, NULL, NULL, 0);

REPLACE INTO User("Id", "EmailAddress", "IsEmailAddressConfirmed", "DisplayName", "PasswordHash", "ResetHash", "InviteHash", "AccessFailedCount", "LockoutEnabled", "LockoutEnd", "LastLogin", "IsDeleted") 
VALUES ('fa7515df-0744-e811-bd87-f8633fc30ac7', 'saul.tigh@battlestar.com', 1, 'Saul Tigh', NULL, NULL, NULL, 0, 0, NULL, NULL, 0);

