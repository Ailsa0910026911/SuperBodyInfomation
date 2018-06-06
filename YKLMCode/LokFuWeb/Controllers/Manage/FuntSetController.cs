using LokFu.Extensions;
using LokFu.Repositories;
using System;
using System.Linq;
using System.Web.Mvc;

namespace LokFu.Areas.Manage.Controllers
{
    /// <summary>
    /// 功能设置
    /// </summary>
    public class FuntSetController : BaseController
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
        public object Save(SysSet SysSet, int? AnsyAgent3, int? AnsyAgent4)
        {
            if (SysSet.AgentGet > 10000 || SysSet.AgentGet < 0)
            {
                ViewBag.ErrorMsg = "请填写代理商分润小于1万，大于0";
                return View("Error");
            }
            SysSet baseSysSet = Entity.SysSet.FirstOrDefault(n => n.Id == SysSet.Id);
            if (!SysSet.AgentGet.IsNullOrEmpty())
            {
                SysSet.AgentGet = SysSet.AgentGet / 10000;
            }
            if (SysSet.CashPayWay != baseSysSet.CashPayWay)
            {
                int TaskCount = Entity.TaskCash.Count(n => n.State == 1 || n.State == 2);
                if (TaskCount > 0) {
                    ViewBag.ErrorMsg = "提现任务列表中有未执行完成操作，暂时不能修改结算途径";
                    return View("Error");
                }
            }
            SysSet.BaoUserAlert = SysSet.BaoUserAlert ?? string.Empty;
            baseSysSet = Request.ConvertRequestToModel<SysSet>(baseSysSet, SysSet);
            Entity.SaveChanges();

            if (AnsyAgent3 == 1)
            {
                //使用删除全部后根据用户表生成，有效解决了因接口关闭或新增加接口，老用户没有配置问题
                string SQL = "Update SysAgent Set Set3=" + SysSet.IosSet3 + " Where Tier=1 and IsTeiPai=1";
                Entity.ExecuteStoreCommand(SQL);
            }
            if (AnsyAgent4 == 1)
            {
                //使用删除全部后根据用户表生成，有效解决了因接口关闭或新增加接口，老用户没有配置问题
                string SQL = "Update SysAgent Set Set4=" + SysSet.IosSet4 + " Where Tier=1 and IsTeiPai=1";
                Entity.ExecuteStoreCommand(SQL);
            }
            Response.Redirect("/Manage/FuntSet/Edit.html");
            return null;
        }
    }
}
