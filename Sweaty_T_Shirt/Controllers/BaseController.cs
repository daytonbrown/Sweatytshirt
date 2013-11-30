using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Sweaty_T_Shirt.Controllers
{
    public class BaseController : Controller
    {
        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            System.Web.HttpContext.Current.Items[ControllerHelpers.ISADMIN] = User.IsInRole(Sweaty_T_Shirt.DAL.AccountRepository.AdminRole);
            base.OnActionExecuting(filterContext);
        }
    }
}
