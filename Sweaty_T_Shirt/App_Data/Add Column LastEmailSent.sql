USE sweatydb
GO


IF NOT EXISTS (
  SELECT * 
  FROM   sys.columns 
  WHERE  object_id = OBJECT_ID(N'[dbo].[UserProfile]') 
         AND name = 'LastEmailSent'
)
alter table dbo.UserProfile add LastEmailSent DateTime NOT NULL default GetDate()
