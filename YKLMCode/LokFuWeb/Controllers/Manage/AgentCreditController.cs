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
    public class AgentCreditController : BaseController
    {
      
        public ActionResult Index(ApplyCredit ApplyCredit, EFPagingInfo<ApplyCredit> p, bool? IsShowSupAgent, int IsFirst = 0)
        {

            if (ApplyCredit.STime.IsNullOrEmpty())
            {
                ApplyCredit.STime = DateTime.Parse(DateTime.Now.ToString("yyyy-MM-dd"));
            }
            if (ApplyCredit.ETime.IsNullOrEmpty())
            {
               // ApplyCredit.ETime = DateTime.Parse(DateTime.Now.ToString("yyyy-MM-dd"));
                 ApplyCredit.ETime = DateTime.Now;
            }
            if (IsShowSupAgent == null)
            {
                IsShowSupAgent = false;
            }
            if (!ApplyCredit.TrueName.IsNullOrEmpty()) { p.SqlWhere.Add(f => f.TrueName == ApplyCredit.TrueName); }
            if (!ApplyCredit.BankId.IsNullOrEmpty()) { p.SqlWhere.Add(f => f.BankId == ApplyCredit.BankId); }
            if (!ApplyCredit.Education.IsNullOrEmpty()) { p.SqlWhere.Add(f => f.Education.Contains(ApplyCredit.Education)); }
            if (!ApplyCredit.HasSheBao.IsNullOrEmpty()) { p.SqlWhere.Add(f => f.HasSheBao == (ApplyCredit.HasSheBao == 99 ? 0 : ApplyCredit.HasSheBao)); }
            if (!ApplyCredit.Marry.IsNullOrEmpty()) { p.SqlWhere.Add(f => f.Marry == ApplyCredit.Marry); }
            if (!ApplyCredit.HasCar.IsNullOrEmpty()) { p.SqlWhere.Add(f => f.HasCar == (ApplyCredit.HasCar == 99 ? 0 : ApplyCredit.HasCar)); }
            if (!ApplyCredit.House.IsNullOrEmpty()) { p.SqlWhere.Add(f => f.House == ApplyCredit.House); }
            if (!ApplyCredit.HasCredit.IsNullOrEmpty()) { p.SqlWhere.Add(f => f.HasCredit == ApplyCredit.HasCredit); }
            if (!ApplyCredit.State.IsNullOrEmpty()) { p.SqlWhere.Add(f => f.State == ApplyCredit.State); }
            if (!ApplyCredit.AgentPay.IsNullOrEmpty()) { p.SqlWhere.Add(f => f.AgentPay == (ApplyCredit.AgentPay == 99 ? 0 : ApplyCredit.AgentPay)); }
            if (!ApplyCredit.AgentAId.IsNullOrEmpty())
            {
                //是否要显示下级
                if ((bool)IsShowSupAgent)
                {
                    SysAgent LowerLevelAgent = Entity.SysAgent.Where(s => s.Id == ApplyCredit.AgentAId).FirstOrNew();
                    IList<SysAgent> SysAgentList = LowerLevelAgent.GetSupAgent(Entity);
                    IList<int> UID = new List<int>();
                    foreach (var s in SysAgentList)
                    {
                        UID.Add(s.Id);
                    }
                    p.SqlWhere.Add(f => UID.Contains(f.AgentAId));
                }
                else
                {
                    p.SqlWhere.Add(f => f.AgentAId == ApplyCredit.AgentAId);
                }
            }
            if (!ApplyCredit.STime.IsNullOrEmpty() && !ApplyCredit.ETime.IsNullOrEmpty())
            {
                DateTime ETime = ApplyCredit.ETime;
                p.SqlWhere.Add(f => f.PayTime > ApplyCredit.STime && f.PayTime < ETime);
            }
            p.SqlWhere.Add(f => f.PayState == 1);
            p.PageSize = 99999999;
            p.OrderByList.Add("Id", "DESC");
            IPageOfItems<ApplyCredit> ApplyCreditList = null;
            if (IsFirst == 0)
            {
               ApplyCreditList = new PageOfItems<ApplyCredit>(new List<ApplyCredit>(), 0, 10, 0, new Hashtable());
            }
            else
            {
                ApplyCreditList = Entity.Selects<ApplyCredit>(p);
            }
            ViewBag.ApplyCreditList = ApplyCreditList;
            ViewBag.ApplyCredit = ApplyCredit;
            ViewBag.BasicBankList = Entity.BasicBank.Where(n => n.State == 1).ToList();
            IList<ApplyCredit> List = ApplyCreditList.GroupBy(n => n.AgentId).Select(n => n.First()).ToList();
            List<int> AgentId = new List<int>();
            foreach (var pp in List)
            {
                AgentId.Add(pp.AgentId);
            }
            ViewBag.SysAgentList = Entity.SysAgent.Where(n => AgentId.Contains(n.Id) && n.Tier == 1).ToList();
            ViewBag.SelectSysAgentList = Entity.SysAgent.Where(n => n.Tier == 1).ToList();
            ViewBag.IsShowSupAgent = IsShowSupAgent;
            ViewBag.Xls = this.checkPower("Xls");
            return View();
        }
        public ActionResult Edit(ApplyCredit ApplyCredit)
        {
            if (ApplyCredit.Id != 0) ApplyCredit = Entity.ApplyCredit.FirstOrDefault(n => n.Id == ApplyCredit.Id);
            if (ApplyCredit.PayState != 1)
            {
                ViewBag.ErrorMsg = "未支付，不能结算！";
                return View("Error");
            }
            if (ApplyCredit.AgentPay != 0)
            {
                ViewBag.ErrorMsg = "已经结算，不能重复结算！";
                return View("Error");
            }
            if (ApplyCredit == null)
            {
                ViewBag.ErrorMsg = "数据不存在";
                return View("Error");
            }
            ViewBag.ApplyCredit = ApplyCredit;
            if (Request.UrlReferrer != null)
            {
                Session["Url"] = Request.UrlReferrer.ToString();
            }
            return View();
        }
        [ValidateInput(false)]
        public void Save(ApplyCredit ApplyCredit)
        {
            ApplyCredit baseApplyCredit = Entity.ApplyCredit.FirstOrDefault(n => n.Id == ApplyCredit.Id);
            baseApplyCredit.AgentPay = 1;
            Entity.SaveChanges();
            BaseRedirect();
        }
        public void XLSDo(ApplyCredit ApplyCredit, EFPagingInfo<ApplyCredit> p)
        {
            if (ApplyCredit.STime.IsNullOrEmpty())
            {
                ApplyCredit.STime = DateTime.Parse(DateTime.Now.ToString("yyyy-MM-dd"));
            }
            if (ApplyCredit.ETime.IsNullOrEmpty())
            {
                //ApplyCredit.ETime = DateTime.Parse(DateTime.Now.ToString("yyyy-MM-dd"));
                ApplyCredit.ETime = DateTime.Now;
            }
            if (!ApplyCredit.TrueName.IsNullOrEmpty()) { p.SqlWhere.Add(f => f.TrueName == ApplyCredit.TrueName); }
            if (!ApplyCredit.BankId.IsNullOrEmpty()) { p.SqlWhere.Add(f => f.BankId == ApplyCredit.BankId); }
            if (!ApplyCredit.Education.IsNullOrEmpty()) { p.SqlWhere.Add(f => f.Education.Contains(ApplyCredit.Education)); }
            if (!ApplyCredit.HasSheBao.IsNullOrEmpty()) { p.SqlWhere.Add(f => f.HasSheBao == (ApplyCredit.HasSheBao == 99 ? 0 : ApplyCredit.HasSheBao)); }
            if (!ApplyCredit.Marry.IsNullOrEmpty()) { p.SqlWhere.Add(f => f.Marry == ApplyCredit.Marry); }
            if (!ApplyCredit.HasCar.IsNullOrEmpty()) { p.SqlWhere.Add(f => f.HasCar == (ApplyCredit.HasCar == 99 ? 0 : ApplyCredit.HasCar)); }
            if (!ApplyCredit.House.IsNullOrEmpty()) { p.SqlWhere.Add(f => f.House == ApplyCredit.House); }
            if (!ApplyCredit.HasCredit.IsNullOrEmpty()) { p.SqlWhere.Add(f => f.HasCredit == ApplyCredit.HasCredit); }
            if (!ApplyCredit.State.IsNullOrEmpty()) { p.SqlWhere.Add(f => f.State == ApplyCredit.State); }
            if (!ApplyCredit.AgentPay.IsNullOrEmpty()) { p.SqlWhere.Add(f => f.AgentPay == (ApplyCredit.AgentPay == 99 ? 0 : ApplyCredit.AgentPay)); }
            if (!ApplyCredit.AgentAId.IsNullOrEmpty()) { p.SqlWhere.Add(f => f.AgentAId == ApplyCredit.AgentAId); }
            if (!ApplyCredit.STime.IsNullOrEmpty() && !ApplyCredit.ETime.IsNullOrEmpty())
            {
                DateTime ETime = ApplyCredit.ETime;
                p.SqlWhere.Add(f => f.PayTime > ApplyCredit.STime && f.PayTime < ETime);
            }
            p.SqlWhere.Add(f => f.PayState == 1);
            p.PageSize = 99999999;
            p.OrderByList.Add("Id", "DESC");
            IPageOfItems<ApplyCredit> ApplyCreditList = Entity.Selects<ApplyCredit>(p);
            IList<BasicBank> BasicBankList = Entity.BasicBank.Where(n => n.State == 1).ToList();
            IList<ApplyCredit> List = ApplyCreditList.GroupBy(n => n.AgentId).Select(n => n.First()).ToList();
            List<int> AgentId = new List<int>();
            foreach (var pp in List)
            {
                AgentId.Add(pp.AgentId);
            }
            IList<SysAgent> SysAgentList = Entity.SysAgent.Where(n => AgentId.Contains(n.Id) && n.Tier == 1).ToList();
            if (SysAgentList.Count > 0)
            {
                //加载对应银行模板
                string tempname = "AgentCredit.xlsx";
                string file = Server.MapPath("/template") + "\\" + tempname;
                ExcelPackage package = new ExcelPackage(new FileInfo(file), true);
                //设置数据开始行
                int Befor = 2;
                int maxCol = 7;//最大列数
                //加载模板第一张表
                var sheet = package.Workbook.Worksheets[1];
                var cells = sheet.Cells;
                decimal TotalMoney = 0;//计算总金额
                decimal TotalPrice = 0;//计算总金额
                int Rows = ApplyCreditList.Count() + SysAgentList.Count();
                sheet.InsertRow(Befor + 1, Rows - 1, Befor);
                int i = Befor;
                foreach (var item in SysAgentList)
                {
                    IList<ApplyCredit> ListSub = ApplyCreditList.Where(n => n.AgentId == item.Id).ToList();
                    decimal AgentMoney = 0;//计算供应商金额
                    decimal AgentPrice = 0;//计算供应商金额
                    foreach (var s in ListSub)
                    {
                        sheet.Row(i).Height = 20;//设置行高
                        BasicBank BasicBank = BasicBankList.FirstOrNew(n => n.Id == s.BankId);
                        //分支机构
                        cells["A" + i].Value = item.Name;
                        //编号
                        cells["B" + i].Value = s.Id;
                        //姓名
                        cells["C" + i].Value = s.TrueName;
                        //申请银行
                        cells["D" + i].Value = BasicBank.Name;
                        //售出金额
                        cells["E" + i].Value = s.Amoney;
                        //售出时间
                        cells["F" + i].Value = s.PayTime;
                        //结算金额
                        cells["G" + i].Value = s.AgentMoney;
                        AgentMoney += s.Amoney;
                        AgentPrice += s.AgentMoney;
                        i++;
                    }
                    sheet.Row(i).Height = 28;//设置行高
                    //交易金额汇总
                    cells["E" + i].Value = AgentMoney;
                    //结算金额汇总
                    cells["G" + i].Value = AgentPrice;
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
                cells["E" + i].Value = TotalMoney;
                //结算金额汇总
                cells["G" + i].Value = TotalPrice;
                sheet.Cells[Befor, 1, i, maxCol].Style.Border.Left.Style = ExcelBorderStyle.Thin;
                sheet.Cells[Befor, 1, i, maxCol].Style.Border.Right.Style = ExcelBorderStyle.Thin;
                sheet.Cells[Befor, 1, i, maxCol].Style.Border.Top.Style = ExcelBorderStyle.Thin;
                sheet.Cells[Befor, 1, i, maxCol].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                sheet.Cells[Befor, 5, i, 5].Style.Numberformat.Format = "\"¥\"#,##0.00_);[Red](\"¥\"#,##0.00)";
                sheet.Cells[Befor, 6, i, 6].Style.Numberformat.Format = "yyyy-mm-dd hh:mm";
                sheet.Cells[Befor, 7, i, 7].Style.Numberformat.Format = "\"¥\"#,##0.00_);[Red](\"¥\"#,##0.00)";
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
