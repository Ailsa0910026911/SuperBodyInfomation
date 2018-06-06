using Aop.Api;
using Aop.Api.Request;
using Aop.Api.Response;
using LokFu.Extensions;
using LokFu.Infrastructure;
using LokFu.Models;
using LokFu.PayMent.ALF2FPAY;
using LokFu.PayMent.WxPayAPI;
using LokFu.Repositories;
using LokFu.Repositories.SqlServer;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Caching;
using System.Web.Mvc;
namespace LokFu.Areas.Agent.Controllers
{
    public class FastOrderController : BaseController
    {
        public ActionResult Index(FastOrder FastOrder, EFPagingInfo<FastOrder> p, bool? IsShowSupAgent, int? LowerLevel, int IsFirst = 0)
        {
            if (IsShowSupAgent == null)
            {
                IsShowSupAgent = false;
            }
            LowerLevel = LowerLevel == null ? 0 : LowerLevel;
            p = this.Condition(FastOrder, p, IsShowSupAgent.Value, LowerLevel.Value);
            IPageOfItems<FastOrder> FastOrderList = null;
            if (IsFirst == 0)
            {
                FastOrder.STime = DateTime.Now.AddDays(-1).Date;
                FastOrder.ETime = DateTime.Now;
                FastOrderList = new PageOfItems<FastOrder>(new List<FastOrder>(), 0, 10, 0, new Hashtable());
            }
            else
            {
                FastOrderList = Entity.Selects<FastOrder>(p);
            }
            ViewBag.FastOrderList = FastOrderList;
            ViewBag.FastOrder = FastOrder;
            //副表信息
            List<int> UId = FastOrderList.Select(o => o.UId).Distinct().ToList();
            ViewBag.UsersList = Entity.Users.Where(n => n.State == 1 && UId.Contains(n.Id)).ToList();

            //其他信息
            ViewBag.IsShowSupAgent = IsShowSupAgent;//是否显示下级
            ViewBag.LowerLevel = LowerLevel;

            //权限相关
            ViewBag.Edit = this.checkPower("Edit");
            return View();
        }

        public ActionResult Info(FastOrder FastOrder)
        {
            if (!FastOrder.Id.IsNullOrEmpty())
            {
                FastOrder = Entity.FastOrder.FirstOrDefault(n => n.Id == FastOrder.Id);
            }
            else if (!FastOrder.TNum.IsNullOrEmpty())
            {
                FastOrder = Entity.FastOrder.FirstOrDefault(n => n.TNum == FastOrder.TNum);
            }
            if (FastOrder == null)
            {
                ViewBag.ErrorMsg = "查询的交易不存在";
                return View("Error");
            }
            if (!IsBelongToAgent(FastOrder.Agent))
            {
                ViewBag.ErrorMsg = AgentLanguage.Surmount;
                return View("Error");
            }
            ViewBag.FastOrder = FastOrder;
            ViewBag.Users = Entity.Users.FirstOrNew(n => n.Id == FastOrder.UId);
            ViewBag.SysAgent = Entity.SysAgent.FirstOrNew(n => n.Id == FastOrder.Agent);
            return View("Edit");
        }

        private EFPagingInfo<FastOrder> Condition(FastOrder FastOrder, EFPagingInfo<FastOrder> p, bool IsShowSupAgent, int LowerLevel)
        {
            #region 筛选条件
            if (!FastOrder.TNum.IsNullOrEmpty())
            {
                if (FastOrder.UId == 1)
                {
                    p.SqlWhere.Add(f => f.TNum == FastOrder.TNum);
                }
                else
                {
                    IList<Users> UList = Entity.Users.Where(n => n.TrueName.Contains(FastOrder.TNum) || n.NeekName.Contains(FastOrder.TNum) || n.UserName == FastOrder.TNum).ToList();
                    List<int> UIds = new List<int>();
                    foreach (var pp in UList)
                    {
                        UIds.Add(pp.Id);
                    }
                    p.SqlWhere.Add(f => UIds.Contains(f.UId));
                }
            }
            if (!FastOrder.UserState.IsNullOrEmpty())
            {
                var value = FastOrder.UserState == 99 ? 0 : FastOrder.UserState;
                p.SqlWhere.Add(o => o.UserState == value);
            }
            IList<SysAgent> SysAgentList = null;
            if ((bool)IsShowSupAgent)
            {
                IList<int> UID = new List<int>();
                if (LowerLevel != 0)
                {
                    SysAgent LowerLevelAgent = Entity.SysAgent.Where(s => s.Id == LowerLevel).FirstOrNew();
                    SysAgentList = LowerLevelAgent.GetSupAgent(Entity, true);
                }
                else
                {
                    SysAgentList = BasicAgent.GetSupAgent(Entity, true);//获取所有下级代理商信息
                }
                UID = SysAgentList.Select(o => o.Id).ToList();
                p.SqlWhere.Add(f => UID.Contains(f.Agent));
            }
            else
            {
                p.SqlWhere.Add(f => f.Agent == BasicAgent.Id);//读取全部分支机构
            }
            
            if (!FastOrder.STime.IsNullOrEmpty() && !FastOrder.ETime.IsNullOrEmpty())
            {
                DateTime ETime = FastOrder.ETime;
                p.SqlWhere.Add(f => f.AddTime > FastOrder.STime && f.AddTime < ETime);
            }

            #region 交易类型条件判断
            if (!FastOrder.OType.IsNullOrEmpty())
            {
                p.SqlWhere.Add(f => f.OType == FastOrder.OType);//读取对应的类型
            }
            if (!FastOrder.State.IsNullOrEmpty())
            {
                switch (FastOrder.State)
                {
                    case 1://未付
                        p.SqlWhere.Add(f => f.State == 1 && f.PayState == 0);
                        break;
                    case 2://已付
                        p.SqlWhere.Add(f => f.State == 1 && f.PayState == 1);
                        break;
                    case 99://交易关闭
                        p.SqlWhere.Add(f => f.State == 0);
                        break;
                }
            }
            #endregion
           
            #endregion
            p.OrderByList.Add("Id", "DESC");

            return p;
        }


        /// <summary>
        /// 统计
        /// </summary>
        /// <returns></returns>
        public ActionResult IndexStats(FastOrder FastOrder, EFPagingInfo<FastOrder> p, bool IsShowSupAgent, int LowerLevel)
        {
            if (FastOrder.STime.IsNullOrEmpty() || FastOrder.ETime.IsNullOrEmpty())
            {
                ViewBag.ErrorMsg = "请选择日期";
                return View("Error");
            }
            Dictionary<string, string> dicChar = new Dictionary<string, string>();
            dicChar.Add("STIME",FastOrder.STime.ToString("yyyy-MM-dd HH:mm:ss"));
            dicChar.Add("ETIME",FastOrder.ETime.ToString("yyyy-MM-dd HH:mm:ss"));
            string AGENTID = "";
            string IFALL = "";
            if ((bool)IsShowSupAgent)
            {
                IFALL = "1";
            }
            else
            {
                IFALL = "0";
            }
            if (LowerLevel != 0)
            {
                AGENTID = LowerLevel.ToString();
            }
            else
            {
                AGENTID = BasicAgent.Id.ToString();//获取所有下级代理商信息
            }
            dicChar.Add("AGENTID", AGENTID);
            dicChar.Add("AGENT", BasicAgent.Id.ToString());
           
            if (!FastOrder.OType.IsNullOrEmpty())
            {
                dicChar.Add("OType", FastOrder.OType.ToString());
            }
            else
            {
                dicChar.Add("OType", "");
            }
            string State = "";
            string PayState = "";
            string TNum = "";
            string CODE = "";
            string USERSTATE = "";
            if (!FastOrder.State.IsNullOrEmpty())
            {
                switch (FastOrder.State)
                {
                    case 1://未付
                        State = "1";
                        PayState = "0";
                        break;
                    case 2://已付
                         State = "1";
                        PayState = "1";
                        break;
                    case 99://交易关闭
                        State = "0";
                        break;
                }
            }
           //商户号
            if (!FastOrder.TNum.IsNullOrEmpty())
            {
                if (FastOrder.UId == 1)
                {
                   TNum= FastOrder.TNum;
                }
                else
                {
                    CODE =FastOrder.TNum;
                }
            }
            if (!FastOrder.UserState.IsNullOrEmpty())
            {
                USERSTATE = (FastOrder.UserState == 99 ? 0 : FastOrder.UserState).ToString();
                
            }
            dicChar.Add("State", State);
            dicChar.Add("PayState", PayState);
            dicChar.Add("TNum", TNum);
            dicChar.Add("CODE", CODE);
            dicChar.Add("IFALL", IFALL);
            dicChar.Add("USERSTATE", USERSTATE);
            IList<TongJiFastOrder> TongJiFastOrder = Entity.GetSPExtensions<TongJiFastOrder>("SP_Statistics_Fastorder_sum", dicChar);
            ViewBag.SumAmoney = TongJiFastOrder[0].Amoney;
            ViewBag.SumProfit = TongJiFastOrder[0].Profit;
            return this.View();
        }

        public ActionResult IndexOrderProfitLog(string tnum)
        {
            FastOrder FastOrder = Entity.FastOrder.FirstOrDefault(n => n.TNum == tnum);
            if (FastOrder == null)
            {
                ViewBag.ErrorMsg = "查询的交易不存在";
                return View("Error");
            }
            var query = this.Entity.OrderProfitLog.Where(o => o.TNum == tnum);
            if (FastOrder.Agent == this.BasicAgent.Id)
            {
                query = query.Where(o => (o.Agent == this.BasicAgent.Id || o.LogType == 1));
            }
            else
            {
                query = query.Where(o => o.Agent == this.BasicAgent.Id);
            }
            var OrderProfitLog = query.OrderByDescending(o => o.Id).ToList();
            ViewBag.OrderProfitLog = OrderProfitLog;

            var UIds = OrderProfitLog.Select(o => o.UId).ToList();
            var UserNameList = this.Entity.Users.Where(o => UIds.Contains(o.Id)).ToDictionary(o => o.Id, o => o.TrueName);
            ViewBag.UserNameList = UserNameList;

            var Agents = OrderProfitLog.Select(o => o.Agent).ToList();
            var AgentsList = this.Entity.SysAgent.Where(o => Agents.Contains(o.Id)).ToDictionary(o => o.Id, o => o.Name);
            ViewBag.SysAgentList = AgentsList;

            return View();
        }
    }
    public class TongJiFastOrder
    {
      
        /// 交易金额
        /// </summary>
        public Decimal Amoney { get; set; }
      
        /// <summary>
        /// 分润
        /// </summary>
        public Decimal Profit { get; set; }
       
    }
}
