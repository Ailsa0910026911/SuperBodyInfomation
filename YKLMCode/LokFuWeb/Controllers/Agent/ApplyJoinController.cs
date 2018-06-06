using LokFu.Extensions;
using LokFu.Infrastructure;
using LokFu.Models;
using LokFu.Repositories;
using LokFu.Repositories.SqlServer;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
namespace LokFu.Areas.Agent.Controllers
{
    public class ApplyJoinController : BaseController
    {
        public ActionResult Index(ApplyJoin ApplyJoin, EFPagingInfo<ApplyJoin> p, int IsFirst = 0 ,int IsShowSupAgent = -1)
        {
            ViewBag.Save = checkPower("Save");
            ViewBag.IsShowSupAgent = IsShowSupAgent;
            if (IsFirst == 0)
            {
                PageOfItems<ApplyJoin> ApplyJoinList1 = new PageOfItems<ApplyJoin>(new List<ApplyJoin>(), 0, 10, 0, new Hashtable());
                ViewBag.ApplyJoinList = ApplyJoinList1;
                ViewBag.ApplyJoin = ApplyJoin;
                ViewBag.BasicProvinceList = Entity.BasicProvince.ToList();
                ViewBag.BasicCityList = Entity.BasicCity.ToList();
                return View();
            }
            if (!ApplyJoin.ServiceType.IsNullOrEmpty()) { p.SqlWhere.Add(f => f.ServiceType == ApplyJoin.ServiceType); }
            if (!ApplyJoin.ApplyType.IsNullOrEmpty()) { p.SqlWhere.Add(f => f.ApplyType == ApplyJoin.ApplyType); }
            if (!ApplyJoin.Linker.IsNullOrEmpty()) { p.SqlWhere.Add(f => f.Linker.Contains(ApplyJoin.Linker)); }
            if (!ApplyJoin.Mobile.IsNullOrEmpty()) { p.SqlWhere.Add(f => f.Mobile.Contains(ApplyJoin.Mobile)); }
            if (!ApplyJoin.ComName.IsNullOrEmpty()) { p.SqlWhere.Add(f => f.ComName.Contains(ApplyJoin.ComName)); }
            if (!ApplyJoin.Province.IsNullOrEmpty()) { p.SqlWhere.Add(f => f.Province == ApplyJoin.Province); }
            if (!ApplyJoin.City.IsNullOrEmpty()) { p.SqlWhere.Add(f => f.City == ApplyJoin.City); }
            if (!ApplyJoin.State.IsNullOrEmpty()) { p.SqlWhere.Add(f => f.State == ApplyJoin.State); }
            //List<Int32> AgentId = new List<Int32>();
            //IList<SysAgent> SysAgentList = BasicAgent.GetSupAgent(Entity);
            //foreach (var pp in SysAgentList)
            //{
            //    AgentId.Add(pp.Id);
            //}
            //p.SqlWhere.Add(f =>AgentId.Contains(f.AgentId));
            if (IsShowSupAgent == 1)
            {
                List<Int32> AgentId = new List<Int32>();
                IList<SysAgent> SysAgentList = BasicAgent.GetSupAgent(Entity);
                foreach (var pp in SysAgentList)
                {
                    AgentId.Add(pp.Id);
                }
                p.SqlWhere.Add(f => AgentId.Contains(f.AgentId));
            }
            else
            {
                p.SqlWhere.Add(f => f.AgentId == BasicAgent.Id);
            }
            p.OrderByList.Add("Id", "DESC");
            IPageOfItems<ApplyJoin> ApplyJoinList = Entity.Selects<ApplyJoin>(p);
            ViewBag.ApplyJoinList = ApplyJoinList;
            ViewBag.ApplyJoin = ApplyJoin;
            ViewBag.BasicProvinceList = Entity.BasicProvince.ToList();
            ViewBag.BasicCityList = Entity.BasicCity.ToList();
            return View();
        }
        public ActionResult Edit(ApplyJoin ApplyJoin)
        {
            ViewBag.Save = checkPower("Save");
            ViewBag.Remark = ApplyJoin.Remark;
            if (ApplyJoin.Id != 0) ApplyJoin = Entity.ApplyJoin.FirstOrDefault(n => n.Id == ApplyJoin.Id);
            if (ApplyJoin == null)
            {
                ViewBag.ErrorMsg = AgentLanguage.Empty;
                return View("Error");
            }
            if (!ApplyJoin.Id.IsNullOrEmpty())
            {
                if (!IsBelongToAgent(ApplyJoin.AgentId))
                {
                    ViewBag.ErrorMsg = AgentLanguage.Surmount;
                    return View("Error");
                }
            }
            ViewBag.ApplyJoin = ApplyJoin;
            if (Request.UrlReferrer != null)
            {
                Session["Url"] = Request.UrlReferrer.ToString();
            }
            ViewBag.BasicProvince = Entity.BasicProvince.FirstOrNew(n => n.Id == ApplyJoin.Province);
            ViewBag.BasicCity = Entity.BasicCity.FirstOrNew(n => n.Id == ApplyJoin.City);
            return View();
        }
        [ValidateInput(false)]
        public ActionResult Save(ApplyJoin ApplyJoin)
        {
            ApplyJoin baseApplyJoin = Entity.ApplyJoin.FirstOrDefault(n => n.Id == ApplyJoin.Id);
            if (baseApplyJoin == null)
            {
                ViewBag.ErrorMsg = AgentLanguage.Empty;
                return View("Error");
            }
            if (!IsBelongToAgent(baseApplyJoin.AgentId))
            {
                ViewBag.ErrorMsg = AgentLanguage.Surmount;
                return View("Error");
            }
            if (ApplyJoin.Remark.IsNullOrEmpty())
            {
                ApplyJoin.Remark = "无备注";
            }
            string State = "无改变";
            if (ApplyJoin.State == 1)
            {
                baseApplyJoin.State = 2;
            }
            if (ApplyJoin.State == 2)
            {
                State = "有意向";
                baseApplyJoin.State = 2;
            }
            else if (ApplyJoin.State == 3)
            {
                State = "无意向";
                baseApplyJoin.State = 3;
            }
            else if (ApplyJoin.State == 4)
            {
                State = "已合作";
                baseApplyJoin.State = 4;
            }
            string Remark = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "§" + ApplyJoin.Remark + "§" + State + "§" + AdminUser.TrueName; ;
            if (baseApplyJoin.Remark.IsNullOrEmpty())
            {
                baseApplyJoin.Remark = Remark;
            }
            else
            {
                baseApplyJoin.Remark += "№" + Remark;
            }
            // baseApplyJoin = Request.ConvertRequestToModel<ApplyJoin>(baseApplyJoin, ApplyJoin);
            Entity.SaveChanges();
            // BaseRedirect();
            //CloseArt
            return View("ReloadFrame");
        }
    }
}
