using LokFu.Extensions;
using LokFu.Infrastructure;
using LokFu.Models;
using LokFu.Repositories;
using LokFu.Repositories.SqlServer;
using System.Linq;
using System.Web.Mvc;
namespace LokFu.Areas.Agent.Controllers
{
    public class UserIdCardController : BaseController
    {
        public UserIdCardController()
        {
            ViewBag.Authorization = true;//允许权限
        }
        public ActionResult Log(Users Users, EFPagingInfo<SysLog> p)
        {
            string uid = string.Format("Id={0}&", Users.Id);
            p.SqlWhere.Add(f => f.ControllerName == "UserIdCard");
            p.SqlWhere.Add(f => f.ActionName == "Save");
            p.SqlWhere.Add(f => f.POSTData.Contains(uid) || f.RQData.Contains(uid));
            p.OrderByList.Add("Id", "DESC");
            p.PageSize = 9999;
            IPageOfItems<SysLog> SysLogList = Entity.Selects<SysLog>(p);
            ViewBag.SysLogList = SysLogList;
            ViewBag.SysAdminList = Entity.SysAdmin.Where(n => n.AgentId == 0).ToList();
            return View();
        }
    }
}
