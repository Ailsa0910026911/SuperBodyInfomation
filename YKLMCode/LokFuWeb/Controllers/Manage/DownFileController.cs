using LokFu.Extensions;
using LokFu.Infrastructure;
using LokFu.Models;
using LokFu.Repositories;
using LokFu.Repositories.SqlServer;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
namespace LokFu.Areas.Manage.Controllers
{
    public class DownFileController : BaseController
    {

        public ActionResult Index(DownFile DownFile, EFPagingInfo<DownFile> p, int IsFirst = 0)
        {
            if (IsFirst == 0)
            {
                PageOfItems<DownFile> DownFileList1 = new PageOfItems<DownFile>(new List<DownFile>(), 0, 10, 0, new Hashtable());
                ViewBag.DownFileList = DownFileList1;
                ViewBag.DownFile = DownFile;
                ViewBag.DownFileTagList = Entity.DownFileTag.OrderBy(o => o.Sort).Where(n => n.State == 1).ToList();
                ViewBag.Add = this.checkPower("Add");
                ViewBag.Edit = this.checkPower("Edit");
                ViewBag.Delete = this.checkPower("Delete");
                ViewBag.Save = this.checkPower("Save");
                return View();
            }
            if (!DownFile.Pic.IsNullOrEmpty()) { p.SqlWhere.Add(f => f.Pic.Contains(DownFile.Pic)); }
            if (!DownFile.TId.IsNullOrEmpty())
            {
                p.SqlWhere.Add(f => f.TId == DownFile.TId);
            }
            if (!DownFile.State.IsNullOrEmpty()) { p.SqlWhere.Add(f => f.State == (DownFile.State == 99 ? 0 : DownFile.State)); }
            p.OrderByList.Add("Sort", "ESC");
            IPageOfItems<DownFile> DownFileList = Entity.Selects<DownFile>(p);
            ViewBag.DownFileList = DownFileList;
            ViewBag.DownFile = DownFile;
            ViewBag.DownFileTagList = Entity.DownFileTag.OrderBy(o => o.Sort).Where(n => n.State == 1).ToList();
            ViewBag.Add = this.checkPower("Add");
            ViewBag.Edit = this.checkPower("Edit");
            ViewBag.Delete = this.checkPower("Delete");
            ViewBag.Save = this.checkPower("Save");
            return View();
        }
        public ActionResult Edit(DownFile DownFile)
        {
            if (DownFile.Id != 0) DownFile = Entity.DownFile.FirstOrDefault(n => n.Id == DownFile.Id);
            if (DownFile == null)
            {
                ViewBag.ErrorMsg = "数据不存在";
                return View("Error");
            }
            ViewBag.DownFile = DownFile;
            ViewBag.DownFileTagList = Entity.DownFileTag.OrderBy(o => o.Sort).Where(n => n.State == 1).ToList();
            if (Request.UrlReferrer != null)
            {
                Session["Url"] = Request.UrlReferrer.ToString();
            }
            return View();
        }
        [ValidateInput(false)]
        public ActionResult Add(DownFile DownFile)
        {
            DownFile.AddTime = DateTime.Now;
            DownFile = Request.ConvertRequestToModel<DownFile>(DownFile, DownFile);
            if (DownFile.Pic == "System.Web.HttpPostedFileWrapper")
            {
                ViewBag.ErrorMsg = "文件格式不正确!";
                return View("Error");
            }
            Entity.DownFile.AddObject(DownFile);
            Entity.SaveChanges();
            return this.Redirect(Session["Url"].ToString());
            //BaseRedirect();
        }
        [ValidateInput(false)]
        public ActionResult Save(DownFile DownFile)
        {
            DownFile.AddTime = DateTime.Now;
            DownFile baseDownloadFile = Entity.DownFile.FirstOrDefault(n => n.Id == DownFile.Id);
            var old = baseDownloadFile.Pic;
            baseDownloadFile = Request.ConvertRequestToModel<DownFile>(baseDownloadFile, DownFile);
            if (baseDownloadFile.Pic == "System.Web.HttpPostedFileWrapper" || baseDownloadFile.Pic == old)
            {
                ViewBag.ErrorMsg = "文件格式不正确!";
                return View("Error");
            }
            Entity.SaveChanges();
            return this.Redirect(Session["Url"].ToString());
            //BaseRedirect();
        }
        public void ChangeStatus(DownFile DownFile, string InfoList, string Clomn, string Value)
        {
            if (string.IsNullOrEmpty(InfoList)) { InfoList = DownFile.Id.ToString(); }
            int Ret = Entity.ChangeEntity<DownFile>(InfoList, Clomn, Value);
            Entity.SaveChanges();
            Response.Write(Ret);
        }
        public void Delete(DownFile DownFile, string InfoList, int? IsDel)
        {
            if (string.IsNullOrEmpty(InfoList)) { InfoList = DownFile.Id.ToString(); }
            int Ret = Entity.MoveToDeleteEntity<DownFile>(InfoList, IsDel, AdminUser.UserName);
            Entity.SaveChanges();
            Response.Write(Ret);
        }
    }
}
