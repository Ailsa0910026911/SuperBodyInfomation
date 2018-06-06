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
    public class OrderHouseController : BaseController
    {

        public ActionResult Index(OrderHouse OrderHouse, EFPagingInfo<OrderHouse> p, bool? IsShowSupAgent, int IsFirst = 0)
        {
            if (IsShowSupAgent == null) IsShowSupAgent = false;
            if (Request.QueryString["PageSize"].IsNullOrEmpty())
            {
                p.PageSize = 30;
            }
            p.SqlWhere.Add(f => f.OrderState == 2);
            p.SqlWhere.Add(f => f.PayState == 1 || f.PayState == 2);
            //if (OrderHouse.AddTime.IsNullOrEmpty())
            //{
            //    OrderHouse.AddTime = DateTime.Parse(DateTime.Now.ToString("yyyy-MM-dd"));
            //}
            //if (OrderHouse.FTime.IsNullOrEmpty())
            //{
            //    OrderHouse.FTime = DateTime.Now;
            //}
            if (IsFirst == 0)
            {
                PageOfItems<OrderHouse> OrderHouseList1 = new PageOfItems<OrderHouse>(new List<OrderHouse>(), 0, 10, 0, new Hashtable());
                ViewBag.OrderHouseList = OrderHouseList1;
                ViewBag.OrderHouse = OrderHouse;
                ViewBag.UsersList = new List<Users>();
                ViewBag.SysAgentList = Entity.SysAgent.Where(n => n.State == 1 && n.Tier == 1).ToList();
                ViewBag.IsShowSupAgent = IsShowSupAgent;
                ViewBag.Edit = this.checkPower("Edit");
                ViewBag.Cancel = this.checkPower("Cancel");
                ViewBag.Save = this.checkPower("Save");
                return View();
            }
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
            if (!OrderHouse.FState.IsNullOrEmpty()) { p.SqlWhere.Add(f => f.FState == (OrderHouse.FState == 99 ? 0 : OrderHouse.FState)); }
            if (!OrderHouse.TrunType.IsNullOrEmpty()) { p.SqlWhere.Add(f => f.TrunType == (OrderHouse.TrunType == 99 ? 0 : OrderHouse.TrunType)); }
            if (!OrderHouse.AId.IsNullOrEmpty()) { p.SqlWhere.Add(f => f.AId == OrderHouse.AId); }
            if (!OrderHouse.FId.IsNullOrEmpty()) { p.SqlWhere.Add(f => f.FId == OrderHouse.FId); }
            if (!OrderHouse.AgentState.IsNullOrEmpty()) { p.SqlWhere.Add(f => f.AgentState == OrderHouse.AgentState); }
            if (!OrderHouse.AddTime.IsNullOrEmpty() && !OrderHouse.FTime.IsNullOrEmpty())
            {
               // DateTime FTime = ((DateTime)OrderHouse.FTime).AddDays(1);
                DateTime FTime = OrderHouse.FTime.Value;
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
            ViewBag.Edit = this.checkPower("Edit");
            ViewBag.Cancel = this.checkPower("Cancel");
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
        public void Save(OrderHouse OrderHouse)
        {
            OrderHouse baseOrderHouse = Entity.OrderHouse.FirstOrDefault(n => n.Id == OrderHouse.Id);
            Orders Orders = Entity.Orders.FirstOrDefault(n => n.TNum == baseOrderHouse.OId);
            if (BasicSet.CashPayWay == 1)//开启自动结算时执行
            {
                //自动出款
                baseOrderHouse.PayCash(Orders, Entity);//去付款
            }
            else
            {
                if (baseOrderHouse.OrderState == 2 && baseOrderHouse.PayState == 1 && baseOrderHouse.FState == 0)
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
                }
            }
            Orders.SendMsg(Entity);//发送消息类
            BaseRedirect();
        }
        public ActionResult Cancel(OrderHouse OrderHouse)
        {
            ViewBag.BasicDescList = GetBasicDescList(BasicCodeEnum.Fztk);
            if (OrderHouse.Id != 0) OrderHouse = Entity.OrderHouse.FirstOrDefault(n => n.Id == OrderHouse.Id);
            if (OrderHouse == null)
            {
                ViewBag.ErrorMsg = "数据不存在";
                return View("Error");
            }
            ViewBag.OrderHouse = OrderHouse;
            Orders Orders = Entity.Orders.FirstOrDefault(n => n.TNum == OrderHouse.OId);
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
            return View();
        }
        [ValidateInput(false)]
        public void CancelSave(OrderHouse OrderHouse)
        {
            OrderHouse baseOrderHouse = Entity.OrderHouse.FirstOrDefault(n => n.Id == OrderHouse.Id);
            if (baseOrderHouse.OrderState == 2 && ((baseOrderHouse.PayState == 1 && baseOrderHouse.FState == 0) || (baseOrderHouse.PayState == 2 && baseOrderHouse.FState == 1)))
            {
                baseOrderHouse.PayState = 3;
                baseOrderHouse.Remark = OrderHouse.Remark;
                Orders Orders = Entity.Orders.FirstOrDefault(n => n.TNum == baseOrderHouse.OId);
                Orders.PayState = 3;
                Orders.Remark = OrderHouse.Remark;
                Entity.SaveChanges();
                Orders.SendMsg(Entity);//发送消息类
            }
            BaseRedirect();
        }
        public void ChangeStatus(OrderHouse OrderHouse, string InfoList, string Clomn, string Value)
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
                    IList<OrderHouse> List = Entity.OrderHouse.Where(n => ArrInt.Contains(n.Id)).ToList();
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
                            OrderHouse OH = p.PayAgent(Entity, 1);
                            Orders.AgentPayGet = (decimal)OH.AgentPayGet;
                            Entity.SaveChanges();
                            Orders.PayState = 2;
                        }
                        Orders.SendMsg(Entity);//发送消息类
                    }
                    Entity.SaveChanges();
                    Response.Write(List.Count);
                }
            }
        }
        public ActionResult Xls()
        {
            return View();
        }
        public void XLSDo(OrderHouse OrderHouse, EFPagingInfo<OrderHouse> p)
        {
            if (OrderHouse.AddTime.IsNullOrEmpty())
            {
                OrderHouse.AddTime = DateTime.Parse(DateTime.Now.ToString("yyyy-MM-dd"));
            }
            if (OrderHouse.FTime.IsNullOrEmpty())
            {
                //OrderHouse.FTime = DateTime.Parse(DateTime.Now.ToString("yyyy-MM-dd"));
                OrderHouse.FTime = DateTime.Now;
            }
            if (!OrderHouse.AddTime.IsNullOrEmpty() && !OrderHouse.FTime.IsNullOrEmpty())
            {
               // OrderHouse.FTime = ((DateTime)OrderHouse.FTime).AddDays(1);
                p.SqlWhere.Add(f => f.AddTime > OrderHouse.AddTime && f.AddTime < OrderHouse.FTime);
            }
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
            if (!OrderHouse.Agent.IsNullOrEmpty()) { p.SqlWhere.Add(f => f.Agent == OrderHouse.Agent); }
            if (!OrderHouse.TrunType.IsNullOrEmpty()) { p.SqlWhere.Add(f => f.TrunType == (OrderHouse.TrunType == 99 ? 0 : OrderHouse.TrunType)); }
            if (!OrderHouse.AId.IsNullOrEmpty()) { p.SqlWhere.Add(f => f.AId == OrderHouse.AId); }
            if (!OrderHouse.FId.IsNullOrEmpty()) { p.SqlWhere.Add(f => f.FId == OrderHouse.FId); }
            if (!OrderHouse.AgentState.IsNullOrEmpty()) { p.SqlWhere.Add(f => f.AgentState == OrderHouse.AgentState); }
            p.SqlWhere.Add(f => f.OrderState == 2 && f.PayState == 1 && f.FState == 0);
            p.PageSize = 99999999;
            p.OrderByList.Add("Id", "DESC");
            IPageOfItems<OrderHouse> OrderHouseList = Entity.Selects<OrderHouse>(p);
            if (OrderHouseList.Count() > 0)
            {
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
                IList<OrderHouse> ListMS = OrderHouseList.Where(n => n.Bank == BankNameMS).ToList();
                //工商
                IList<OrderHouse> ListGS = OrderHouseList.Where(n => n.Bank == BankNameGS).ToList();
                //其它
                IList<OrderHouse> ListOT = OrderHouseList.Where(n => n.Bank != BankNameMS && n.Bank != BankNameGS).ToList();
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
                    foreach (var item in ListOT)
                    {
                        string zdlx = "2";
                        decimal Money = item.PayMoney;
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
                        cells["H" + i].Value = item.HouseOwner;
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
                        cells["N" + i].Value = "房租";
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
                        decimal Money = item.PayMoney;
                        TotalMoney += Money;
                        //收款账号
                        cells["A" + i].Value = item.CardNum;
                        //交易金额
                        cells["B" + i].Value = Money.ToString("F2");
                        //收款人姓名
                        cells["C" + i].Value = item.HouseOwner;
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
                        decimal Money = item.PayMoney;
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
                        cells["M" + i].Value = item.HouseOwner;
                        //金额
                        cells["N" + i].Value = Money.ToString("F2");
                        //汇款用途
                        cells["O" + i].Value = "房租";
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
    }
}
