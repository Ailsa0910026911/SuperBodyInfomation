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
    public class AppUpdateController : BaseController
    {
        public ActionResult Index(AppUpdate AppUpdate, EFPagingInfo<AppUpdate> p)
        {
            p.PageSize = 999;
            p.OrderByList.Add("Id", "ASC");
            IPageOfItems<AppUpdate> AppUpdateList = Entity.Selects<AppUpdate>(p);
            ViewBag.AppUpdateList = AppUpdateList;
            ViewBag.AppUpdate = AppUpdate;
            ViewBag.Add = this.checkPower("Add");
            ViewBag.Save = this.checkPower("Save");
            ViewBag.Delete = this.checkPower("Delete");
            return View();
        }

        public ActionResult Edit(AppUpdate AppUpdate)
        {
            if (AppUpdate.Id != 0) AppUpdate = Entity.AppUpdate.FirstOrDefault(n => n.Id == AppUpdate.Id);
            if (AppUpdate == null)
            {
                ViewBag.ErrorMsg = "数据不存在";
                return View("Error");
            }
            ViewBag.AppUpdate = AppUpdate;
            if (Request.UrlReferrer != null)
            {
                Session["Url"] = Request.UrlReferrer.ToString();
            }
            return View();
        }

        [ValidateInput(false)]
        public void Add(AppUpdate AppUpdate)
        {
            AppUpdate.AddTime = DateTime.Now;
            Entity.AppUpdate.AddObject(AppUpdate);
            Entity.SaveChanges();
            BaseRedirect();
        }

        [ValidateInput(false)]
        public void Save(AppUpdate AppUpdate)
        {
            AppUpdate baseAppUpdate = Entity.AppUpdate.FirstOrDefault(n => n.Id == AppUpdate.Id);
            baseAppUpdate = Request.ConvertRequestToModel<AppUpdate>(baseAppUpdate, AppUpdate);
            Entity.SaveChanges();
            BaseRedirect();
        }

        public void ChangeStatus(AppUpdate AppUpdate, string InfoList, string Clomn, string Value)
        {
            if (string.IsNullOrEmpty(InfoList)) { InfoList = AppUpdate.Id.ToString(); }
            int Ret = Entity.ChangeEntity<AppUpdate>(InfoList, Clomn, Value);
            Entity.SaveChanges();
            Response.Write(Ret);
        }

        public void Delete(AppUpdate AppUpdate, string InfoList, int? IsDel)
        {
            if (string.IsNullOrEmpty(InfoList)) { InfoList = AppUpdate.Id.ToString(); }
            int Ret = Entity.MoveToDeleteEntity<AppUpdate>(InfoList, IsDel, AdminUser.UserName);
            Entity.SaveChanges();
            Response.Write(Ret);
        }
    }
}
