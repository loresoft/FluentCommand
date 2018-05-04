-- Table [Priority] data
INSERT INTO [Priority] ([Id],[Name],[Order],[Description],[CreatedDate],[ModifiedDate]) VALUES (1,'High',1,'A High Priority','2011-09-08 08:57:02','2011-09-08 08:57:02');
INSERT INTO [Priority] ([Id],[Name],[Order],[Description],[CreatedDate],[ModifiedDate]) VALUES (2,'Normal',2,'A Normal Priority','2011-09-08 08:57:02','2011-09-08 08:57:02');
INSERT INTO [Priority] ([Id],[Name],[Order],[Description],[CreatedDate],[ModifiedDate]) VALUES (3,'Low',3,'A Low Priority','2011-09-08 08:57:02','2011-09-08 08:57:02');

-- Table [Role] data
INSERT INTO [Role] ([Id],[Name],[Description],[CreatedDate],[ModifiedDate]) VALUES (1,'Admin','Admin Role','2011-09-08 08:57:02','2011-09-08 08:57:02');
INSERT INTO [Role] ([Id],[Name],[Description],[CreatedDate],[ModifiedDate]) VALUES (2,'Manager','','2011-09-08 08:57:02','2011-09-08 08:57:02');
INSERT INTO [Role] ([Id],[Name],[Description],[CreatedDate],[ModifiedDate]) VALUES (3,'Newb','','2011-09-08 08:57:02','2011-09-08 08:57:02');
INSERT INTO [Role] ([Id],[Name],[Description],[CreatedDate],[ModifiedDate]) VALUES (4,'Nobody','','2011-09-08 08:57:02','2011-09-08 08:57:02');
INSERT INTO [Role] ([Id],[Name],[Description],[CreatedDate],[ModifiedDate]) VALUES (5,'Power User','','2011-09-08 08:57:02','2011-09-08 08:57:02');

-- Table [Status] data
INSERT INTO [Status] ([Id],[Name],[Description],[Order],[CreatedDate],[ModifiedDate]) VALUES (1,'Not Started','',1,'2011-09-08 08:57:02','2011-09-08 08:57:02');
INSERT INTO [Status] ([Id],[Name],[Description],[Order],[CreatedDate],[ModifiedDate]) VALUES (2,'In Progress','',2,'2011-09-08 08:57:02','2011-09-08 08:57:02');
INSERT INTO [Status] ([Id],[Name],[Description],[Order],[CreatedDate],[ModifiedDate]) VALUES (3,'Completed','',3,'2011-09-08 08:57:02','2011-09-08 08:57:02');
INSERT INTO [Status] ([Id],[Name],[Description],[Order],[CreatedDate],[ModifiedDate]) VALUES (4,'Waiting on someone else','',4,'2011-09-08 08:57:02','2011-09-08 08:57:02');
INSERT INTO [Status] ([Id],[Name],[Description],[Order],[CreatedDate],[ModifiedDate]) VALUES (5,'Deferred','',5,'2011-09-08 08:57:02','2011-09-08 08:57:02');
INSERT INTO [Status] ([Id],[Name],[Description],[Order],[CreatedDate],[ModifiedDate]) VALUES (6,'Done','',6,'2011-09-08 08:57:02','2011-09-08 08:57:02');

-- Table [User] data
INSERT INTO [User] ([Id],[EmailAddress],[FirstName],[LastName],[Avatar],[CreatedDate],[ModifiedDate],[PasswordHash],[PasswordSalt],[Comment],[IsApproved],[LastLoginDate],[LastActivityDate],[LastPasswordChangeDate],[AvatarType]) VALUES (1,'william.adama@battlestar.com','William','Adama',NULL,'2009-05-06 17:46:20.597','2009-05-06 17:46:20.597','1+v5rvSXnyX7tvwTKfM+aq+s0hDmNXsduGZfq8sQv1ggPnGlQdDdBdbUP0bUmbMbiU40PvRQWKRAy5QUd1xrAA','?#nkY','Data Merge 635324524904242477','1','2014-04-07 07:26:54','2009-05-06 17:46:20',NULL,NULL);
INSERT INTO [User] ([Id],[EmailAddress],[FirstName],[LastName],[Avatar],[CreatedDate],[ModifiedDate],[PasswordHash],[PasswordSalt],[Comment],[IsApproved],[LastLoginDate],[LastActivityDate],[LastPasswordChangeDate],[AvatarType]) VALUES (2,'laura.roslin@battlestar.com','Laura','Roslin',NULL,'2009-05-06 17:47:00.330','2009-05-06 17:47:00.330','Sx/jwRHFW/CQpO0E6G8d+jo344AmAKfX+C48a0iAZyMrb4sE8VoDuyZorbhbLZAX9f4UZk67O7eCjk854OrYSg','Ph)6;','Data Merge 635324524904242477','1','2014-04-07 07:26:54','2009-05-06 17:47:00',NULL,NULL);
INSERT INTO [User] ([Id],[EmailAddress],[FirstName],[LastName],[Avatar],[CreatedDate],[ModifiedDate],[PasswordHash],[PasswordSalt],[Comment],[IsApproved],[LastLoginDate],[LastActivityDate],[LastPasswordChangeDate],[AvatarType]) VALUES (3,'kara.thrace@battlestar.com','Kara','Thrace',NULL,'2009-05-06 17:47:43.417','2009-05-06 17:47:43.417','5KhtS4ax7G1aGkq97w02ooVZMmJp8bcySEKMSxruXu/Z/wRKoNAxMlkjRVY1yLavrC3GRYQZjy5b6JW8cR5EWg','!]@2/','Data Merge 635324524147981355','1','2014-04-07 07:26:54','2009-05-06 17:47:43',NULL,NULL);
INSERT INTO [User] ([Id],[EmailAddress],[FirstName],[LastName],[Avatar],[CreatedDate],[ModifiedDate],[PasswordHash],[PasswordSalt],[Comment],[IsApproved],[LastLoginDate],[LastActivityDate],[LastPasswordChangeDate],[AvatarType]) VALUES (4,'lee.adama@battlestar.com','Lee','Adama',NULL,'2009-05-06 17:48:02.367','2009-05-06 17:48:02.367','IrK8OhI2n4Ev3YA4y5kP7vy+n2CffX2MgcONbAh6/kZpNZYBYoYyrMhqdYztehL0NAIdvcInQ6zOjMplq+zWQA','e@_a{','Data Merge 635324524147981355','1','2014-04-07 07:26:54','2009-05-06 17:48:02',NULL,NULL);
INSERT INTO [User] ([Id],[EmailAddress],[FirstName],[LastName],[Avatar],[CreatedDate],[ModifiedDate],[PasswordHash],[PasswordSalt],[Comment],[IsApproved],[LastLoginDate],[LastActivityDate],[LastPasswordChangeDate],[AvatarType]) VALUES (5,'gaius.baltar@battlestar.com','Gaius','Baltar',NULL,'2009-05-06 17:48:26.273','2009-05-06 17:48:26.273','7tfajMaEerDNVgi6Oi6UJ6JxsUXZ0u4zQeUrZQxnaXJQ2f2vd9AzBR4sVOaH7LZtCjQopMzlQ38QqNEnpK0B/g','_qpA2','Data Merge 635324524147981355','1','2014-04-07 07:26:54','2009-05-06 17:48:26',NULL,NULL);
INSERT INTO [User] ([Id],[EmailAddress],[FirstName],[LastName],[Avatar],[CreatedDate],[ModifiedDate],[PasswordHash],[PasswordSalt],[Comment],[IsApproved],[LastLoginDate],[LastActivityDate],[LastPasswordChangeDate],[AvatarType]) VALUES (6,'saul.tigh@battlestar.com','Saul','Tigh',NULL,'2009-05-06 17:49:26.023','2009-05-06 17:49:26.023','wzkR89zRXe7hND1jqAP9xgupYJBvEZcjsfUe3TaU45kxRajjjS9u0fOTLK+uglzk67EGochJdeoikWs7hxMNRA','h]-zG','Data Merge 635324524147981355','1','2014-04-07 07:26:54','2009-05-06 17:49:26',NULL,NULL);

-- Table [Task] data
INSERT INTO [Task] ([Id],[StatusId],[PriorityId],[CreatedId],[Summary],[Details],[StartDate],[DueDate],[CompleteDate],[AssignedId],[CreatedDate],[ModifiedDate],[LastModifiedBy]) VALUES (1,1,1,2,'Make it to Earth','Find and make it to earth while avoiding the cylons.',NULL,NULL,NULL,1,'2009-12-18 05:01:58','2009-12-18 05:01:58','laura.roslin@battlestar.com');

