using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Sweaty_T_Shirt.Models;
using System.Data.Entity;
using System.Diagnostics;
using WebMatrix.WebData;
using System.Web.Security;
using Sweaty_T_Shirt.Controllers;
//http://www.asp.net/mvc/tutorials/getting-started-with-ef-using-mvc/creating-an-entity-framework-data-model-for-an-asp-net-mvc-application
//Scott Gnu's blog on poco:
//http://weblogs.asp.net/scottgu/archive/2010/07/16/code-first-development-with-entity-framework-4.aspx

namespace Sweaty_T_Shirt.DAL
{
    /// <summary>
    /// TODO once the database is final this initializer must be removed from the application.
    /// </summary>
    public class SweatyTShirtInitializer : DropCreateDatabaseIfModelChanges<SweatyTShirtContext>
    {
        public const string AdminUserName = "Administrator";
        public const string AdminEmail = "RoyalOakBikes@hotmail.com";
        public const string FullName1 = "Tom Regan Hotmail";
        public const string User1Email = "tregan3@hotmail.com";
        public const string FullName2 = "Tom Regan Gmail";
        public const string User2Email = "regan.tom@gmail.com";
        public const string FullName3 = "Dayton Brown Gmail";
        public const string User3Email = "daytonbrown2@gmail.com";
        public const string FullName4 = "Dayton Brown";
        public const string User4Email = "dayton.brown@daytongroupinc.com";

        private UserProfile CreateUser(string email, 
            string fullName,
            SimpleMembershipProvider membership, 
            SimpleRoleProvider roles, 
            SweatyTShirtContext context)
        {
            string userName = Guid.NewGuid().ToString();
            Dictionary<string, object> values = new Dictionary<string, object>();
            values.Add("Email", email);
            values.Add("LastEmailSent", DateTime.Now.AddDays(-365));
            membership.CreateUserAndAccount(userName, AccountRepository.AllUsersPassword, values);
            roles.AddUsersToRoles(new[] { userName }, new[] { AccountRepository.UserRole });
            UserProfile userProfile =  context.UserProfiles.FirstOrDefault(o => o.Email == email);
            userProfile.FullName = fullName;
            context.SaveChanges();
            return userProfile;
        }

        private Competition CreateCompetition(SweatyTShirtContext context, 
            int creatorUserID,
            string name,
            string description,
            int? points,
            DateTime? endDate)
        {
            Competition competition = new Competition()
            {
                CreatedDate = DateTime.Now,
                CreatorUserID = creatorUserID,
                Description = description,
                Points = points,
                EndDate = endDate,
                IsActive = true,
                Name = name
            };

            context.Competitions.Add(competition);
            context.SaveChanges();
            return competition;
        }

        private void AddUserToCompetition(SweatyTShirtContext context,
            UserProfile userProfile,
            Competition competition)
        {
            context.UserInCompetitions.Add(new UserInCompetition()
                {
                    CompetitionID = competition.CompetitionID,
                    DateAccepted = null,
                    DateInvited = DateTime.Now,
                    IsActive = true,
                    UserID = userProfile.UserId,
                    Email = userProfile.Email,
                    FullName = userProfile.FullName
                }
            );
            context.SaveChanges();
        }

        private void AddSweatyTShirt(SweatyTShirtContext context,
            Competition competition,
            UserProfile userProfile,
            string description,
            int amount)
        {
            context.SweatyTShirts.Add(
                new SweatyTShirt()
                {
                    Amount = amount,
                    CompetitionID = competition.CompetitionID,
                    CreatedDate = DateTime.Now,
                    Description = description,
                    IsFavorite = true,
                    UserID = userProfile.UserId
                }
                );
            context.SaveChanges();
        }

        protected override void Seed(SweatyTShirtContext context)
        {
            WebSecurity.InitializeDatabaseConnection("SweatyTShirtContext", "UserProfile", "UserId", "UserName", autoCreateTables: true);
            var roles = (SimpleRoleProvider)Roles.Provider;
            var membership = (SimpleMembershipProvider)Membership.Provider;

            #region create administrator
            roles.CreateRole(AccountRepository.AdminRole);
            Dictionary<string, object> values = new Dictionary<string, object>();
            values.Add("Email", AdminEmail);
            values.Add("LastEmailSent", DateTime.Now.AddDays(-365));
            membership.CreateUserAndAccount(AdminUserName, AccountRepository.AllUsersPassword, values);
            roles.AddUsersToRoles(new[] { AdminUserName }, new[] { AccountRepository.AdminRole });
            #endregion

            #region create 4 users
            roles.CreateRole(AccountRepository.UserRole);
            UserProfile user1 = CreateUser(User1Email, FullName1, membership, roles, context);
            UserProfile user2 = CreateUser(User2Email, FullName2, membership, roles, context);
            UserProfile user3 = CreateUser(User3Email, FullName3, membership, roles, context);
            UserProfile user4 = CreateUser(User4Email, FullName4, membership, roles, context);

            roles.AddUsersToRoles(new[] { user1.UserName,user4.UserName }, new[] { AccountRepository.AdminRole });
            #endregion

            #region create some competitions
            Competition competition = CreateCompetition(
                context,
                user1.UserId,
                "Iron Man",
                "Winner gets $10 from everyone",
                25,
                null);

            Competition competition2 = CreateCompetition(
                context,
                user1.UserId,
                "Iron Man 2",
                "Winner gets $20 from everyone",
                50,
                new DateTime(2013,11,1));

            Competition competition3 = CreateCompetition(
                context,
                user2.UserId,
                "Iron Man 3",
                "Winner gets $10 from everyone",
                25,
                null);

            Competition competition4 = CreateCompetition(
                context,
                user2.UserId,
                "Iron Man 4",
                "Winner gets $20 from everyone",
                50,
                new DateTime(2013, 12, 1));

            Competition competition5 = CreateCompetition(
                context,
                user3.UserId,
                "Iron Man 5",
                "Winner gets $10 from everyone",
                25,
                null);

            Competition competition6 = CreateCompetition(
                context,
                user3.UserId,
                "Iron Man 6",
                "Winner gets $20 from everyone",
                50,
                new DateTime(2014, 11, 30));

            Competition competition7 = CreateCompetition(
                context,
                user4.UserId,
                "Iron Man 7",
                "Winner gets $10 from everyone",
                25,
                null);

            Competition competition8 = CreateCompetition(
                context,
                user4.UserId,
                "Iron Man 8",
                "Winner gets $20 from everyone",
                50,
                new DateTime(2014, 5, 1));
            #endregion

            #region add users to competitions
            AddUserToCompetition(context, user1, competition);
            AddUserToCompetition(context, user2, competition);
            AddUserToCompetition(context, user3, competition);

            AddUserToCompetition(context, user1, competition2);
            AddUserToCompetition(context, user3, competition2);
            AddUserToCompetition(context, user4, competition2);

            AddUserToCompetition(context, user1, competition3);
            AddUserToCompetition(context, user2, competition3);
            AddUserToCompetition(context, user3, competition3);

            AddUserToCompetition(context, user1, competition4);
            AddUserToCompetition(context, user3, competition4);
            AddUserToCompetition(context, user4, competition4);

            AddUserToCompetition(context, user1, competition5);
            AddUserToCompetition(context, user2, competition5);
            AddUserToCompetition(context, user3, competition5);

            AddUserToCompetition(context, user1, competition6);
            AddUserToCompetition(context, user3, competition6);
            AddUserToCompetition(context, user4, competition6);

            AddUserToCompetition(context, user1, competition7);
            AddUserToCompetition(context, user2, competition7);
            AddUserToCompetition(context, user3, competition7);

            AddUserToCompetition(context, user1, competition8);
            AddUserToCompetition(context, user3, competition8);
            AddUserToCompetition(context, user4, competition8);
            #endregion

            #region add sweaty t-shirts
            AddSweatyTShirt(context, competition, user1, "Jogged", 1);
            AddSweatyTShirt(context, competition, user1, "Swam", 1);
            AddSweatyTShirt(context, competition, user1, "Lifted Weights", 2);
            AddSweatyTShirt(context, competition2, user1, "Jogged", 2);
            AddSweatyTShirt(context, competition2, user1, "Swam", 1);
            AddSweatyTShirt(context, competition2, user1, "Lifted Weights", 3);

            AddSweatyTShirt(context, competition, user2, "Jogged", 2);
            AddSweatyTShirt(context, competition, user2, "Swam", 3);
            AddSweatyTShirt(context, competition, user2, "Lifted Weights", 1);

            AddSweatyTShirt(context, competition, user3, "Jogged", 2);
            AddSweatyTShirt(context, competition, user3, "Swam", 1);
            AddSweatyTShirt(context, competition, user3, "Lifted Weights", 3);
            AddSweatyTShirt(context, competition2, user3, "Jogged", 3);
            AddSweatyTShirt(context, competition2, user3, "Swam", 2);
            AddSweatyTShirt(context, competition2, user3, "Lifted Weights", 1);

            AddSweatyTShirt(context, competition2, user4, "Jogged", 2);
            AddSweatyTShirt(context, competition2, user4, "Swam", 1);
            AddSweatyTShirt(context, competition2, user4, "Lifted Weights", 2);

            AddSweatyTShirt(context, competition3, user1, "Jogged", 1);
            AddSweatyTShirt(context, competition3, user1, "Swam", 1);
            AddSweatyTShirt(context, competition3, user1, "Lifted Weights", 2);
            AddSweatyTShirt(context, competition4, user1, "Jogged", 2);
            AddSweatyTShirt(context, competition4, user1, "Swam", 1);
            AddSweatyTShirt(context, competition4, user1, "Lifted Weights", 3);

            AddSweatyTShirt(context, competition3, user2, "Jogged", 2);
            AddSweatyTShirt(context, competition3, user2, "Swam", 3);
            AddSweatyTShirt(context, competition3, user2, "Lifted Weights", 1);

            AddSweatyTShirt(context, competition3, user3, "Jogged", 2);
            AddSweatyTShirt(context, competition3, user3, "Swam", 1);
            AddSweatyTShirt(context, competition3, user3, "Lifted Weights", 3);
            AddSweatyTShirt(context, competition4, user3, "Jogged", 3);
            AddSweatyTShirt(context, competition4, user3, "Swam", 2);
            AddSweatyTShirt(context, competition4, user3, "Lifted Weights", 1);

            AddSweatyTShirt(context, competition4, user4, "Jogged", 2);
            AddSweatyTShirt(context, competition4, user4, "Swam", 1);
            AddSweatyTShirt(context, competition4, user4, "Lifted Weights", 2);

            AddSweatyTShirt(context, competition5, user1, "Jogged", 1);
            AddSweatyTShirt(context, competition5, user1, "Swam", 1);
            AddSweatyTShirt(context, competition5, user1, "Lifted Weights", 2);
            AddSweatyTShirt(context, competition6, user1, "Jogged", 2);
            AddSweatyTShirt(context, competition6, user1, "Swam", 1);
            AddSweatyTShirt(context, competition6, user1, "Lifted Weights", 3);

            AddSweatyTShirt(context, competition5, user2, "Jogged", 2);
            AddSweatyTShirt(context, competition5, user2, "Swam", 3);
            AddSweatyTShirt(context, competition5, user2, "Lifted Weights", 1);

            AddSweatyTShirt(context, competition5, user3, "Jogged", 2);
            AddSweatyTShirt(context, competition5, user3, "Swam", 1);
            AddSweatyTShirt(context, competition5, user3, "Lifted Weights", 3);
            AddSweatyTShirt(context, competition6, user3, "Jogged", 3);
            AddSweatyTShirt(context, competition6, user3, "Swam", 2);
            AddSweatyTShirt(context, competition6, user3, "Lifted Weights", 1);

            AddSweatyTShirt(context, competition6, user4, "Jogged", 2);
            AddSweatyTShirt(context, competition6, user4, "Swam", 1);
            AddSweatyTShirt(context, competition6, user4, "Lifted Weights", 2);

            AddSweatyTShirt(context, competition7, user1, "Jogged", 1);
            AddSweatyTShirt(context, competition7, user1, "Swam", 1);
            AddSweatyTShirt(context, competition7, user1, "Lifted Weights", 2);
            AddSweatyTShirt(context, competition8, user1, "Jogged", 2);
            AddSweatyTShirt(context, competition8, user1, "Swam", 1);
            AddSweatyTShirt(context, competition8, user1, "Lifted Weights", 3);

            AddSweatyTShirt(context, competition7, user2, "Jogged", 2);
            AddSweatyTShirt(context, competition7, user2, "Swam", 3);
            AddSweatyTShirt(context, competition7, user2, "Lifted Weights", 1);

            AddSweatyTShirt(context, competition7, user3, "Jogged", 2);
            AddSweatyTShirt(context, competition7, user3, "Swam", 1);
            AddSweatyTShirt(context, competition7, user3, "Lifted Weights", 3);
            AddSweatyTShirt(context, competition8, user3, "Jogged", 3);
            AddSweatyTShirt(context, competition8, user3, "Swam", 2);
            AddSweatyTShirt(context, competition8, user3, "Lifted Weights", 1);

            AddSweatyTShirt(context, competition8, user4, "Jogged", 2);
            AddSweatyTShirt(context, competition8, user4, "Swam", 1);
            AddSweatyTShirt(context, competition8, user4, "Lifted Weights", 2);
            #endregion
            //create the error logging database
            //this needs be re-written into discreate batches with no GO statements
            //context.Database.ExecuteSqlCommand(SQLScripts.CreateElmah);
        }

    }
}