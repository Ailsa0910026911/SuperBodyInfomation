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
    public class MsgCallBackController : BaseController
    {

        public ActionResult Index(MsgCallBack MsgCallBack, EFPagingInfo<MsgCallBack> p, int IsFirst = 0)
        {
            if (IsFirst == 0)
            {
                PageOfItems<MsgCallBack> MsgCallBackList1 = new PageOfItems<MsgCallBack>(new List<MsgCallBack>(), 0, 10, 0, new Hashtable());
                ViewBag.MsgCallBackList = MsgCallBackList1;
                ViewBag.MsgCallBack = MsgCallBack;
                ViewBag.Edit = this.checkPower("Edit");
                ViewBag.Delete = this.checkPower("Delete");
                ViewBag.Save = this.checkPower("Save");
                return View();
            }
            if (!MsgCallBack.NeekName.IsNullOrEmpty()) { p.SqlWhere.Add(f => f.NeekName.Contains(MsgCallBack.NeekName)); }
            if (!MsgCallBack.Linker.IsNullOrEmpty()) { p.SqlWhere.Add(f => f.Linker.Contains(MsgCallBack.Linker)); }
            if (!MsgCallBack.Name.IsNullOrEmpty()) { p.SqlWhere.Add(f => f.Name.Contains(MsgCallBack.Name)); }
            if (!MsgCallBack.State.IsNullOrEmpty()) { p.SqlWhere.Add(f => f.State == MsgCallBack.State); }
            p.OrderByList.Add("Id", "DESC");
            IPageOfItems<MsgCallBack> MsgCallBackList = Entity.Selects<MsgCallBack>(p);
            ViewBag.MsgCallBackList = MsgCallBackList;
            ViewBag.MsgCallBack = MsgCallBack;
            ViewBag.Edit = this.checkPower("Edit");
            ViewBag.Delete = this.checkPower("Delete");
            ViewBag.Save = this.checkPower("Save");
            return View();
        }
        public ActionResult Edit(MsgCallBack MsgCallBack)
        {
            if (MsgCallBack.Id != 0) MsgCallBack = Entity.MsgCallBack.FirstOrDefault(n => n.Id == MsgCallBack.Id);
            if (MsgCallBack == null)
            {
                ViewBag.ErrorMsg = "数据不存在";
                return View("Error");
            }
            ViewBag.MsgCallBack = MsgCallBack;
            if (Request.UrlReferrer != null)
            {
                Session["Url"] = Request.UrlReferrer.ToString();
            }
            return View();
        }
        [ValidateInput(false)]
        public void Save(MsgCallBack MsgCallBack)
        {
            MsgCallBack baseMsgCallBack = Entity.MsgCallBack.FirstOrDefault(n => n.Id == MsgCallBack.Id);
            baseMsgCallBack = Request.ConvertRequestToModel<MsgCallBack>(baseMsgCallBack, MsgCallBack);
            baseMsgCallBack.State = 2;
            baseMsgCallBack.EditTime = DateTime.Now;
            Entity.SaveChanges();
            BaseRedirect();
        }
        public void ChangeStatus(MsgCallBack MsgCallBack, string InfoList, string Clomn, string Value)
        {
            if (string.IsNullOrEmpty(InfoList)) { InfoList = MsgCallBack.Id.ToString(); }
            int Ret = Entity.ChangeEntity<MsgCallBack>(InfoList, Clomn, Value);
            Entity.SaveChanges();
            Response.Write(Ret);
        }
        public void Delete(MsgCallBack MsgCallBack, string InfoList, int? IsDel)
        {
            if (string.IsNullOrEmpty(InfoList)) { InfoList = MsgCallBack.Id.ToString(); }
            int Ret = Entity.MoveToDeleteEntity<MsgCallBack>(InfoList, IsDel, AdminUser.UserName);
            Entity.SaveChanges();
            Response.Write(Ret);
        }
    }
}
