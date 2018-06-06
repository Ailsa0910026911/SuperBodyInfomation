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
    public class CardController : BaseController
    {
        //
        // GET: /Home/
        public ActionResult Index(string En)
        {
            string De = LokFuEncode.Base64Decode(En);
            string[] Arr = De.Split('|');
            SysAgent SysAgent = new SysAgent();
            if (Arr.Length==2) {
                string card = Arr[0];
                ViewBag.Card = card;
                ViewBag.PWD = Arr[1];
                Card Card = Entity.Card.FirstOrNew(n => n.Code == card);
                if (!Card.AId.IsNullOrEmpty()) {
                    SysAgent = Entity.SysAgent.FirstOrNew(n => n.Id == Card.AId && n.State == 1 && n.IsTeiPai == 1);
                }
            }
            ViewBag.En = En;
            ViewBag.SysAgent = SysAgent;
            return View();
        }
    }
}
 