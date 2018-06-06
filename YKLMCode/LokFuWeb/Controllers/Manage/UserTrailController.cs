using LokFu.Extensions;
using LokFu.Infrastructure;
using LokFu.Models;
using LokFu.Repositories;
using LokFu.Repositories.SqlServer;
using System.Linq;
using System.Web.Mvc;
namespace LokFu.Areas.Manage.Controllers
{
    public class UserTrailController : BaseController
    {
        public ActionResult Index(UserTrack UserTrack, EFPagingInfo<UserTrack> p)
        {
            if (UserTrack.UId.IsNullOrEmpty())
            {
                UserTrack.UId = 0;
            }
            p.SqlWhere.Add(f => f.UId == UserTrack.UId);
            p.OrderByList.Add("Id", "DESC");
            IPageOfItems<UserTrack> UserTrackList = Entity.Selects<UserTrack>(p);
            ViewBag.UserTrackList = UserTrackList;
            Users Users = Entity.Users.FirstOrDefault(n => n.Id == UserTrack.UId);
            if (Users == null)
            {
                ViewBag.ErrorMsg = "数据不存在";
                return View("Error");
            }
            ViewBag.Users = Users;
            return View();
        }
    }
}
