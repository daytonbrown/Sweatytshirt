using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Sweaty_T_Shirt.Models;
using Sweaty_T_Shirt.DAL;
using WebMatrix.WebData;
using Mvc.Mailer;

namespace Sweaty_T_Shirt.Controllers
{
    [Authorize(Roles = AccountRepository.UserOrAdminRole)]
    public class HomeController : BaseController
    {
        public ActionResult Index(SweatyTShirt sweatyTShirt)
        {
            int userID = UserID;

            sweatyTShirt.UserID = userID;

            using (CompetitionRepository competitionRepository = new CompetitionRepository())
            {
                if (sweatyTShirt.IsSave)
                {
                    sweatyTShirt.CreatedDate = DateTime.Now;
                    sweatyTShirt.SendEmail = true;  //per Dayton
                    competitionRepository.AddSweatyTShirt(sweatyTShirt);
                    ViewBag.Purr = new Purr() { Title = "Success", Message = "Sweaty-T-Shirt was successfully added." };
                }

                sweatyTShirt.Competitions = competitionRepository
                    .GetUserInCompetitionsForUser(userID)
                    .Where(o => o.IsActive)
                    .Select(o => o.Competition).ToList();

                if (sweatyTShirt.Competitions.Count > 0)
                {
                    if (sweatyTShirt.CompetitionID > 0)
                    {
                        sweatyTShirt.Competition = sweatyTShirt.Competitions.FirstOrDefault(o => o.CompetitionID == sweatyTShirt.CompetitionID);
                    }
                    else
                    {
                        sweatyTShirt.Competition = sweatyTShirt.Competitions[0];
                        sweatyTShirt.CompetitionID = sweatyTShirt.Competition.CompetitionID;
                    }

                    if (sweatyTShirt.Competition == null)
                    {
                        throw new ApplicationException(string.Format("Unable to retrieve Competition object for sweatyTShirt.CompetitionID {0}", sweatyTShirt.CompetitionID));
                    }

                    sweatyTShirt.Competition.CompetitionProgressBars =
                        ControllerHelpers.GetCompetitionProgressBars(competitionRepository,
                        sweatyTShirt.CompetitionID);
                    
                    if (sweatyTShirt.IsSave)
                    {
                        if (sweatyTShirt.SendEmail)
                        {
                            SendMail sendMail = new SendMail();

                            foreach (CompetitionProgressBar cpb in sweatyTShirt.Competition.CompetitionProgressBars)
                            {
                                using (var msg = sendMail.SweatyTShirtAdded(sweatyTShirt, cpb, true))
                                {
                                   // msg.SendAsync( userState: sweatyTShirt.UserProfile.Email);
                                    msg.Send();
                                   // msg.Dispose();
                                }
                            }
                        }
                        if (sweatyTShirt.PostToFacebook)
                        {
                            FacebookRepository.PostToFacebook(sweatyTShirt);
                        }
                    }
                }
            }

            if (TempData[ControllerHelpers.PURR] != null)
            {
                ViewBag.Purr = TempData[ControllerHelpers.PURR];
                TempData[ControllerHelpers.PURR] = null;
            }

            sweatyTShirt.IsSave = false;
            sweatyTShirt.Description = null;
            //will get client side validation errors because manually adding model to view, need to clear them.
            ModelState.Clear();
            return View(sweatyTShirt);
        }

        public ActionResult About()
        {
            //ViewBag.Message = "HomeController About().";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "HomeController Contact().";

            return View();
        }
    }
}
