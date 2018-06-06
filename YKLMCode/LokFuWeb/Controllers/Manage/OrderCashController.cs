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
using System.Threading;
using System.Web.Mvc;
namespace LokFu.Areas.Manage.Controllers
{
    public class OrderCashController : BaseController
    {
        public ActionResult Index(OrderCash OrderCash, EFPagingInfo<OrderCash> p, bool? IsShowSupAgent, int IsFirst = 0)
        {
            if (IsShowSupAgent == null)
            {
                IsShowSupAgent = false;
            }
            p = this.Condition(OrderCash, p, IsShowSupAgent);
            if (Request.QueryString["PageSize"].IsNullOrEmpty())
            {
                p.PageSize = 30;
            }
            p.OrderByList.Add("Id", "DESC");
            IPageOfItems<OrderCash> OrderCashList = null;
            if(IsFirst == 0)
            {
                OrderCashList = new PageOfItems<OrderCash>(new List<OrderCash>(), 0, 10, 0, new Hashtable());
            }
            else
            {
                OrderCashList = Entity.Selects<OrderCash>(p);
            }
            ViewBag.OrderCashList = OrderCashList;
            ViewBag.OrderCash = OrderCash;
            IList<OrderCash> List = OrderCashList.GroupBy(n => n.UId).Select(n => n.First()).ToList();
            List<int> UId = new List<int>();
            foreach (var pp in List)
            {
                UId.Add(pp.UId);
            }
            ViewBag.UsersList = Entity.Users.Where(n => n.State == 1 && UId.Contains(n.Id)).ToList();
            ViewBag.SysAgentList = Entity.SysAgent.Where(n => n.State == 1).ToList();
            ViewBag.IsShowSupAgent = IsShowSupAgent;//是否显示下级

            bool PayCash = checkPower("PayCash");
            ViewBag.PayCash = PayCash;
            ViewBag.Edit = this.checkPower("Edit");
            ViewBag.Cancel = this.checkPower("Cancel");
            ViewBag.Save = this.checkPower("Save");
            ViewBag.Xls = this.checkPower("Xls");
            ViewBag.XLSDoShanFu = this.checkPower("XLSDoShanFu");
            return View();
        }

        /// <summary>
        /// 统计
        /// </summary>
        /// <returns></returns>
        public ActionResult IndexStats(OrderCash OrderCash, EFPagingInfo<OrderCash> p, bool? IsShowSupAgent)
        {
            if (IsShowSupAgent == null)
            {
                IsShowSupAgent = false;
            }
            p = this.Condition(OrderCash, p, IsShowSupAgent);
            var Iquery = this.Entity.OrderCash.AsQueryable();
            foreach (var item in p.SqlWhere)
            {
                Iquery = Iquery.Where(item);
            }
            var Count = Iquery.Count();
            var SumAmoney = Iquery.Sum(o => (decimal?)o.Amoney);
            var SumPaymoney = Iquery.Sum(o => (double?)o.UserRate);
            this.ViewBag.SumAmoney = SumAmoney;
            this.ViewBag.SumPaymoney = SumPaymoney;
            this.ViewBag.Count = Count;
            return this.View();
        }

        public ActionResult Edit(OrderCash OrderCash)
        {
            ViewBag.BasicDescList = GetBasicDescList(BasicCodeEnum.Txtk);
            if (OrderCash.Id != 0) OrderCash = Entity.OrderCash.FirstOrDefault(n => n.Id == OrderCash.Id);
            if (OrderCash == null)
            {
                ViewBag.ErrorMsg = "数据不存在";
                return View("Error");
            }
            ViewBag.OrderCash = OrderCash;
            Orders Orders = Entity.Orders.FirstOrDefault(n => n.TNum == OrderCash.OId);
            if (Orders.PayState != 1)
            {
                ViewBag.ErrorMsg = "当前状态，不能付款！";
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
        public void Save(OrderCash OrderCash)
        {
            OrderCash baseOrderCash = Entity.OrderCash.FirstOrDefault(n => n.Id == OrderCash.Id);
            Orders Orders = Entity.Orders.FirstOrDefault(n => n.TNum == baseOrderCash.OId);
            var OrderCashLog = new OrderCashLog()
            {
                AddTime = DateTime.Now,
                AdminId = this.AdminUser.Id,
                AdminName = this.AdminUser.TrueName,
                TNum = baseOrderCash.OId,
                Remark = string.Empty,
                LogType = 1,
            };
            this.Entity.OrderCashLog.AddObject(OrderCashLog);
            Entity.SaveChanges();
            if (BasicSet.CashPayWay == 1)//开启自动结算时执行
            {
                //自动出款
                baseOrderCash.PayCash(Orders, Entity);//去付款
            }
            else
            {
                if (baseOrderCash.OrderState == 2 && baseOrderCash.PayState == 1 && baseOrderCash.FState == 0)
                {
                    baseOrderCash.PayState = 2;
                    baseOrderCash.FState = 1;
                    baseOrderCash.FTime = DateTime.Now;
                    Orders.PayState = 2;
                    Entity.SaveChanges();
                    //======分润======
                    baseOrderCash = baseOrderCash.PayAgent(Entity, 1);
                    Orders.AgentPayGet = (decimal)baseOrderCash.AgentCashGet;
                    Entity.SaveChanges();
                }
            }
            Orders.SendMsg(Entity);//发送消息类
            BaseRedirect();
        }
        public ActionResult Cancel(OrderCash OrderCash)
        {
            ViewBag.BasicDescList = GetBasicDescList(BasicCodeEnum.Txtk);
            if (OrderCash.Id != 0) OrderCash = Entity.OrderCash.FirstOrDefault(n => n.Id == OrderCash.Id);
            ViewBag.OrderCash = OrderCash;
            Orders Orders = Entity.Orders.FirstOrDefault(n => n.TNum == OrderCash.OId);
            if (Orders==null)
            {
                ViewBag.ErrorMsg = "订单不存在！";
                return View("Error");
            }
            if (Orders.PayState != 1 && Orders.PayState != 2)
            {
                ViewBag.ErrorMsg = "当前状态不能申请退款！";
                return View("Error");
            }
            if (Orders.TState != 2)
            {
                ViewBag.ErrorMsg = "交易不成功，不能退款！";
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
            return View("Edit");
        }
        [ValidateInput(false)]
        public void CancelSave(OrderCash OrderCash)
        {
            OrderCash.Remark = OrderCash.Remark.IsNullOrEmpty() ? string.Empty : OrderCash.Remark;
            OrderCash baseOrderCash = Entity.OrderCash.FirstOrDefault(n => n.Id == OrderCash.Id);
            if (baseOrderCash.OrderState == 2 && ((baseOrderCash.PayState == 1 && baseOrderCash.FState == 0) || (baseOrderCash.PayState == 2 && baseOrderCash.FState == 1)))
            {
                baseOrderCash.PayState = 3;
                baseOrderCash.Remark = OrderCash.Remark;
                Orders Orders = Entity.Orders.FirstOrDefault(n => n.TNum == baseOrderCash.OId);
                Orders.PayState = 3;
                Orders.Remark = OrderCash.Remark;

                var OrderCashLog = new OrderCashLog()
                {
                    AddTime = DateTime.Now,
                    AdminId = this.AdminUser.Id,
                    AdminName = this.AdminUser.TrueName,
                    TNum = baseOrderCash.OId,
                    Remark = baseOrderCash.Remark,
                    LogType = 2,
                };
                this.Entity.OrderCashLog.AddObject(OrderCashLog);

                Entity.SaveChanges();
                Orders.SendMsg(Entity);//发送消息类
            }
            BaseRedirect();
        }
        public void ChangeStatus(OrderCash OrderCash, string InfoList, string Clomn, string Value)
        {
            //Clomn	FState
            //InfoList	
            //206,205,192,191,190,189,188,187,186,47,39
            //value	1
            if (Clomn == "FState" && Value == "1")
            {
                string[] Arr = InfoList.Split(',');
                if (Arr.Length > 0)
                {
                    List<int> ArrInt = new List<int>();
                    foreach (string p in Arr)
                    {
                        ArrInt.Add(Int32.Parse(p));
                    }
                    IList<OrderCash> List = Entity.OrderCash.Where(n => ArrInt.Contains(n.Id)).ToList();
                    foreach (var p in List)
                    {
                        Orders Orders = Entity.Orders.FirstOrDefault(n => n.TNum == p.OId);
                        if (BasicSet.CashPayWay == 1)//开启自动结算时执行
                        {
                            //自动出款
                            p.PayCash(Orders, Entity);//去付款
                            Thread.Sleep(1200);
                        }
                        else
                        {
                            p.PayState = 2;
                            p.FState = 1;
                            p.FTime = DateTime.Now;
                            //======分润======
                            OrderCash OC = p.PayAgent(Entity, 1);
                            Orders.AgentPayGet = (decimal)OC.AgentCashGet;
                            Orders.PayState = 2;
                        }
                        Orders.SendMsg(Entity);//发送消息类
                    }
                    Entity.SaveChanges();
                    Response.Write(List.Count);
                }
            }
            //if (string.IsNullOrEmpty(InfoList)) { InfoList = OrderCash.Id.ToString(); }
            //int Ret = Entity.ChangeEntity<OrderCash>(InfoList, Clomn, Value);
            //Entity.SaveChanges();
            //Response.Write(Ret);
        }
        public ActionResult Xls()
        {
            return View();
        }
        public void XLSDo(OrderCash OrderCash, EFPagingInfo<OrderCash> p, bool? IsShowSupAgent)
        {
            if (IsShowSupAgent == null)
            {
                IsShowSupAgent = false;
            }
            p = this.Condition(OrderCash, p, IsShowSupAgent);
            p.PageSize = 99999999;
            p.OrderByList.Add("Id", "ASC");
            IPageOfItems<OrderCash> OrderCashList = Entity.Selects<OrderCash>(p);

            if (OrderCashList.Count() > 0)
            {
                //当前银行名称
                //string thisBankName = Entity.BasicBank.FirstOrNew(n => n.Id == OrderCash.Id).Name;
                //当前导出缩合表，民生跨行，工行，民生同行
                int BankIdMS = 13;//民生ID
                int BankIdGS = 1;//工商Id
                string BankNameMS = Entity.BasicBank.FirstOrNew(n => n.Id == BankIdMS).Name;
                string BankNameGS = Entity.BasicBank.FirstOrNew(n => n.Id == BankIdGS).Name;
                //加载对应银行模板
                string tempname = "bank.xlsx";
                string file = Server.MapPath("/template") + "\\" + tempname;
                ExcelPackage package = new ExcelPackage(new FileInfo(file), true);
                //民生
                IList<OrderCash> ListMS = OrderCashList.Where(n => n.Bank == BankNameMS).ToList();
                //工商
                IList<OrderCash> ListGS = OrderCashList.Where(n => n.Bank == BankNameGS).ToList();
                //其它
                IList<OrderCash> ListOT = OrderCashList.Where(n => n.Bank != BankNameMS && n.Bank != BankNameGS).ToList();
                //设置数据开始行
                int Befor = 0;
                int maxCol = 0;//最大列数
                if (ListOT.Count > 0)
                {
                    //加载模板第一张表
                    var sheet = package.Workbook.Worksheets[1];
                    var cells = sheet.Cells;
                    decimal TotalMoney = 0;//计算总金额
                    int Rows = ListOT.Count();
                    Befor = 5;//民生他行从5开始
                    sheet.InsertRow(Befor + 1, Rows - 1, Befor);
                    int i = Befor;
                    //sheet.Row(i - 1).Height = 18;//设置行高
                    foreach (var item in ListOT)
                    {
                        string zdlx = "2";
                        //sheet.Row(i).Height = 18;//设置行高
                        decimal Money = item.Amoney - (decimal)item.UserRate;
                        TotalMoney += Money;
                        //制单类型
                        cells["A" + i].Value = zdlx.ToString();
                        //企业自制凭证号
                        cells["B" + i].Value = item.Id.ToString().PadLeft(8, '0');
                        //客户号
                        cells["C" + i].Value = "2200488356";
                        //预约标志
                        cells["D" + i].Value = "0";
                        //付款账号
                        cells["E" + i].Value = "613111800";
                        //交易金额
                        cells["F" + i].Value = Money;
                        //收款账号
                        cells["G" + i].Value = item.CardNum;
                        //收款人姓名
                        cells["H" + i].Value = item.Owner;
                        //收款账户类型
                        cells["I" + i].Value = "1";
                        //子客户号
                        cells["J" + i].Value = "";
                        //子付款账号
                        cells["K" + i].Value = "";
                        //子付款账户名
                        cells["L" + i].Value = "";
                        //子付款账户开户行名
                        cells["M" + i].Value = "";
                        //用途
                        cells["N" + i].Value = "提现";
                        //汇路
                        cells["O" + i].Value = Money > 50000 ? "7" : "6";
                        //是否通知收款人
                        cells["P" + i].Value = "0";
                        //手机
                        cells["Q" + i].Value = item.Mobile;
                        //邮箱
                        cells["R" + i].Value = "";
                        //支付行号&支付行名称
                        cells["S" + i].Value = item.Bin + "&" + item.Deposit;
                        maxCol = 19;
                        i++;
                    }
                    i--;
                    //审核方式（文件类型）
                    cells["B1"].Value = "1";
                    //总金额
                    cells["B2"].Value = TotalMoney.ToString("F2");
                    //总交易数
                    cells["B3"].Value = Rows.ToString();
                    sheet.Cells[Befor, 6, i, 6].Style.Numberformat.Format = "\"¥\"#,##0.00_);[Red](\"¥\"#,##0.00)";
                    //cells["B" + (i + 2)].Value = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                    //sheet.Cells[Befor, 1, i, maxCol].Style.Border.Left.Style = ExcelBorderStyle.Thin;
                    //sheet.Cells[Befor, 1, i, maxCol].Style.Border.Right.Style = ExcelBorderStyle.Thin;
                    //sheet.Cells[Befor, 1, i, maxCol].Style.Border.Top.Style = ExcelBorderStyle.Thin;
                    //sheet.Cells[Befor, 1, i, maxCol].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                    //sheet.Cells[Befor, 1, i, maxCol].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;//居中
                    //sheet.Cells[Befor, 5, i, 5].Style.Numberformat.Format = "yyyy-mm-dd HH:mm:ss";
                    //for (int j = i + 1; j <= i + 2; j++)
                    //{
                    //    sheet.Row(j).Height = 20;//设置行高
                    //}
                }
                if (ListMS.Count > 0)
                {
                    //加载模板第二张表
                    var sheet = package.Workbook.Worksheets[2];
                    var cells = sheet.Cells;
                    decimal TotalMoney = 0;//计算总金额
                    int Rows = ListMS.Count();
                    Befor = 9;//民生本行从9开始
                    sheet.InsertRow(Befor + 1, Rows - 1, Befor);
                    int i = Befor;
                    foreach (var item in ListMS)
                    {
                        decimal Money = item.Amoney - (decimal)item.UserRate;
                        TotalMoney += Money;
                        //收款账号
                        cells["A" + i].Value = item.CardNum;
                        //交易金额
                        cells["B" + i].Value = Money.ToString("F2");
                        //收款人姓名
                        cells["C" + i].Value = item.Owner;
                        //用途
                        cells["D" + i].Value = "349";
                        maxCol = 4;
                        i++;
                    }
                    i--;
                    //总金额
                    cells["B6"].Value = TotalMoney.ToString();
                    //总交易数
                    cells["B7"].Value = Rows.ToString();
                }
                if (ListGS.Count > 0)
                {
                    //加载模板第三张表
                    var sheet = package.Workbook.Worksheets[3];
                    var cells = sheet.Cells;
                    decimal TotalMoney = 0;//计算总金额
                    int Rows = ListGS.Count();
                    Befor = 2;//工行本行从2开始
                    sheet.InsertRow(Befor + 1, Rows - 1, Befor);
                    int i = Befor;
                    IList<BasicProvince> PList = Entity.BasicProvince.Where(n => n.State == 1).ToList();
                    IList<BasicCity> CList = Entity.BasicCity.Where(n => n.State == 1).ToList();
                    foreach (var item in ListGS)
                    {
                        decimal Money = item.Amoney - (decimal)item.UserRate;
                        TotalMoney += Money;
                        //币种
                        cells["A" + i].Value = "RMB";
                        //日期
                        cells["B" + i].Value = DateTime.Now.ToString("yyyyMMdd");
                        //明细标志
                        cells["C" + i].Value = "";
                        //顺序号
                        cells["D" + i].Value = i - Befor + 1;
                        //付款账号开户行
                        cells["E" + i].Value = "工商银行";
                        //付款账号
                        cells["F" + i].Value = "4000051709100125887";
                        //付款账号名称
                        cells["G" + i].Value = "好付支付（深圳）有限公司";
                        //收款账号开户行
                        cells["H" + i].Value = "工商银行";
                        //收款账号省份
                        cells["I" + i].Value = PList.FirstOrNew(n => n.Id == item.Province).Name;
                        //收款账号地市
                        cells["J" + i].Value = CList.FirstOrNew(n => n.Id == item.City).Name;
                        //收款账号地区码
                        cells["K" + i].Value = "";
                        //收款账号
                        cells["L" + i].Value = item.CardNum;
                        //收款账号名称
                        cells["M" + i].Value = item.Owner;
                        //金额
                        cells["N" + i].Value = Money.ToString("F2");
                        //汇款用途
                        cells["O" + i].Value = "提现";
                        //备注信息
                        cells["P" + i].Value = "";
                        //汇款方式
                        cells["Q" + i].Value = 1;
                        //收款账户短信通知手机号码
                        cells["R" + i].Value = "";
                        //自定义序号
                        cells["S" + i].Value = item.Id;
                        maxCol = 19;
                        i++;
                    }
                    i--;
                }
                //无数据表删除
                if (ListGS.Count == 0)
                {
                    package.Workbook.Worksheets.Delete(package.Workbook.Worksheets[3]);
                }
                if (ListMS.Count == 0)
                {
                    package.Workbook.Worksheets.Delete(package.Workbook.Worksheets[2]);
                }
                if (ListOT.Count == 0)
                {
                    package.Workbook.Worksheets.Delete(package.Workbook.Worksheets[1]);
                }
                Response.BinaryWrite(package.GetAsByteArray());//输出
                Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                Response.AddHeader("content-disposition", "attachment;  filename=" + DateTime.Now.ToString("yyyyMMddHHmmss") + new Random().Next(10, 99) + ".xlsx");
            }
            else
            {
                Response.Write("暂无符合条件数据");
            }
        }

        public void PayCash(OrderCash OrderCash, EFPagingInfo<OrderCash> p, bool? IsShowSupAgent)
        {
            if (IsShowSupAgent == null)
            {
                IsShowSupAgent = false;
            }
            p.PageSize = 99999999;
            p.SqlWhere.Add(f => f.OrderState == 2);
            p.SqlWhere.Add(f => f.PayState == 1 || f.PayState == 2);

            if (OrderCash.AddTime.IsNullOrEmpty())
            {
                OrderCash.AddTime = DateTime.Parse(DateTime.Now.ToString("yyyy-MM-dd"));
            }
            if (OrderCash.FTime.IsNullOrEmpty())
            {
                OrderCash.FTime = DateTime.Now;
            }
            if (!OrderCash.Owner.IsNullOrEmpty())
            {
                IList<Users> UList = Entity.Users.Where(n => n.TrueName.Contains(OrderCash.Owner) || n.NeekName.Contains(OrderCash.Owner) || n.UserName == OrderCash.Owner).ToList();
                List<int> UIds = new List<int>();
                foreach (var pp in UList)
                {
                    UIds.Add(pp.Id);
                }
                p.SqlWhere.Add(f => UIds.Contains(f.UId));
            }
            if (!OrderCash.Cash.IsNullOrEmpty())
            {
                p.SqlWhere.Add(f => f.Amoney >= OrderCash.Cash);
            }
            if (!OrderCash.ECash.IsNullOrEmpty())
            {
                p.SqlWhere.Add(f => f.Amoney <= OrderCash.ECash);
            }
            if (!OrderCash.OId.IsNullOrEmpty()) { p.SqlWhere.Add(f => f.OId == OrderCash.OId); }
            if (!OrderCash.Agent.IsNullOrEmpty())
            {
                //是否要显示下级
                if ((bool)IsShowSupAgent)
                {
                    SysAgent LowerLevelAgent = Entity.SysAgent.Where(s => s.Id == OrderCash.Agent).FirstOrNew();
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
                    p.SqlWhere.Add(f => f.Agent == OrderCash.Agent);
                }
            }
            if (!OrderCash.FState.IsNullOrEmpty()) { p.SqlWhere.Add(f => f.FState == (OrderCash.FState == 99 ? 0 : OrderCash.FState)); }
            if (!OrderCash.TrunType.IsNullOrEmpty()) { p.SqlWhere.Add(f => f.TrunType == (OrderCash.TrunType == 99 ? 0 : OrderCash.TrunType)); }
            if (!OrderCash.AId.IsNullOrEmpty()) { p.SqlWhere.Add(f => f.AId == OrderCash.AId); }
            if (!OrderCash.FId.IsNullOrEmpty()) { p.SqlWhere.Add(f => f.FId == OrderCash.FId); }
            if (!OrderCash.AgentState.IsNullOrEmpty()) { p.SqlWhere.Add(f => f.AgentState == OrderCash.AgentState); }
            if (!OrderCash.AddTime.IsNullOrEmpty() && !OrderCash.FTime.IsNullOrEmpty())
            {
               // DateTime FTime = ((DateTime)OrderCash.FTime).AddDays(1);
                DateTime FTime = OrderCash.FTime.Value;
                p.SqlWhere.Add(f => f.AddTime > OrderCash.AddTime && f.AddTime < FTime);
            }
            p.OrderByList.Add("Id", "ASC");
            IPageOfItems<OrderCash> OrderCashList = Entity.Selects<OrderCash>(p);

            TaskCash TaskCash = new TaskCash();
            TaskCash.State = 0;
            TaskCash.AddTime = DateTime.Now;
            TaskCash.Total = OrderCashList.Count();
            TaskCash.Success = 0;
            TaskCash.Fail = 0;
            Entity.TaskCash.AddObject(TaskCash);
            Entity.SaveChanges();
            int suc = 0;
            int fa = 0;
            int same = 0;
            foreach (var O in OrderCashList)
            {
                if (O.PayState == 1 && O.OrderState == 2)
                {
                    TaskCashInfo TaskCashInfo = Entity.TaskCashInfo.FirstOrDefault(n => n.OId == O.OId);
                    if (TaskCashInfo == null)
                    {
                        TaskCashInfo = new TaskCashInfo();
                        TaskCashInfo.OId = O.OId;
                        TaskCashInfo.TId = TaskCash.Id;
                        TaskCashInfo.State = 0;
                        TaskCashInfo.OState = 0;
                        TaskCashInfo.NState = 0;
                        TaskCashInfo.AddTime = DateTime.Now;
                        Entity.TaskCashInfo.AddObject(TaskCashInfo);
                        suc++;
                    }
                    else
                    {
                        same++;
                    }
                }
                else
                {
                    fa++;
                }
            }
            TaskCash.Total = suc;
            Entity.SaveChanges();
            System.Web.HttpContext.Current.Response.AddHeader("content-type", "application/json");
            Response.Write("{\"id\":" + TaskCash.Id + ",\"same\":" + same + ",\"success\":" + suc + ",\"fail\":" + fa + "}");
        }

        public void PayCashDo(string List)
        {
            string[] Arr = List.Split(',');
            List<int> ArrInt = new List<int>();
            if (Arr.Length > 0)
            {
                foreach (string p in Arr)
                {
                    ArrInt.Add(Int32.Parse(p));
                }
            }
            else
            {
                Response.Write("{\"id\":0\"same\":0,\"success\":0,\"fail\":0}");
                return;
            }
            IList<OrderCash> OrderCashList = Entity.OrderCash.Where(n => ArrInt.Contains(n.Id)).ToList();

            TaskCash TaskCash = new TaskCash();
            TaskCash.State = 0;
            TaskCash.AddTime = DateTime.Now;
            TaskCash.Total = OrderCashList.Count();
            TaskCash.Success = 0;
            TaskCash.Fail = 0;
            Entity.TaskCash.AddObject(TaskCash);
            Entity.SaveChanges();
            int suc = 0;
            int fa = 0;
            int same = 0;
            foreach (var O in OrderCashList)
            {
                if (O.PayState == 1 && O.OrderState == 2)
                {
                    TaskCashInfo TaskCashInfo = Entity.TaskCashInfo.FirstOrDefault(n => n.OId == O.OId);
                    if (TaskCashInfo == null)
                    {
                        TaskCashInfo = new TaskCashInfo();
                        TaskCashInfo.OId = O.OId;
                        TaskCashInfo.TId = TaskCash.Id;
                        TaskCashInfo.State = 0;
                        TaskCashInfo.OState = 0;
                        TaskCashInfo.NState = 0;
                        TaskCashInfo.AddTime = DateTime.Now;
                        Entity.TaskCashInfo.AddObject(TaskCashInfo);
                        suc++;
                    }
                    else
                    {
                        same++;
                    }
                }
                else
                {
                    fa++;
                }
            }
            TaskCash.Total = suc;
            Entity.SaveChanges();
            System.Web.HttpContext.Current.Response.AddHeader("content-type", "application/json");
            Response.Write("{\"id\":" + TaskCash.Id + ",\"same\":" + same + ",\"success\":" + suc + ",\"fail\":" + fa + "}");
        }

        public void PayCashSetTask(int Id)
        {
            TaskCash TaskCash = Entity.TaskCash.FirstOrNew(n => n.Id == Id);
            TaskCash.State = 1;
            Entity.SaveChanges();
            Entity.ExecuteStoreCommand("Update TaskCashInfo Set State=1 Where TId=" + Id + " and State=0");//同时变成待执行
        }

        public void PayCashDelTask(int Id)
        {
            Entity.ExecuteStoreCommand("Delete TaskCash Where Id=" + Id);
            Entity.ExecuteStoreCommand("Delete TaskCashInfo Where TId=" + Id);
        }

        public ActionResult InfoTask1()
        {
            DateTime SDate = DateTime.Parse(DateTime.Now.ToString("yyyy-MM-dd")).AddDays(-30);
            IList<TaskCash> TaskCashList = Entity.TaskCash.Where(n => n.State > 0 && n.AddTime > SDate).OrderByDescending(n => n.Id).ToList();
            ViewBag.TaskCashList = TaskCashList;
            return View();
        }

        public ActionResult InfoTask2(int Id)
        {
            TaskCash TaskCash = Entity.TaskCash.FirstOrNew(n => n.Id == Id);
            IList<TaskCashInfo> TaskCashInfoList = Entity.TaskCashInfo.Where(n => n.TId == Id).OrderBy(n => n.Id).ToList();

            int state1 = TaskCashInfoList.Count(n => n.State == 1);
            int state2 = TaskCashInfoList.Count(n => n.State == 2);
            int state3 = TaskCashInfoList.Count(n => n.State == 3);
            int state4 = TaskCashInfoList.Count(n => n.State == 4);
            int state5 = TaskCashInfoList.Count(n => n.State == 5);

            if (state1 == 0 && state2 == 0 && TaskCashInfoList.Count() > 0)
            {
                TaskCash.State = 3;
                TaskCash.ETime = TaskCashInfoList.Last().ETime;
            }
            TaskCash.Success = state3 + state5;
            TaskCash.Fail = state4;

            Entity.SaveChanges();

            ViewBag.TaskCash = TaskCash;
            ViewBag.TaskCashInfoList = TaskCashInfoList;
            return View();
        }

        /// <summary>
        /// 导出善付单
        /// </summary>
        /// <param name="OrderCash"></param>
        /// <param name="p"></param>
        /// <param name="IsShowSupAgent"></param>
        public void XLSDoShanFu(OrderCash OrderCash, EFPagingInfo<OrderCash> p, bool? IsShowSupAgent)
        {
            if (IsShowSupAgent == null)
            {
                IsShowSupAgent = false;
            }
            p = this.Condition(OrderCash, p, IsShowSupAgent);
            var Iquery = this.Entity.OrderCash.AsQueryable();
            foreach(var item in p.SqlWhere)
            {
                Iquery = Iquery.Where(item);
            }
            
            var OrderCashList = Iquery.Join(this.Entity.Users, o => o.UId, x => x.Id, (o, x) => new 
            {
                Id = o.Id,
                CardNum = o.CardNum,
                CardId = x.CardId,
                Owner = o.Owner,
                Mobile = o.Mobile,
                Amoney = o.Amoney,
                UserRate = o.UserRate,
            }).OrderByDescending(o=>o.Id).ToList();

            //加载对应银行模板
            string tempname = "zx_demo.xlsx";
            string file = Server.MapPath("/template") + "\\" + tempname;
            ExcelPackage package = new ExcelPackage(new FileInfo(file), true);

            if (OrderCashList.Count() > 0)
            {
                decimal adjust = 0.3M;//金额调节
                //加载模板第二张表
                var sheet = package.Workbook.Worksheets[1];
                var cells = sheet.Cells;
                int Rows = OrderCashList.Count();
                int Befor = 2;
                int i = Befor;
                sheet.InsertRow(Befor + 1, Rows - 1, Befor);
                foreach (var item in OrderCashList)
                {
                    decimal Money = (item.Amoney - (decimal)item.UserRate) + adjust;
                    //TotalMoney += Money;
                    //银行卡号
                    cells["A" + i].Value = item.CardNum;
                    //身份证号
                    cells["B" + i].Value = item.CardId;
                    //真实姓名
                    cells["C" + i].Value = item.Owner;
                    //银行卡绑定的手机号
                    cells["D" + i].Value = item.Mobile;
                    //银行卡背面的cvn2
                    cells["E" + i].Value = string.Empty;
                    //银行卡有效期
                    cells["F" + i].Value = string.Empty;
                    //交易金额
                    cells["G" + i].Value = Money.ToString("f2");
                    i++;
                }
                i--;
                
                Response.BinaryWrite(package.GetAsByteArray());//输出
                Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                Response.AddHeader("content-disposition", "attachment;  filename=" + OrderCash.AddTime.ToString("yyyyMMddHHmmss") + "至" + OrderCash.FTime.Value.ToString("yyyyMMddHHmmss") + ".xlsx");
            }
            else
            {
                Response.Write("暂无符合条件数据");
            }
        }

        /// <summary>
        /// 条件
        /// </summary>
        private EFPagingInfo<OrderCash> Condition(OrderCash OrderCash, EFPagingInfo<OrderCash> p, bool? IsShowSupAgent)
        {
            p.SqlWhere.Add(f => f.OrderState == 2);
            p.SqlWhere.Add(f => f.PayState == 1 || f.PayState == 2);
            //if (OrderCash.AddTime.IsNullOrEmpty())
            //{
            //    OrderCash.AddTime = DateTime.Parse(DateTime.Now.ToString("yyyy-MM-dd"));
            //}
            //if (OrderCash.FTime.IsNullOrEmpty())
            //{
            //    //OrderCash.FTime = DateTime.Parse(DateTime.Now.ToString("yyyy-MM-dd"));
            //    OrderCash.FTime = DateTime.Now;
            //}
            //if (!OrderCash.UId.IsNullOrEmpty()) { p.SqlWhere.Add(f => f.UId == OrderCash.UId); }
            if (!OrderCash.Owner.IsNullOrEmpty())
            {
                IList<Users> UList = Entity.Users.Where(n => n.TrueName == OrderCash.Owner || n.NeekName == OrderCash.Owner || n.UserName == OrderCash.Owner).ToList();
                List<int> UIds = new List<int>();
                foreach (var pp in UList)
                {
                    UIds.Add(pp.Id);
                }
                p.SqlWhere.Add(f => UIds.Contains(f.UId));
            }
            if (!OrderCash.Cash.IsNullOrEmpty())
            {
                p.SqlWhere.Add(f => f.Amoney >= OrderCash.Cash);
            }
            if (!OrderCash.ECash.IsNullOrEmpty())
            {
                p.SqlWhere.Add(f => f.Amoney <= OrderCash.ECash);
            }
            if (!OrderCash.OId.IsNullOrEmpty()) { p.SqlWhere.Add(f => f.OId == OrderCash.OId); }
            if (!OrderCash.Agent.IsNullOrEmpty())
            {
                //是否要显示下级
                if ((bool)IsShowSupAgent)
                {
                    SysAgent LowerLevelAgent = Entity.SysAgent.Where(s => s.Id == OrderCash.Agent).FirstOrNew();
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
                    p.SqlWhere.Add(f => f.Agent == OrderCash.Agent);
                }
            }
            if (!OrderCash.FState.IsNullOrEmpty()) { p.SqlWhere.Add(f => f.FState == (OrderCash.FState == 99 ? 0 : OrderCash.FState)); }
            if (!OrderCash.TrunType.IsNullOrEmpty()) { p.SqlWhere.Add(f => f.TrunType == (OrderCash.TrunType == 99 ? 0 : OrderCash.TrunType)); }
            if (!OrderCash.AId.IsNullOrEmpty()) { p.SqlWhere.Add(f => f.AId == OrderCash.AId); }
            if (!OrderCash.FId.IsNullOrEmpty()) { p.SqlWhere.Add(f => f.FId == OrderCash.FId); }
            if (!OrderCash.AgentState.IsNullOrEmpty()) { p.SqlWhere.Add(f => f.AgentState == OrderCash.AgentState); }
            if (!OrderCash.AddTime.IsNullOrEmpty() && !OrderCash.FTime.IsNullOrEmpty())
            {
                // DateTime FTime = ((DateTime)OrderCash.FTime).AddDays(1);
                DateTime FTime = OrderCash.FTime.Value;
                p.SqlWhere.Add(f => f.AddTime > OrderCash.AddTime && f.AddTime < FTime);
            }
            return p;
        }

        public ActionResult IndexOrderCashLog(string TNum)
        {
            var OrderCashLogList = this.Entity.OrderCashLog.Where(o => o.TNum == TNum).ToList();
            this.ViewBag.OrderCashLogList = OrderCashLogList;
            return View();
        }

        public ActionResult Info(OrderCash OrderCash)
        {
            ViewBag.BasicDescList = GetBasicDescList(BasicCodeEnum.Txtk);
            if (OrderCash.Id != 0) OrderCash = Entity.OrderCash.FirstOrDefault(n => n.Id == OrderCash.Id);
            ViewBag.OrderCash = OrderCash;
            Orders Orders = Entity.Orders.FirstOrDefault(n => n.TNum == OrderCash.OId);
            if (Orders==null)
            {
                ViewBag.ErrorMsg = "订单不存在！";
                return View("Error");
            }
            ViewBag.Orders = Orders;
            ViewBag.Users = Entity.Users.FirstOrNew(n => n.Id == Orders.UId);
            ViewBag.SysAgent = Entity.SysAgent.FirstOrNew(n => n.Id == Orders.Agent);
            ViewBag.SysAdmin = Entity.SysAdmin.FirstOrNew(n => n.Id == Orders.AId);
            ViewBag.FinAdmin = Entity.SysAdmin.FirstOrNew(n => n.Id == Orders.FId);
            return View("Edit");
        }
    }
}
