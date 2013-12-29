using Mvc.Mailer;
using Sweaty_T_Shirt.DAL;
using Sweaty_T_Shirt.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Net.Configuration;
using System.Net.Mail;
using System.Web.Configuration;

namespace Sweaty_T_Shirt.Controllers
{
    public class SendMail : MailerBase, ISendMail
    {
        public SendMail()
        {
            MasterName = "_Layout"; 
        }

        private const string SweatyTShirtEmailHtml = @"<html>
	<head></head>
	<body>
          <h1>@Title</h1>
            <p>
                Dear @RecipientFullName,
            </p>
            <p>
                @SweatyTShirtFullName (@SweatyTShirtEmailAddress) 
                just did @Description for @Amount.
            </p>
            <p>
                Sincerely,
            </p>
            <p>
                <a href='http://www.sweatytshirt.com'>Sweaty T-Shirt.com</a>.
            </p>
	</body>
</html>";

        private static string _fromEmailAddress = null;
        private static string FromEmailAddress
        {
            get
            {
                if (string.IsNullOrEmpty(_fromEmailAddress))
                {
                    Configuration configurationFile = WebConfigurationManager.OpenWebConfiguration("~");
                    MailSettingsSectionGroup mailSettings = configurationFile.GetSectionGroup("system.net/mailSettings") as MailSettingsSectionGroup;
                    _fromEmailAddress = mailSettings.Smtp.From;
                }
                return _fromEmailAddress;
            }
        }

        public virtual void SendEmails()
        {
            var sweatyTShirtEmails = new CompetitionRepository().SweatyTShirtEmails();

            if (sweatyTShirtEmails == null || sweatyTShirtEmails.Count <= 0)
            {
                return;
            }

            List<int> distinctUserIds = new List<int>();

            //if web.config has configuration is automatically reads it.
            using (SmtpClient smtpClient = new SmtpClient())
            {
                MailAddress fromAddress = new MailAddress(FromEmailAddress, "Sweaty T-Shirt");
                string subject = "Sweaty T-Shirt Added";

                foreach (var sweatyTShirtEmail in sweatyTShirtEmails)
                {
                    try
                    {
                        //because this runs on background thread cannot use MvcMail package, the httpContext is not available.
                        string emailBody = SweatyTShirtEmailHtml.Replace("@Title", subject)
                            .Replace("@RecipientFullName", sweatyTShirtEmail.RecipientFullName)
                            .Replace("@RecipientEmailAddress", sweatyTShirtEmail.RecipientEmailAddress)
                            .Replace("@SweatyTShirtFullName", sweatyTShirtEmail.SweatyTShirtFullName)
                            .Replace("@SweatyTShirtEmailAddress", sweatyTShirtEmail.SweatyTShirtEmailAddress)
                            .Replace("@Description", sweatyTShirtEmail.Description)
                            .Replace("@Amount", sweatyTShirtEmail.Amount.ToString());

                        MailAddress toAddress = new MailAddress(sweatyTShirtEmail.RecipientEmailAddress,
                            sweatyTShirtEmail.RecipientFullName);

                        MailMessage mailMessage = new MailMessage(fromAddress, toAddress)
                        {
                            Subject = subject,
                            Body = emailBody,
                            IsBodyHtml = true
                        };

                        //smtpClient.Send(mailMessage);
                        if (!distinctUserIds.Contains(sweatyTShirtEmail.RecipientUserID))
                        {
                            new AccountRepository().UpdateLastEmailSent(sweatyTShirtEmail.RecipientUserID);
                            distinctUserIds.Add(sweatyTShirtEmail.RecipientUserID);
                        }
                    }
                    catch (Exception ex)
                    {
                        Exception ex2 = new Exception(string.Format("Error sending sweaty t shirt email to {0}", sweatyTShirtEmail.RecipientEmailAddress), ex);
                        Elmah.ErrorLog.GetDefault(System.Web.HttpContext.Current).Log(new Elmah.Error(ex2));
                    }
                }
            }
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