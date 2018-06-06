using System.Linq;
using System.Web.Mvc;
using LokFu.Repositories;
using System;
namespace LokFu.Areas.Mobile.Controllers
{
    public class HomeController : BaseController
    {
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult Error(string msg)
        {
            ViewBag.ErrorMsg = msg;
            return View();
        }
        public ActionResult WeiXinErr(string msg)
        {
            ViewBag.ErrorMsg = msg;
            return View();
        }
        public ActionResult About(MsgAbout MsgAbout)
        {
            ViewBag.MsgAbout = Entity.MsgAbout.FirstOrDefault(n => n.Id == MsgAbout.Id);
            return View();
        }
        public ActionResult Help(MsgHelp MsgHelp)
        {
            ViewBag.MsgHelp = Entity.MsgHelp.FirstOrDefault(n => n.Id == MsgHelp.Id);
            return View();
        }

        //public string Check(DateTime day,int uid)
        //{
        //    var userlogs = Entity.UserLog.Where(o => o.UId == uid && System.Data.Objects.SqlClient.SqlFunctions.DateDiff("day", o.AddTime, day) == 0)
        //    .OrderBy(o => o.Id).ToList();
        //    UserLog OldUserlog = userlogs.FirstOrDefault();
        //    string s = string.Empty;
        //    foreach (var item in userlogs)
        //    {
        //        if (item.Id != OldUserlog.Id)
        //        {
        //            if (OldUserlog.AfterAmount != item.BeforAmount || OldUserlog.AfterFrozen != item.BeforFrozen)
        //            {
        //                s += item.Id + "前后不符";
        //            }
        //            OldUserlog = item;

        //        }
        //    }
        //    if (s == string.Empty)
        //    {
        //        s += "都符合";
        //    }
        //    return s;
        //}
    }
      
}
 