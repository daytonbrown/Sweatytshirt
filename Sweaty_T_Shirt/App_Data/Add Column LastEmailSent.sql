USE sweatydb
GO


IF NOT EXISTS (
  SELECT * 
  FROM   sys.columns 
  WHERE  object_id = OBJECT_ID(N'[dbo].[UserProfile]') 
         AND name = 'LastEmailSent'
)
alter table dbo.UserProfile add LastEmailSent DateTime NOT NULL default GetDate();

IF NOT EXISTS (
  SELECT * 
  FROM   sys.columns 
  WHERE  object_id = OBJECT_ID(N'[dbo].[UserProfile]') 
         AND name = 'Notifications'
)
alter table dbo.UserProfile add Notifications INT NULL;

IF NOT EXISTS (
  SELECT * 
  FROM   sys.columns 
  WHERE  object_id = OBJECT_ID(N'[dbo].[Competition]') 
         AND name = 'ImageSrc'
)
alter table dbo.Competition add ImageSrc NVARCHAR(500) NULL;
