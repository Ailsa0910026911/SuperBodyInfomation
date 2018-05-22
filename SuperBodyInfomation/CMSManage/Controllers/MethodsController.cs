using CTModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CMSManage.Controllers
{
    public class MethodsController : Controller
    {
        private CTContext ct = new CTContext();
        public string ChangeAllState()
        {
            string[] ids = Request["IDs"].Split(new char[] { ',' });
            var Now = DateTime.Now;
            try
            {
                foreach (var id in ids)
                {
                    int nid = int.Parse(id);
                    var model = ct.FastOrder.Where(o => o.Id == nid).FirstOrDefault();
                    model.PayState = 1;
                    model.UserState = 1;
                    model.AgentWay = 1;
                    model.PayTime = Now;
                    model.UserTime = Now;
                }
                ct.SaveChanges();
                return "1";
            }
            catch
            {
                return "0";
            }
        }
        public string ChangeState()
        {
            string id = Request["id"];
            var Now = DateTime.Now;
            try
            {
                int nid = int.Parse(id);
                var model = ct.FastOrder.Where(o => o.Id == nid).FirstOrDefault();
                model.PayState = 1;
                model.UserState = 1;
                model.AgentWay = 1;
                model.PayTime = Now;
                model.UserTime = Now;
                ct.SaveChanges();
                return "1";
            }
            catch
            {
                return "0";
            }
        }
        // GET: Methods
        public ActionResult Index()
        {
            return View();
        }
    }
}