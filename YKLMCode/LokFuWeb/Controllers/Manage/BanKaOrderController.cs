using LokFu.Extensions;
using LokFu.Infrastructure;
using LokFu.Models;
using LokFu.Repositories;
using LokFu.Repositories.SqlServer;
using System.Linq;
using System.Web.Mvc;
namespace LokFu.Areas.Manage.Controllers
{
    public class BanKaOrderController : BaseController
    {
        public ActionResult Index(BanKaOrder BanKaOrder, EFPagingInfo<BanKaOrder> p)
        {
            if (!BanKaOrder.BKTId.IsNullOrEmpty()) { p.SqlWhere.Add(f => f.BKTId == BanKaOrder.BKTId); }
            if (!BanKaOrder.UId.IsNullOrEmpty()) { p.SqlWhere.Add(f => f.UId == BanKaOrder.UId); }
            if (!BanKaOrder.OId.IsNullOrEmpty()) { p.SqlWhere.Add(f => f.OId == BanKaOrder.OId); }
            if (!BanKaOrder.Agent.IsNullOrEmpty()) { p.SqlWhere.Add(f => f.Agent == BanKaOrder.Agent); }
            if (!BanKaOrder.AgentGet.IsNullOrEmpty()) { p.SqlWhere.Add(f => f.AgentGet == BanKaOrder.AgentGet); }
            if (!BanKaOrder.AgentState.IsNullOrEmpty()) { p.SqlWhere.Add(f => f.AgentState == BanKaOrder.AgentState); }
            if (!BanKaOrder.AId.IsNullOrEmpty()) { p.SqlWhere.Add(f => f.AId == BanKaOrder.AId); }
            if (!BanKaOrder.FId.IsNullOrEmpty()) { p.SqlWhere.Add(f => f.FId == BanKaOrder.FId); }
            if (!BanKaOrder.OrderState.IsNullOrEmpty()) { p.SqlWhere.Add(f => f.OrderState == BanKaOrder.OrderState); }
            if (!BanKaOrder.PayState.IsNullOrEmpty()) { p.SqlWhere.Add(f => f.PayState == BanKaOrder.PayState); }
            if (!BanKaOrder.Remark.IsNullOrEmpty()) { p.SqlWhere.Add(f => f.Remark.Contains(BanKaOrder.Remark)); }
            p.OrderByList.Add("Id", "DESC");
            IPageOfItems<BanKaOrder> BanKaOrderList = Entity.Selects<BanKaOrder>(p);
            ViewBag.BanKaOrderList = BanKaOrderList;
            ViewBag.BanKaOrder = BanKaOrder;
            ViewBag.BanKaTypeList = Entity.BanKaType.Where(n => n.State == 1).OrderBy(n => n.Sort).ToList();
            return View();
        }
    }
}
