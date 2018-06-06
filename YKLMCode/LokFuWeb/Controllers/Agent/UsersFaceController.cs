using LokFu.Extensions;
using LokFu.Infrastructure;
using LokFu.Models;
using LokFu.Repositories;
using LokFu.Repositories.SqlServer;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
namespace LokFu.Areas.Agent.Controllers
{
    public class UsersFaceController : BaseController
    {

        public ActionResult Index(UsersFace UsersFace, EFPagingInfo<UsersFace> p, int IsFirst = 0)
        {
            if (IsFirst == 0)
            {
                PageOfItems<UsersFace> UsersFaceList1 = new PageOfItems<UsersFace>(new List<UsersFace>(), 0, 10, 0, new Hashtable());
                ViewBag.UsersFaceList = UsersFaceList1;
                ViewBag.UsersFace = UsersFace;
                ViewBag.SysAdminList = Entity.SysAdmin.Where(n => n.State == 1 && n.AgentId == BasicAgent.Id).ToList();
                return View();
            }
            //代理绑定子帐户不显示
            p.SqlWhere.Add(f => f.IsDaiLi == 0 && f.CType == 1);
            if (checkPower("ALL"))
            {
                p.SqlWhere.Add(f => f.Agent == BasicAgent.Id);//读取全部分支机构
            }
            else
            {
                p.SqlWhere.Add(f => f.AId == AdminUser.Id);//读取用户
            }
            if (!UsersFace.TrueName.IsNullOrEmpty()) { p.SqlWhere.Add(f => f.TrueName.Contains(UsersFace.TrueName)); }
            if (!UsersFace.Mobile.IsNullOrEmpty()) { p.SqlWhere.Add(f => f.Mobile.Contains(UsersFace.Mobile)); }
            if (!UsersFace.CardStae.IsNullOrEmpty()) { p.SqlWhere.Add(f => f.CardStae == UsersFace.CardStae); }
            if (!UsersFace.MobileType.IsNullOrEmpty()) { p.SqlWhere.Add(f => f.MobileType == UsersFace.MobileType); }
            if (!UsersFace.State.IsNullOrEmpty()) { p.SqlWhere.Add(f => f.State == UsersFace.State); }
            p.OrderByList.Add("UpdateTime", "DESC");
            IPageOfItems<UsersFace> UsersFaceList = Entity.Selects<UsersFace>(p);
            ViewBag.UsersFaceList = UsersFaceList;
            ViewBag.UsersFace = UsersFace;
            ViewBag.SysAdminList = Entity.SysAdmin.Where(n => n.State == 1 && n.AgentId == BasicAgent.Id).ToList();
            return View();
        }
        public ActionResult Edit(UsersFace UsersFace)
        {
            ViewBag.Remark = UsersFace.Remark;
            if (UsersFace.Id != 0) UsersFace = Entity.UsersFace.FirstOrDefault(n => n.Id == UsersFace.Id && n.Agent == BasicAgent.Id);
            if (UsersFace==null)
            {
                ViewBag.ErrorMsg = AgentLanguage.Empty;
                return View("Error");
            }
            ViewBag.UsersFace = UsersFace;
            if (Request.UrlReferrer != null)
            {
                Session["Url"] = Request.UrlReferrer.ToString();
            }
            return View();
        }
        [ValidateInput(false)]
        public void Save(UsersFace UsersFace)
        {
            UsersFace baseUsersFace = Entity.UsersFace.FirstOrDefault(n => n.Id == UsersFace.Id && n.Agent == BasicAgent.Id);
            if (UsersFace.Remark.IsNullOrEmpty())
            {
                UsersFace.Remark = "无备注";
            }
            string State = "无改变";
            if (baseUsersFace.State == 1)
            {
                baseUsersFace.State = 2;
            }
            if (UsersFace.State == 2)
            {
                State = "有意向";
                baseUsersFace.State = 2;
            }
            else if (UsersFace.State == 3)
            {
                State = "无意向";
                baseUsersFace.State = 3;
            }
            string Remark = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "§" + UsersFace.Remark + "§" + State + "§" + AdminUser.TrueName;
            if (baseUsersFace.Remark.IsNullOrEmpty())
            {
                baseUsersFace.Remark = Remark;
            }
            else
            {
                baseUsersFace.Remark += "№" + Remark;
            }
            baseUsersFace.IsNew = 0;
            Entity.SaveChanges();
            BaseRedirect();
        }
    }
}
