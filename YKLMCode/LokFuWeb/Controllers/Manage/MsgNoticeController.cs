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
    public class MsgNoticeController : BaseController
    {

        public ActionResult Index(MsgNotice MsgNotice, EFPagingInfo<MsgNotice> p, int IsFirst = 0)
        {
            if (IsFirst == 0)
            {
                PageOfItems<MsgNotice> MsgNoticeList1 = new PageOfItems<MsgNotice>(new List<MsgNotice>(), 0, 10, 0, new Hashtable());
                ViewBag.MsgNoticeList = MsgNoticeList1;
                ViewBag.MsgNotice = MsgNotice;
                ViewBag.Add = this.checkPower("Add");
                ViewBag.Edit = this.checkPower("Edit");
                ViewBag.Delete = this.checkPower("Delete");
                ViewBag.Save = this.checkPower("Save");
                return View();
            }
            if (!MsgNotice.NType.IsNullOrEmpty()) { p.SqlWhere.Add(f => f.NType == (MsgNotice.NType == 99 ? 0 : MsgNotice.NType)); }
            if (!MsgNotice.Name.IsNullOrEmpty()) { p.SqlWhere.Add(f => f.Name.Contains(MsgNotice.Name)); }
            if (!MsgNotice.State.IsNullOrEmpty()) { p.SqlWhere.Add(f => f.State == (MsgNotice.State == 99 ? 0 : MsgNotice.State)); }
            p.OrderByList.Add("Id", "DESC");
            IPageOfItems<MsgNotice> MsgNoticeList = Entity.Selects<MsgNotice>(p);
            ViewBag.MsgNoticeList = MsgNoticeList;
            ViewBag.MsgNotice = MsgNotice;
            ViewBag.Add = this.checkPower("Add");
            ViewBag.Edit = this.checkPower("Edit");
            ViewBag.Delete = this.checkPower("Delete");
            ViewBag.Save = this.checkPower("Save");
            return View();
        }
        public ActionResult Edit(MsgNotice MsgNotice)
        {
            if (MsgNotice.Id != 0) MsgNotice = Entity.MsgNotice.FirstOrDefault(n => n.Id == MsgNotice.Id);
            if (MsgNotice == null)
            {
                ViewBag.ErrorMsg = "数据不存在";
                return View("Error");
            }
            ViewBag.MsgNotice = MsgNotice;
            if (Request.UrlReferrer != null)
            {
                Session["Url"] = Request.UrlReferrer.ToString();
            }
            ViewBag.Save = this.checkPower("Save");
            ViewBag.Add = this.checkPower("Add");
            return View();
        }
        [ValidateInput(false)]
        public void Add(MsgNotice MsgNotice)
        {
            MsgNotice.Level = 0;
            MsgNotice.AddTime = DateTime.Now;
            MsgNotice.ReadAdmin = string.Empty;
            MsgNotice.ReadUsers = string.Empty;
            Entity.MsgNotice.AddObject(MsgNotice);
            Entity.SaveChanges();

            if (MsgNotice.IsPush == 1 && MsgNotice.NType == 3)
            {
                MsgNotice.PushMsg(this.Entity);
            }

            BaseRedirect();
        }
        [ValidateInput(false)]
        public void Save(MsgNotice MsgNotice)
        {
            MsgNotice baseMsgNotice = Entity.MsgNotice.FirstOrDefault(n => n.Id == MsgNotice.Id);
            baseMsgNotice = Request.ConvertRequestToModel<MsgNotice>(baseMsgNotice, MsgNotice);
            Entity.SaveChanges();
            BaseRedirect();
        }
        public void ChangeStatus(MsgNotice MsgNotice, string InfoList, string Clomn, string Value)
        {
            if (string.IsNullOrEmpty(InfoList)) { InfoList = MsgNotice.Id.ToString(); }
            int Ret = Entity.ChangeEntity<MsgNotice>(InfoList, Clomn, Value);
            Entity.SaveChanges();
            Response.Write(Ret);
        }
        public void Delete(MsgNotice MsgNotice, string InfoList, int? IsDel)
        {
            if (string.IsNullOrEmpty(InfoList)) { InfoList = MsgNotice.Id.ToString(); }
            int Ret = Entity.MoveToDeleteEntity<MsgNotice>(InfoList, IsDel, AdminUser.UserName);
            Entity.SaveChanges();
            Response.Write(Ret);
        }
    }
}
