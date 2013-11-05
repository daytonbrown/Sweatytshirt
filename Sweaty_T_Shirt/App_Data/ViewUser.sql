declare @UserID BigInt;
SET @UserID = 8;

select up.UserName, up.Email, up.FullName, wm.IsConfirmed, wom.Provider, wom.ProviderUserId, wr.RoleName
from dbo.userProfile up 
left outer join dbo.webpages_Membership wm on up.UserId = wm.UserId
left outer join dbo.webpages_OAuthMembership wom on up.UserId = wom.UserId
left outer join dbo.webpages_UsersInRoles wur on up.UserId = wur.UserId
left outer join dbo.webpages_Roles wr on wur.RoleId = wr.RoleId
where up.UserID = @UserID
/*
select * from dbo.webpages_Membership  where UserID = 8 = @UserID

select * from dbo.webpages_OAuthMembership  where UserID = @UserID

select * from dbo.webpages_UsersInRoles where UserID = @UserID
*/

