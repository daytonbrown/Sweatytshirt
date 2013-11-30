using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebMatrix.WebData;
using Sweaty_T_Shirt.Models;
using System.Data.Entity;
using System.Data.Objects;
using System.Web.Security;
using System.Data.Entity.Validation;
using System.Transactions;

namespace Sweaty_T_Shirt.DAL
{
    public class CompetitionRepository : BaseRepository
    {
        public List<UserInCompetition> GetUserInCompetitionsForUser(int userID)
        {
            //use Include to eager load the desired related object.  Because it is declared "virtual" it is 
            //lazy-loaded by default, meaning it is left null unless you refer to it while still inside
            //the DbContext.
            return _context.UserInCompetitions.Include(o => o.Competition)
                .Where(o => o.Competition.IsActive && o.UserID == userID).ToList();
        }

        public List<Competition> GetCompetitionsCreatedByUser(int userID)
        {
            return _context.Competitions.Where(o => o.CreatorUserID == userID).ToList();
        }

        public Competition GetCompetition(long competitionID)
        {
            return _context.Competitions.Include(o => o.UserInCompetitions.Select(uic => uic.UserProfile))
                .Include(o => o.UserProfile)
                .FirstOrDefault(o => o.CompetitionID == competitionID);
        }

        public UserInCompetition GetUserInCompetition(long competitionID, int userID)
        {
            return _context.UserInCompetitions.Include(o => o.UserProfile)
                .Include(o => o.Competition)
                .FirstOrDefault(o => o.CompetitionID == competitionID
                && o.UserID == userID);
        }

        public List<SweatyTShirt> GetSweatyTShirtsInCompetition(long competionID)
        {
            return (from s in _context.SweatyTShirts
                        join uic in _context.UserInCompetitions on s.CompetitionID equals uic.CompetitionID
                        join u in _context.UserProfiles on s.UserID equals u.UserId
                        where uic.IsActive
                        && uic.CompetitionID == competionID
                        && uic.UserID == u.UserId
                        && s.CompetitionID == competionID
                        select s).Include(o => o.UserProfile).ToList();
        }

        public List<SweatyTShirt> GetSweatyTShirtsForUser(int userID, long? competitionID = null)
        {
            if (competitionID.HasValue)
            {
                return _context.SweatyTShirts.Where(o => o.UserID == userID && o.CompetitionID == competitionID.Value)
                    .Include(o => o.Competition).Include(o => o.UserProfile).ToList();
            }
            else
            {
                return _context.SweatyTShirts.Where(o => o.UserID == userID)
                    .Include(o => o.Competition).ToList();
            }
        }

        public void AddSweatyTShirt(SweatyTShirt sweatyTShirt)
        {
            if (sweatyTShirt.CompetitionID <= 0)
            {
                throw new ApplicationException("Missing required property CompetitionID");
            }
            if (sweatyTShirt.UserID <= 0)
            {
                throw new ApplicationException("Missing required property UserID");
            }
            if (sweatyTShirt.Amount < SweatyTShirt.AmountMin || sweatyTShirt.Amount > SweatyTShirt.AmountMax)
            {
                throw new ApplicationException(string.Format("Amount must be between {0} and {1}.", SweatyTShirt.AmountMin, SweatyTShirt.AmountMax));
            }
            _context.SweatyTShirts.Add(sweatyTShirt);
            _context.SaveChanges();
        }
        public void SaveCompetition(Competition competition)
        {
            if (competition.CompetitionID <= 0)
            {
                _context.Competitions.Add(competition);
            }
            else
            {
                #region EF 4 entity states.
/*            
http://stackoverflow.com/questions/4667621/how-to-update-ef-4-entity-in-asp-net-mvc-3
http://www.asp.net/mvc/tutorials/getting-started-with-ef-using-mvc/implementing-basic-crud-functionality-with-the-entity-framework-in-asp-net-mvc-application
http://blogs.msdn.com/b/adonet/archive/2011/01/29/using-dbcontext-in-ef-feature-ctp5-part-4-add-attach-and-entity-states.aspx
An entity may be in one of the following states:
    •Added. The entity does not yet exist in the database. The SaveChanges method must issue an INSERT statement. 
    •Unchanged. Nothing needs to be done with this entity by the SaveChanges method. When you read an entity from the database, the entity starts out with this status. 
    •Modified. Some or all of the entity's property values have been modified. The SaveChanges method must issue an UPDATE statement. 
    •Deleted. The entity has been marked for deletion. The SaveChanges method must issue a DELETE statement. 
    •Detached. The entity isn't being tracked by the database context. 

In a desktop application, state changes are typically set automatically. In this type of application, you read an entity and make changes to some of its property values. This causes its entity state to automatically be changed to Modified. Then when you call SaveChanges, the Entity Framework generates a SQL UPDATE statement that updates only the actual properties that you changed. 

However, in a web application this sequence is interrupted, because the database context instance that reads an entity is disposed after a page is rendered. When the HttpPost Edit action method is called, this is the result of a new request and you have a new instance of the context, so you have to manually set the entity state to Modified. Then when you call SaveChanges, the Entity Framework updates all columns of the database row, because the context has no way to know which properties you changed.
 
If you want the SQL Update statement to update only the fields that the user actually changed, you can save the original values in some way (such as hidden fields) so that they are available when the HttpPost Edit method is called. Then you can create a Student entity using the original values, call the Attach method with that original version of the entity, update the entity's values to the new values, and then call SaveChanges. For more information, see Add/Attach and Entity States and Local Data on the Entity Framework team blog.
                 */
                #endregion

                //using Attach avoids need to fetch the object from db
                //disadvantage, need set both the value and the IsModified property.
                _context.Competitions.Attach(competition);
                var c = _context.Entry(competition);
                c.Property(o => o.Description).CurrentValue = competition.Description;
                c.Property(o => o.Description).IsModified = true;
                c.Property(o => o.Points).CurrentValue = competition.Points;
                c.Property(o => o.Points).IsModified = true;
                c.Property(o => o.IsActive).CurrentValue = competition.IsActive;
                c.Property(o => o.IsActive).IsModified = true;
                c.Property(o => o.Name).CurrentValue = competition.Name;
                c.Property(o => o.Name).IsModified = true;

                /*
                //set the entire entity to Modified, then list the fields that should be ignored on the update (fields that are not displayed editable on EditCompetition.cshtml)
                //DOES NOT WORK, get error: Setting IsModified to false for a modified property is not supported.
                c.State = System.Data.EntityState.Modified;
                c.Property(o => o.CompetitionID).IsModified = false;
                c.Property(o => o.CreatedDate).IsModified = false;
                c.Property(o => o.CreatorUserID).IsModified = false;
                c.Property(o => o.SweatyTShirts).IsModified = false;
                c.Property(o => o.UserInCompetitions).IsModified = false;
                c.Property(o => o.UserProfile).IsModified = false;
                 */

                //other option: fetch fresh object and individually set the properties that I want updated;
                //EF tracks changes when inside the context and only saves the changed fields
                //disadvantage is extra call to database.
                /*
                Competition dbCompetition = _context.Competitions.FirstOrDefault(o => o.CompetitionID == competition.CompetitionID);
                dbCompetition.Description = competition.Description;
                dbCompetition.Points = competition.Points;
                dbCompetition.IsActive = competition.IsActive;
                dbCompetition.Name = competition.Name;
                 */
            }

            _context.SaveChanges();
        }

        public bool ToggleCompetition(long competitionID)
        {
            Competition competition = _context.Competitions.FirstOrDefault(o => o.CompetitionID == competitionID);
            if (competition == null)
            {
                throw new ArgumentException(string.Format("Unable to retrieve Competition object: competitionID: {0}", competitionID));
            }
            competition.IsActive = (competition.IsActive ? false : true);
            bool isActive = competition.IsActive;
            _context.SaveChanges();
            return isActive;
        }

        private const string DeleteCompetitionSqlCommand = @"DELETE FROM dbo.UserInCompetition WHERE CompetitionID = {0};"
            + "DELETE FROM dbo.SweatyTShirt WHERE CompetitionID = {0};"
            + "DELETE FROM dbo.Competition WHERE CompetitionID = {0};";
        public void DeleteCompetition(long competitionID)
        {
            _context.Database.ExecuteSqlCommand(DeleteCompetitionSqlCommand, competitionID);
            #region if doing it in LINQ
            ////must include all children on fetch so EF deletes them too,
            ////otherwise get constraint violation error.
            //Competition competition = _context.Competitions
            //    .Include(o => o.SweatyTShirts)
            //    .Include(o => o.UserInCompetitions)
            //    .FirstOrDefault(o => o.CompetitionID == competitionID);

            //if (competition == null)
            //{
            //    throw new ArgumentException(string.Format("Unable to retrieve Competition object: competitionID: {0}", competitionID));
            //}

            //using (TransactionScope ts = new TransactionScope())
            //{
            //    foreach (var sts in competition.SweatyTShirts.ToList())
            //    {
            //        _context.SweatyTShirts.Remove(sts);
            //    }

            //    foreach (var uic in competition.UserInCompetitions.ToList())
            //    {
            //        _context.UserInCompetitions.Remove(uic);
            //    }
            //    _context.Competitions.Remove(competition);
            //    _context.SaveChanges();
            //    ts.Complete();
            //}
            #endregion
        }

        private const string DeleteUserInCompetitionSqlCommand = @"DELETE FROM dbo.UserInCompetition WHERE UserID = {1} AND CompetitionID = {0};"
            + "DELETE FROM dbo.SweatyTShirt WHERE UserID = {1} AND CompetitionID = {0};";

        public void DeleteUserInCompetition(long competitionID, int userID)
        {
            _context.Database.ExecuteSqlCommand(DeleteUserInCompetitionSqlCommand, competitionID, userID);
        }

        public bool ToggleUserInCompetition(long competitionID, int userID)
        {
            UserInCompetition userInCompetition = _context.UserInCompetitions.FirstOrDefault(o => o.CompetitionID == competitionID
                && o.UserID == userID);
            if (userInCompetition == null)
            {
                throw new ArgumentException(string.Format("Unable to retrieve UserInCompetition object: competitionID: {0}, userID: {1}", competitionID, userID));
            }
            userInCompetition.IsActive = (userInCompetition.IsActive ? false : true);
            bool isActive = userInCompetition.IsActive;

            //HACK the NotMapped fields that are required will throw entity validation errors
            userInCompetition.FullName = "x";
            userInCompetition.Email = "x";
            try
            {
                _context.SaveChanges();
            }
            catch (DbEntityValidationException ex)
            {
                foreach (var eve in ex.EntityValidationErrors)
                {
                    Console.WriteLine(eve.Entry.Entity.GetType().Name);
                    Console.WriteLine(eve.Entry.State);
                    foreach (var ve in eve.ValidationErrors)
                    {
                        Console.WriteLine(ve.PropertyName);
                        Console.WriteLine(ve.ErrorMessage);
                    }
                }
                Elmah.ErrorLog.GetDefault(System.Web.HttpContext.Current).Log(new Elmah.Error(ex));
            }
            return isActive;
        }

        public void AddUserInCompetition(UserInCompetition userInCompetition,
            string allUsersPassword)
        {
            //see if the user is already registered.
            var userProfile = _context.UserProfiles.FirstOrDefault(o => !string.IsNullOrEmpty(o.Email) && o.Email.ToUpper() == userInCompetition.Email.ToUpper());
            if (userProfile != null)
            {
                //user is already registered.  ignore the FullName passed in.

                //see if the user is already in the competition
                var existingUserInCompetition = _context.UserInCompetitions.FirstOrDefault(o => o.CompetitionID == userInCompetition.CompetitionID && o.UserID == userProfile.UserId);
                if (existingUserInCompetition != null)
                {
                    throw new RepositoryException(string.Format("The user {0} with email address {1} is already in this competition.", userInCompetition.FullName, userInCompetition.Email));
                }
            }
            else
            {
                try
                {
                    using (AccountRepository accountRepository = new AccountRepository(_context))
                    {
                        accountRepository.CreateUser(userInCompetition.FullName, userInCompetition.Email);
                    }
                }
                catch (MembershipCreateUserException e)
                {
                    throw new RepositoryException(string.Format("Error creating user {0}, email {1}: {2}", userInCompetition.FullName, userInCompetition.Email, e.StatusCode));
                }

                userProfile = _context.UserProfiles.FirstOrDefault(o => !string.IsNullOrEmpty(o.Email) && o.Email.ToUpper() == userInCompetition.Email.ToUpper());
                if (userProfile == null)
                {
                    throw new ApplicationException(string.Format("Unable to retrieve just-added user profile for user name {0}, email [1}", userInCompetition.FullName, userInCompetition.Email));
                }
            }
            userInCompetition.DateInvited = DateTime.Now;
            userInCompetition.IsActive = true;
            userInCompetition.UserID = userProfile.UserId;
            
            //HACK Competition property is instantiated to an object with ID of 0 because we have UserInCompetition.Competition.Name in hidden input on view.
            //if left that way EF tries to create a new Competition record using the parameters passed in for the UserInCompetition!
            userInCompetition.Competition = null;  

            //HACK NotMapped Required fields will throw validation error even though are not in database
            userInCompetition.FullName = userProfile.FullName;
            userInCompetition.Email = userProfile.Email;

            _context.UserInCompetitions.Add(userInCompetition);
            
            _context.SaveChanges();
        }

        internal void DeleteSweatyTShirt(long sweatyTShirtID)
        {
            SweatyTShirt sweatyTShirt = _context.SweatyTShirts.FirstOrDefault(o => o.SweatyTShirtID == sweatyTShirtID);

            if (sweatyTShirt == null)
            {
                throw new ArgumentException(string.Format("Unable to retrieve SweatyTShirt object: sweatyTShirtID =  {0}", sweatyTShirtID));
            }

            _context.SweatyTShirts.Remove(sweatyTShirt);
            _context.SaveChanges();
        }
    }
}