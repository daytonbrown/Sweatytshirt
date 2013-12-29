using Mvc.Mailer;
using Sweaty_T_Shirt.Models;
namespace Sweaty_T_Shirt.Controllers
{
    public interface ISendMail
    {
        void SendEmails();

        MvcMailMessage UserInCompetitionAdded(UserInCompetition userInCompetition, bool isAsync = false);
    }
}