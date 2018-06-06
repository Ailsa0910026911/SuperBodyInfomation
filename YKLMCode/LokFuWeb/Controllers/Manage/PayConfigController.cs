using LokFu.Extensions;
using LokFu.Infrastructure;
using LokFu.Models;
using LokFu.Repositories;
using LokFu.Repositories.SqlServer;
using System.Collections;
using System.Linq;
using System.Web.Mvc;
namespace LokFu.Areas.Manage.Controllers
{
    public class PayConfigController : BaseController
    {
        public ActionResult Index(PayConfig PayConfig, EFPagingInfo<PayConfig> p)
        {
            if (!PayConfig.Name.IsNullOrEmpty()) { p.SqlWhere.Add(f => f.Name.Contains(PayConfig.Name)); }
            if (!PayConfig.State.IsNullOrEmpty()) { p.SqlWhere.Add(f => f.State == PayConfig.State); }
            p.PageSize = 9999;
            p.OrderByList.Add("Sort", "ASC");
            
            IPageOfItems<PayConfig> PayConfigList = Entity.Selects<PayConfig>(p);
            ViewBag.PayConfigList = PayConfigList.OrderByDescending(o=>o.State).ThenBy(o=>o.Sort).ToList();
            ViewBag.PayConfig = PayConfig;
            ViewBag.Save = this.checkPower("Save");
            return View();
        }
        public ActionResult Edit(PayConfig PayConfig)
        {
            if (PayConfig.Id != 0) PayConfig = Entity.PayConfig.FirstOrDefault(n => n.Id == PayConfig.Id);
            if (PayConfig == null)
            {
                ViewBag.ErrorMsg = "数据不存在";
                return View("Error");
            }
            ViewBag.PayConfig = PayConfig;
            if (Request.UrlReferrer != null)
            {
                Session["Url"] = Request.UrlReferrer.ToString();
            }
            return View();
        }
        [ValidateInput(false)]
        public void Save(PayConfig PayConfig, string[] queryArray, int? AnsyCash, int? AnsyAgent)
        {
            PayConfig.Cost = PayConfig.Cost / 1000;
            PayConfig.CostAgent = PayConfig.CostAgent / 1000;
            PayConfig.CostUser = PayConfig.CostUser / 1000;
            if (PayConfig.Cost < 0 || PayConfig.CostUser < 0 || PayConfig.CostUser > 1 || PayConfig.Cost > 1 || PayConfig.Cost > PayConfig.CostUser) {
                Response.Redirect("/Manage/Home/Error.html?IsAjax=1&msg=费率设置有误");
                return;
            }
            PayConfig basePayConfig = Entity.PayConfig.FirstOrDefault(n => n.Id == PayConfig.Id);
            //如果是微信支付配置的子商户号没有填写的话，去掉这个元素
            if (basePayConfig.DllName == "WeiXin")
            {
                if(queryArray[4].IsNullOrEmpty())
                {
                    var temp = new ArrayList(queryArray);
                    temp.RemoveAt(4);
                    queryArray = (string[])temp.ToArray(typeof(string));
                }
            }
            if (queryArray != null)
            { 
                basePayConfig.QueryArray = string.Join(",", queryArray);
            }
            basePayConfig = Request.ConvertRequestToModel<PayConfig>(basePayConfig, PayConfig);
            Entity.SaveChanges();
            if (AnsyCash == 1) {
                //使用删除全部后根据用户表生成，有效解决了因接口关闭或新增加接口，老用户没有配置问题
                string SQL="Delete UserPay Where PId="+basePayConfig.Id;
                Entity.ExecuteStoreCommand(SQL);
                SQL = "INSERT INTO UserPay(UId,PId,Cost,IsDel) Select ID," + basePayConfig.Id + " As PId," + basePayConfig.CostUser + " As Cost, 0 As IsDel From Users";
                Entity.ExecuteStoreCommand(SQL);
            }
            if (AnsyAgent == 1)
            {
                //使用删除全部后根据用户表生成，有效解决了因接口关闭或新增加接口，老用户没有配置问题
                string SQL="Delete UserPayAgent Where PId="+basePayConfig.Id;
                Entity.ExecuteStoreCommand(SQL);
                SQL = "INSERT INTO UserPayAgent(AId,PId,Cost,IsDel) Select ID," + basePayConfig.Id + " As PId," + basePayConfig.CostUser + " As Cost, 0 As IsDel From SysAgent";
                Entity.ExecuteStoreCommand(SQL);
            }

            if (AnsyAgent == 1 || AnsyCash == 1)
            {
                APIExtensions.ClearCacheAll();
            }

            BaseRedirect();
        }
        public void ChangeStatus(PayConfig PayConfig, string InfoList, string Clomn, string Value)
        {
            if (string.IsNullOrEmpty(InfoList)) { InfoList = PayConfig.Id.ToString(); }
            int Ret = Entity.ChangeEntity<PayConfig>(InfoList, Clomn, Value);
            Entity.SaveChanges();
            Response.Write(Ret);
        }
    }
}
