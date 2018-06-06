using System.Linq;
using System.Web.Mvc;
using LokFu.Repositories;
using LokFu.Extensions;
using LokFu.Models;
using LokFu.Infrastructure;
using LokFu.Repositories.SqlServer;
using System.Collections.Generic;
using System;
using System.Collections;
namespace LokFu.Areas.Mobile.Controllers
{
    public class HaoWoolController : BaseController
    {
        public ActionResult Index(EFPagingInfo<HaoWool> p,string title, int IsAjax = 0)
        {
            if (!IsLokFu)
            {
                ViewBag.ErrorMsg = "请使用" + BasicSet.Name + "打开链接";
                return View("Error");
            }
        
            if (!title.IsNullOrEmpty())
            {
                p.SqlWhere.Add(f => f.Title.Contains(title) || f.SmallTitle.Contains(title));
            }
            
            p.OrderByList.Add("AddTime", "DESC");
            p.PageSize = 10;
            p.SqlWhere.Add(n => n.State == 1);
            IPageOfItems<HaoWool> HaoWoolList = Entity.Selects<HaoWool>(p);
            ViewBag.HaoWoolList = HaoWoolList;
            ViewBag.titles = title;
            if (IsAjax == 1)
            {
                return View("Index");
            }
            return View();
        }
        public ActionResult Info(HaoWool HaoWool)
        {
            if (!IsLokFu)
            {
                ViewBag.ErrorMsg = "请使用" + BasicSet.Name + "打开链接";
                return View("Error");
            }
            HaoWool = Entity.HaoWool.FirstOrNew(n => n.Id == HaoWool.Id);
            HaoWool.Click++;
            Entity.SaveChanges();
            ViewBag.HaoWool = HaoWool;
            return View();
        }
    }
}
 