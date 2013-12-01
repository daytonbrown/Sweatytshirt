using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebMatrix.WebData;

namespace Sweaty_T_Shirt.Controllers
{
    public class BaseController : Controller
    {
        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            //this is needed on _Layout.cshtml
            System.Web.HttpContext.Current.Items["IsAdmin"] = (bool?)User.IsInRole(Sweaty_T_Shirt.DAL.AccountRepository.AdminRole);
            base.OnActionExecuting(filterContext);
        }
        protected bool IsUserAdmin
        {
            get
            {
                bool? isUserAdmin = (bool?)System.Web.HttpContext.Current.Items["IsAdmin"];
                if (!isUserAdmin.HasValue)
                {
                    isUserAdmin = (bool?)User.IsInRole(Sweaty_T_Shirt.DAL.AccountRepository.AdminRole);
                    System.Web.HttpContext.Current.Items["IsAdmin"] = isUserAdmin;
                }
                return isUserAdmin.Value;
            }
        }

        protected int UserID
        {
            get
            {
                int? userID = (int?)System.Web.HttpContext.Current.Items["UserID"];
                if (!userID.HasValue)
                {
                    userID = (int?)WebSecurity.GetUserId(User.Identity.Name);
                    if (!userID.HasValue || userID <= 0)
                    {
                        throw new ApplicationException(string.Format("Unable to retrieve UserID for '{0}'", User.Identity.Name));
                    }
                    System.Web.HttpContext.Current.Items["UserID"] = userID;
                }
                return userID.Value;
            }
        }
    }
}
