using System.Linq;
using System.Web.Mvc;
using LokFu.Repositories;
using LokFu.Extensions;
using LokFu.Models;
using LokFu.Infrastructure;
using LokFu.Repositories.SqlServer;
using System.Collections.Generic;
namespace LokFu.Areas.Base.Controllers
{
    public class QrCodeController : BaseController
    {
        //
        // GET: /Home/
        public ActionResult Index(string n)
        {
            if (n.IsNullOrEmpty())
            {
                ViewBag.ErrorMsg = "无效二维码";
                return View();
            }
            QRCode QRCode = Entity.QRCode.FirstOrDefault(m => m.Num == n);
            if (QRCode == null) {
                ViewBag.ErrorMsg = "无效二维码";
                return View();
            }
            if (QRCode.State==0)
            {
                ViewBag.ErrorMsg = "二维码已经失效";
                return View();
            }
            if (QRCode.State==1)
            {
                ViewBag.ErrorMsg = "该二维码未绑定商户";
                return View();
            }
            if (QRCode.Url.IsNullOrEmpty())
            {
                ViewBag.ErrorMsg = "该二维码未绑定商户[1]";
                return View();
            }
            Response.Redirect(QRCode.Url);
            return View("Empty");
        }
    }
}
 