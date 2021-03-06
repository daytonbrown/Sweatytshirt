﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebMatrix.WebData;
using Sweaty_T_Shirt.Models;
using System.Data.Entity;
using Sweaty_T_Shirt.DAL;
using System.IO;

namespace Sweaty_T_Shirt.Controllers
{
    [Authorize(Roles = AccountRepository.UserOrAdminRole)]
    public class CompetitionController : BaseController
    {
        [HttpGet]
        public ActionResult Index()
        {
            List<Competition> competitions = null;
            //always put CompetitionRepository inside a using block, so Dispose is called and database connection is closed.
            //failure to do that could cause serious memory leak.
            using (CompetitionRepository repository = new CompetitionRepository())
            {
                competitions = repository.GetCompetitionsCreatedByUser(UserID);
            }

            //if another method redirected to here show the purr message
            if (TempData[ControllerHelpers.PURR] != null)
            {
                ViewBag.Purr = TempData[ControllerHelpers.PURR];
                TempData[ControllerHelpers.PURR] = null;
            }

            //if the view name is the same as the name of this method (eg Index), then there is no need to
            //pass name of view to View() constructor.
            return View(competitions);
        }

        /// <summary>
        /// When there are two methods with same name, the method with the [HttpGet]
        /// attribute fetches the item that is being edited/created.  Convention: pass 0 for ID
        /// to create a new object.
        /// </summary>
        /// <param name="competitionID"></param>
        /// <returns></returns>
        [HttpGet]
        public ActionResult EditCompetition(long competitionID)
        {
            Competition competition = null;

            if (competitionID > 0)
            {
                using (CompetitionRepository repository = new CompetitionRepository())
                {
                    competition = repository.GetCompetition(competitionID);
                    if (competition == null)
                    {
                        throw new ApplicationException(string.Format("Unable to retrieve Competition object for competitionID: ",competitionID));
                    }
                    competition.UseDefaultImage = competition.IsUsingDefaultImage();
                    competition.CompetitionProgressBars =
                        ControllerHelpers.GetCompetitionProgressBars(repository,
                        competitionID);
                    ViewBag.AllowEdit = (IsUserAdmin || competition.CreatorUserID == UserID);
                }
            }
            else
            {
                competition = new Competition() {IsActive = true };
                ViewBag.AllowEdit = true;
            }

            //if another method redirected to here show the purr message
            if (TempData[ControllerHelpers.PURR] != null)
            {
                ViewBag.Purr = TempData[ControllerHelpers.PURR];
                TempData[ControllerHelpers.PURR] = null;
            }

            return View(competition);
        }

        /// <summary>
        /// When there are two "Edit..." methods with same name, the method with the [HttpPost]
        /// attribute saves to database and closes the popup.  Convention: pass 0 for competition.CompetitionID
        /// to create a new object.
        /// </summary>
        [HttpPost]
        public ActionResult EditCompetition(Competition competition, HttpPostedFileBase CustomImage)
        {
            if (!IsUserAdmin && competition.CompetitionID > 0 && competition.CreatorUserID != UserID)
            {
                throw new ApplicationException(string.Format("Possible security breach, User {0} attempted to edit Competition {1}.", UserID, competition.CompetitionID));
            }

            if (!competition.UseDefaultImage && CustomImage != null)
            {
                string fileName = Guid.NewGuid().ToString() + Path.GetExtension(CustomImage.FileName);

                string filePath = string.Format(@"{0}\{1}\{2}",
                    System.Web.HttpContext.Current.Request.PhysicalApplicationPath,
                    ControllerHelpers.CustomImageFolder,
                    fileName);

                if (ControllerHelpers.SaveImage(CustomImage.InputStream, filePath))
                {
                    competition.ImageSrc = fileName;
                }
            }
            else if (competition.UseDefaultImage && !string.IsNullOrEmpty(competition.ImageSrc))
            {
                string filePath = string.Format(@"{0}\{1}\{2}",
                    System.Web.HttpContext.Current.Request.PhysicalApplicationPath,
                    ControllerHelpers.CustomImageFolder,
                    competition.ImageSrc);

                try
                {
                    if (System.IO.File.Exists(filePath))
                    {
                        System.IO.File.Delete(filePath);
                    }
                }
                catch{ }
                competition.ImageSrc = null;
            }


            using (CompetitionRepository repository = new CompetitionRepository())
            {
                if (competition.CompetitionID <= 0)
                {
                    competition.CreatedDate = DateTime.Now;
                    competition.CreatorUserID = UserID;
                }
                repository.SaveCompetition(competition);
            }

            
            //the ClosePopup closes EditCompetition.cshtml and causes Index.cshtml to refresh, 
            //which in turn calls the Index action.
            //cannot just set ViewBag because it is lost on redirect.
            //TempData survives one redirect, store it there.
            TempData[ControllerHelpers.PURR] = new Purr() { Title = "Success", Message = "Competition was saved successfully." };

            return View("../Shared/ClosePopup");
        }

        /// <summary>
        /// User clicked "Activate" or "Deactivate" on Index.cshtml in the list of competitions.  Desires to activate
        /// or deactivate a competition.
        /// </summary>
        /// <param name="competitionID"></param>
        /// <param name="userID"></param>
        /// <returns></returns>
        [HttpGet]
        public ActionResult ToggleCompetition(long competitionID)
        {
            if (competitionID <= 0)
            {
                throw new ArgumentException(string.Format("Invalid competitionID must be greater than zero: competitionID: {0}", competitionID));
            }

            string action = null;
            using (CompetitionRepository repository = new CompetitionRepository())
            {
                //ToggleCompetition returns what Competition.IsActive was set to.
                action = (repository.ToggleCompetition(competitionID) == true? string.Empty : "de");
            }

            TempData[ControllerHelpers.PURR] = new Purr() { Title = "Success", Message = string.Format("Competition was successfully {0}activated.", action) };

            //entire page gets refreshed.
            return RedirectToAction("Index");
        }

        [HttpGet]
        public ActionResult DeleteCompetition(long competitionID)
        {
            using (CompetitionRepository repository = new CompetitionRepository())
            {
                repository.DeleteCompetition(competitionID);
            }

            TempData[ControllerHelpers.PURR] = new Purr() { Title = "Success", Message = "Competition was successfully deleted." };

            //entire page gets refreshed 
            return RedirectToAction("Index");
        }

        [ActionName("DeleteCompetitionWithRedirect")]
        public ActionResult DeleteCompetition(long competitionID,
            int userID,
            string redirectToAction,
            string redirectToController)
        {
            using (CompetitionRepository repository = new CompetitionRepository())
            {
                repository.DeleteCompetition(competitionID);
            }

            TempData[ControllerHelpers.PURR] = new Purr() { Title = "Success", Message = "Competition was successfully deleted." };

            switch (redirectToAction)
            {
                case "EditUser":
                    return RedirectToAction(redirectToAction, redirectToController, new { userID = userID });
                default:
                    if (!string.IsNullOrEmpty(redirectToController))
                    {
                        return RedirectToAction(redirectToAction, redirectToController);
                    }
                    else
                    {
                        return RedirectToAction(redirectToAction);
                    }
            }
        }

        /// <summary>
        /// User clicked "Delete" on Competition/EditCompetition.cshtml, Home/Index.cshtml, or
        /// Competition/UserInCompetitions.cshtml.
        /// </summary>
        /// <param name="competitionID"></param>
        /// <param name="userID"></param>
        /// <returns></returns>
        public ActionResult DeleteUserInCompetition(long competitionID,
            int userID,
            string redirectToAction,
            string redirectToController = null)
        {
            using (CompetitionRepository repository = new CompetitionRepository())
            {
                repository.DeleteUserInCompetition(competitionID, userID);
            }

            TempData[ControllerHelpers.PURR] = new Purr() { Title = "Success", Message = "The User was successfully deleted from the Competition." };

            switch (redirectToAction.ToLower())
            {
                case "editcompetition":
                    return RedirectToAction(redirectToAction, new {competitionID = competitionID});
                case "edituser":
                    return RedirectToAction(redirectToAction, redirectToController, new { userID = userID });
                default:
                    if (!string.IsNullOrEmpty(redirectToController))
                    {
                        return RedirectToAction(redirectToAction, redirectToController);
                    }
                    else
                    {
                        return RedirectToAction(redirectToAction);
                    }
            }

        }

        [HttpGet]
        public ActionResult AddUserInCompetition(long competitionID)
        {
            UserInCompetition userInCompetition = new UserInCompetition() { CompetitionID = competitionID };
            using (CompetitionRepository repository = new CompetitionRepository())
            {
                userInCompetition.Competition = repository.GetCompetition(competitionID);
            }
            return View(userInCompetition);
        }

        [HttpPost]
        public ActionResult AddUserInCompetition(UserInCompetition userInCompetition)
        {
            try
            {
                //HACK the competition property is set NULL in AddUserInCompetition.
                //this is needed to send email
                Competition competition = null;
                if (userInCompetition.SendEmail)
                {
                    competition = userInCompetition.Competition;
                }

                using (CompetitionRepository repository = new CompetitionRepository())
                {
                    repository.AddUserInCompetition(userInCompetition, AccountRepository.AllUsersPassword);
                }

                if (userInCompetition.SendEmail)
                {
                    userInCompetition.Competition = competition;
                    using (var msg = new SendMail().UserInCompetitionAdded(userInCompetition, true))
                    {
                        //msg.SendAsync(userState: userInCompetition.Email);
                        msg.Send();
                        //msg.Dispose();
                    }
                }

                TempData[ControllerHelpers.PURR] = new Purr() { Title = "Success", Message = "The User was successfully added to the Competition." };

                //entire page gets refreshed when this view calls parent.location.reload.  
                //TODO: put the users table on a partial view and update it via a JSON call.
                return View("../Shared/ClosePopup");
            }
            catch (RepositoryException ex)
            {
                //clear model errors for DateInvited
                ModelState.Clear();
                ModelState.AddModelError("AddUserInCompetition", ex.Message);
            }
            return View(userInCompetition);
        }

        /// <summary>
        /// User clicked "Activate" or "Deactivate" on EditCompetition.cshtml in the list of users.  Desires to activate
        /// or deactivate user from a competition.
        /// </summary>
        /// <param name="competitionID"></param>
        /// <param name="userID"></param>
        /// <returns></returns>
        [HttpGet]
        public ActionResult ToggleUserInCompetition(long competitionID, 
            int userID, 
            string redirectToAction,
            string redirectToController)
        {
            if (competitionID <= 0 || userID <= 0)
            {
                throw new ArgumentException(string.Format("Invalid competitionID or userID argument, both must be greater than zero: competitionID: {0}, userID: {1}", competitionID, userID));
            }

            string action = null;
            using (CompetitionRepository repository = new CompetitionRepository())
            {
                action = (repository.ToggleUserInCompetition(competitionID, userID) == true ? string.Empty : "de");
            }

            //entire page gets refreshed.  TODO: put the table on a partial view
            //and update it via a JSON call.
            if (string.IsNullOrEmpty(redirectToAction))
            {
                return Content(string.Empty);
            }
            else
            {
                TempData[ControllerHelpers.PURR] = new Purr() { Title = "Success", Message = string.Format("The User in the Competition was successfully {0}activated.", action) };
                switch (redirectToAction)
                {
                    case "EditCompetition":
                        return RedirectToAction(redirectToAction, new { competitionID = competitionID });
                    case "EditUser":
                        return RedirectToAction(redirectToAction, redirectToController, new { userID = userID });
                    default:
                        if (!string.IsNullOrEmpty(redirectToController))
                        {
                            return RedirectToAction(redirectToAction, redirectToController);
                        }
                        else
                        {
                            return RedirectToAction(redirectToAction);
                        }
                }
            }
        }

        public ActionResult UserInCompetitions()
        {
            List<UserInCompetition> list = null;
            using (CompetitionRepository competitionRepository = new CompetitionRepository())
            {
                list = competitionRepository.GetUserInCompetitionsForUser(UserID);
            }

            //if another method redirected to here show the purr message
            if (TempData[ControllerHelpers.PURR] != null)
            {
                ViewBag.Purr = TempData[ControllerHelpers.PURR];
                TempData[ControllerHelpers.PURR] = null;
            }

            return View(list);
        }

        [ActionName("SweatyTShirtsCPB")]
        public ActionResult SweatyTShirts(int userID, long competitionID)
        {
            if (userID <= 0)
            {
                throw new ApplicationException("Missing argument UserID");
            }

            ViewBag.IsUserAdmin = IsUserAdmin;
            ViewBag.UserID = UserID;
            ViewBag.ShowUserName = false;

            List<SweatyTShirt> list = null;
            using (CompetitionRepository competitionRepository = new CompetitionRepository())
            {
                list = competitionRepository.GetSweatyTShirtsForUser(userID, competitionID)
                    .OrderByDescending(o => o.CreatedDate)
                    .ToList();
            }

            //if DeleteSweatyTShirt redirected to here show the purr message
            if (TempData[ControllerHelpers.PURR] != null)
            {
                ViewBag.Purr = TempData[ControllerHelpers.PURR];
                TempData[ControllerHelpers.PURR] = null;
            }

            return View("SweatyTShirts", list);
        }

        public ActionResult SweatyTShirts(long competitionID)
        {
            ViewBag.IsUserAdmin = IsUserAdmin;
            ViewBag.UserID = UserID;
            ViewBag.ShowUserName = true;

            List<SweatyTShirt> list = null;
            using (CompetitionRepository competitionRepository = new CompetitionRepository())
            {
                list = competitionRepository.GetSweatyTShirtsInCompetition(competitionID)
                    .OrderBy(o => o.UserProfile.FullName)
                    .OrderByDescending(o => o.CreatedDate)
                    .ToList();
            }

            //if DeleteSweatyTShirt redirected to here show the purr message
            if (TempData[ControllerHelpers.PURR] != null)
            {
                ViewBag.Purr = TempData[ControllerHelpers.PURR];
                TempData[ControllerHelpers.PURR] = null;
            }

            return View(list);
        }

        private void DeleteSweatyTShirt(long sweatyTShirtID)
        {
            using (CompetitionRepository competitionRepository = new CompetitionRepository())
            {
                competitionRepository.DeleteSweatyTShirt(sweatyTShirtID);
            }

            TempData[ControllerHelpers.PURR] = new Purr() { Title = "Success", Message = "The Sweaty-T-Shirt was successfully deleted." };
        }

        /// <summary>
        /// userID not needed, but shares an actionlink with CPB method.
        /// </summary>
        /// <param name="sweatyTShirtID"></param>
        /// <param name="competitionID"></param>
        /// <param name="userID"></param>
        /// <returns></returns>
        public ActionResult DeleteSweatyTShirt(long sweatyTShirtID, long competitionID, int userID)
        {
            DeleteSweatyTShirt(sweatyTShirtID);
            return RedirectToAction("SweatyTShirts", new {competitionID = competitionID});
        }

        public ActionResult DeleteSweatyTShirtCPB(long sweatyTShirtID, long competitionID, int userID)
        {
            DeleteSweatyTShirt(sweatyTShirtID);
            return RedirectToAction("SweatyTShirtsCPB", new { userID = userID, competitionID = competitionID });
        }

        [ActionName("DeleteSweatyTShirtEditUser")]
        public ActionResult DeleteSweatyTShirt(long sweatyTShirtID, 
            int userID)
        {
            DeleteSweatyTShirt(sweatyTShirtID);
            return RedirectToAction("EditUser", "Account", new { userID = userID });
        }
    }
}
