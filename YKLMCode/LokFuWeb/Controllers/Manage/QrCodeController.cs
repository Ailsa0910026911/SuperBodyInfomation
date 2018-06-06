using LokFu.Extensions;
using LokFu.Infrastructure;
using LokFu.Models;
using LokFu.Repositories;
using LokFu.Repositories.SqlServer;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
namespace LokFu.Areas.Manage.Controllers
{
    public class QrCodeController : BaseController
    {
        public ActionResult Index(QRCode QRCode, EFPagingInfo<QRCode> p, int IsFirst = 0)
        {
            if (IsFirst==0)
            {
                PageOfItems<QRCode> QRCodeList1 = new PageOfItems<QRCode>(new List<QRCode>(), 0, 10, 0, new Hashtable());
                ViewBag.QRCodeList = QRCodeList1;
                ViewBag.QRCode = QRCode;
                ViewBag.UsersList = new List<Users>();
                return View();
            }
            if (!QRCode.UId.IsNullOrEmpty())
            {
                p.SqlWhere.Add(f => f.UId == QRCode.UId);
                p.PageSize = 99999;
            }
            if (!QRCode.Num.IsNullOrEmpty())
            {
                p.SqlWhere.Add(f => f.Num == QRCode.Num);
            }
            if (!QRCode.State.IsNullOrEmpty())
            {
                p.SqlWhere.Add(f => f.State == (QRCode.State == 99 ? 0 : QRCode.State));
            }
            p.OrderByList.Add("Id", "DESC");
            IPageOfItems<QRCode> QRCodeList = Entity.Selects<QRCode>(p);
            ViewBag.QRCodeList = QRCodeList;
            ViewBag.QRCode = QRCode;
            IList<int> Ids = new List<int>();
            foreach (var P in QRCodeList.Where(n=>n.UId>0)) {
                Ids.Add(P.UId);
            }
            IList<Users> UsersList = new List<Users>();
            if (Ids.Count() > 0 && QRCode.UId.IsNullOrEmpty())
            {
                UsersList = Entity.Users.Where(n => Ids.Contains(n.Id)).ToList();
            }
            ViewBag.UsersList = UsersList;
            return View();
        }

        public void ChangeStatus(QRCode QRCode, string InfoList, string Clomn, string Value)
        {
            if (string.IsNullOrEmpty(InfoList)) { InfoList = QRCode.Id.ToString(); }
            int Ret = Entity.ChangeEntity<QRCode>(InfoList, Clomn, Value);
            Entity.SaveChanges();
            Response.Write(Ret);
        }
    }
}
