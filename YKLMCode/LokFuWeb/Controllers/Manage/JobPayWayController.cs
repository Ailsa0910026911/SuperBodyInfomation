using LokFu.Extensions;
using LokFu.Infrastructure;
using LokFu.Models;
using LokFu.Repositories;
using LokFu.Repositories.SqlServer;
using System;
using System.Collections;
using System.Linq;
using System.Web.Mvc;
namespace LokFu.Areas.Manage.Controllers
{
    public class JobPayWayController : BaseController
    {
        public ActionResult Index(JobPayWay JobPayWay, EFPagingInfo<JobPayWay> p)
        {
            p.PageSize = 9999;
            p.OrderByList.Add("Sort", "ASC");
            IPageOfItems<JobPayWay> JobPayWayList = Entity.Selects<JobPayWay>(p);
            ViewBag.JobPayWayList = JobPayWayList.OrderByDescending(o=>o.State).ThenBy(o=>o.Sort).ToList();
            ViewBag.JobPayWay = JobPayWay;
            ViewBag.Edit = this.checkPower("Edit");
            ViewBag.Save = this.checkPower("Save");
            return View();
        }
        public ActionResult Edit(JobPayWay JobPayWay)
        {
            if (JobPayWay.Id != 0) JobPayWay = Entity.JobPayWay.FirstOrDefault(n => n.Id == JobPayWay.Id);
            if (JobPayWay == null)
            {
                ViewBag.ErrorMsg = "数据不存在";
                return View("Error");
            }
            ViewBag.JobPayWay = JobPayWay;
            if (Request.UrlReferrer != null)
            {
                Session["Url"] = Request.UrlReferrer.ToString();
            }
            return View();
        }
        [ValidateInput(false)]
        public ActionResult Save(JobPayWay JobPayWay)
        {
            JobPayWay.Cost = JobPayWay.Cost / 1000;
            JobPayWay.CostAgent = JobPayWay.CostAgent / 1000;
            if (JobPayWay.Cost < 0 || JobPayWay.CostAgent < 0 || JobPayWay.Cost >= 1)
            {
                ViewBag.ErrorMsg = "费率设置有误";
                return View("Error");
            }
            JobPayWay baseJobPayWay = Entity.JobPayWay.FirstOrDefault(n => n.Id == JobPayWay.Id);
            baseJobPayWay = Request.ConvertRequestToModel<JobPayWay>(baseJobPayWay, JobPayWay);
            //如果是微信支付配置的子商户号没有填写的话，去掉这个元素
            Entity.SaveChanges();
            ViewBag.Msg = "操作成功";
            return View("Succeed");
        }
        public void ChangeStatus(JobPayWay JobPayWay, string InfoList, string Clomn, string Value)
        {
            if (string.IsNullOrEmpty(InfoList)) { InfoList = JobPayWay.Id.ToString(); }
            int Ret = Entity.ChangeEntity<JobPayWay>(InfoList, Clomn, Value);
            Entity.SaveChanges();
            Response.Write(Ret);
        }
    }
}
