using System.Collections.Generic;
using System.Linq;
using Sweaty_T_Shirt.DAL;
using Sweaty_T_Shirt.Models;

namespace Sweaty_T_Shirt.Controllers
{
    public class ControllerHelpers
    {
        public const string PURR = "PURR";
        public const string ISADMIN = "ISADMIN";

        public static List<CompetitionProgressBar> GetCompetitionProgressBars(
            CompetitionRepository competitionRepository,
            long competitionID)
        {
            List<SweatyTShirt> sweatyTShirts = competitionRepository
                .GetSweatyTShirtsInCompetition(competitionID);

            //group by the unique UserProfile.UserID.  Then update with the possibly non-unique UserProfile.FullName
            List<CompetitionProgressBar> competitionProgressBars = sweatyTShirts.GroupBy(o => o.UserProfile.UserId, o => o.Amount, (key, g) => new CompetitionProgressBar() { UserID = key, Amount = g.Sum() }).ToList();

            foreach (CompetitionProgressBar cpb in competitionProgressBars)
            {
                UserProfile userProfile = sweatyTShirts.FirstOrDefault(o => o.UserID == cpb.UserID).UserProfile;
                cpb.FullName = userProfile.FullName;
                cpb.Email = userProfile.Email;
            }

            return competitionProgressBars;
        }
    }
}