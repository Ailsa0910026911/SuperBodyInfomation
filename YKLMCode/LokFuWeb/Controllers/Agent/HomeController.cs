using System.Web.Mvc;
using LokFu.Repositories;
using System;
using LokFu.Extensions;
namespace LokFu.Areas.Agent.Controllers
{
    public class HomeController : BaseController
    {
        public HomeController()
        {
            ViewBag.Authorization = true;//允许权限
        }
        //
        // GET: /Home/
        public ActionResult Index()
        {
            return View();
        }
        // GET: /Home/
        public ActionResult Error(string msg)
        {
            ViewBag.ErrorMsg = msg;
            return View();
        }
    }
}
 