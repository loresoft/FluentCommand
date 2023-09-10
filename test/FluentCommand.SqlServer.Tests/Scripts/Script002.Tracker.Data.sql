-- Table [dbo].[Priority] data

MERGE INTO [dbo].[Priority] AS t
USING 
(
    VALUES
    (1, 'High', 'High Priority', 1, 1), 
    (2, 'Normal', 'Normal Priority', 2, 1), 
    (3, 'Low', 'Low Priority', 3, 1)
) 
AS s
([Id], [Name], [Description], [DisplayOrder], [IsActive])
ON (t.[Id] = s.[Id])
WHEN NOT MATCHED BY TARGET THEN 
    INSERT ([Id], [Name], [Description], [DisplayOrder], [IsActive])
    VALUES (s.[Id], s.[Name], s.[Description], s.[DisplayOrder], s.[IsActive])
WHEN MATCHED THEN 
    UPDATE SET t.[Name] = s.[Name], t.[Description] = s.[Description], t.[DisplayOrder] = s.[DisplayOrder], t.[IsActive] = s.[IsActive]
OUTPUT $action as [Action];

-- Table [dbo].[Status] data

MERGE INTO [dbo].[Status] AS t
USING 
(
    VALUES
    (1, 'Not Started', 'Not Starated', 1, 1), 
    (2, 'In Progress', 'In Progress', 2, 1), 
    (3, 'Completed', 'Completed', 3, 1), 
    (4, 'Blocked', 'Blocked', 4, 1), 
    (5, 'Deferred', 'Deferred', 5, 1), 
    (6, 'Done', 'Done', 6, 1)
) 
AS s
([Id], [Name], [Description], [DisplayOrder], [IsActive])
ON (t.[Id] = s.[Id])
WHEN NOT MATCHED BY TARGET THEN 
    INSERT ([Id], [Name], [Description], [DisplayOrder], [IsActive])
    VALUES (s.[Id], s.[Name], s.[Description], s.[DisplayOrder], s.[IsActive])
WHEN MATCHED THEN 
    UPDATE SET t.[Name] = s.[Name], t.[Description] = s.[Description], t.[DisplayOrder] = s.[DisplayOrder], t.[IsActive] = s.[IsActive]
OUTPUT $action as [Action];

-- Table [dbo].[User] data

MERGE INTO [dbo].[User] AS t
USING 
(
    VALUES
    ('83507c95-0744-e811-bd87-f8633fc30ac7', 'william.adama@battlestar.com', 1, 'William Adama'), 
    ('490312a6-0744-e811-bd87-f8633fc30ac7', 'laura.roslin@battlestar.com', 1, 'Laura Roslin'), 
    ('38da04bb-0744-e811-bd87-f8633fc30ac7', 'kara.thrace@battlestar.com', 1, 'Kara Thrace'), 
    ('589d67c6-0744-e811-bd87-f8633fc30ac7', 'lee.adama@battlestar.com', 1, 'Lee Adama'), 
    ('118b84d4-0744-e811-bd87-f8633fc30ac7', 'gaius.baltar@battlestar.com', 1, 'Gaius Baltar'), 
    ('fa7515df-0744-e811-bd87-f8633fc30ac7', 'saul.tigh@battlestar.com', 1, 'Saul Tigh')
) 
AS s
([Id], [EmailAddress], [IsEmailAddressConfirmed], [DisplayName])
ON (t.[Id] = s.[Id])
WHEN NOT MATCHED BY TARGET THEN 
    INSERT ([Id], [EmailAddress], [IsEmailAddressConfirmed], [DisplayName])
    VALUES (s.[Id], s.[EmailAddress], s.[IsEmailAddressConfirmed], s.[DisplayName])
WHEN MATCHED THEN 
    UPDATE SET t.[EmailAddress] = s.[EmailAddress], t.[IsEmailAddressConfirmed] = s.[IsEmailAddressConfirmed], t.[DisplayName] = s.[DisplayName]
OUTPUT $action as [Action];

-- Table [dbo].[Role] data

MERGE INTO [dbo].[Role] AS t
USING 
(
    VALUES
    ('b2d78522-0944-e811-bd87-f8633fc30ac7', 'Administrator', 'Administrator'), 
    ('b3d78522-0944-e811-bd87-f8633fc30ac7', 'Manager', 'Manager'), 
    ('acbffa29-0944-e811-bd87-f8633fc30ac7', 'Member', 'Member')
) 
AS s
([Id], [Name], [Description])
ON (t.[Id] = s.[Id])
WHEN NOT MATCHED BY TARGET THEN 
    INSERT ([Id], [Name], [Description])
    VALUES (s.[Id], s.[Name], s.[Description])
WHEN MATCHED THEN 
    UPDATE SET t.[Name] = s.[Name], t.[Description] = s.[Description]
OUTPUT $action as [Action];

-- Table [dbo].[UserRole] data

MERGE INTO [dbo].[UserRole] AS t
USING 
(
    VALUES
    ('83507c95-0744-e811-bd87-f8633fc30ac7', 'b2d78522-0944-e811-bd87-f8633fc30ac7'), 
    ('490312a6-0744-e811-bd87-f8633fc30ac7', 'b2d78522-0944-e811-bd87-f8633fc30ac7'), 
    ('fa7515df-0744-e811-bd87-f8633fc30ac7', 'b3d78522-0944-e811-bd87-f8633fc30ac7')
) 
AS s
([UserId], [RoleId])
ON (t.[UserId] = s.[UserId] and t.[RoleId] = s.[RoleId])
WHEN NOT MATCHED BY TARGET THEN 
    INSERT ([UserId], [RoleId])
    VALUES (s.[UserId], s.[RoleId])
OUTPUT $action as [Action];

