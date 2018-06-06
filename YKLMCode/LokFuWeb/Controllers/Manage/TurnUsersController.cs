using LokFu.Extensions;
using LokFu.Infrastructure;
using LokFu.Models;
using LokFu.Repositories;
using LokFu.Repositories.SqlServer;
using System.Linq;
using System.Web.Mvc;
namespace LokFu.Areas.Manage.Controllers
{
    public class TurnUsersController : BaseController
    {
        public ActionResult Index(TurnUsers TurnUsers, EFPagingInfo<TurnUsers> p)
        {
            if (!TurnUsers.UId.IsNullOrEmpty()) { p.SqlWhere.Add(f => f.UId == TurnUsers.UId); }
            p.OrderByList.Add("UId", "DESC");
            IPageOfItems<TurnUsers> TurnUsersList = Entity.Selects<TurnUsers>(p);
            ViewBag.TurnUsersList = TurnUsersList;
            ViewBag.TurnUsers = TurnUsers;
            ViewBag.TurnLog = this.checkPower("TurnLog", "Edit");
            ViewBag.MyUsers = this.checkPower("Users", "MyUsers");
            ViewBag.Save = this.checkPower("Save");
            return View();
        }
        public ActionResult Edit(TurnUsers TurnUsers)
        {
            if (TurnUsers.Id != 0) TurnUsers = Entity.TurnUsers.FirstOrDefault(n => n.Id == TurnUsers.Id);
            if (TurnUsers == null)
            {
                ViewBag.ErrorMsg = "数据不存在";
                return View("Error");
            }
            ViewBag.TurnUsers = TurnUsers;
            if (Request.UrlReferrer != null)
            {
                Session["Url"] = Request.UrlReferrer.ToString();
            }
            return View();
        }
        [ValidateInput(false)]
        public void Save(TurnUsers TurnUsers)
        {
            TurnUsers baseTurnUsers = Entity.TurnUsers.FirstOrDefault(n => n.Id == TurnUsers.Id);
            baseTurnUsers = Request.ConvertRequestToModel<TurnUsers>(baseTurnUsers, TurnUsers);
            Entity.SaveChanges();
            BaseRedirect();
        }
    }
}
