using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using DataAnnotationsExtensions;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel;

namespace Sweaty_T_Shirt.Models
{
    public class UserInCompetition : BaseModel
    {
        public UserInCompetition()
        {
            SendEmail = true;
        }

        [Key, Column(Order=0)]
        [Required]
        [ForeignKey("Competition")]
        public long CompetitionID { get; set; }

        public virtual Competition Competition { get; set; }

        [Key, Column(Order=1)]
        [Required]
        [ForeignKey("UserProfile")]
        public int UserID { get; set; }

        public virtual UserProfile UserProfile { get; set; }

        [Display(Name="Date Invited")]
        [Required(ErrorMessage="Date Invited is required.")]
        public DateTime DateInvited { get; set; }

        [Display(Name = "Date Accepted")]
        public DateTime? DateAccepted { get; set; }

        [Display(Name="Remain in Competion")]
        public bool IsActive { get; set; }

        [NotMapped]
        [Display(Name = "Full Name")]
        [Required(ErrorMessage = "Full Name is required.")]
        [StringLength(100, ErrorMessage = "The maximum allowed length of Full Name is 100 characters")]
        public string FullName { get; set; }

        [NotMapped]
        [Required(ErrorMessage = "Email is required")]
        [Display(Name = "Email")]
        [DataType(DataType.EmailAddress, ErrorMessage = "Email does not have a valid email address format.")]
        [StringLength(100, ErrorMessage = "The maximum allowed length of Email is 100 characters")]
        public string Email { get; set; }

        [NotMapped]
        [Display(Name = "Send Notification Email Now?")]
        [DefaultValue(true)]
        public bool SendEmail { get; set; }

    }
}