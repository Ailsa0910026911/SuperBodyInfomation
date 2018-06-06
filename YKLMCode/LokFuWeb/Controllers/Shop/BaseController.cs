using System;
using System.Collections.Generic;
using System.Linq;
using LokFu.Repositories;
using LokFu.Infrastructure;
using LokFu.Extensions;
using System.Web.Mvc;
namespace LokFu.Areas.Shop.Controllers
{
    public class BaseController : InitController
    {
        public Users BasicUsers;
        public SysSet BasicSet;
        public BaseController() {
            BasicUsers = GetUsers();
            if (BasicUsers == null)
            {
                System.Web.HttpContext.Current.Response.Redirect("/Shop/Login.html");
                System.Web.HttpContext.Current.Response.End();
                ViewBag.View("blank");
                return;
            }
            ViewBag.BasicUsers = BasicUsers;//登录用户
            BasicSet = Entity.SysSet.FirstOrNew();
            ViewBag.BasicSet = BasicSet;
        }
        public Users GetUsers()
        {
            Users user = null;
            string Str = System.Web.HttpContext.Current.Request.Cookies.GetUsers();
            if (Str.IsNullOrEmpty()){
                return user;
            }
            string neiw = System.Configuration.ConfigurationManager.AppSettings["key"];
            string[] UArr = LokFuEncode.LokFuAuthcodeDecode(Str, neiw).Split('|');
            if (UArr.Length == 3)//id|username|md5
            {
                int Id = Int32.Parse(UArr[0]);
                string UName = UArr[1];
                string DTStr = UArr[2];
                user = Entity.Users.Where(n => n.UserName == UName && n.Id == Id && n.PassWord == DTStr).FirstOrDefault();
            }
            return user;
        }
    }
}