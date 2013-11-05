using Mvc.Mailer;
using Sweaty_T_Shirt.Models;
namespace Sweaty_T_Shirt.Controllers
{
    public interface ISendMail
    {
        MvcMailMessage SweatyTShirtAdded(SweatyTShirt sweatyTShirt, CompetitionProgressBar competitionProgressBar, bool isAsync = false);
        MvcMailMessage UserInCompetitionAdded(UserInCompetition userInCompetition, bool isAsync = false);
    }
}