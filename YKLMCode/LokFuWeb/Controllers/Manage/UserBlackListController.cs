using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using LokFu.Extensions;
using LokFu.Infrastructure;
using LokFu.Models;
using LokFu.Repositories;
using LokFu.Repositories.SqlServer;
using System.Collections;

namespace LokFu.Areas.Manage.Controllers
{
    public class UserBlackListController : BaseController
    {
        //
        // GET: /UserBlackList/

        public ActionResult Index(UserBlackList UserBlackList, EFPagingInfo<UserBlackList> p,DateTime? STime, DateTime? ETime, int IsFirst = 0)
        {
            //if (STime.IsNullOrEmpty())
            //{
            //    STime = DateTime.Parse(DateTime.Now.ToString("yyyy-MM-dd"));
            //}
            //if (ETime.IsNullOrEmpty())
            //{
            //    ETime = DateTime.Parse(DateTime.Now.ToString("yyyy-MM-dd"));
            //}
            //ViewBag.ETime = ETime;
            //ViewBag.STime = STime;
            ViewBag.UserBlackList = UserBlackList;
            if (IsFirst==0)
            {
                PageOfItems<UserBlackList> UserBlackListList1 = new PageOfItems<UserBlackList>(new List<UserBlackList>(), 0, 10, 0, new Hashtable());
                ViewBag.UserBlackListList = UserBlackListList1;
                return View();
            }
            if (!UserBlackList.CardNumber.IsNullOrEmpty())
            {
                 p.SqlWhere.Add(f => f.CardNumber == UserBlackList.CardNumber);
            }
            if (UserBlackList.State!=0)
            {
                 p.SqlWhere.Add(f => f.State == UserBlackList.State);
            }
           // ETime = ETime.Value.AddDays(1);
          //  p.SqlWhere.Add(f => f.AddTime >= STime && f.AddTime <= ETime);
            p.OrderByList.Add("Id", "DESC");
            IPageOfItems<UserBlackList> UserBlackListList = Entity.Selects<UserBlackList>(p);
            ViewBag.UserBlackListList = UserBlackListList;
            return View();
        }
        public ActionResult Edit()
        {
            if (Request.UrlReferrer != null)
            {
                Session["Url"] = Request.UrlReferrer.ToString();
            }
            return View();
        }
        public void EditSave(UserBlackList UserBlackList)
        {
            UserBlackList.CardNumber = UserBlackList.CardNumber.Replace(" ", "");
            if (UserBlackList.State == 1)//手机
            {
                UserBlackList BasicBlackTemp = Entity.UserBlackList.FirstOrDefault(o => o.CardNumber == UserBlackList.CardNumber && o.State == 1);
                if (BasicBlackTemp != null)
                {
                    Response.Write("该手机号已存在");
                    return;
                }

            }
            if (UserBlackList.State == 2)//身份证
            {
                UserBlackList BasicBlackTemp = Entity.UserBlackList.FirstOrDefault(o => o.CardNumber == UserBlackList.CardNumber && o.State == 2);
                if (BasicBlackTemp != null)
                {
                    Response.Write("该身份证已存在");
                    return;
                }

            }
            if (UserBlackList.State == 3)//银行卡
            {
                UserBlackList BasicBlackTemp = Entity.UserBlackList.FirstOrDefault(o => o.CardNumber == UserBlackList.CardNumber && o.State == 3);
                if (BasicBlackTemp != null)
                {
                    Response.Write("该银行卡已存在");
                    return;
                }
            }
            UserBlackList.AId = AdminUser.Id;
            UserBlackList.AddTime = DateTime.Now;
            Entity.UserBlackList.AddObject(UserBlackList);
            Entity.SaveChanges();
            BaseRedirect();
        }
        [ValidateInput(false)]
        public JsonResult EditCheckCardNumber(string fieldId, string fieldValue)
        {
            if (Entity.UserBlackList.FirstOrDefault(u => u.CardNumber == fieldValue) != null)
            {
                return Json(new object []{ "CardNumber",false }, JsonRequestBehavior.AllowGet);
            }
            return Json(new object[] { "CardNumber", true }, JsonRequestBehavior.AllowGet);
        }
        public void Delete(UserBlackList UserBlackList, string InfoList, int? IsDel)
        {
            if (string.IsNullOrEmpty(InfoList)) { InfoList = UserBlackList.Id.ToString(); }
            int Ret = Entity.MoveToDeleteEntity<UserBlackList>(InfoList, IsDel, AdminUser.UserName);
            Entity.SaveChanges();
            Response.Write(Ret);
        }
    }
}
