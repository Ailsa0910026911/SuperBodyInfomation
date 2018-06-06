using System;
using System.Collections.Generic;
using System.Linq;
using LokFu.Repositories;
using LokFu.Extensions;
using LokFu.Infrastructure;
namespace LokFu.Areas.Mobile.Controllers
{
    public class BaseController : InitController
    {
        public SysAdmin AdminUser;
        public SysAgent BasicAgent;
        public SysSet BasicSet;
        public Users BasicUsers;
        public bool IsLokFu = false;
        public bool IsApple = false;
        public bool IsAndroid = false;
        public BaseController() {
            AdminUser = new SysAdmin();
            BasicAgent = new SysAgent();
            BasicUsers = new Users();
            string mycode = "";
            if (System.Web.HttpContext.Current.Request.Form["mycode"] != null)
            {
                mycode = System.Web.HttpContext.Current.Request.Form["mycode"].ToString();
            }
            if (System.Web.HttpContext.Current.Request.QueryString["mycode"] != null)
            {
                mycode = System.Web.HttpContext.Current.Request.QueryString["mycode"].ToString();
            }
            if (mycode.IsNullOrEmpty())
            {
                mycode = System.Web.HttpContext.Current.Request.Cookies.GetMyCode();
            }
            else {
                System.Web.HttpContext.Current.Response.Cookies.SetMyCode(mycode);
            }
            if (!mycode.IsNullOrEmpty())
            {
                string myid = LokFuEncode.Base64Decode(mycode);
                if (!myid.IsNullOrEmpty())
                {
                    int MyId = Int32.Parse(myid);
                    AdminUser = Entity.SysAdmin.FirstOrNew(n => n.Id == MyId);
                    BasicAgent = Entity.SysAgent.FirstOrNew(n => n.Id == AdminUser.AgentId);
                }
            }
            string mytoken = "";
            if (System.Web.HttpContext.Current.Request.Form["token"] != null)
            {
                mytoken = System.Web.HttpContext.Current.Request.Form["token"].ToString();
            }
            if (System.Web.HttpContext.Current.Request.QueryString["token"] != null)
            {
                mytoken = System.Web.HttpContext.Current.Request.QueryString["token"].ToString();
            }
            if (mytoken.IsNullOrEmpty())
            {
                mytoken = System.Web.HttpContext.Current.Request.Cookies.GetUserToken();
            }
            else
            {
                System.Web.HttpContext.Current.Response.Cookies.SetUserToken(mytoken);
            }
            if (!mytoken.IsNullOrEmpty())
            {
                BasicUsers = Entity.Users.FirstOrNew(n => n.Token == mytoken);
                if (!BasicUsers.Agent.IsNullOrEmpty() && BasicAgent.Id.IsNullOrEmpty()) {
                    BasicAgent = Entity.SysAgent.FirstOrNew(n => n.Id == BasicUsers.Agent);
                }
            }
            String userAgent;
            userAgent = System.Web.HttpContext.Current.Request.UserAgent;
            if (userAgent.IndexOf("HaoFu") > -1)
            {
                IsLokFu = true;
            }
            if (userAgent.IndexOf("HaoFu_iPhone") > -1)
            {
                IsApple = true;
            }
            if (userAgent.IndexOf("HaoFu_Android") > -1)
            {
                IsAndroid = true;
            }
            ViewBag.AdminUser = AdminUser;//用户
            ViewBag.BasicAgent = BasicAgent;
            ViewBag.BasicUsers = BasicUsers;
            BasicSet = Entity.SysSet.FirstOrNew();
            ViewBag.BasicSet = BasicSet;
            ViewBag.IsLokFu = IsLokFu;
            ViewBag.IsAndroid = IsAndroid;
            ViewBag.IsApple = IsApple;
        }
    }
}