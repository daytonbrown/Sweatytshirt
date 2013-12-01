using System;
using System.Collections.Generic;
using System.Linq;
using System.Transactions;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using DotNetOpenAuth.AspNet;
using Microsoft.Web.WebPages.OAuth;
using WebMatrix.WebData;
using Sweaty_T_Shirt.Filters;
using Sweaty_T_Shirt.Models;
using Sweaty_T_Shirt.DAL;

namespace Sweaty_T_Shirt.Controllers
{
    [Authorize]
    //instead of InitializeSimpleMembership attribute, moved initialization
    //to Application_Start, which calls DAL\SweatyTShirtInitializer.cs Seed()
    //[InitializeSimpleMembership]
    public class AccountController : BaseController
    {
        [Authorize(Roles=AccountRepository.AdminRole)]
        public ActionResult ManageUsers()
        {
            List<UserProfile> users = null;
            using (var accountRepository = new AccountRepository())
            {
                users = accountRepository.GetUserProfiles();
            }

            //if another method redirected to here show the purr message
            if (TempData[ControllerHelpers.PURR] != null)
            {
                ViewBag.Purr = TempData[ControllerHelpers.PURR];
                TempData[ControllerHelpers.PURR] = null;
            }

            return View(users);
        }

        [HttpPost]
        [Authorize(Roles = AccountRepository.AdminRole)]
        //[ValidateAntiForgeryToken]
        public ActionResult DeleteUser(int userId)
        {
            //int userId = int.Parse(formCollection["UserId"]);
            int adminUserID = UserID;
            using (var accountRepository = new AccountRepository())
            {
                accountRepository.DeleteUser(userId, adminUserID);
            }

            //ClosePopup closes the warning popup and causes ManageUsers.cshtml to relocate, 
            //which in turn calls the ManageUsers get action.
            //cannot just set ViewBag because it is lost on redirect.
            //TempData survives one redirect, store it there.
            TempData[ControllerHelpers.PURR] = new Purr() { Title = "Success", Message = "User was successfully deleted." };

            ViewBag.Location = "/Account/ManageUsers";  //ClosePopup will reset location to this
            return View("../Shared/ClosePopup");
        }

        [HttpGet]
        [Authorize(Roles = AccountRepository.AdminRole)]
        public ActionResult EditUser(int userID)
        {
            UserProfile userProfile = GetUserProfile(userID);

            //if another method redirected to here show the purr message
            if (TempData[ControllerHelpers.PURR] != null)
            {
                ViewBag.Purr = TempData[ControllerHelpers.PURR];
                TempData[ControllerHelpers.PURR] = null;
            }

            return View(userProfile);
        }

        private UserProfile GetUserProfile(int userID)
        {
            UserProfile userProfile = null;
            if (userID == 0)
            {
                userProfile = new UserProfile();
            }
            else
            {
                using (var accountRepository = new AccountRepository())
                {
                    userProfile = accountRepository.GetUserProfile(userID);
                }
                string[] userRoles = Roles.GetRolesForUser(userProfile.UserName);
                string[] allRoles = Roles.GetAllRoles();
                userProfile.UserRoles = (from r in allRoles
                                         select new SelectListItem()
                                         {
                                             Text = r,
                                             Selected = userRoles.Contains(r),
                                             Value = r
                                         }).ToList();
            }
            return userProfile;
        }

        [HttpPost]
        [Authorize(Roles = AccountRepository.AdminRole)]
        public ActionResult EditUser(UserProfile userProfile)
        {
            int userID = 0;
            using (var accountRepository = new AccountRepository())
            {
                userID = accountRepository.EditUserProfile(userProfile);
            }

            ViewBag.Purr = new Purr() { Title = "Success", Message = "User was successfully edited." };

            userProfile = GetUserProfile(userID);

            return View(userProfile);
        }

        // GET: /Account/Login
        [AllowAnonymous]
        public ActionResult Login(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        // POST: /Account/Login
        [HttpPost]
        [AllowAnonymous]
        //[ValidateAntiForgeryToken]
        public ActionResult Login(LoginModel model, string returnUrl)
        {
            if (string.IsNullOrEmpty(model.Email))
            {
                throw new ApplicationException("Missing required property model.Email");
            }

            UserProfile userProfile = null;
            using (SweatyTShirtContext context = new SweatyTShirtContext())
            {
                userProfile = context.UserProfiles.FirstOrDefault(o => !string.IsNullOrEmpty(o.Email) && o.Email.ToUpper() == model.Email.ToUpper());
            }

            if (userProfile != null)
            {
                if (ModelState.IsValid && WebSecurity.Login(userProfile.UserName, AccountRepository.AllUsersPassword, persistCookie: model.RememberMe))
                {
                    return RedirectToLocal(returnUrl);
                }
            }

            // If we got this far, something failed, redisplay form
            ModelState.AddModelError("", "The email provided is not registered yet, please click the 'Register' link to register.");
            return View(model);
        }

        // POST: /Account/LogOff
        [HttpPost]
        //[ValidateAntiForgeryToken]
        public ActionResult LogOff()
        {
            WebSecurity.Logout();
            Session.Remove(FacebookRepository.FB_ACCESS_TOKEN);
            Session.Remove(FacebookRepository.FB_ID);
            Session.Remove(FacebookRepository.IS_FB_AUTHENTICATED);

            return RedirectToAction("Index", "Home");
        }

        //
        // GET: /Account/Register

        [AllowAnonymous]
        public ActionResult Register()
        {
            return View();
        }

        //
        // POST: /Account/Register

        [HttpPost]
        [AllowAnonymous]
        //[ValidateAntiForgeryToken]
        public ActionResult Register(RegisterModel model)
        {
            if (ModelState.IsValid)
            {
                if (string.IsNullOrEmpty(model.Email))
                {
                    throw new ApplicationException("Missing required property model.Email");
                }

                UserProfile userProfile = null;
                using (SweatyTShirtContext context = new SweatyTShirtContext())
                {
                    userProfile = context.UserProfiles.FirstOrDefault(o => !string.IsNullOrEmpty(o.Email) && o.Email.ToUpper() == model.Email.ToUpper());
                }

                if (userProfile == null)
                {
                    // Attempt to register the user
                    try
                    {
                        string userName = null;
                        using (AccountRepository repository = new AccountRepository())
                        {
                            userName = repository.CreateUser(model.FullName, model.Email);
                        }
                        WebSecurity.Login(userName, AccountRepository.AllUsersPassword);
                        return RedirectToAction("Index", "Home");
                    }
                    catch (MembershipCreateUserException e)
                    {
                        ModelState.AddModelError("", ErrorCodeToString(e.StatusCode));
                    }
                }
                else
                {
                    ModelState.AddModelError("", string.Format("The email address '{0}' is already registered, please click the 'Login' link to log in.", model.Email));
                }
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        //
        // POST: /Account/Disassociate

        [HttpPost]
        //[ValidateAntiForgeryToken]
        public ActionResult Disassociate(string provider, string providerUserId)
        {
            string ownerAccount = OAuthWebSecurity.GetUserName(provider, providerUserId);
            ManageMessageId? message = null;

            // Only disassociate the account if the currently logged in user is the owner
            if (ownerAccount == User.Identity.Name)
            {
                // Use a transaction to prevent the user from deleting their last login credential
                using (var scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = IsolationLevel.Serializable }))
                {
                    bool hasLocalAccount = OAuthWebSecurity.HasLocalAccount(UserID);
                    if (hasLocalAccount || OAuthWebSecurity.GetAccountsFromUserName(User.Identity.Name).Count > 1)
                    {
                        OAuthWebSecurity.DeleteAccount(provider, providerUserId);
                        scope.Complete();
                        message = ManageMessageId.RemoveLoginSuccess;
                    }
                }
            }

            return RedirectToAction("Manage", new { Message = message });
        }

        //
        // GET: /Account/Manage

        public ActionResult Manage(ManageMessageId? message)
        {
            ViewBag.StatusMessage =
                message == ManageMessageId.ChangePasswordSuccess ? "Your password has been changed."
                : message == ManageMessageId.SetPasswordSuccess ? "Your password has been set."
                : message == ManageMessageId.RemoveLoginSuccess ? "The external login was removed."
                : "";
            ViewBag.HasLocalPassword = OAuthWebSecurity.HasLocalAccount(UserID);
            ViewBag.ReturnUrl = Url.Action("Manage");
            return View();
        }

        //
        // POST: /Account/Manage

        [HttpPost]
        //[ValidateAntiForgeryToken]
        public ActionResult Manage(LocalPasswordModel model)
        {
            bool hasLocalAccount = OAuthWebSecurity.HasLocalAccount(UserID);
            ViewBag.HasLocalPassword = hasLocalAccount;
            ViewBag.ReturnUrl = Url.Action("Manage");
            if (hasLocalAccount)
            {
                if (ModelState.IsValid)
                {
                    // ChangePassword will throw an exception rather than return false in certain failure scenarios.
                    bool changePasswordSucceeded;
                    try
                    {
                        changePasswordSucceeded = WebSecurity.ChangePassword(User.Identity.Name, model.OldPassword, model.NewPassword);
                    }
                    catch (Exception)
                    {
                        changePasswordSucceeded = false;
                    }

                    if (changePasswordSucceeded)
                    {
                        return RedirectToAction("Manage", new { Message = ManageMessageId.ChangePasswordSuccess });
                    }
                    else
                    {
                        ModelState.AddModelError("", "The current password is incorrect or the new password is invalid.");
                    }
                }
            }
            else
            {
                // User does not have a local password so remove any validation errors caused by a missing
                // OldPassword field
                ModelState state = ModelState["OldPassword"];
                if (state != null)
                {
                    state.Errors.Clear();
                }

                if (ModelState.IsValid)
                {
                    try
                    {
                        WebSecurity.CreateAccount(User.Identity.Name, model.NewPassword);
                        return RedirectToAction("Manage", new { Message = ManageMessageId.SetPasswordSuccess });
                    }
                    catch (Exception e)
                    {
                        ModelState.AddModelError("", e);
                    }
                }
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        //
        // POST: /Account/ExternalLogin
        [HttpPost]
        [AllowAnonymous]
        //[ValidateAntiForgeryToken]
        public ActionResult ExternalLogin(string provider, string returnUrl)
        {
            return new ExternalLoginResult(provider, Url.Action("ExternalLoginCallback", new { ReturnUrl = returnUrl }));
        }

        [HttpGet]
        [Authorize]
        public ActionResult ExternalLoginGet(string provider, string returnUrl)
        {
            return new ExternalLoginResult(provider, Url.Action("ExternalLoginCallback", new { ReturnUrl = returnUrl }));
        }

        //
        // GET: /Account/ExternalLoginCallback

        [AllowAnonymous]
        public ActionResult ExternalLoginCallback(string returnUrl)
        {
            AuthenticationResult result = OAuthWebSecurity.VerifyAuthentication(Url.Action("ExternalLoginCallback", new { ReturnUrl = returnUrl }));
            if (!result.IsSuccessful)
            {
                return RedirectToAction("ExternalLoginFailure");
            }

            //needed to post to Facebook.
            if (result.ExtraData.Keys.Contains(FacebookRepository.FB_ACCESS_TOKEN))
            {
                Session[FacebookRepository.FB_ACCESS_TOKEN] = result.ExtraData[FacebookRepository.FB_ACCESS_TOKEN];
            }
            if (result.ExtraData.Keys.Contains(FacebookRepository.FB_ID))
            {
                Session[FacebookRepository.FB_ID] = result.ExtraData[FacebookRepository.FB_ID];
            }
            Session[FacebookRepository.IS_FB_AUTHENTICATED] = true;

            if (OAuthWebSecurity.Login(result.Provider, result.ProviderUserId, createPersistentCookie: false))
            {
                return RedirectToLocal(returnUrl);
            }

            if (User.Identity.IsAuthenticated)
            {
                //the current user is already logged in as local user, add the new external account pointer.
                OAuthWebSecurity.CreateOrUpdateAccount(result.Provider, result.ProviderUserId, User.Identity.Name);
                return RedirectToLocal(returnUrl);
            }
            else
            {
                // User is new, get their full name and email.
                string loginData = OAuthWebSecurity.SerializeProviderUserId(result.Provider, result.ProviderUserId);
                ViewBag.ProviderDisplayName = OAuthWebSecurity.GetOAuthClientData(result.Provider).DisplayName;
                ViewBag.ReturnUrl = returnUrl;
                string fullName = string.IsNullOrEmpty(result.ExtraData["name"]) ? result.UserName : result.ExtraData["name"];
                return View("ExternalLoginConfirmation", new RegisterExternalLoginModel
                {
                    UserName = result.UserName,  //this UserName is not actually used anywhere, I create guid for username.
                    ExternalLoginData = loginData,
                    FullName = fullName
                });
            }
        }

        //
        // POST: /Account/ExternalLoginConfirmation

        [HttpPost]
        [AllowAnonymous]
        //[ValidateAntiForgeryToken]
        public ActionResult ExternalLoginConfirmation(RegisterExternalLoginModel model, string returnUrl)
        {
            string provider = null;
            string providerUserId = null;

            if (User.Identity.IsAuthenticated || !OAuthWebSecurity.TryDeserializeProviderUserId(model.ExternalLoginData, out provider, out providerUserId))
            {
                return RedirectToAction("Manage");
            }

            if (ModelState.IsValid)
            {
                // Insert a new user into the database
                using (SweatyTShirtContext context = new SweatyTShirtContext())
                {
                    UserProfile emailMatch = context.UserProfiles.FirstOrDefault(u => !string.IsNullOrEmpty(u.Email) && u.Email.ToLower() == model.Email.ToLower());

                    // Check if user already exists
                    if (emailMatch == null)
                    {
                        //add the user locally.  now the user can login using the login page or conintue to use their external login,
                        //it is the user's choice.
                        string userName = null;
                        using (AccountRepository accountRepository = new AccountRepository(context))
                        {
                            userName = accountRepository.CreateUser(model.FullName, model.Email);
                        }
                        
                        OAuthWebSecurity.CreateOrUpdateAccount(provider, providerUserId, userName);
                        OAuthWebSecurity.Login(provider, providerUserId, createPersistentCookie: false);
                        Session[FacebookRepository.IS_FB_AUTHENTICATED] = true;

                        return RedirectToLocal(returnUrl);
                    }
                    else
                    {
                        if (emailMatch != null)
                        {
                            ModelState.AddModelError("Email", string.Format("A user with the email '{0}', full name '{1}' already exists, please click the 'Login' link and log in.", emailMatch.Email, emailMatch.FullName));
                        }
                    }
                }
            }

            ViewBag.ProviderDisplayName = OAuthWebSecurity.GetOAuthClientData(provider).DisplayName;
            ViewBag.ReturnUrl = returnUrl;
            return View(model);
        }

        //
        // GET: /Account/ExternalLoginFailure

        [AllowAnonymous]
        public ActionResult ExternalLoginFailure()
        {
            return View();
        }

        [AllowAnonymous]
        [ChildActionOnly]
        public ActionResult ExternalLoginsList(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            return PartialView("_ExternalLoginsListPartial", OAuthWebSecurity.RegisteredClientData);
        }

        [ChildActionOnly]
        public ActionResult RemoveExternalLogins()
        {
            ICollection<OAuthAccount> accounts = OAuthWebSecurity.GetAccountsFromUserName(User.Identity.Name);
            List<ExternalLogin> externalLogins = new List<ExternalLogin>();
            foreach (OAuthAccount account in accounts)
            {
                AuthenticationClientData clientData = OAuthWebSecurity.GetOAuthClientData(account.Provider);

                externalLogins.Add(new ExternalLogin
                {
                    Provider = account.Provider,
                    ProviderDisplayName = clientData.DisplayName,
                    ProviderUserId = account.ProviderUserId,
                });
            }

            ViewBag.ShowRemoveButton = externalLogins.Count > 1 || OAuthWebSecurity.HasLocalAccount(UserID);
            return PartialView("_RemoveExternalLoginsPartial", externalLogins);
        }

        #region Helpers
        private ActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
        }

        public enum ManageMessageId
        {
            ChangePasswordSuccess,
            SetPasswordSuccess,
            RemoveLoginSuccess,
        }

        internal class ExternalLoginResult : ActionResult
        {
            public ExternalLoginResult(string provider, string returnUrl)
            {
                Provider = provider;
                ReturnUrl = returnUrl;
            }

            public string Provider { get; private set; }
            public string ReturnUrl { get; private set; }

            public override void ExecuteResult(ControllerContext context)
            {
                OAuthWebSecurity.RequestAuthentication(Provider, ReturnUrl);
            }
        }

        private static string ErrorCodeToString(MembershipCreateStatus createStatus)
        {
            // See http://go.microsoft.com/fwlink/?LinkID=177550 for
            // a full list of status codes.
            switch (createStatus)
            {
                case MembershipCreateStatus.DuplicateUserName:
                    return "User name already exists. Please enter a different user name.";

                case MembershipCreateStatus.DuplicateEmail:
                    return "A user name for that e-mail address already exists. Please enter a different e-mail address.";

                case MembershipCreateStatus.InvalidPassword:
                    return "The password provided is invalid. Please enter a valid password value.";

                case MembershipCreateStatus.InvalidEmail:
                    return "The e-mail address provided is invalid. Please check the value and try again.";

                case MembershipCreateStatus.InvalidAnswer:
                    return "The password retrieval answer provided is invalid. Please check the value and try again.";

                case MembershipCreateStatus.InvalidQuestion:
                    return "The password retrieval question provided is invalid. Please check the value and try again.";

                case MembershipCreateStatus.InvalidUserName:
                    return "The user name provided is invalid. Please check the value and try again.";

                case MembershipCreateStatus.ProviderError:
                    return "The authentication provider returned an error. Please verify your entry and try again. If the problem persists, please contact your system administrator.";

                case MembershipCreateStatus.UserRejected:
                    return "The user creation request has been canceled. Please verify your entry and try again. If the problem persists, please contact your system administrator.";

                default:
                    return "An unknown error occurred. Please verify your entry and try again. If the problem persists, please contact your system administrator.";
            }
        }
        #endregion
    }
}
