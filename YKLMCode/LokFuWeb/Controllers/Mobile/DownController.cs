using System;
using System.Collections.Generic;
using System.Linq;
using LokFu.Repositories;
using LokFu.Extensions;
using LokFu.Infrastructure;
using System.Web.Mvc;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Globalization;
namespace LokFu.Areas.Mobile.Controllers
{
    public class DownController : BaseController
    {
        public ActionResult Index(int? Id)
        {
            SysAgent SysAgent = Entity.SysAgent.FirstOrNew(n => n.Id == Id && n.State == 1 && n.IsTeiPai == 1);
            ViewBag.SysAgent = SysAgent;
            return View();
        }
    }
}
