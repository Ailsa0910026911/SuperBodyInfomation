using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace CMSManage.Controllers
{// 登录认证特性
    public class AuthenticationAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (filterContext.HttpContext.Session["name"] == null)
                filterContext.HttpContext.Response.Redirect("/Default/Login");

            base.OnActionExecuting(filterContext);
        }
    }
}