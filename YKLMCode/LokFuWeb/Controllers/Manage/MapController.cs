using LokFu.Infrastructure;
using LokFu.Models;
using LokFu.Extensions;
using LokFu.Repositories;
using LokFu.Repositories.SqlServer;
using System.Linq;
using System.Web.Mvc;
namespace LokFu.Areas.Manage.Controllers
{
    public class MapController : BaseController
    {
        public MapController()
        {
            ViewBag.Authorization = true;//允许权限
        }
        public ActionResult Users(Users Users)
        {
            if (Users.Id != 0) Users = Entity.Users.FirstOrDefault(n => n.Id == Users.Id);
            if (Users == null)
            {
                ViewBag.ErrorMsg = "数据不存在";
                return View("Error");
            }
            ViewBag.Users = Users;
            return View();
        }
        public ActionResult UserTrail(UserTrail UserTrail)
        {
            if (UserTrail.Id != 0) UserTrail = Entity.UserTrail.FirstOrDefault(n => n.Id == UserTrail.Id);
            if (UserTrail == null)
            {
                ViewBag.ErrorMsg = "数据不存在";
                return View("Error");
            }
            ViewBag.UserTrail = UserTrail;
            return View();
        }
        public ActionResult Orders(Orders Orders)
        {
            if (Orders.Id != 0) Orders = Entity.Orders.FirstOrDefault(n => n.Id == Orders.Id);
            if (Orders == null)
            {
                ViewBag.ErrorMsg = "数据不存在";
                return View("Error");
            }
            ViewBag.Orders = Orders;
            return View();
        }
        public ActionResult GPS(UserTrack UserTrack)
        {
            if (UserTrack.Id != 0) UserTrack = Entity.UserTrack.FirstOrDefault(n => n.Id == UserTrack.Id);
            if (UserTrack == null)
            {
                ViewBag.ErrorMsg = "数据不存在";
                return View("Error");
            }
            ViewBag.UserTrack = UserTrack;
            return View();
        }
    }
}
