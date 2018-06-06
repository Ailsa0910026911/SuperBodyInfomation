using LokFu.Extensions;
using LokFu.Infrastructure;
using LokFu.Models;
using LokFu.Repositories;
using LokFu.Repositories.SqlServer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.Collections;
namespace LokFu.Areas.Manage.Controllers
{
    public class MsgHelpController : BaseController
    {
        public ActionResult Index(MsgHelp MsgHelp, EFPagingInfo<MsgHelp> p, int IsFirst = 0)
        {
            if (IsFirst == 0)
            {
                PageOfItems<MsgHelp> MsgHelpList1 = new PageOfItems<MsgHelp>(new List<MsgHelp>(), 0, 10, 0, new Hashtable());
                ViewBag.MsgHelpList = MsgHelpList1;
                ViewBag.MsgHelp = MsgHelp;
                ViewBag.Add = this.checkPower("Add");
                ViewBag.Edit = this.checkPower("Edit");
                ViewBag.Delete = this.checkPower("Delete");
                ViewBag.Save = this.checkPower("Save");
                return View();
            }
            if (!MsgHelp.Name.IsNullOrEmpty()) { p.SqlWhere.Add(f => f.Name.Contains(MsgHelp.Name)); }
            if (!MsgHelp.State.IsNullOrEmpty()) { p.SqlWhere.Add(f => f.State == (MsgHelp.State == 99 ? 0 : MsgHelp.State)); }
            p.OrderByList.Add("Id", "DESC");
            IPageOfItems<MsgHelp> MsgHelpList = Entity.Selects<MsgHelp>(p);
            ViewBag.MsgHelpList = MsgHelpList;
            ViewBag.MsgHelp = MsgHelp;
            ViewBag.Add = this.checkPower("Add");
            ViewBag.Edit = this.checkPower("Edit");
            ViewBag.Delete = this.checkPower("Delete");
            ViewBag.Save = this.checkPower("Save");
            return View();
        }
        public ActionResult Edit(MsgHelp MsgHelp)
        {
            if (MsgHelp.Id != 0) MsgHelp = Entity.MsgHelp.FirstOrDefault(n => n.Id == MsgHelp.Id);
            if (MsgHelp == null)
            {
                ViewBag.ErrorMsg = "数据不存在";
                return View("Error");
            }
            ViewBag.MsgHelp = MsgHelp;
            ViewBag.MsgHelpList = Entity.MsgHelp.Where(n => n.PId == 0).ToList();
            if (Request.UrlReferrer != null)
            {
                Session["Url"] = Request.UrlReferrer.ToString();
            }
            return View();
        }
        [ValidateInput(false)]
        public void Add(MsgHelp MsgHelp)
        {
            MsgHelp.Info = MsgHelp.Info.Replace("\r\n", "").Trim();
            MsgHelp = Request.ConvertRequestToModel<MsgHelp>(MsgHelp, MsgHelp);
            MsgHelp.Click = 0;
            MsgHelp.AddTime = DateTime.Now;
            Entity.MsgHelp.AddObject(MsgHelp);
            Entity.SaveChanges();
            BaseRedirect();
        }
        [ValidateInput(false)]
        public void Save(MsgHelp MsgHelp)
        {
            MsgHelp.Info = MsgHelp.Info.Replace("\r\n", "").Trim();
            MsgHelp baseMsgHelp = Entity.MsgHelp.FirstOrDefault(n => n.Id == MsgHelp.Id);
            baseMsgHelp = Request.ConvertRequestToModel<MsgHelp>(baseMsgHelp, MsgHelp);
            Entity.SaveChanges();
            BaseRedirect();
        }
        public void ChangeStatus(MsgHelp MsgHelp, string InfoList, string Clomn, string Value)
        {
            if (string.IsNullOrEmpty(InfoList)) { InfoList = MsgHelp.Id.ToString(); }
            int Ret = Entity.ChangeEntity<MsgHelp>(InfoList, Clomn, Value);
            Entity.SaveChanges();
            Response.Write(Ret);
        }
        public void Delete(MsgHelp MsgHelp, string InfoList, int? IsDel)
        {
            if (string.IsNullOrEmpty(InfoList)) { InfoList = MsgHelp.Id.ToString(); }
            int Ret = Entity.MoveToDeleteEntity<MsgHelp>(InfoList, IsDel, AdminUser.UserName);
            Entity.SaveChanges();
            Response.Write(Ret);
        }
    }
}
