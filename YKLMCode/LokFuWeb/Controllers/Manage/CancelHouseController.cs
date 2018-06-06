using LokFu.Extensions;
using LokFu.Infrastructure;
using LokFu.Models;
using LokFu.Repositories;
using LokFu.Repositories.SqlServer;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Mvc;
namespace LokFu.Areas.Manage.Controllers
{
    public class CancelHouseController : BaseController
    {

        public ActionResult Index(OrderHouse OrderHouse, EFPagingInfo<OrderHouse> p, bool? IsShowSupAgent, int IsFirst = 0)
        {
            if (IsShowSupAgent == null)
            {
                IsShowSupAgent = false;
            }
            p.SqlWhere.Add(f => f.OrderState == 2);
            p.SqlWhere.Add(f => f.PayState == 3 || f.PayState == 4);
            //if (!OrderHouse.UId.IsNullOrEmpty()) { p.SqlWhere.Add(f => f.UId == OrderHouse.UId); }
            if (!OrderHouse.HouseOwner.IsNullOrEmpty())
            {
                IList<Users> UList = Entity.Users.Where(n => n.TrueName.Contains(OrderHouse.HouseOwner) || n.NeekName.Contains(OrderHouse.HouseOwner) || n.UserName == OrderHouse.HouseOwner).ToList();
                List<int> UIds = new List<int>();
                foreach (var pp in UList)
                {
                    UIds.Add(pp.Id);
                }
                p.SqlWhere.Add(f => UIds.Contains(f.UId));
            }
            if (!OrderHouse.OId.IsNullOrEmpty()) { p.SqlWhere.Add(f => f.OId == OrderHouse.OId); }
            if (!OrderHouse.Agent.IsNullOrEmpty())
            {
                //是否要显示下级
                if ((bool)IsShowSupAgent)
                {
                    SysAgent LowerLevelAgent = Entity.SysAgent.Where(s => s.Id == OrderHouse.Agent).FirstOrNew();
                    IList<SysAgent> SysAgentList = LowerLevelAgent.GetSupAgent(Entity);
                    IList<int> UID = new List<int>();
                    foreach (var s in SysAgentList)
                    {
                        UID.Add(s.Id);
                    }
                    p.SqlWhere.Add(f => UID.Contains(f.Agent));
                }
                else
                {
                    p.SqlWhere.Add(f => f.Agent == OrderHouse.Agent);
                }
            }
            if (!OrderHouse.PayState.IsNullOrEmpty()) { p.SqlWhere.Add(f => f.PayState == OrderHouse.PayState); }
            if (!OrderHouse.AId.IsNullOrEmpty()) { p.SqlWhere.Add(f => f.AId == OrderHouse.AId); }
            if (!OrderHouse.FId.IsNullOrEmpty()) { p.SqlWhere.Add(f => f.FId == OrderHouse.FId); }
            if (!OrderHouse.AgentState.IsNullOrEmpty()) { p.SqlWhere.Add(f => f.AgentState == OrderHouse.AgentState); }
            if (!OrderHouse.AddTime.IsNullOrEmpty() && !OrderHouse.FTime.IsNullOrEmpty())
            {
               // DateTime FTime = ((DateTime)OrderHouse.FTime).AddDays(1);
                DateTime FTime =OrderHouse.FTime.Value;
                p.SqlWhere.Add(f => f.PayTime > OrderHouse.AddTime && f.PayTime < FTime);
            }
            p.OrderByList.Add("Id", "DESC");
            IPageOfItems<OrderHouse> OrderHouseList = null;
            if (IsFirst == 0)
            {
                OrderHouseList = new PageOfItems<OrderHouse>(new List<OrderHouse>(), 0, 10, 0, new Hashtable());
            }
            else
            { 
                OrderHouseList = Entity.Selects<OrderHouse>(p);
            }
            ViewBag.OrderHouseList = OrderHouseList;
            ViewBag.OrderHouse = OrderHouse;
            IList<OrderHouse> List = OrderHouseList.GroupBy(n => n.UId).Select(n => n.First()).ToList();
            List<int> UId = new List<int>();
            foreach (var pp in List)
            {
                UId.Add(pp.UId);
            }
            ViewBag.UsersList = Entity.Users.Where(n => n.State == 1 && UId.Contains(n.Id)).ToList();
            ViewBag.SysAgentList = Entity.SysAgent.Where(n => n.Tier == 1).ToList();
            ViewBag.IsShowSupAgent = IsShowSupAgent;
            ViewBag.Save = this.checkPower("Save");
            return View();
        }
        public ActionResult Edit(OrderHouse OrderHouse)
        {
            if (OrderHouse.Id != 0) OrderHouse = Entity.OrderHouse.FirstOrDefault(n => n.Id == OrderHouse.Id);
            if (OrderHouse == null)
            {
                ViewBag.ErrorMsg = "数据不存在";
                return View("Error");
            }
            ViewBag.OrderHouse = OrderHouse;
            Orders Orders = Entity.Orders.FirstOrDefault(n => n.TNum == OrderHouse.OId);
            if (Orders.PayState != 3 && Orders.PayState != 4)
            {
                ViewBag.ErrorMsg = "当前状态不能退款！";
                return View("Error");
            }
            if (Orders.TState != 2)
            {
                ViewBag.ErrorMsg = "交易不成功，不能付款！";
                return View("Error");
            }
            ViewBag.Orders = Orders;
            ViewBag.Users = Entity.Users.FirstOrNew(n => n.Id == Orders.UId);
            ViewBag.SysAgent = Entity.SysAgent.FirstOrNew(n => n.Id == Orders.Agent);
            ViewBag.SysAdmin = Entity.SysAdmin.FirstOrNew(n => n.Id == Orders.AId);
            ViewBag.FinAdmin = Entity.SysAdmin.FirstOrNew(n => n.Id == Orders.FId);
            if (Request.UrlReferrer != null)
            {
                Session["Url"] = Request.UrlReferrer.ToString();
            }
            return View();
        }
        [ValidateInput(false)]
        public void SaveEdit(OrderHouse OrderHouse)
        {
            OrderHouse baseOrderHouse = Entity.OrderHouse.FirstOrDefault(n => n.Id == OrderHouse.Id);
            Orders Orders = Entity.Orders.FirstOrDefault(n => n.TNum == baseOrderHouse.OId);
            if (baseOrderHouse.OrderState == 2 && baseOrderHouse.PayState == 3)
            {
                baseOrderHouse.PayState = 2;
                baseOrderHouse.FState = 1;
                baseOrderHouse.FTime = DateTime.Now;
                Orders.PayState = 2;
                Entity.SaveChanges();
                //======分润======
                baseOrderHouse = baseOrderHouse.PayAgent(Entity, 1);
                Orders.AgentPayGet = (decimal)baseOrderHouse.AgentPayGet;
                Entity.SaveChanges();
                Orders.SendMsg(Entity);//发送消息类
            }
            BaseRedirect();
        }
        [ValidateInput(false)]
        public void Save(OrderHouse OrderHouse)
        {
            OrderHouse baseOrderHouse = Entity.OrderHouse.FirstOrDefault(n => n.Id == OrderHouse.Id);
            if (baseOrderHouse.OrderState == 2 && baseOrderHouse.PayState == 3)
            {
                baseOrderHouse.PayState = 4;
                Orders Orders = Entity.Orders.FirstOrDefault(n => n.TNum == baseOrderHouse.OId);
                Orders.PayState = 4;
                //退款到余额
                Users baseUsers = Entity.Users.FirstOrDefault(n => n.Id == baseOrderHouse.UId);
                //计算退款金额
                //手续费=总房租*付房租系统费率
                decimal Poundage = baseOrderHouse.PayMoney * (decimal)baseOrderHouse.UserRate;
                //退款金额=交易总金额-支付手续费
                decimal Amoney = baseOrderHouse.Amoney - Poundage;
                //帐户变动记录
                int USERSID = baseUsers.Id;
                string TNUM = Orders.TNum;
                string SP_Ret = Entity.SP_UsersMoney(USERSID, TNUM, Amoney, 6, "");
                if (SP_Ret != "3")
                {
                    Utils.WriteLog(string.Format("U{0},O{1},T{2}:{3}【{4}】", USERSID, TNUM, 6, Amoney, SP_Ret), "SP_UsersMoney");
                }


                baseOrderHouse = baseOrderHouse.PayAgent(Entity, 2);
                Orders.AgentPayGet = (decimal)baseOrderHouse.AgentPayGet;
                Entity.SaveChanges();
                //======分润======
                Orders.SendMsg(Entity);//发送消息类
                //T0时增加配额
                if (baseOrderHouse.TrunType == 0)
                {
                    decimal Money = baseOrderHouse.PayMoney;
                    DateTime Today = DateTime.Parse(DateTime.Now.ToString("yyyy-MM-dd"));
                    TaskTimeSet TaskTimeSet = Entity.TaskTimeSet.FirstOrDefault(n => n.ODate == Today);
                    if (TaskTimeSet != null)
                    {
                        if (TaskTimeSet.UsedMoney >= Money)
                        {
                            TaskTimeSet.UsedMoney -= Money;
                        }
                        else
                        {
                            TaskTimeSet.UsedMoney = 0;
                        }
                        Entity.SaveChanges();
                    }
                }
            }
            BaseRedirect();
        }
    }
}
