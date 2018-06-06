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
    public class TurntableController : BaseController
    {
        public ActionResult Index(Turntable Turntable, EFPagingInfo<Turntable> p)
        {
            if (!Turntable.Name.IsNullOrEmpty()) { p.SqlWhere.Add(f => f.Name.Contains(Turntable.Name)); }
            if (!Turntable.PInfo.IsNullOrEmpty()) { p.SqlWhere.Add(f => f.PInfo.Contains(Turntable.PInfo)); }
            if (!Turntable.PTips.IsNullOrEmpty()) { p.SqlWhere.Add(f => f.PTips.Contains(Turntable.PTips)); }
            if (!Turntable.ETips.IsNullOrEmpty()) { p.SqlWhere.Add(f => f.ETips.Contains(Turntable.ETips)); }
            if (!Turntable.ETitle.IsNullOrEmpty()) { p.SqlWhere.Add(f => f.ETitle.Contains(Turntable.ETitle)); }
            if (!Turntable.EInfo.IsNullOrEmpty()) { p.SqlWhere.Add(f => f.EInfo.Contains(Turntable.EInfo)); }
            if (!Turntable.BaseNum.IsNullOrEmpty()) { p.SqlWhere.Add(f => f.BaseNum == Turntable.BaseNum); }
            if (!Turntable.State.IsNullOrEmpty()) { p.SqlWhere.Add(f => f.State == Turntable.State); }
            p.OrderByList.Add("Id", "DESC");
            IPageOfItems<Turntable> TurntableList = Entity.Selects<Turntable>(p);
            ViewBag.TurntableList = TurntableList;
            ViewBag.Turntable = Turntable;

            ViewBag.Add = this.checkPower("Add");
            ViewBag.Edit = this.checkPower("Edit");
            ViewBag.Delete = this.checkPower("Delete");
            ViewBag.Save = this.checkPower("Save");
            ViewBag.TurnProc = this.checkPower("TurnProc", "Index");
            ViewBag.TurnProcEdit = this.checkPower("TurnProc", "Edit");
            ViewBag.TurnLogEdit = this.checkPower("TurnLog", "Edit");
            ViewBag.TurnLog = this.checkPower("TurnLog", "Index");
            return View();
        }
        public ActionResult Edit(Turntable Turntable)
        {
            if (Turntable.Id != 0) Turntable = Entity.Turntable.FirstOrDefault(n => n.Id == Turntable.Id);
            if (Turntable == null)
            {
                ViewBag.ErrorMsg = "数据不存在";
                return View("Error");
            }
            ViewBag.Turntable = Turntable;
            if (Request.UrlReferrer != null)
            {
                Session["Url"] = Request.UrlReferrer.ToString();
            }
            return View();
        }
        [ValidateInput(false)]
        public void Add(Turntable Turntable)
        {
            Turntable.AddTime = DateTime.Now;
            Entity.Turntable.AddObject(Turntable);
            Entity.SaveChanges();
            BaseRedirect();
        }
        [ValidateInput(false)]
        public void Save(Turntable Turntable)
        {
            Turntable baseTurntable = Entity.Turntable.FirstOrDefault(n => n.Id == Turntable.Id);
            baseTurntable = Request.ConvertRequestToModel<Turntable>(baseTurntable, Turntable);
            Entity.SaveChanges();
            BaseRedirect();
        }
        public void ChangeStatus(Turntable Turntable, string InfoList, string Clomn, string Value)
        {
            if (string.IsNullOrEmpty(InfoList)) { InfoList = Turntable.Id.ToString(); }
            int Ret = Entity.ChangeEntity<Turntable>(InfoList, Clomn, Value);
            Entity.SaveChanges();
            Response.Write(Ret);
        }
        public void Delete(Turntable Turntable, string InfoList, int? IsDel)
        {
            if (string.IsNullOrEmpty(InfoList)) { InfoList = Turntable.Id.ToString(); }
            int Ret = Entity.MoveToDeleteEntity<Turntable>(InfoList, IsDel, AdminUser.UserName);
            Entity.SaveChanges();
            Response.Write(Ret);
        }
    }
}
