using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using Sweaty_T_Shirt.Models;
using WebMatrix.WebData;
using System.Web.Security;

namespace Sweaty_T_Shirt.DAL
{
    public class AccountRepository : BaseRepository
    {
        public AccountRepository() { }

        public AccountRepository(SweatyTShirtContext context):base(context){}
        /// <summary>
        /// No password, user just enters his user name and email address to register.
        /// </summary>
        public const string AllUsersPassword = "AllUsersPassword";

        public const string AdminRole = "Admin";
        public const string UserRole = "User";
        public const string UserOrAdminRole = "User,Admin";

        private const string DeleteUserSqlCommand = @"DELETE FROM dbo.UserInCompetition WHERE UserID = {0};"
            + "DELETE FROM dbo.SweatyTShirt WHERE UserID = {0};"
            + "UPDATE dbo.Competition SET CreatorUserID = {1} WHERE CreatorUserID = {0};"
            + "DELETE FROM dbo.webpages_UsersInRoles WHERE UserID = {0};"
            + "DELETE FROM dbo.webpages_Membership WHERE UserID = {0};"
            + "DELETE FROM dbo.webpages_OAuthMembership WHERE UserID = {0};"
            + "DELETE FROM dbo.UserProfile WHERE UserID = {0};";

        public void DeleteUser(int userID, int adminUserID)
        {
            _context.Database.ExecuteSqlCommand(DeleteUserSqlCommand, userID, adminUserID);
        }

        public string CreateUser(string fullName, string email)
        {
            string userName = Guid.NewGuid().ToString();
            WebSecurity.CreateUserAndAccount(userName, AllUsersPassword);

            var userProfile = _context.UserProfiles.FirstOrDefault(o => o.UserName == userName);
            if (userProfile == null)
            {
                throw new ApplicationException(string.Format("Unable to retrieve user profile for newly-created user {0}, email {1}, full name {2}", userName, email, fullName));
            }
            userProfile.Email = email;
            userProfile.FullName = fullName;
            _context.SaveChanges();
            return userName;
        }

        public UserProfile GetUserProfile(string userName)
        {
            return _context.UserProfiles.FirstOrDefault(o => o.UserName == userName);
        }

        internal List<UserProfile> GetUserProfiles()
        {
            return _context.UserProfiles.OrderBy(o => o.FullName).ToList();
        }

        internal UserProfile GetUserProfile(int userID)
        {
            return _context.UserProfiles
                .Include(o => o.SweatyTShirts)
                .Include("SweatyTShirts.Competition")
                .Include(o => o.UserInCompetitions)
                .Include("UserInCompetitions.Competition")
                .Include(o => o.Competitions)
                .Single(o => o.UserId == userID);
        }

        internal UserProfile UpdateUserNotifications(UserProfile userProfile)
        {
            var dbUserProfile = _context.UserProfiles.Single(o => o.UserId == userProfile.UserId);
            dbUserProfile.Notifications = userProfile.IsImmediateNotification ? null : userProfile.Notifications;
            _context.SaveChanges();
            return dbUserProfile;
        }

        internal void UpdateLastEmailSent(int userID)
        {
            var dbUserProfile = _context.UserProfiles.Single(o => o.UserId == userID);
            dbUserProfile.LastEmailSent = DateTime.Now;
            _context.SaveChanges();
        }

        /// <summary>
        /// None of the collections are edited.
        /// </summary>
        /// <param name="userProfile"></param>
        /// <returns></returns>
        internal int EditUserProfile(UserProfile userProfile)
        {
            var roles = (SimpleRoleProvider)Roles.Provider;
            var membership = (SimpleMembershipProvider)Membership.Provider;
            UserProfile dbUserProfile = null;

            if (userProfile.UserId == 0)
            {
                string userName = Guid.NewGuid().ToString();
                Dictionary<string, object> values = new Dictionary<string, object>();
                values.Add("Email", userProfile.Email);
                values.Add("LastEmailSent", DateTime.Now.AddDays(-365));
                membership.CreateUserAndAccount(userName, AccountRepository.AllUsersPassword, values);
                if (userProfile.UserRoles.Where(o => o.Selected).Count() > 0)
                {
                    roles.AddUsersToRoles(new[] { userName }, userProfile.UserRoles.Where(o => o.Selected).Select(o => o.Value).ToArray());
                }
                dbUserProfile = _context.UserProfiles.FirstOrDefault(o => o.Email == userProfile.Email);
            }
            else
            {
                dbUserProfile = _context.UserProfiles.Single(o => o.UserId == userProfile.UserId);
                dbUserProfile.Email = userProfile.Email;

                string[] allRoles = Roles.GetAllRoles();
                string[] allRolesUserShouldBeIn = userProfile.UserRoles.Where(o => o.Selected).Select(o => o.Value).ToArray();

                foreach (string role in allRolesUserShouldBeIn)
                {
                    if (!allRoles.Contains(role))
                    {
                        roles.CreateRole(role);
                    }
                    if (!roles.IsUserInRole(userProfile.UserName, role))
                    {
                        roles.AddUsersToRoles(new[] { userProfile.UserName }, new[] { role });
                    }
                }

                string[] allRolesForUser = Roles.GetRolesForUser(userProfile.UserName);
                foreach (string role in allRolesForUser)
                {
                    if (!allRolesUserShouldBeIn.Contains(role))
                    {
                        Roles.RemoveUserFromRole(userProfile.UserName, role);
                    }
                }
            }

            dbUserProfile.FullName = userProfile.FullName;
            _context.SaveChanges();
            return dbUserProfile.UserId;
        }
    }
}