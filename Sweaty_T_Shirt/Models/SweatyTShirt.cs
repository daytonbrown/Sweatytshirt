using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
//visual studio 2012 not need DataAnnotationsExtensions, they are in .Net 4.5
using DataAnnotationsExtensions;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel;
using Sweaty_T_Shirt.DAL;

namespace Sweaty_T_Shirt.Models
{
    public class SweatyTShirt
    {
        public SweatyTShirt()
        {
            IsSave = false;
            if (HttpContext.Current.Session != null)
            {
                PostToFacebook = (HttpContext.Current.Session[FacebookRepository.IS_FB_AUTHENTICATED] != null
                    && (bool)HttpContext.Current.Session[FacebookRepository.IS_FB_AUTHENTICATED] == true);
            }
        }
        [Key]
        [DatabaseGeneratedAttribute(DatabaseGeneratedOption.Identity)]
        public long SweatyTShirtID { get; set; }

        [Required]
        [ForeignKey("UserProfile")]
        public int UserID { get; set; }

        /// <summary>
        /// Virtual means lazy-loaded.
        /// </summary>
        public virtual UserProfile UserProfile { get; set; }

        [Required]
        [ForeignKey("Competition")]
        public long CompetitionID { get; set; }

        public virtual Competition Competition { get; set; }

        [Display(Name = "Date Created")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString="{0:d}")]
        public DateTime CreatedDate { get; set; }

        public const int AmountMin = 1;
        public const int AmountMax = 3;
        [Range(AmountMin,AmountMax, ErrorMessage="The number of Sweaty T-Shirts must be between 1 and 3.")]
        [Integer(ErrorMessage = "Amount must be numeric")]
        public int Amount { get; set; }

        [Display(Name = "Description")]
        [Required(AllowEmptyStrings = false, ErrorMessage = "Description is required.")]
        [StringLength(100, MinimumLength = 1)]
        public string Description { get; set; }

        [Display(Name="Add to Favorites")]
        public bool IsFavorite { get; set; }

        /// <summary>
        /// Dropdown of all competitions this user is active in on Home page.
        /// </summary>
        [NotMapped]
        public virtual List<Competition> Competitions { get; set; }

        /// <summary>
        /// On Home/Index.cshtml, differentiate between postback to save vs. postback because user changed competitions.
        /// </summary>
        [NotMapped]
        ///DefaultValue has no effect, but could write code to make it set default value:  http://stackoverflow.com/questions/7637022/default-value-in-an-asp-net-mvc-view-model
        ///[DefaultValue(false)]
        public virtual bool IsSave { get; set; }

        [NotMapped]
        [Display(Name = "Post to Facebook Wall?")]
        public bool PostToFacebook { get; set; }
    }
}