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
using System.Drawing;
using System.IO;
using System.Linq;
using System.Web.Mvc;
namespace LokFu.Areas.Manage.Controllers
{
    public class OrdersPayController : BaseController
    {

        public ActionResult Index(Orders Orders, EFPagingInfo<Orders> p, bool? IsShowSupAgent, int IsFirst = 0)
        {
            this.TempData.Remove("SysAgentList");
            #region 校验与初始化
            if (IsShowSupAgent == null)
            {
                IsShowSupAgent = false;
            }

            if (Orders.STime.IsNullOrEmpty())
            {
                Orders.STime = DateTime.Parse(DateTime.Now.ToString("yyyy-MM-dd"));
            }
            if (Orders.ETime.IsNullOrEmpty())
            {
                //Orders.ETime = DateTime.Parse(DateTime.Now.ToString("yyyy-MM-dd"));
                Orders.ETime = DateTime.Now;
            }
            p.SqlWhere.Add(f => f.PayState == 1);
            p.SqlWhere.Add(f => f.TState == 2);
            if (!Orders.TNum.IsNullOrEmpty()) { p.SqlWhere.Add(f => f.TNum == Orders.TNum); }
            if (!Orders.TName.IsNullOrEmpty())
            {
                IList<Users> UList = Entity.Users.Where(n => n.TrueName == Orders.TName || n.NeekName == Orders.TName || n.UserName == Orders.TName).ToList();
                List<int> UIds = new List<int>();
                foreach (var pp in UList)
                {
                    UIds.Add(pp.Id);
                }
                p.SqlWhere.Add(f => UIds.Contains(f.UId));
            }
            if (!Orders.TType.IsNullOrEmpty()) { p.SqlWhere.Add(f => f.TType == Orders.TType); }
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
            if (!Orders.AgentState.IsNullOrEmpty()) { p.SqlWhere.Add(f => f.AgentState == (Orders.AgentState == 99 ? 0 : Orders.AgentState)); }
            if (!Orders.STime.IsNullOrEmpty() && !Orders.ETime.IsNullOrEmpty())
            {
                DateTime ETime = Orders.ETime;
                p.SqlWhere.Add(f => f.PayTime > Orders.STime && f.PayTime < ETime);
            }
            #endregion
            p.PageSize = 99999999;
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
            ViewBag.UsersList = Entity.Users.Where(n => n.State == 1 && UId.Contains(n.Id)).ToList();
            ViewBag.PayConfigList = Entity.PayConfig.Where(n => n.State == 1).ToList();
            IList<Orders> List1 = OrdersList.GroupBy(n => n.Agent).Select(n => n.First()).ToList();
            List<int> AgentId = new List<int>();
            foreach (var pp in List1)
            {
                AgentId.Add(pp.Agent);
            }
            ViewBag.SysAgentList = Entity.SysAgent.Where(n => AgentId.Contains(n.Id) && n.Tier == 1).ToList();
            this.TempData["SysAgentList"] = ViewBag.SysAgentList;
            ViewBag.SelectSysAgentList = Entity.SysAgent.Where(n => n.State == 1).ToList();
            ViewBag.IsShowSupAgent = IsShowSupAgent;
            ViewBag.Xls = this.checkPower("Xls");
            return View();
        }
        public ActionResult Edit(Orders Orders)
        {
            if (Orders.Id != 0) Orders = Entity.Orders.FirstOrDefault(n => n.Id == Orders.Id);
            if (Orders == null)
            {
                ViewBag.ErrorMsg = "数据不存在";
                return View("Error");
            }
            if (Orders.PayState != 1)
            {
                ViewBag.ErrorMsg = "交易未支付，不能结算！";
                return View("Error");
            }
            if (Orders.TState != 2)
            {
                ViewBag.ErrorMsg = "交易未成功，不能结算！";
                return View("Error");
            }
            ViewBag.Orders = Orders;
            ViewBag.Users = Entity.Users.FirstOrNew(n => n.Id == Orders.UId);
            ViewBag.SysAgent = Entity.SysAgent.FirstOrNew(n => n.Id == Orders.Agent);
            ViewBag.SysAdmin = Entity.SysAdmin.FirstOrNew(n => n.Id == Orders.AId);
            ViewBag.FinAdmin = Entity.SysAdmin.FirstOrNew(n => n.Id == Orders.FId);
            ViewBag.UserTrailIndex = this.checkPower("UserTrail", "Index");
            if (Request.UrlReferrer != null)
            {
                Session["Url"] = Request.UrlReferrer.ToString();
            }
            return View("../Orders/Edit");
        }
        [ValidateInput(false)]
        public void Save(Orders Orders)
        {
            Response.Write("系统已是秒结，无需操作~");
        }
        public void XLSDo(Orders Orders, EFPagingInfo<Orders> p)
        {
            if (Orders.STime.IsNullOrEmpty())
            {
                Orders.STime = DateTime.Parse(DateTime.Now.ToString("yyyy-MM-dd"));
            }
            if (Orders.ETime.IsNullOrEmpty())
            {
                Orders.ETime = DateTime.Parse(DateTime.Now.ToString("yyyy-MM-dd"));
            }
            p.SqlWhere.Add(f => f.PayState == 1);
            p.SqlWhere.Add(f => f.TState == 2);
            if (!Orders.TNum.IsNullOrEmpty()) { p.SqlWhere.Add(f => f.TNum == Orders.TNum); }
            //if (!Orders.UId.IsNullOrEmpty()) { p.SqlWhere.Add(f => f.UId == Orders.UId); }
            if (!Orders.TName.IsNullOrEmpty())
            {
                IList<Users> UList = Entity.Users.Where(n => n.TrueName.Contains(Orders.TName) || n.NeekName.Contains(Orders.TName) || n.UserName == Orders.TName).ToList();
                List<int> UIds = new List<int>();
                foreach (var pp in UList)
                {
                    UIds.Add(pp.Id);
                }
                p.SqlWhere.Add(f => UIds.Contains(f.UId));
            }
            if (!Orders.TType.IsNullOrEmpty()) { p.SqlWhere.Add(f => f.TType == Orders.TType); }
            if (!Orders.Agent.IsNullOrEmpty()) { p.SqlWhere.Add(f => f.Agent == Orders.Agent); }
            if (!Orders.AgentState.IsNullOrEmpty()) { p.SqlWhere.Add(f => f.AgentState == (Orders.AgentState == 99 ? 0 : Orders.AgentState)); }
            if (!Orders.STime.IsNullOrEmpty() && !Orders.ETime.IsNullOrEmpty())
            {
                DateTime ETime = Orders.ETime;
                p.SqlWhere.Add(f => f.PayTime > Orders.STime && f.PayTime < ETime);
            }
            p.PageSize = 99999999;
            p.OrderByList.Add("Id", "DESC");
            IPageOfItems<Orders> OrdersList = Entity.Selects<Orders>(p);
            IList<Orders> List = OrdersList.GroupBy(n => n.UId).Select(n => n.First()).ToList();
            List<int> UId = new List<int>();
            foreach (var pp in List)
            {
                UId.Add(pp.UId);
            }
            IList<Users> UsersList = Entity.Users.Where(n => n.State == 1 && UId.Contains(n.Id)).ToList();
            IList<Orders> List1 = OrdersList.GroupBy(n => n.Agent).Select(n => n.First()).ToList();
            List<int> AgentId = new List<int>();
            foreach (var pp in List1)
            {
                AgentId.Add(pp.Agent);
            }
            if (this.TempData["SysAgentList"] == null)
            {
                Response.Write("当前页面数据已过期");
            }
            this.TempData.Keep("SysAgentList");
            IList<SysAgent> SysAgentList = this.TempData["SysAgentList"] as IList<SysAgent>;//Entity.SysAgent.Where(n => n.State == 1 && AgentId.Contains(n.Id)).ToList();

            if (SysAgentList != null && SysAgentList.Count > 0)
            {
                //加载对应模板
                string tempname = "order.xlsx";
                string file = Server.MapPath("/template") + "\\" + tempname;
                ExcelPackage package = new ExcelPackage(new FileInfo(file), true);
                //设置数据开始行
                int Befor = 2;
                int maxCol = 8;//最大列数
                //加载模板第一张表
                var sheet = package.Workbook.Worksheets[1];
                var cells = sheet.Cells;
                decimal TotalMoney = 0;//计算总金额
                double TotalPrice = 0;//计算总金额
                int Rows = OrdersList.Count() + SysAgentList.Count();
                sheet.InsertRow(Befor + 1, Rows - 1, Befor);
                int i = Befor;
                foreach (var item in SysAgentList)
                {
                    IList<Orders> ListSub = OrdersList.Where(n => n.Agent == item.Id).ToList();
                    decimal AgentMoney = 0;//计算供应商金额
                    double AgentPrice = 0;//计算供应商金额
                    foreach (var s in ListSub)
                    {
                        sheet.Row(i).Height = 20;//设置行高
                        Users U = UsersList.FirstOrNew(n => n.Id == s.UId);
                        //分支机构
                        cells["A" + i].Value = item.Name;
                        //交易号
                        cells["B" + i].Value = s.TNum;
                        //交易商户
                        cells["C" + i].Value = U.TrueName;
                        //交易类型
                        cells["D" + i].Value = s.GetTTName();
                        //交易内容
                        cells["E" + i].Value = s.GetPayNameWithTName();
                        //交易金额
                        cells["F" + i].Value = s.Amoney;
                        //交易时间
                        cells["G" + i].Value = s.PayTime;
                        //结算金额
                        double agentmoney = s.GetAgentMoney(Entity);
                        cells["H" + i].Value = agentmoney;
                        AgentMoney += s.Amoney;
                        AgentPrice += agentmoney;
                        i++;
                    }
                    sheet.Row(i).Height = 28;//设置行高
                    //交易金额汇总
                    cells["F" + i].Value = AgentMoney;
                    //结算金额汇总
                    cells["H" + i].Value = AgentPrice;
                    Color bgColor = ColorTranslator.FromHtml("#DDDDDD");
                    Color fColor = ColorTranslator.FromHtml("#FF0000");
                    sheet.Cells[i, 1, i, maxCol].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    sheet.Cells[i, 1, i, maxCol].Style.Fill.BackgroundColor.SetColor(bgColor);
                    sheet.Cells[i, 1, i, maxCol].Style.Font.Size = 16;
                    sheet.Cells[i, 1, i, maxCol].Style.Font.Bold = true;
                    sheet.Cells[i, 1, i, maxCol].Style.Font.Color.SetColor(fColor);
                    TotalMoney += AgentMoney;
                    TotalPrice += AgentPrice;
                    i++;
                }
                Color bgColor_ = ColorTranslator.FromHtml("#7030A0");
                Color fColor_ = ColorTranslator.FromHtml("#FFFFFF");
                sheet.Cells[i, 1, i, maxCol].Style.Fill.PatternType = ExcelFillStyle.Solid;
                sheet.Cells[i, 1, i, maxCol].Style.Fill.BackgroundColor.SetColor(bgColor_);
                sheet.Cells[i, 1, i, maxCol].Style.Font.Size = 20;
                sheet.Cells[i, 1, i, maxCol].Style.Font.Bold = true;
                sheet.Cells[i, 1, i, maxCol].Style.Font.Color.SetColor(fColor_);
                sheet.Row(i).Height = 40;//设置行高
                //交易金额汇总
                cells["F" + i].Value = TotalMoney;
                //结算金额汇总
                cells["H" + i].Value = TotalPrice;
                sheet.Cells[Befor, 1, i, maxCol].Style.Border.Left.Style = ExcelBorderStyle.Thin;
                sheet.Cells[Befor, 1, i, maxCol].Style.Border.Right.Style = ExcelBorderStyle.Thin;
                sheet.Cells[Befor, 1, i, maxCol].Style.Border.Top.Style = ExcelBorderStyle.Thin;
                sheet.Cells[Befor, 1, i, maxCol].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                sheet.Cells[Befor, 6, i, 6].Style.Numberformat.Format = "\"¥\"#,##0.00_);[Red](\"¥\"#,##0.00)";
                sheet.Cells[Befor, 7, i, 7].Style.Numberformat.Format = "yyyy-mm-dd hh:mm";
                sheet.Cells[Befor, 8, i, 8].Style.Numberformat.Format = "\"¥\"#,##0.00_);[Red](\"¥\"#,##0.00)";
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
