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

/* Table [dbo].[DataType] data */

MERGE INTO [dbo].[DataType] AS t
USING
(
    VALUES
    (1, N'Testing', 1, 123, 132123, 123.12, 123.12, 123.1000, '2024-01-30 00:00:00.000', '2024-01-30 00:00:00.000000-06:00', '744f7775-5369-4809-994b-62abfae17724', '11:30:15', '2024-01-30 00:00:00.000', '13:30:15', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL),
    (2, N'Full', 0, 222, 2353, 563.23, 456.56, 2552.1000, '2024-02-15 00:00:00.000', '2024-02-15 00:00:00.000000-06:00', 'cca7e4a4-446f-4f24-9c3a-9d2a5b547877', '15:15:30', '2024-02-06 00:00:00.000', '01:23:11', 0, 333, 454, 563.23, 456.56, 2552.0130, '2024-02-15 00:00:00.000', '2024-02-15 00:00:00.000000-06:00', 'cca7e4a4-446f-4f24-9c3a-9d2a5b547877', '15:15:30', '2024-02-06 00:00:00.000', '01:23:11')
)
AS s
([Id], [Name], [Boolean], [Short], [Long], [Float], [Double], [Decimal], [DateTime], [DateTimeOffset], [Guid], [TimeSpan], [DateOnly], [TimeOnly], [BooleanNull], [ShortNull], [LongNull], [FloatNull], [DoubleNull], [DecimalNull], [DateTimeNull], [DateTimeOffsetNull], [GuidNull], [TimeSpanNull], [DateOnlyNull], [TimeOnlyNull])
ON (t.[Id] = s.[Id])
WHEN NOT MATCHED BY TARGET THEN
    INSERT ([Id], [Name], [Boolean], [Short], [Long], [Float], [Double], [Decimal], [DateTime], [DateTimeOffset], [Guid], [TimeSpan], [DateOnly], [TimeOnly], [BooleanNull], [ShortNull], [LongNull], [FloatNull], [DoubleNull], [DecimalNull], [DateTimeNull], [DateTimeOffsetNull], [GuidNull], [TimeSpanNull], [DateOnlyNull], [TimeOnlyNull])
    VALUES (s.[Id], s.[Name], s.[Boolean], s.[Short], s.[Long], s.[Float], s.[Double], s.[Decimal], s.[DateTime], s.[DateTimeOffset], s.[Guid], s.[TimeSpan], s.[DateOnly], s.[TimeOnly], s.[BooleanNull], s.[ShortNull], s.[LongNull], s.[FloatNull], s.[DoubleNull], s.[DecimalNull], s.[DateTimeNull], s.[DateTimeOffsetNull], s.[GuidNull], s.[TimeSpanNull], s.[DateOnlyNull], s.[TimeOnlyNull])
WHEN MATCHED THEN
    UPDATE SET t.[Name] = s.[Name], t.[Boolean] = s.[Boolean], t.[Short] = s.[Short], t.[Long] = s.[Long], t.[Float] = s.[Float], t.[Double] = s.[Double], t.[Decimal] = s.[Decimal], t.[DateTime] = s.[DateTime], t.[DateTimeOffset] = s.[DateTimeOffset], t.[Guid] = s.[Guid], t.[TimeSpan] = s.[TimeSpan], t.[DateOnly] = s.[DateOnly], t.[TimeOnly] = s.[TimeOnly], t.[BooleanNull] = s.[BooleanNull], t.[ShortNull] = s.[ShortNull], t.[LongNull] = s.[LongNull], t.[FloatNull] = s.[FloatNull], t.[DoubleNull] = s.[DoubleNull], t.[DecimalNull] = s.[DecimalNull], t.[DateTimeNull] = s.[DateTimeNull], t.[DateTimeOffsetNull] = s.[DateTimeOffsetNull], t.[GuidNull] = s.[GuidNull], t.[TimeSpanNull] = s.[TimeSpanNull], t.[DateOnlyNull] = s.[DateOnlyNull], t.[TimeOnlyNull] = s.[TimeOnlyNull]
OUTPUT $action as MergeAction;

/* Table [dbo].[Table1 $ Test] data */

MERGE INTO [dbo].[Table1 $ Test] AS t
USING
(
    VALUES
    (N'testing   ', N'value     ', 123, 456, N'123 Main', N'City, MN', N'55555')
)
AS s
([Test$], [Blah #], [Table Example ID], [TableExampleObject], [1stNumber], [123Street], [123 Test 123])
ON (t.[Test$] = s.[Test$])
WHEN NOT MATCHED BY TARGET THEN
    INSERT ([Test$], [Blah #], [Table Example ID], [TableExampleObject], [1stNumber], [123Street], [123 Test 123])
    VALUES (s.[Test$], s.[Blah #], s.[Table Example ID], s.[TableExampleObject], s.[1stNumber], s.[123Street], s.[123 Test 123])
WHEN MATCHED THEN
    UPDATE SET t.[Blah #] = s.[Blah #], t.[Table Example ID] = s.[Table Example ID], t.[TableExampleObject] = s.[TableExampleObject], t.[1stNumber] = s.[1stNumber], t.[123Street] = s.[123Street], t.[123 Test 123] = s.[123 Test 123]
OUTPUT $action as MergeAction;

