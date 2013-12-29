using DataAnnotationsExtensions;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Globalization;
using System.Web.Mvc;
using System.Web.Security;

/// <summary>
/// Note, commented out models with passwords, anticipating that passwords might be used some day.  
/// Not using any model that has Password property, and removed password from Views.
/// </summary>
namespace Sweaty_T_Shirt.Models
{
    #region NoPasswordModels
    public class LoginModel
    {
        [Required(ErrorMessage = "Email is required")]
        [Display(Name = "Email")]
        [DataType(DataType.EmailAddress, ErrorMessage = "Email does not have a valid email address format.")]
        [StringLength(100, ErrorMessage = "The maximum allowed length of Email is 100 characters")]
        public string Email { get; set; }

        [Display(Name = "Remember me?")]
        public bool RememberMe { get; set; }
    }

    public class RegisterModel
    {
        [Display(Name = "Full Name")]
        [Required(ErrorMessage = "Full Name is required.")]
        [StringLength(100, ErrorMessage = "The maximum allowed length of Full Name is 100 characters")]
        public string FullName { get; set; }

        [Required(ErrorMessage = "Email is required")]
        [Display(Name = "Email")]
        [DataType(DataType.EmailAddress, ErrorMessage = "Email does not have a valid email address format.")]
        [StringLength(100, ErrorMessage = "The maximum allowed length of Email is 100 characters")]
        public string Email { get; set; }
    }
    #endregion

    [Table("UserProfile")]
    public class UserProfile
    {
        [Key]
        [DatabaseGeneratedAttribute(DatabaseGeneratedOption.Identity)]
        public int UserId { get; set; }
        public string UserName { get; set; }

        public string Email { get; set; }
        public string FullName { get; set; }

        public DateTime LastEmailSent { get; set; }

        [NotMapped]
        public bool IsImmediateNotification { get; set; }

        [Display(Name="Notifications")]
        [Integer(ErrorMessage="Notification setting must be numeric.")]
        public int? Notifications { get; set; }

        /// <summary>
        /// Only used by EditUser page, to add or remove user from role(s).
        /// </summary>
        [NotMapped]
        [Display(Name = "Roles")]
        public List<SelectListItem> UserRoles{get;set;}

        /// <summary>
        /// Virtual keyword means the collection is lazy-loaded by default.  These collections
        /// are only accessed via the ManageUsers page, only seen by administrator.
        /// </summary>
        public virtual ICollection<SweatyTShirt> SweatyTShirts { get; set; }
        public virtual ICollection<UserInCompetition> UserInCompetitions { get; set; }
        public virtual ICollection<Competition> Competitions { get; set; }
    }

    public class RegisterExternalLoginModel
    {
        [Required]
        [Display(Name = "User name")]
        public string UserName { get; set; }

        public string ExternalLoginData { get; set; }

        /// <summary>
        /// 2 properties below added by me.
        /// </summary>
        [Display(Name = "Full Name")]
        [Required(ErrorMessage = "Full Name is required.")]
        [StringLength(100, ErrorMessage = "The maximum allowed length of Full Name is 100 characters")]
        public string FullName { get; set; }

        [Required(ErrorMessage = "Email is required")]
        [Display(Name = "Email")]
        [DataType(DataType.EmailAddress, ErrorMessage = "Email does not have a valid email address format.")]
        [StringLength(100, ErrorMessage = "The maximum allowed length of Email is 100 characters")]
        public string Email { get; set; }
    }

    public class LocalPasswordModel
    {
        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Current password")]
        public string OldPassword { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "New password")]
        public string NewPassword { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm new password")]
        //[System.ComponentModel.DataAnnotations.Compare("NewPassword", ErrorMessage = "The new password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }
    }
/*
    public class LoginModel
    {
        [Required]
        [Display(Name = "User name")]
        public string UserName { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [Display(Name = "Remember me?")]
        public bool RememberMe { get; set; }
    }

    public class RegisterModel
    {
        [Required]
        [Display(Name = "User name")]
        public string UserName { get; set; }

        [Required]
        [Display(Name = "Email")]
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }
    }
*/
    public class ExternalLogin
    {
        public string Provider { get; set; }
        public string ProviderDisplayName { get; set; }
        public string ProviderUserId { get; set; }
    }
}
