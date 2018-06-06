using LokFu.Extensions;
using LokFu.Infrastructure;
using LokFu.Repositories;
using System;
using System.Configuration;
using System.Linq;
using System.Web.Mvc;
namespace LokFu.Areas.Manage.Controllers
{
    public class LoginController : InitController
    {
        public ActionResult Index()
        {
            SysSet SysSet = Entity.SysSet.FirstOrNew();
            ViewBag.BasicSet = SysSet;
            return View();
        }
        public void CHKLogin(string userName, string passWord)
        {
            passWord = passWord.GetAdminMD5();
            SysAdmin user = Entity.SysAdmin.Where(n => n.UserName == userName && n.PassWord == passWord && n.AgentId == 0).FirstOrDefault();
            if (user != null)
            {
                if (user.State == 0)
                {
                    Response.Redirect("/manage/login.html?IsError=2");
                    return;
                }
                DateTime now = DateTime.Now;
                string neiw = System.Configuration.ConfigurationManager.AppSettings["key"].ToString();
                string IsLockScreen = "0";//锁屏状态 登录时默认是不锁屏(1为锁屏,0为不锁屏)
                string UserNameAndPassWord = LokFuEncode.LokFuAuthcodeEncode(string.Format("{0}|{1}|{2}|{3}", user.Id, userName, now.ToString("yyyy-MM-dd HH:mm:ss"), IsLockScreen), neiw);
                Response.Cookies.SetAdmin(UserNameAndPassWord);
                user.LoginTimes++;
                user.LastTime = now;
                user.LoginIp = Tools.GetIp();
                if (!user.QQNum.IsNullOrEmpty()) {
                    user.QQState = 1;
                }
                Entity.SaveChanges();
                Response.Redirect("/manage/Home/Index.html");
                return;
            }
            else
            {
                Response.Redirect("/manage/login.html?IsError=1");
            }
            return;
        }
        public object RemoveLogin()
        {
            string neiw = ConfigurationManager.AppSettings["key"];
            string Str = Request.Cookies.GetAdmin();
            if (Str.IsNullOrEmpty())
            {
                return Redirect("/manage/login.html");
            }
            string[] UArr = LokFuEncode.LokFuAuthcodeDecode(Str, neiw).Split('|');
            if (UArr.Length == 4)
            {
                int Id = Int32.Parse(UArr[0]);
                string UName = UArr[1];
                string DTStr = UArr[2];
                SysAdmin user = Entity.SysAdmin.Where(n => n.UserName == UName && n.Id == Id).FirstOrDefault();
                if (user != null)
                {
                    DateTime now = (DateTime)user.LastTime;
                    if (DTStr == now.ToString("yyyy-MM-dd HH:mm:ss"))
                    {
                        user.QQState = 0;
                        Entity.SaveChanges();
                    }
                }
            }
            Response.Cookies.SetAdmin(string.Empty);
            return Redirect("/manage/login.html");
        }
    }
}
