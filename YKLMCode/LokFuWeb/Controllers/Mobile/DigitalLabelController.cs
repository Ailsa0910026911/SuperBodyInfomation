using System.Linq;
using System.Web.Mvc;
using LokFu.Repositories;
using LokFu.Extensions;
using LokFu.Models;
using LokFu.Infrastructure;
using LokFu.Repositories.SqlServer;
using System.Collections.Generic;
using System;
namespace LokFu.Areas.Mobile.Controllers
{
    public class DigitalLabelController : BaseController
    {
        public ActionResult Index(int Id)
        {
            if (Id.IsNullOrEmpty())
            {
                return View("Empty");
            }
            Users Users= Entity.Users.FirstOrDefault(n => n.Id == Id);
            if (Users == null)
            {
                return View("Empty");
            }
            if (Users.State != 1) {
                return View("Empty");
            }
            if (Users != null) {
                ViewBag.Code = Users.QRCode();
            }
            return View();
        }
    }
}
 