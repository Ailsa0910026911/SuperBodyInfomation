using LokFu.Extensions;
using LokFu.Repositories;
using System;
using System.Linq;
using System.Web.Mvc;
namespace LokFu.Areas.Manage.Controllers
{
    /// <summary>
    /// 提现名称设置
    /// </summary>
    public class CashSetController : BaseController
    {
        /// <summary>
        /// 查看
        /// </summary>
        /// <returns></returns>
        public ActionResult Edit()
        {
            SysSet SysSet = Entity.SysSet.FirstOrDefault();
            if (SysSet == null)
            {
                ViewBag.ErrorMsg = "数据不存在";
                return View("Error");
            }
            ViewBag.SysSet = SysSet;
            ViewBag.Save = this.checkPower("Save");
            return View();
        }
        /// <summary>
        /// 保存
        /// </summary>
        /// <param name="SysSet"></param>
        /// <returns></returns>
        [ValidateInput(false)]
        public object Save(SysSet SysSet)
        {
            SysSet baseSysSet = Entity.SysSet.FirstOrDefault(n => n.Id == SysSet.Id);
            if (!SysSet.SW1eTime.IsNullOrEmpty())
            {
              //  SysSet.SW1eTime = ((DateTime)SysSet.SW1eTime).AddDays(1);
                SysSet.SW1eTime = ((DateTime)SysSet.SW1eTime);
            }
            if (!SysSet.SW2eTime.IsNullOrEmpty())
            {
                //SysSet.SW2eTime = ((DateTime)SysSet.SW2eTime).AddDays(1);
                SysSet.SW2eTime = ((DateTime)SysSet.SW2eTime);
            }
            baseSysSet = Request.ConvertRequestToModel<SysSet>(baseSysSet, SysSet);
            Entity.SaveChanges();
            Response.Redirect("/Manage/CashSet/Edit.html");
            return null;
        }
    }
}
