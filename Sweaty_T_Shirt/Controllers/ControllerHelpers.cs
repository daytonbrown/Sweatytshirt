using System.Collections.Generic;
using System.Linq;
using Sweaty_T_Shirt.DAL;
using Sweaty_T_Shirt.Models;
using System.Drawing.Imaging;
using System.IO;
using System.Drawing;

namespace Sweaty_T_Shirt.Controllers
{
    public class ControllerHelpers
    {
        public const string PURR = "PURR";

        public const string DefaultImageSrc = "Images/sweaty_tshirt.jpg";
        public const string CustomImageFolder = @"Images\CompetitionImages";
        public const string CustomImageVirtualFolder = "Images/CompetitionImages/";
        private static string _virtualRoot = null;

        private static ImageFormat[] ValidFormats = new[] { ImageFormat.Jpeg, ImageFormat.Png };
        public static bool SaveImage(Stream image, string filePath)
        {
            try
            {
                using (var img = Image.FromStream(image))
                {
                    if (ValidFormats.Contains(img.RawFormat))
                    {
                        using (var fileStream = File.Create(filePath))
                        {
                            image.Seek(0, SeekOrigin.Begin);
                            image.CopyTo(fileStream);
                            return true;
                        }
                    }
                }
                return false;
            }
            catch
            {
                return false;
            }
        }
        public static string GetImageSrc(string imageSrc)
        {
            if (string.IsNullOrEmpty(_virtualRoot))
            {
                _virtualRoot = System.Web.HttpContext.Current.Request.Url.AbsoluteUri.Replace(
                System.Web.HttpContext.Current.Request.Url.AbsolutePath, "/");
            }
            return _virtualRoot + (string.IsNullOrEmpty(imageSrc) ? DefaultImageSrc : CustomImageVirtualFolder + imageSrc);
        }

        public static void SendSweatyTShirtNotices()
        {

        }

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