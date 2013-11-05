using Mvc.Mailer;
using Sweaty_T_Shirt.Models;
using System;

namespace Sweaty_T_Shirt.Controllers
{
    public class SendMail : MailerBase, ISendMail
    {
        public SendMail()
        {
            MasterName = "_Layout";
        }

        public virtual MvcMailMessage SweatyTShirtAdded(SweatyTShirt sweatyTShirt,
            CompetitionProgressBar competitionProgressBar,
            bool isAsync = false)
        {
            //this is in Views/SendMail/_Layout.cshtml
            ViewBag.Title = "Sweaty T-Shirt Added";

            //see Views/SendMail/SweatyTShirtAdded.cshtml
            ViewBag.RecipientFullName = competitionProgressBar.FullName;

            //allows us to use strongly-typed model in Views/SendMail/SweatyTShirtAdded.cshtml
            ViewData.Model = sweatyTShirt;

            if (isAsync)
            {
                var smtpClientWrapper = new SmtpClientWrapper();
                smtpClientWrapper.SendCompleted += new System.Net.Mail.SendCompletedEventHandler(SmtpClientWrapper_SendCompleted);
            }

            return Populate(x =>
            {
                x.Subject = "Sweaty T-Shirt Added";
                x.ViewName = "SweatyTShirtAdded";
                x.To.Add(competitionProgressBar.Email);
            });
        }

        public virtual MvcMailMessage UserInCompetitionAdded(UserInCompetition userInCompetition,
            bool isAsync = false)
        {
            //this is in Views/SendMail/_Layout.cshtml
            ViewBag.Title = "User Added to Competition";

            if (isAsync)
            {
                var smtpClientWrapper = new SmtpClientWrapper();
                smtpClientWrapper.SendCompleted += new System.Net.Mail.SendCompletedEventHandler(SmtpClientWrapper_SendCompleted);
            }

            //allows us to use strongly-typed model in Views/SendMail/UserInCompetitionAdded.cshtml
            ViewData.Model = userInCompetition;
            return Populate(x =>
            {
                x.Subject = "User Added To Competition";
                x.ViewName = "UserInCompetitionAdded";
                x.To.Add(userInCompetition.Email);
            });
        }
        public void SmtpClientWrapper_SendCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            if (e.Error != null || e.Cancelled)
            {
                Exception ex = null;
                if (e.Error != null)
                {
                    ex = new Exception("Error sending email to " + e.UserState, e.Error);
                }
                else
                {
                    ex = new Exception("Cancellation sending email to " + e.UserState);
                }
                Elmah.ErrorLog.GetDefault(System.Web.HttpContext.Current).Log(new Elmah.Error(ex));
                //TODO could use SignalR to send message to browser informing user that email was not sent.
            }
        }
    }
}