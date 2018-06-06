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
namespace LokFu.Areas.Manage.Controllers
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
                ViewBag.SysAgentList = Entity.SysAgent.Where(n => n.State == 1).ToList();
                return View();
            }
            p.SqlWhere.Add(f => f.CType == 1);
            if (!UsersFace.TrueName.IsNullOrEmpty()) { p.SqlWhere.Add(f => f.TrueName.Contains(UsersFace.TrueName)); }
            if (!UsersFace.Mobile.IsNullOrEmpty()) { p.SqlWhere.Add(f => f.Mobile.Contains(UsersFace.Mobile)); }
            if (!UsersFace.CardStae.IsNullOrEmpty())
            {
                if (UsersFace.CardStae == 2)
                {
                    p.SqlWhere.Add(f => f.CardStae == 2);
                }
                else
                {
                    p.SqlWhere.Add(f => f.CardStae != 2);
                }
            }
            if (!UsersFace.Agent.IsNullOrEmpty()) { p.SqlWhere.Add(f => f.Agent == UsersFace.Agent); }
            if (!UsersFace.IsDaiLi.IsNullOrEmpty())
            {
                if (UsersFace.IsDaiLi == 99)
                {
                    UsersFace.IsDaiLi = 0;
                }
                p.SqlWhere.Add(f => f.IsDaiLi == UsersFace.IsDaiLi);
            }
            if (!UsersFace.MobileType.IsNullOrEmpty()) { p.SqlWhere.Add(f => f.MobileType == UsersFace.MobileType); }
            if (!UsersFace.State.IsNullOrEmpty()) { p.SqlWhere.Add(f => f.State == UsersFace.State); }
            p.OrderByList.Add("UpdateTime", "DESC");
            IPageOfItems<UsersFace> UsersFaceList = Entity.Selects<UsersFace>(p);
            ViewBag.UsersFaceList = UsersFaceList;
            ViewBag.UsersFace = UsersFace;
            ViewBag.SysAgentList = Entity.SysAgent.Where(n => n.State == 1).ToList();
            return View();
        }
        public ActionResult Edit(UsersFace UsersFace)
        {
            ViewBag.Remark = UsersFace.Remark;
            if (UsersFace.Id != 0) UsersFace = Entity.UsersFace.FirstOrDefault(n => n.Id == UsersFace.Id);
            if (UsersFace == null)
            {
                ViewBag.ErrorMsg = "数据不存在";
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
            UsersFace baseUsersFace = Entity.UsersFace.FirstOrDefault(n => n.Id == UsersFace.Id);
            if (UsersFace.Remark.IsNullOrEmpty())
            {
                UsersFace.Remark = "无备注";
            }
            string State = "无改变";
            if (baseUsersFace.State == 1)
            {
                baseUsersFace.Agent = 0;
                baseUsersFace.AId = AdminUser.Id;
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
            else if (UsersFace.State == 4)
            {
                State = "已完成";
                baseUsersFace.State = 4;
            }
            string Remark = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "§" + UsersFace.Remark + "§" + State + "§" + AdminUser.TrueName; ;
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
