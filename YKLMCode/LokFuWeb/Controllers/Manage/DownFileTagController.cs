using LokFu.Extensions;
using LokFu.Infrastructure;
using LokFu.Models;
using LokFu.Repositories;
using LokFu.Repositories.SqlServer;
using System;
using System.Linq;
using System.Web.Mvc;
namespace LokFu.Areas.Manage.Controllers
{
    public class DownFileTagController : BaseController
    {
        public ActionResult Index(DownFileTag DownFileTag, EFPagingInfo<DownFileTag> p)
        {
            p.OrderByList.Add("Sort", "ASC");
            IPageOfItems<DownFileTag> DownFileTagList = Entity.Selects<DownFileTag>(p);
            ViewBag.DownFileTagList = DownFileTagList;
            ViewBag.DownFileTag = DownFileTag;
            ViewBag.Add = this.checkPower("Add");
            ViewBag.Edit = this.checkPower("Edit");
            ViewBag.Delete = this.checkPower("Delete");
            ViewBag.Save = this.checkPower("Save");
            return View();
        }
        public ActionResult Edit(DownFileTag DownFileTag)
        {
            if (DownFileTag.Id != 0)
            {
                DownFileTag = Entity.DownFileTag.FirstOrDefault(n => n.Id == DownFileTag.Id);
            }
            if (DownFileTag == null)
            {
                ViewBag.ErrorMsg = "数据不存在";
                return View("Error");
            }
            ViewBag.DownFileTag = DownFileTag;
            if (Request.UrlReferrer != null)
            {
                Session["Url"] = Request.UrlReferrer.ToString();
            }
            return View();
        }
        [ValidateInput(false)]
        public void Add(DownFileTag DownFileTag)
        {
            DownFileTag.AddTime = DateTime.Now;
            Entity.DownFileTag.AddObject(DownFileTag);
            Entity.SaveChanges();
            BaseRedirect();
        }
        [ValidateInput(false)]
        public void Save(DownFileTag DownFileTag)
        {
            DownFileTag baseDownFileTag = Entity.DownFileTag.FirstOrDefault(n => n.Id == DownFileTag.Id);
            baseDownFileTag = Request.ConvertRequestToModel<DownFileTag>(baseDownFileTag, DownFileTag);
            Entity.SaveChanges();
            BaseRedirect();
        }
        public void ChangeStatus(DownFileTag DownFileTag, string InfoList, string Clomn, string Value)
        {
            if (string.IsNullOrEmpty(InfoList)) { InfoList = DownFileTag.Id.ToString(); }
            int Ret = Entity.ChangeEntity<DownFileTag>(InfoList, Clomn, Value);
            Response.Write(Ret);
        }
        public void Delete(DownFileTag DownFileTag, string InfoList, int? IsDel)
        {
            if (string.IsNullOrEmpty(InfoList)) { InfoList = DownFileTag.Id.ToString(); }
            int Ret = Entity.MoveToDeleteEntity<DownFileTag>(InfoList, IsDel, AdminUser.UserName);
            Response.Write(Ret);
        }
    }
}
