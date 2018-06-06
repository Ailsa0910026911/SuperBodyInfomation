using LokFu.Infrastructure;
using LokFu.Models;
using LokFu.Repositories;
using LokFu.Extensions;
using LokFu.Repositories.SqlServer;
using System.Linq;
using System.Web.Mvc;
using System;
namespace LokFu.Areas.Agent.Controllers
{
    public class LoginController : InitController
    {
        public ActionResult Index()
        {
            SysSet SysSet = Entity.SysSet.FirstOrNew();
            ViewBag.BasicSet = SysSet;
            return View();
        }
        public void CHKLogin(string userName, string passWord, string saveUsername)
        {
            if (!saveUsername.IsNullOrEmpty())  
            {
                var cookie = new System.Web.HttpCookie("saveUsername");
                cookie.Values.Add("saveUsername", userName);
                cookie.Expires = DateTime.Now.AddMonths(1);
                this.HttpContext.Response.Cookies.Add(cookie);
            }
            passWord = passWord.GetAdminMD5();
            SysAdmin user1 = Entity.SysAdmin.Where(o => o.UserName == userName && o.PassWord == passWord).FirstOrDefault();
            SysAdmin user = Entity.SysAdmin.Where(n => n.UserName == userName && n.PassWord == passWord && n.AgentId > 0).FirstOrDefault();
            if (user != null)
            {
                if (user.State == 0)
                {
                    Response.Redirect("/agent/login.html?IsError=2");
                    return;
                }
                SysAgent SysAgent = Entity.SysAgent.FirstOrNew(n => n.Id == user.AgentId);
                if (SysAgent.Id.IsNullOrEmpty()) {
                    Response.Redirect("/agent/login.html?IsError=4");
                    return;
                }
                if (SysAgent.State != 1)
                {
                    Response.Redirect("/agent/login.html?IsError=4");
                    return;
                }
                DateTime now = DateTime.Now;
                string neiw = System.Configuration.ConfigurationManager.AppSettings["key"].ToString();
                string UserNameAndPassWord = LokFuEncode.LokFuAuthcodeEncode(string.Format("{0}|{1}|{2}", user.Id, userName, now.ToString("yyyy-MM-dd HH:mm")), neiw);
                Response.Cookies.SetAgent(UserNameAndPassWord);
                user.LoginTimes++;
                user.LastTime = now;
                user.LoginIp = Tools.GetIp();
                Entity.SaveChanges();
                Response.Redirect("/agent/Home/Index.html");
                return;
            }
            else
            {
                Response.Redirect("/agent/login.html?IsError=1");
            }
            return;
        }
        public object RemoveLogin()
        {
            Response.Cookies.SetAgent(string.Empty);
            return Redirect("/agent/login.html");
        }
    }
}
