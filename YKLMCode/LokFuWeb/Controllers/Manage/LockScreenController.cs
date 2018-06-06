using LokFu.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
namespace LokFu.Areas.Manage.Controllers
{
    public class LockScreenController : BaseController
    {
        //
        // GET: /LockScreen/
        public LockScreenController()
        {
            ViewBag.Authorization = true;//允许权限
        }
        public ActionResult Index()
        {
            if (AdminUser == null)
            {
                ViewBag.Error = "2";
                return View();
            }
            return View();
        }
        /// <summary>
        /// 验证锁屏密码是否正确
        /// </summary>
        /// <param name="PassWord">输入的锁屏密码(验证是否和用户密码一样)</param>
        [AdminFilter(true, "解锁屏幕")]
        public void CHK_PWD(string PassWord)
        {
            AdminUser = GetAdmin();
            if (AdminUser == null)
            {
                return;
            }
            PassWord = PassWord.GetAdminMD5();
            if (PassWord.Equals(AdminUser.PassWord))
            {
                string neiw = System.Configuration.ConfigurationManager.AppSettings["key"];
                //DateTime Now = DateTime.Now;
                //AdminUser.LastTime = Now;
                //Entity.SaveChanges();
                string UserNameAndPassWord = LokFuEncode.LokFuAuthcodeEncode(string.Format("{0}|{1}|{2}|{3}", AdminUser.Id, AdminUser.UserName, AdminUser.LastTime.Value.ToString("yyyy-MM-dd HH:mm:ss"), "0"), neiw);
                Response.Cookies.SetAdmin(UserNameAndPassWord);
                Response.Write("1");
            }
            else {
                Response.Write("0");
            }
        }
        /// <summary>
        /// 打开锁屏
        /// </summary>
        [AdminFilter(true, "锁屏")]
        public void LockScreenON()
        {
            string neiw = System.Configuration.ConfigurationManager.AppSettings["key"];
            string UserNameAndPassWord = LokFuEncode.LokFuAuthcodeEncode(string.Format("{0}|{1}|{2}|{3}", AdminUser.Id, AdminUser.UserName, AdminUser.LastTime.Value.ToString("yyyy-MM-dd HH:mm:ss"), "1"), neiw);
            Response.Cookies.SetAdmin(UserNameAndPassWord);
        }
    }
}
