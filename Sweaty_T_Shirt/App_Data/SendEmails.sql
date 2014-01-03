use SweatyTShirt
GO

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[SendEmails]') AND type in (N'P', N'PC'))
    DROP PROCEDURE [dbo].SendEmails
GO

CREATE PROCEDURE dbo.SendEmails AS
/*  
exec dbo.SendEmails

*/

DECLARE @now DATETIME;
SET @now = GETDATE();

select sw.Description,
sw.Amount,
upSW.Email AS SweatyTShirtEmailAddress,
upSW.FullName AS SweatyTShirtFullName,
up.Email AS RecipientEmailAddress,
up.FullName AS RecipientFullName,
up.UserId AS RecipientUserID
from dbo.SweatyTShirt sw
inner join dbo.UserProfile upSW ON sw.UserID = upSW.UserId
inner join dbo.UserInCompetition uic ON sw.CompetitionID = uic.CompetitionID
inner join dbo.Competition c on sw.CompetitionID = c.CompetitionID
inner join dbo.UserProfile up ON uic.UserID = up.UserId
where c.IsActive = 1
and uic.IsActive = 1
AND LEN(up.Email) > 0
AND sw.CreatedDate > up.LastEmailSent
and (
	ISNULL(up.Notifications,0)  = 0
	OR 
	@now >= DATEADD(hour, up.Notifications, up.LastEmailSent)
	);
GO
 

