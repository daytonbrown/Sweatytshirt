declare @UserID BigInt;
SET @UserID = 8;

delete from dbo.webpages_UsersInRoles where UserID = @UserID;
delete from dbo.userProfile where UserID = @UserID;
delete from dbo.webpages_OAuthMembership where UserID = @UserID;
delete from dbo.webpages_Membership where UserID = @UserID;




