using LokFu.Extensions;
using LokFu.Infrastructure;
using LokFu.Models;
using LokFu.Repositories;
using LokFu.Repositories.SqlServer;
using System.Linq;
using System.Web.Mvc;
namespace LokFu.Areas.Manage.Controllers
{
    public class TurnProcController : BaseController
    {
        public ActionResult Index(TurnProc TurnProc, EFPagingInfo<TurnProc> p)
        {
            if (!TurnProc.TId.IsNullOrEmpty()) { p.SqlWhere.Add(f => f.TId == TurnProc.TId); }
            if (!TurnProc.Name.IsNullOrEmpty()) { p.SqlWhere.Add(f => f.Name.Contains(TurnProc.Name)); }
            if (!TurnProc.Num.IsNullOrEmpty()) { p.SqlWhere.Add(f => f.Num == TurnProc.Num); }
            if (!TurnProc.UNum.IsNullOrEmpty()) { p.SqlWhere.Add(f => f.UNum == TurnProc.UNum); }
            p.OrderByList.Add("Id", "ASC");
            IPageOfItems<TurnProc> TurnProcList = Entity.Selects<TurnProc>(p);
            ViewBag.TurnProcList = TurnProcList;
            ViewBag.TurnProc = TurnProc;
            ViewBag.Add = this.checkPower("Add");
            ViewBag.Delete = this.checkPower("Delete");
            ViewBag.Save = this.checkPower("Save");
            return View();
        }
        public ActionResult Edit(TurnProc TurnProc)
        {
            if (TurnProc.Id != 0) TurnProc = Entity.TurnProc.FirstOrDefault(n => n.Id == TurnProc.Id);
            if (TurnProc == null)
            {
                ViewBag.ErrorMsg = "数据不存在";
                return View("Error");
            }
            ViewBag.TurnProc = TurnProc;
            if (Request.UrlReferrer != null)
            {
                Session["Url"] = Request.UrlReferrer.ToString();
            }
            return View();
        }
        [ValidateInput(false)]
        public void Add(TurnProc TurnProc)
        {
            Entity.TurnProc.AddObject(TurnProc);
            Entity.SaveChanges();
            BaseRedirect();
        }
        [ValidateInput(false)]
        public void Save(TurnProc TurnProc)
        {
            TurnProc baseTurnProc = Entity.TurnProc.FirstOrDefault(n => n.Id == TurnProc.Id);
            baseTurnProc = Request.ConvertRequestToModel<TurnProc>(baseTurnProc, TurnProc);
            Entity.SaveChanges();
            BaseRedirect();
        }
        public void ChangeStatus(TurnProc TurnProc, string InfoList, string Clomn, string Value)
        {
            if (string.IsNullOrEmpty(InfoList)) { InfoList = TurnProc.Id.ToString(); }
            int Ret = Entity.ChangeEntity<TurnProc>(InfoList, Clomn, Value);
            Entity.SaveChanges();
            Response.Write(Ret);
        }
        public void Delete(TurnProc TurnProc, string InfoList, int? IsDel)
        {
            if (string.IsNullOrEmpty(InfoList)) { InfoList = TurnProc.Id.ToString(); }
            int Ret = Entity.MoveToDeleteEntity<TurnProc>(InfoList, IsDel, AdminUser.UserName);
            Entity.SaveChanges();
            Response.Write(Ret);
        }
    }
}
