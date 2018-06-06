using LokFu.Extensions;
using LokFu.Infrastructure;
using LokFu.Models;
using LokFu.Repositories;
using LokFu.Repositories.SqlServer;
using System;
using System.Linq;
using System.Web.Mvc;
namespace LokFu.Areas.Manage.Controllers
{
    public class BaoConfigController : BaseController
    {
        public ActionResult Edit()
        {
            BaoConfig BaoConfig = Entity.BaoConfig.FirstOrNew();
            if (BaoConfig == null)
            {
                ViewBag.ErrorMsg = "数据不存在";
                return View("Error");
            }
            ViewBag.BaoConfig = BaoConfig;
            ViewBag.Save = this.checkPower("Save");
            if (Request.UrlReferrer != null)
            {
                Session["Url"] = Request.Url.ToString(); ;
            }
            return View();
        }
        [ValidateInput(false)]
        public void Save(BaoConfig BaoConfig)
        {
            BaoConfig baseBaoConfig = Entity.BaoConfig.FirstOrNew();
            baseBaoConfig = Request.ConvertRequestToModel<BaoConfig>(baseBaoConfig, BaoConfig);
            if (!baseBaoConfig.GetCost.IsNullOrEmpty())
            {
                decimal v = baseBaoConfig.GetCost;
                decimal w = v / 10000;
                decimal n = 1 + w;
                decimal p = (decimal)Math.Pow((double)n, 365);
                decimal y = p - 1;
                decimal r = y * 100;
                baseBaoConfig.YearPer = r;
            }
            else
            {
                baseBaoConfig.YearPer = 0;
            }

            Entity.SaveChanges();
            BaseRedirect();
        }
    }
}
