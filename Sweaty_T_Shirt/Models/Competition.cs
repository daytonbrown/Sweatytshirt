using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
//visual studio 2012 not need DataAnnotationsExtensions, they are in .Net 4.5
using DataAnnotationsExtensions;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sweaty_T_Shirt.Models
{
    public class Competition
    {
        [Key]
        [DatabaseGeneratedAttribute(DatabaseGeneratedOption.Identity)]
        [Integer(ErrorMessage = "CompetitionID must be numeric.")]
        public long CompetitionID { get; set; }

        /// <summary>
        /// The userid of the creator of this competition.
        /// </summary>
        [ForeignKey("UserProfile")]
        public int CreatorUserID { get; set; }

        /// <summary>
        /// This is the user profile linked to CreatorUserID.
        /// </summary>
        public virtual UserProfile UserProfile { get; set; }

        public DateTime CreatedDate { get; set; }

        [Display(Name = "Name")]
        [Required(AllowEmptyStrings = false, ErrorMessage = "Name is required.")]
        [StringLength(100, MinimumLength = 1)]
        public string Name { get; set; }

        [Display(Name = "Description")]
        [StringLength(4000)]
        public string Description { get; set; }

        private int _maxPoints = 0;
        /// <summary>
        /// If Points is null or empty use this value to have maxiumum for progress bar display.
        /// </summary>
        [NotMapped]
        [Display(Name = "Points")]
        public int MaxPoints
        {
            get
            {
                if (this.CompetitionProgressBars != null)
                {
                    _maxPoints = CompetitionProgressBars.Max(o => o.Amount);
                }
                else
                {
                    _maxPoints = 0;
                }

                return _maxPoints;
            }
            set
            {
                //this is called by default serializer and passes 0, do not use 
            }
        }

        public bool IsUsingDefaultImage()
        {
            return string.IsNullOrEmpty(ImageSrc) ? true : ImageSrc.Contains(Sweaty_T_Shirt.Controllers.ControllerHelpers.DefaultImageSrc);
        }

        [NotMapped]
        public bool UseDefaultImage {get;set;}

        [Display(Name = "Competition Icon")]
        [StringLength(500)]
        public string ImageSrc { get; set; }

        [Display(Name = "Points")]
        [Integer(ErrorMessage = "Points must be numeric.")]
        public int? Points { get; set; }

        [Display(Name = "End Date")]
        public DateTime? EndDate { get; set; }

        [Display(Name = "Competition Is Active?")]
        public bool IsActive { get; set; }

        /// <summary>
        /// Virtual keyword means the collection is lazy-loaded by default.
        /// </summary>
        public virtual ICollection<SweatyTShirt> SweatyTShirts { get; set; }

        public virtual ICollection<UserInCompetition> UserInCompetitions { get; set; }

        /// <summary>
        /// Progress bars showing competitors and their number of sweaty t-shirts in this competition.
        /// </summary>
        [NotMapped]
        public virtual List<CompetitionProgressBar> CompetitionProgressBars { get; set; }
    }
}