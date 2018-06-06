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
    public class MsgAboutController : BaseController
    {
        public ActionResult Index(MsgAbout MsgAbout, EFPagingInfo<MsgAbout> p)
        {
            if (!MsgAbout.Name.IsNullOrEmpty()) { p.SqlWhere.Add(f => f.Name.Contains(MsgAbout.Name)); }
            if (!MsgAbout.State.IsNullOrEmpty()) { p.SqlWhere.Add(f => f.State == MsgAbout.State); }
            p.OrderByList.Add("Id", "DESC");
            IPageOfItems<MsgAbout> MsgAboutList = Entity.Selects<MsgAbout>(p);
            ViewBag.MsgAboutList = MsgAboutList;
            ViewBag.MsgAbout = MsgAbout;
            ViewBag.Add = this.checkPower("Add");
            ViewBag.Edit = this.checkPower("Edit");
            ViewBag.Delete = this.checkPower("Delete");
            ViewBag.Save = this.checkPower("Save");
            return View();
        }
        public ActionResult Edit(MsgAbout MsgAbout)
        {
            if (MsgAbout.Id != 0) MsgAbout = Entity.MsgAbout.FirstOrDefault(n => n.Id == MsgAbout.Id);
            if (MsgAbout == null)
            {
                ViewBag.ErrorMsg = "数据不存在";
                return View("Error");
            }
            ViewBag.MsgAbout = MsgAbout;
            if (Request.UrlReferrer != null)
            {
                Session["Url"] = Request.UrlReferrer.ToString();
            }
            return View();
        }
        [ValidateInput(false)]
        public void Add(MsgAbout MsgAbout)
        {
            MsgAbout.PId = 0;
            MsgAbout.AddTime = DateTime.Now;
            Entity.MsgAbout.AddObject(MsgAbout);
            Entity.SaveChanges();
            BaseRedirect();
        }
        [ValidateInput(false)]
        public void Save(MsgAbout MsgAbout)
        {
            MsgAbout baseMsgAbout = Entity.MsgAbout.FirstOrDefault(n => n.Id == MsgAbout.Id);
            baseMsgAbout = Request.ConvertRequestToModel<MsgAbout>(baseMsgAbout, MsgAbout);
            Entity.SaveChanges();
            BaseRedirect();
        }
        public void ChangeStatus(MsgAbout MsgAbout, string InfoList, string Clomn, string Value)
        {
            if (string.IsNullOrEmpty(InfoList)) { InfoList = MsgAbout.Id.ToString(); }
            int Ret = Entity.ChangeEntity<MsgAbout>(InfoList, Clomn, Value);
            Entity.SaveChanges();
            Response.Write(Ret);
        }
        public void Delete(MsgAbout MsgAbout, string InfoList, int? IsDel)
        {
            if (string.IsNullOrEmpty(InfoList)) { InfoList = MsgAbout.Id.ToString(); }
            int Ret = Entity.MoveToDeleteEntity<MsgAbout>(InfoList, IsDel, AdminUser.UserName);
            Entity.SaveChanges();
            Response.Write(Ret);
        }
    }
}
