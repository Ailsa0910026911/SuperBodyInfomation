using LokFu.Extensions;
using LokFu.Infrastructure;
using LokFu.Models;
using LokFu.Repositories;
using LokFu.Repositories.SqlServer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
namespace LokFu.Areas.Manage.Controllers
{
    public class SysControlController : BaseController
    {
        public ActionResult Index()
        {
            IList<SysControl> SysControlList1 = Entity.SysControl.Where(o => o.State == 1).OrderBy(o => o.Sort).ToList();
            IList<SysControl> SysControlList2 = Entity.SysControl.Where(o => o.State == 0).OrderBy(o => o.Sort).ToList();
            IList<SysControl> SysControlList3 = Entity.SysControl.Where(o => o.State == 2).OrderBy(o => o.Sort).ToList();
            ViewBag.SysControlList1 = SysControlList1;
            ViewBag.SysControlList2 = SysControlList2;
            ViewBag.SysControlList3 = SysControlList3;
            ViewBag.Add = this.checkPower("Add");
            ViewBag.Save = this.checkPower("Save");
            ViewBag.Delete = this.checkPower("Delete");
            return View();
        }
        public ActionResult Edit(SysControl SysControl)
        {
            if (SysControl.Id != 0) SysControl = Entity.SysControl.FirstOrDefault(n => n.Id == SysControl.Id);
            if (SysControl == null)
            {
                ViewBag.ErrorMsg = "数据不存在";
                return View("Error");
            }
            ViewBag.SysControl = SysControl;
            if (Request.UrlReferrer != null)
            {
                Session["Url"] = Request.UrlReferrer.ToString();
            }
            IList<PayConfig> PayConfigList = Entity.PayConfig.Where(n => n.State == 1).OrderBy(n => n.Sort).ToList();
            ViewBag.PayConfigList = PayConfigList;
            return View();
        }
        [ValidateInput(false)]
        public ActionResult Add(SysControl SysControl, int STimeHH, int STimemm, int ETimeHH, int ETimemm)
        {
            //判断维一
            if (SysControl.PayWay > 0)
            {
                SysControl tempSysControl = Entity.SysControl.FirstOrDefault(n => n.PayWay == SysControl.PayWay);
                if (tempSysControl != null)
                {
                    ViewBag.ErrorMsg = "已经绑定在“" + tempSysControl.CName + "”";
                    return View("Error");
                }
            }

            DateTime STime = DateTime.Parse("1990-01-01 " + STimeHH + ":" + STimemm + ":00");
            DateTime ETime = DateTime.Parse("1990-01-01 " + ETimeHH + ":" + ETimemm + ":" + (ETimeHH == 23 && ETimemm == 59 ? "59" : "00"));
            SysControl.STime = STime;
            SysControl.ETime = ETime;
            SysControl.AddTime = DateTime.Now;
            Entity.SysControl.AddObject(SysControl);
            Entity.SaveChanges();
            if (SysControl.Tag == "RecMoneyMulti" || SysControl.Tag == "RecMoneyLocal") {
                if (SysControl.State == 1 || SysControl.State == 2)
                {
                    if (SysControl.Tag == "RecMoneyMulti") {
                        Entity.ExecuteStoreCommand("Update SysControl Set State=0 Where Tag='RecMoneyLocal'");
                    }
                    if (SysControl.Tag == "RecMoneyLocal")
                    {
                        Entity.ExecuteStoreCommand("Update SysControl Set State=0 Where Tag='RecMoneyMulti'");
                    }
                }
                Entity.SaveChanges();
            }
            BaseRedirect();
            return View("Succeed");
        }
        [ValidateInput(false)]
        public ActionResult Save(SysControl SysControl, int STimeHH, int STimemm, int ETimeHH, int ETimemm)
        {
            DateTime STime = DateTime.Parse("1990-01-01 " + STimeHH + ":" + STimemm + ":00");
            DateTime ETime = DateTime.Parse("1990-01-01 " + ETimeHH + ":" + ETimemm + ":" + (ETimeHH == 23 && ETimemm == 59 ? "59" : "00"));
            SysControl baseSysControl = Entity.SysControl.FirstOrDefault(n => n.Id == SysControl.Id);
            if (SysControl.PayWay != baseSysControl.PayWay && SysControl.PayWay > 0)
            { 
                //修改了通道
                SysControl tempSysControl = Entity.SysControl.FirstOrDefault(n => n.Id != baseSysControl.Id && n.PayWay == SysControl.PayWay);
                if (tempSysControl != null) {
                    ViewBag.ErrorMsg = "已经绑定在“" + tempSysControl.CName + "”";
                    return View("Error");
                }
            }
            baseSysControl = Request.ConvertRequestToModel<SysControl>(baseSysControl, SysControl);
            baseSysControl.STime = STime;
            baseSysControl.ETime = ETime;
            Entity.SaveChanges();
            if (SysControl.Tag == "RecMoneyMulti" || SysControl.Tag == "RecMoneyLocal")
            {
                if (SysControl.State == 1 || SysControl.State == 2)
                {
                    if (SysControl.Tag == "RecMoneyMulti")
                    {
                        Entity.ExecuteStoreCommand("Update SysControl Set State=0 Where Tag='RecMoneyLocal'");
                    }
                    if (SysControl.Tag == "RecMoneyLocal")
                    {
                        Entity.ExecuteStoreCommand("Update SysControl Set State=0 Where Tag='RecMoneyMulti'");
                    }
                }
                Entity.SaveChanges();
            }
            BaseRedirect();
            return View("Succeed");
        }
        public void ChangeStatus(SysControl SysControl, string InfoList, string Clomn, string Value)
        {
            if (string.IsNullOrEmpty(InfoList)) { InfoList = SysControl.Id.ToString(); }
            int Ret = Entity.ChangeEntity<SysControl>(InfoList, Clomn, Value);
            Entity.SaveChanges();
            Response.Write(Ret);
        }
        public void Delete(SysControl SysControl, string InfoList, int? IsDel)
        {
            if (string.IsNullOrEmpty(InfoList)) { InfoList = SysControl.Id.ToString(); }
            int Ret = Entity.MoveToDeleteEntity<SysControl>(InfoList, IsDel, AdminUser.UserName);
            Entity.SaveChanges();
            Response.Write(Ret);
        }
    }
}
