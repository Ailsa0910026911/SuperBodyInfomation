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
    public class FinHaoDaiController : BaseController
    {

        public ActionResult Index(Orders Orders, bool? IsShowSupAgent, int IsFirst = 0)
        {
            if (IsShowSupAgent == null) IsShowSupAgent = false;
            if (Orders.STime.IsNullOrEmpty())
            {
                Orders.STime = DateTime.Parse(DateTime.Now.ToString("yyyy-MM-dd"));
            }
            if (Orders.ETime.IsNullOrEmpty())
            {
               // Orders.ETime = DateTime.Parse(DateTime.Now.ToString("yyyy-MM-dd"));
                Orders.ETime = DateTime.Now;
            }
            EFPagingInfo<ApplyCredit> cp = new EFPagingInfo<ApplyCredit>();
            EFPagingInfo<ApplyLoan> lp = new EFPagingInfo<ApplyLoan>();
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
                    cp.SqlWhere.Add(f => UID.Contains(f.AgentId));
                    lp.SqlWhere.Add(f => UID.Contains(f.AgentId));
                }
                else
                {
                    cp.SqlWhere.Add(f => f.AgentId == Orders.Agent);
                    lp.SqlWhere.Add(f => f.AgentId == Orders.Agent);
                }
            }
            if (!Orders.STime.IsNullOrEmpty() && !Orders.ETime.IsNullOrEmpty())
            {
                DateTime ETime = Orders.ETime;
                cp.SqlWhere.Add(f => f.PayTime > Orders.STime && f.PayTime < ETime);
                lp.SqlWhere.Add(f => f.PayTime > Orders.STime && f.PayTime < ETime);
            }
            cp.SqlWhere.Add(f => f.PayState == 1);
            lp.SqlWhere.Add(f => f.PayState == 1);
            cp.PageSize = 99999999;
            lp.PageSize = 99999999;
            cp.OrderByList.Add("Id", "DESC");
            lp.OrderByList.Add("Id", "DESC");
            IPageOfItems<ApplyCredit> ApplyCreditList = null;
            IPageOfItems<ApplyLoan> ApplyLoanList = null;
            if (IsFirst == 0)
            {
                ApplyLoanList = new PageOfItems<ApplyLoan>(new List<ApplyLoan>(), 0, 10, 0, new Hashtable());
                ApplyCreditList = new PageOfItems<ApplyCredit>(new List<ApplyCredit>(), 0, 10, 0, new Hashtable());
            }
            else
            {
                if (Orders.TType.IsNullOrEmpty())
                {
                    ApplyCreditList = Entity.Selects<ApplyCredit>(cp);
                    ApplyLoanList = Entity.Selects<ApplyLoan>(lp);
                }
                else if (Orders.TType == 1)
                {
                    ApplyCreditList = Entity.Selects<ApplyCredit>(cp);
                    ApplyLoanList = new PageOfItems<ApplyLoan>(new List<ApplyLoan>(), 0, 10, 0, new Hashtable());
                }
                else if (Orders.TType == 2)
                {
                    ApplyLoanList = Entity.Selects<ApplyLoan>(lp);
                    ApplyCreditList = new PageOfItems<ApplyCredit>(new List<ApplyCredit>(), 0, 10, 0, new Hashtable());
                }
                else
                {
                    ApplyLoanList = new PageOfItems<ApplyLoan>(new List<ApplyLoan>(), 0, 10, 0, new Hashtable());
                    ApplyCreditList = new PageOfItems<ApplyCredit>(new List<ApplyCredit>(), 0, 10, 0, new Hashtable());
                }
            }
            ViewBag.ApplyLoanList = ApplyLoanList;
            ViewBag.ApplyCreditList = ApplyCreditList;
            ViewBag.Orders = Orders;
            ViewBag.SysAgentList = Entity.SysAgent.Where(n => n.Tier == 1).ToList();
            ViewBag.IsShowSupAgent = IsShowSupAgent;
            ViewBag.Xls = this.checkPower("Xls");
            return View();
        }
        public void XLSDo(Orders Orders)
        {
            if (Orders.STime.IsNullOrEmpty())
            {
                Orders.STime = DateTime.Parse(DateTime.Now.ToString("yyyy-MM-dd"));
            }
            if (Orders.ETime.IsNullOrEmpty())
            {
                //Orders.ETime = DateTime.Parse(DateTime.Now.ToString("yyyy-MM-dd"));
                 Orders.ETime = DateTime.Now;
            }
            EFPagingInfo<ApplyCredit> cp = new EFPagingInfo<ApplyCredit>();
            EFPagingInfo<ApplyLoan> lp = new EFPagingInfo<ApplyLoan>();
            if (!Orders.Agent.IsNullOrEmpty())
            {
                cp.SqlWhere.Add(f => f.AgentId == Orders.Agent);
                lp.SqlWhere.Add(f => f.AgentId == Orders.Agent);
            }
            if (!Orders.STime.IsNullOrEmpty() && !Orders.ETime.IsNullOrEmpty())
            {
                DateTime ETime = Orders.ETime;
                cp.SqlWhere.Add(f => f.PayTime > Orders.STime && f.PayTime < ETime);
                lp.SqlWhere.Add(f => f.PayTime > Orders.STime && f.PayTime < ETime);
            }
            cp.SqlWhere.Add(f => f.PayState == 1);
            lp.SqlWhere.Add(f => f.PayState == 1);
            cp.PageSize = 99999999;
            lp.PageSize = 99999999;
            cp.OrderByList.Add("Id", "DESC");
            lp.OrderByList.Add("Id", "DESC");
            IPageOfItems<ApplyCredit> ApplyCreditList = null;
            IPageOfItems<ApplyLoan> ApplyLoanList = null;
            if (Orders.TType.IsNullOrEmpty())
            {
                ApplyCreditList = Entity.Selects<ApplyCredit>(cp);
                ApplyLoanList = Entity.Selects<ApplyLoan>(lp);
            }
            else if (Orders.TType == 1)
            {
                ApplyCreditList = Entity.Selects<ApplyCredit>(cp);
            }
            else if (Orders.TType == 2)
            {
                ApplyLoanList = Entity.Selects<ApplyLoan>(lp);
            }
            if (ApplyCreditList.Count() > 0 || ApplyLoanList.Count() > 0)
            {
                string file = Server.MapPath("/template") + "\\finhaodai.xlsx";
                ExcelPackage package = new ExcelPackage(new FileInfo(file), true);
                var sheet = package.Workbook.Worksheets[1];
                var cells = sheet.Cells;
                int Rows = ApplyCreditList.Count() + ApplyLoanList.Count();
                //设置数据开始行
                int Befor = 2;
                sheet.InsertRow(Befor + 1, Rows - 1, Befor);
                int i = Befor;
                sheet.Row(i - 1).Height = 22;//设置行高
                int maxCol = 0;
                //获取机构及管理员
                IList<SysAgent> SysAgentList = Entity.SysAgent.Where(n => n.State == 1).ToList();
                foreach (var item in ApplyCreditList)
                {
                    string AName = "--";
                    if (item.AId > 0)
                    {
                        SysAgent SA = SysAgentList.FirstOrNew(n => n.Id == item.AgentId);
                        if (!SA.Name.IsNullOrEmpty())
                        {
                            AName = SA.Name;
                        }
                    }
                    sheet.Row(i).Height = 22;//设置行高
                    //信息编号1
                    cells["A" + i].Value = "X" + item.Id.ToString().PadLeft(8, '0');
                    //分支机构2
                    cells["B" + i].Value = AName;
                    //信息类型3
                    cells["C" + i].Value = "信用卡申请";
                    //申请时间4
                    cells["D" + i].Value = item.AddTime;
                    //售出金额5
                    cells["E" + i].Value = item.Amoney;
                    //售出时间6
                    cells["F" + i].Value = item.PayTime;
                    //利润7
                    cells["G" + i].Value = item.Amoney - item.AgentMoney;
                    //结算金额8
                    cells["H" + i].Value = item.AgentMoney;
                    i++;
                }
                foreach (var item in ApplyLoanList)
                {
                    string AName = "--";
                    if (item.AId > 0)
                    {
                        SysAgent SA = SysAgentList.FirstOrNew(n => n.Id == item.AgentId);
                        if (!SA.Name.IsNullOrEmpty())
                        {
                            AName = SA.Name;
                        }
                    }
                    sheet.Row(i).Height = 22;//设置行高
                    //信息编号1
                    cells["A" + i].Value = "D" + item.Id.ToString().PadLeft(8, '0');
                    //分支机构2
                    cells["B" + i].Value = AName;
                    //信息类型3
                    cells["C" + i].Value = "贷款申请";
                    //申请时间4
                    cells["D" + i].Value = item.AddTime;
                    //售出金额5
                    cells["E" + i].Value = item.Amoney;
                    //售出时间6
                    cells["F" + i].Value = item.PayTime;
                    //利润7
                    cells["G" + i].Value = item.Amoney - item.AgentMoney;
                    //结算金额8
                    cells["H" + i].Value = item.AgentMoney;
                    i++;
                }
                sheet.Row(i).Height = 40;//设置行高
                //交易总额5
                cells["E" + i].Formula = "SUM(E" + Befor + ":E" + (i - 1) + ")";
                //利润7
                cells["G" + i].Formula = "SUM(G" + Befor + ":G" + (i - 1) + ")";
                //结算金额8
                cells["H" + i].Formula = "SUM(H" + Befor + ":H" + (i - 1) + ")";
                i--;
                maxCol = 8;
                //cells["B" + (i + 2)].Value = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                sheet.Cells[Befor, 1, i, maxCol].Style.Border.Left.Style = ExcelBorderStyle.Thin;
                sheet.Cells[Befor, 1, i, maxCol].Style.Border.Right.Style = ExcelBorderStyle.Thin;
                sheet.Cells[Befor, 1, i, maxCol].Style.Border.Top.Style = ExcelBorderStyle.Thin;
                sheet.Cells[Befor, 1, i, maxCol].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                sheet.Cells[Befor, 1, i, maxCol].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;//居中
                sheet.Cells[Befor, 4, i, 4].Style.Numberformat.Format = "yyyy-mm-dd HH:mm:ss";
                sheet.Cells[Befor, 6, i, 6].Style.Numberformat.Format = "yyyy-mm-dd HH:mm:ss";
                //sheet.Cells[Befor, 9, i, 9].Style.Numberformat.Format = "#0.00%";
                //sheet.Cells[Befor, 11, i, 11].Style.Numberformat.Format = "#0.00%";
                i++;
                sheet.Cells[Befor, 5, i, 5].Style.Numberformat.Format = "\"¥\"#,##0.00_);[Red](\"¥\"#,##0.00)";
                sheet.Cells[Befor, 7, i, 7].Style.Numberformat.Format = "\"¥\"#,##0.00_);[Red](\"¥\"#,##0.00)";
                sheet.Cells[Befor, 8, i, 8].Style.Numberformat.Format = "\"¥\"#,##0.00_);[Red](\"¥\"#,##0.00)";
                Response.BinaryWrite(package.GetAsByteArray());
                //输出
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
