using LokFu.Extensions;
using LokFu.Infrastructure;
using LokFu.Models;
using LokFu.Repositories;
using LokFu.Repositories.SqlServer;
using System.Linq;
using System.Web.Mvc;
namespace LokFu.Areas.Manage.Controllers
{
    public class TurnLogController : BaseController
    {
       public ActionResult Index(TurnLog TurnLog, EFPagingInfo<TurnLog> p)
       {
            if(!TurnLog.UId.IsNullOrEmpty()){p.SqlWhere.Add(f => f.UId==TurnLog.UId);}
            if(!TurnLog.TId.IsNullOrEmpty()){p.SqlWhere.Add(f => f.TId==TurnLog.TId);}
            p.OrderByList.Add("Id", "DESC");
            IPageOfItems<TurnLog> TurnLogList = Entity.Selects<TurnLog>(p);
            ViewBag.TurnLogList = TurnLogList;
            ViewBag.TurnLog = TurnLog;
            return View();
       }
    }
}
