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
    public class FinOrderController : BaseController
    {

        public ActionResult Index(Orders Orders, EFPagingInfo<Orders> p, bool? IsShowSupAgent, int IsFirst = 0, int TimeType=1)
        {
            if (IsShowSupAgent == null)
            {
                IsShowSupAgent = false;
            }
            p = this.Condition(Orders, p, IsShowSupAgent,TimeType);
            p.PageSize = 1000;
            p.OrderByList.Add("Id", "DESC");
            IPageOfItems<Orders> OrdersList = null;
            if (IsFirst == 0)
            {
                OrdersList = new PageOfItems<Orders>(new List<Orders>(), 0, 10, 0, new Hashtable());
            }
            else
            {
                OrdersList = Entity.Selects<Orders>(p);
            }
          
            ViewBag.OrdersList = OrdersList;
            ViewBag.Orders = Orders;
            IList<Orders> List = OrdersList.GroupBy(n => n.UId).Select(n => n.First()).ToList();
            List<int> UId = new List<int>();
            foreach (var pp in List)
            {
                UId.Add(pp.UId);
            }
            var agentIDs = OrdersList.Select(o => o.Agent).Distinct().ToList();
            var OrdersListSysAgent = Entity.SysAgent.Where(o => agentIDs.Contains(o.Id)).ToList();//订单对应的代理
            ViewBag.UsersList = Entity.Users.Where(n => n.State == 1 && UId.Contains(n.Id)).ToList();
            ViewBag.PayConfigList = Entity.PayConfig.Where(n => n.State == 1).ToList();
            ViewBag.SysAgentList = Entity.SysAgent.Where(n => n.State == 1).ToList();
            ViewBag.IsShowSupAgent = IsShowSupAgent;
            ViewBag.OrdersListSysAgent = OrdersListSysAgent;
            ViewBag.Xls = this.checkPower("Xls");
            ViewBag.TimeType = TimeType;
            return View();
        }
        public ActionResult Info(Orders Orders)
        {
            return this.RedirectToAction("Info", "Orders", new { Id = Orders.Id,IsAjax=1 }); ;
        }
        public void XLSDo(Orders Orders, EFPagingInfo<Orders> p, bool? IsShowSupAgent, int TimeType=1)
        {
            if (IsShowSupAgent == null)
            {
                IsShowSupAgent = false;
            }
            p = this.Condition(Orders, p, IsShowSupAgent, TimeType);
            
            p.PageSize = 99999999;
            p.OrderByList.Add("Id", "DESC");
            IPageOfItems<Orders> OrdersList = Entity.Selects<Orders>(p);

            if (OrdersList.Count() > 0)
            {
                #region 导出
                string file = Server.MapPath("/template") + "\\finorders.xlsx";
                ExcelPackage package = new ExcelPackage(new FileInfo(file), true);
                var sheet = package.Workbook.Worksheets[1];
                var cells = sheet.Cells;
                int Rows = OrdersList.Count();
                //设置数据开始行
                int Befor = 2;
                sheet.InsertRow(Befor + 1, Rows - 1, Befor);
                int i = Befor;
                sheet.Row(i - 1).Height = 22;//设置行高
                int maxCol = 0;
                //获取机构及管理员
                IList<Orders> List = OrdersList.GroupBy(n => n.UId).Select(n => n.First()).ToList();
                List<int> UId = new List<int>();
                foreach (var pp in List)
                {
                    UId.Add(pp.UId);
                }
                IList<Users> UsersList = Entity.Users.Where(n => n.State == 1 && UId.Contains(n.Id)).ToList();
                IList<PayConfig> PayConfigList = Entity.PayConfig.Where(n => n.State == 1).ToList();
                IList<SysAgent> SysAgentList = Entity.SysAgent.ToList();
                foreach (var item in OrdersList)
                {
                    #region MyRegion
                    string AName = "--";
                    if (item.AId > 0)
                    {
                        SysAgent SA = SysAgentList.FirstOrNew(n => n.Id == item.Agent);
                        if (!SA.Name.IsNullOrEmpty())
                        {
                            AName = SA.Name;
                        }
                    }
                    string BName = "--";
                    Users OU = UsersList.FirstOrNew(n => n.Id == item.UId);
                    if (!OU.TrueName.IsNullOrEmpty())
                    {
                        BName = OU.TrueName;
                    }
                    PayConfig PayConfig = PayConfigList.FirstOrNew(n => n.Id == item.PayWay);
                    sheet.Row(i).Height = 22;//设置行高
                    //机构
                    cells["A" + i].Value = AName;
                    //交易号
                    cells["B" + i].Value = item.TNum;
                    //交易商户
                    cells["C" + i].Value = OU.NeekName + "[" + OU.TrueName + "]";
                    //交易类型
                    cells["D" + i].Value = item.GetTTName();
                    //交易内容
                    cells["E" + i].Value = item.GetPayNameWithTName();
                    //交易金额
                    cells["F" + i].Value = item.Amoney.ToString("f2");
                    //交易时间
                    cells["G" + i].Value = item.PayTime;
                    //入账时间
                    cells["H" + i].Value = (item.InTimed!=null?Convert.ToDateTime(item.InTimed).ToString():"");
                    //支付方式
                    cells["I" + i].Value = PayConfig.Name;
                    //订单状态
                    cells["J" + i].Value = item.GetState();
                    string temp = string.Empty;
                    if (item.TType == 2 || item.TType == 5)
                    {
                        temp = "T+" + item.TrunType;
                    }
                    if (item.TType == 1 || item.TType == 7 || item.TType == 8 || item.TType == 9)
                    {
                        temp = "T+" + item.LagEntryDay;
                    }
                    //接收方式
                    cells["K" + i].Value = temp;
                    double I = 0, K = 0, N = 0;
                    decimal J = 0, L = 0, M = 0;
                    string Is = "";
                    #region MyRegion
                    if (item.TType == 1)
                    { //银联卡支付订单
                        OrderRecharge OrderRecharge = Entity.OrderRecharge.FirstOrNew(n => n.OId == item.TNum);
                        I = OrderRecharge.UserRate;//商户费率
                        J = OrderRecharge.Poundage;//手续费
                        K = OrderRecharge.SysRate; //第三方费率
                        L = ((decimal)OrderRecharge.SysRate * OrderRecharge.Amoney).Ceiling();//第三方手续费
                        N = OrderRecharge.AgentPayGet; //分支机构提成
                        M = OrderRecharge.Poundage - (decimal)OrderRecharge.SysRate * OrderRecharge.Amoney - (decimal)OrderRecharge.AgentPayGet; //利润
                    }
                    if (item.TType == 2)
                    { //提现订单
                        OrderCash OrderCash = Entity.OrderCash.FirstOrNew(n => n.OId == item.TNum);
                        I = 0;
                        if (!OrderCash.ECash.IsNullOrEmpty() && !OrderCash.Cash.IsNullOrEmpty())
                        {
                            Is = OrderCash.ECash.ToMoney() + "+" + (OrderCash.Cash * 100).ToString("F2") + "%";
                        }
                        else if (!OrderCash.ECash.IsNullOrEmpty())
                        {
                            Is = OrderCash.ECash.ToMoney();
                        }
                        else if (!OrderCash.Cash.IsNullOrEmpty())
                        {
                            Is = (OrderCash.Cash * 100).ToString("F2") + "%";
                        }
                        else
                        {
                            Is = "0";
                        }
                        J = (decimal)OrderCash.UserRate;//手续费
                        K = 0;//第三方费率
                        L = OrderCash.CashRate;//第三方手续费
                        N = OrderCash.AgentCashGet; //分支机构提成
                        M = (decimal)OrderCash.UserRate - OrderCash.CashRate - (decimal)OrderCash.AgentCashGet; //利润
                    }
                    if (item.TType == 3)
                    { //转帐订单
                        OrderTransfer OrderTransfer = Entity.OrderTransfer.FirstOrNew(n => n.OId == item.TNum);
                        I = OrderTransfer.UserRate;//商户费率
                        J = OrderTransfer.Poundage;//手续费
                        K = OrderTransfer.SysRate;//第三方费率
                        L = ((decimal)OrderTransfer.SysRate * OrderTransfer.Amoney).Ceiling();//第三方手续费
                        N = OrderTransfer.AgentPayGet; //分支机构提成
                        M = OrderTransfer.Amoney - OrderTransfer.PayMoney - L - (decimal)OrderTransfer.AgentPayGet; //利润
                    }
                    if (item.TType == 5)
                    { //付房租订单
                        OrderHouse OrderHouse = Entity.OrderHouse.FirstOrNew(n => n.OId == item.TNum);
                        I = OrderHouse.UserRate + (double)OrderHouse.CashRate;//商户费率
                        J = OrderHouse.Poundage;//手续费
                        K = OrderHouse.SysRate;//第三方费率
                        L = ((decimal)OrderHouse.SysRate * OrderHouse.Amoney).Ceiling();//第三方手续费
                        N = OrderHouse.AgentPayGet; //分支机构提成
                        M = OrderHouse.Amoney - OrderHouse.PayMoney - L - (decimal)OrderHouse.AgentPayGet; //利润
                    }
                    if (item.TType == 6)
                    { //升级订单
                        PayConfigOrder PayConfigOrder = Entity.PayConfigOrder.FirstOrNew(n => n.OId == item.TNum);
                        I = 0;//商户费率
                        J = PayConfigOrder.Poundage;//手续费
                        K = PayConfigOrder.SysRate;//第三方费率
                        L = ((decimal)PayConfigOrder.SysRate * PayConfigOrder.Amoney).Ceiling();//第三方手续费
                        N = PayConfigOrder.AgentGet; //分支机构提成
                        M = PayConfigOrder.Amoney - (decimal)PayConfigOrder.AgentGet; //利润
                    }
                    if (item.TType == 7 || item.TType == 8 || item.TType == 9)
                    { //当面付
                        OrderF2F OrderF2F = Entity.OrderF2F.FirstOrNew(n => n.OId == item.TNum);
                        I = OrderF2F.UserRate;//商户费率
                        J = OrderF2F.Poundage;//手续费
                        K = OrderF2F.SysRate; //第三方费率
                        L = ((decimal)OrderF2F.SysRate * OrderF2F.Amoney).Ceiling();//第三方手续费
                        N = OrderF2F.AgentPayGet; //分支机构提成
                        M = OrderF2F.Poundage - (decimal)OrderF2F.SysRate * OrderF2F.Amoney - (decimal)OrderF2F.AgentPayGet; //利润
                    }
                    if (item.TType == 10)
                    { //升级订单
                        DaiLiOrder DaiLiOrder = Entity.DaiLiOrder.FirstOrNew(n => n.OId == item.TNum);
                        I = 0;//商户费率
                        J = 0;//手续费
                        K = 0;//第三方费率
                        L = 0;//第三方手续费
                        N = DaiLiOrder.AgentGet; //分支机构提成
                        M = DaiLiOrder.Amoney - (decimal)DaiLiOrder.AgentGet; //利润
                    } 
                    #endregion
                    //用户费率9
                    if (item.TType == 2)
                    {
                        cells["L" + i].Value = Is;
                    }
                    else
                    {
                        cells["L" + i].Value = I;
                    }
                    //用户手续费10 
                    cells["M" + i].Value = J;
                    //支出费率11 系统费率
                    cells["N" + i].Value = K;
                    //支出手续费12
                    cells["O" + i].Value = L;
                    //利润13
                    cells["P" + i].Value = M;
                    //结算金额14
                    cells["Q" + i].Value = N;
                    i++; 
                    #endregion
                }
                sheet.Row(i).Height = 40;//设置行高
                //交易总额
                cells["F" + i].Formula = "SUM(F" + Befor + ":F" + (i - 1) + ")";
                //用户手续费10
                cells["M" + i].Formula = "SUM(M" + Befor + ":M" + (i - 1) + ")";
                //支出手续费12
                cells["O" + i].Formula = "SUM(O" + Befor + ":O" + (i - 1) + ")";
                //利润13
                cells["P" + i].Formula = "SUM(P" + Befor + ":P" + (i - 1) + ")";
                //结算金额14
                cells["Q" + i].Formula = "SUM(Q" + Befor + ":Q" + (i - 1) + ")";
               
                i--;
                maxCol = 17;
                //cells["B" + (i + 2)].Value = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                sheet.Cells[Befor, 1, i, maxCol].Style.Border.Left.Style = ExcelBorderStyle.Thin;
                sheet.Cells[Befor, 1, i, maxCol].Style.Border.Right.Style = ExcelBorderStyle.Thin;
                sheet.Cells[Befor, 1, i, maxCol].Style.Border.Top.Style = ExcelBorderStyle.Thin;
                sheet.Cells[Befor, 1, i, maxCol].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                sheet.Cells[Befor, 1, i, maxCol].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;//居中
                sheet.Cells[Befor, 7, i, 7].Style.Numberformat.Format = "yyyy-mm-dd HH:mm:ss";
                sheet.Cells[Befor, 8, i, 8].Style.Numberformat.Format = "yyyy-mm-dd HH:mm:ss";
                sheet.Cells[Befor, 12, i, 12].Style.Numberformat.Format = "#0.00%";
                sheet.Cells[Befor, 14, i, 14].Style.Numberformat.Format = "#0.00%";
                i++;
                sheet.Cells[Befor, 6, i, 6].Style.Numberformat.Format = "\"¥\"#,##0.00_);[Red](\"¥\"#,##0.00)";
                sheet.Cells[Befor, 13, i, 13].Style.Numberformat.Format = "\"¥\"#,##0.00_);[Red](\"¥\"#,##0.00)";
                sheet.Cells[Befor, 15, i, 15].Style.Numberformat.Format = "\"¥\"#,##0.00_);[Red](\"¥\"#,##0.00)";
                sheet.Cells[Befor, 16, i, 16].Style.Numberformat.Format = "\"¥\"#,##0.00_);[Red](\"¥\"#,##0.00)";
                sheet.Cells[Befor, 17, i, 17].Style.Numberformat.Format = "\"¥\"#,##0.00_);[Red](\"¥\"#,##0.00)";
                Response.BinaryWrite(package.GetAsByteArray());
                //输出
                Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                Response.AddHeader("content-disposition", "attachment;  filename=" + DateTime.Now.ToString("yyyyMMddHHmmss") + new Random().Next(10, 99) + ".xlsx");
                #endregion
            }
            else
            {
                Response.Write("暂无符合条件数据");
            }
        }
        [NonAction]
        private EFPagingInfo<Orders> Condition(Orders Orders, EFPagingInfo<Orders> p, bool? IsShowSupAgent, int TimeType=1)
        {
            if (Orders.STime.IsNullOrEmpty())
            {
                Orders.STime = DateTime.Parse(DateTime.Now.ToString("yyyy-MM-dd"));
            }
            if (Orders.ETime.IsNullOrEmpty())
            {
                Orders.ETime = DateTime.Now;
            }
            if (!Orders.TType.IsNullOrEmpty()) { p.SqlWhere.Add(f => f.TType == Orders.TType); }
            if (!Orders.UId.IsNullOrEmpty()) { p.SqlWhere.Add(f => f.UId == Orders.UId); }
            //是否选择了一个分支机构
            if (!Orders.Agent.IsNullOrEmpty())
            {
                //是否要显示下级
                if ((bool)IsShowSupAgent)
                {
                    SysAgent LowerLevelAgent = Entity.SysAgent.Where(s => s.Id == Orders.Agent).FirstOrNew();
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
                    p.SqlWhere.Add(f => f.Agent == Orders.Agent);
                }
            }
            if (true)
            {

            }
            if (TimeType == 1)//交易时间
            {
                if (!Orders.STime.IsNullOrEmpty() && !Orders.ETime.IsNullOrEmpty())
                {
                    DateTime ETime = Orders.ETime.AddMilliseconds(999); 
                    p.SqlWhere.Add(f => f.PayTime > Orders.STime && f.PayTime < ETime);
                }
                #region 交易类型条件判断
                if (!Orders.TType.IsNullOrEmpty())
                {
                    p.SqlWhere.Add(f => f.TType == Orders.TType);//读取对应的类型
                    if (Orders.TType == 1)
                    {
                        if (!Orders.TState.IsNullOrEmpty())
                        {
                            switch (Orders.TState)
                            {
                                case 1://未付
                                    p.SqlWhere.Add(f => f.TState == 1 && f.PayState == 0);
                                    break;
                                case 2://已付
                                    p.SqlWhere.Add(f => f.TState == 2 && f.PayState == 1);
                                    break;
                                case 3://待传证照
                                    p.SqlWhere.Add(f => f.TState == 1 && f.PayState == 1 && f.IdCardState == 1);
                                    break;
                                case 4://待审核
                                    p.SqlWhere.Add(f => f.TState == 1 && f.PayState == 1 && f.IdCardState == 2);
                                    break;
                                case 5://审核失败
                                    p.SqlWhere.Add(f => f.TState == 3 && f.PayState == 1 && f.IdCardState == 4);
                                    break;
                                case 6://退单
                                    p.SqlWhere.Add(f => f.TState == 4);
                                    break;
                                case 7://待入帐
                                    p.SqlWhere.Add(f => f.TState == 1 && f.PayState == 1 && f.InState == 0);
                                    break;
                            }
                        }
                    }
                    if (Orders.TType == 2)
                    {
                        //if (!Orders.TState.IsNullOrEmpty()) { p.SqlWhere.Add(f => f.TState == Orders.TState); }
                        switch (Orders.TState)
                        {
                            case 1://处理中
                                p.SqlWhere.Add(f => f.TState == 1);
                                break;
                            case 2://已汇出
                                p.SqlWhere.Add(f => f.TState == 2 && f.PayState == 2);
                                break;
                            case 3://提现失败
                                p.SqlWhere.Add(f => f.TState == 3);
                                break;
                            case 4://出款中
                                p.SqlWhere.Add(f => f.TState == 2 && f.PayState == 1);
                                break;
                        }
                    }
                    if (Orders.TType == 3)
                    {
                        switch (Orders.TState)
                        {
                            case 1://未付
                                p.SqlWhere.Add(f => f.TState == 1 && f.PayState == 0);
                                break;
                            case 2://已付
                                p.SqlWhere.Add(f => f.TState == 2 && f.PayState == 1);
                                break;
                            case 3://待传证照
                                p.SqlWhere.Add(f => f.TState == 1 && f.PayState == 1 && f.IdCardState == 1);
                                break;
                            case 4://待审核
                                p.SqlWhere.Add(f => f.TState == 1 && f.PayState == 1 && f.IdCardState == 2);
                                break;
                            case 5://审核失败
                                p.SqlWhere.Add(f => f.TState == 3 && f.PayState == 1 && f.IdCardState == 4);
                                break;
                        }
                    }
                    if (Orders.TType == 5)
                    {
                        if (!Orders.TState.IsNullOrEmpty())
                        {
                            switch (Orders.TState)
                            {
                                case 99://未付
                                    p.SqlWhere.Add(f => f.TState == 1 && f.PayState == 0);
                                    break;
                                case 1://处理中
                                    p.SqlWhere.Add(f => f.TState == 1 && f.PayState == 1 && f.IdCardState == 0);
                                    break;
                                case 2://已汇出
                                    p.SqlWhere.Add(f => f.TState == 2 && f.PayState == 2);
                                    break;
                                case 4://出款中
                                    p.SqlWhere.Add(f => (f.TState == 2 && f.PayState == 2 && f.IdCardState == 0) || (f.TState == 2 && f.PayState == 1 && f.IdCardState == 3));
                                    break;
                                case 3://审核失败
                                    p.SqlWhere.Add(f => (f.TState == 1 || f.TState == 3) && f.PayState == 1 && f.IdCardState == 4);
                                    break;
                                case 5://退款中
                                    p.SqlWhere.Add(f => f.TState == 2 && f.PayState == 3);
                                    break;
                                case 6://已退款
                                    p.SqlWhere.Add(f => f.TState == 2 && f.PayState == 4);
                                    break;
                                case 7://待传身份证
                                    p.SqlWhere.Add(f => f.TState == 1 && f.PayState == 1 && f.IdCardState == 1);
                                    break;
                                case 8://已传身份证
                                    p.SqlWhere.Add(f => f.TState == 1 && f.PayState == 1 && f.IdCardState == 2);
                                    break;
                            }
                        }
                    }
                    if (Orders.TType == 6)
                    {
                        if (!Orders.TState.IsNullOrEmpty()) { p.SqlWhere.Add(f => f.TState == Orders.TState); }
                    }
                    if (Orders.TType == 7 || Orders.TType == 8 || Orders.TType == 9)
                    {
                        if (!Orders.TState.IsNullOrEmpty())
                        {
                            if (Orders.TState == 99)
                            {
                                p.SqlWhere.Add(f => f.TState == 0);
                            }
                            if (Orders.TState == 1)
                            {
                                p.SqlWhere.Add(f => f.TState == 1 && f.PayState == 0);
                            }
                            if (Orders.TState == 2)
                            {
                                p.SqlWhere.Add(f => f.TState == 2 && f.PayState == 1);
                            }
                            if (Orders.TState == 3)//退单
                            {
                                p.SqlWhere.Add(f => f.TState == 4);
                            }
                            if (Orders.TState == 4)//待入帐
                            {
                                p.SqlWhere.Add(f => f.TState == 1 && f.PayState == 1 && f.InState == 0);
                            }
                            if (Orders.TState == 5)//待审核
                            {
                                p.SqlWhere.Add(f => f.TState == 1 && f.PayState == 1 && f.InState == 1 && f.IdCardState == 2);
                            }
                            if (Orders.TState == 6)//待传证照
                            {
                                p.SqlWhere.Add(f => f.TState == 1 && f.PayState == 1 && f.InState == 1 && f.IdCardState == 1);
                            }
                            if (Orders.TState == 7)//审核失败
                            {
                                p.SqlWhere.Add(f => f.TState == 1 && f.PayState == 1 && f.InState == 1 && f.IdCardState == 4);
                            }
                        }
                    }
                    if (Orders.TType == 10)
                    {
                        if (!Orders.TState.IsNullOrEmpty()) { p.SqlWhere.Add(f => f.TState == Orders.TState); }
                    }
                }
                #endregion
            }
            else
            {
                if (!Orders.STime.IsNullOrEmpty() && !Orders.ETime.IsNullOrEmpty())
                {
                    DateTime ETime = Orders.ETime.AddMilliseconds(999);
                    p.SqlWhere.Add(f => f.InTimed > Orders.STime && f.InTimed < ETime);
                }
                if (!Orders.TType.IsNullOrEmpty())
                {
                    p.SqlWhere.Add(f =>f.TType==Orders.TType && f.InState == 1);
                }
                else
                {
                    int[] temp = { 1, 7, 8, 9 };
                    List<int> UID = new List<int>(temp);
                    p.SqlWhere.Add(f => UID.Contains(f.TType) && f.InState == 1);
                }
            }
            return p;
        }

    }
}
