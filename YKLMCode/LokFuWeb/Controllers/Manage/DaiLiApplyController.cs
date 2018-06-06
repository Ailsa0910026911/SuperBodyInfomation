using LokFu.Extensions;
using LokFu.Infrastructure;
using LokFu.Models;
using LokFu.Repositories;
using LokFu.Repositories.SqlServer;
using System.Linq;
using System.Web.Mvc;
namespace LokFu.Areas.Manage.Controllers
{
    public class DaiLiApplyController : BaseController
    {
        public ActionResult Index(DaiLiApply DaiLiApply, EFPagingInfo<DaiLiApply> p)
        {
            p.OrderByList.Add("Id", "DESC");
            IPageOfItems<DaiLiApply> DaiLiApplyList = Entity.Selects<DaiLiApply>(p);
            ViewBag.DaiLiApplyList = DaiLiApplyList;
            ViewBag.DaiLiApply = DaiLiApply;
            ViewBag.SysAgentList = Entity.SysAgent.Where(n => n.State == 1).ToList();
            return View();
        }
        public ActionResult Edit(DaiLiApply DaiLiApply)
        {
            if (DaiLiApply.Id != 0) DaiLiApply = Entity.DaiLiApply.FirstOrDefault(n => n.Id == DaiLiApply.Id);
            if (DaiLiApply == null)
            {
                ViewBag.ErrorMsg = "数据不存在";
                return View("Error");
            }
            ViewBag.DaiLiApply = DaiLiApply;
            ViewBag.SysAgent = Entity.SysAgent.FirstOrNew(n => n.Id == DaiLiApply.Agent);
            if (Request.UrlReferrer != null)
            {
                Session["Url"] = Request.UrlReferrer.ToString();
            }
            return View();
        }
        [ValidateInput(false)]
        public void Save(DaiLiApply DaiLiApply)
        {
            DaiLiApply baseDaiLiApply = Entity.DaiLiApply.FirstOrDefault(n => n.Id == DaiLiApply.Id);
            baseDaiLiApply = Request.ConvertRequestToModel<DaiLiApply>(baseDaiLiApply, DaiLiApply);
            Entity.SaveChanges();
            BaseRedirect();
        }
    }
}
