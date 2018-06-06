using LokFu.Extensions;
using LokFu.Infrastructure;
using LokFu.Models;
using LokFu.Repositories;
using LokFu.Repositories.SqlServer;
using System;
using System.Linq;
using System.Web.Mvc;
using OfficeOpenXml;
using System.IO;
using System.Drawing;
using OfficeOpenXml.Style;
namespace LokFu.Areas.Manage.Controllers
{
    public class UserLogController : BaseController
    {
        public ActionResult Index(UserLog UserLog, EFPagingInfo<UserLog> p, DateTime? STime, DateTime? ETime)
       {
           if (!STime.HasValue)
           {
               STime = new DateTime(DateTime.Now.AddMonths(-1).Year, DateTime.Now.AddMonths(-1).Month, DateTime.Now.AddMonths(-1).Day);
              
           }
           if (!ETime.HasValue)
           {
              ETime = new DateTime(DateTime.Now.Year,DateTime.Now.Month,DateTime.Now.Day,DateTime.Now.Hour,DateTime.Now.Minute,DateTime.Now.Second,999);
            
           }
            p.SqlWhere.Add(f => f.UId==UserLog.UId);
            if(!UserLog.OId.IsNullOrEmpty()){p.SqlWhere.Add(f => f.OId == UserLog.OId);}
            if (STime.HasValue)
            {
                p.SqlWhere.Add(f => f.AddTime >= STime);
            }
            if (ETime.HasValue)
            {
                if (ETime < new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day))
                {
                    ETime = new DateTime(ETime.Value.Year, ETime.Value.Month, ETime.Value.Day, 23, 59, 59, 999);

                } 
                p.SqlWhere.Add(f => f.AddTime <= ETime);
            }

            ViewBag.Xls = this.checkPower("Xls");
            p.OrderByList.Add("Id", "DESC");
            IPageOfItems<UserLog> UserLogList = Entity.Selects<UserLog>(p);
            ViewBag.UserLogList = UserLogList;
            ViewBag.UserLog = UserLog;
            ViewBag.STime = STime;
            ViewBag.ETime = ETime;
            return View();
       }
        /// <summary>
        /// 导出
        /// </summary>
        /// <param name="WTimes"></param>
        public void XlsDo(UserLog UserLog, EFPagingInfo<UserLog> p, DateTime? STime, DateTime? ETime)
        {
            p.SqlWhere.Add(f => f.UId == UserLog.UId);
            p.PageSize = 99999999;
            if (!UserLog.OId.IsNullOrEmpty()) { p.SqlWhere.Add(f => f.OId == UserLog.OId); }
            if (STime.HasValue)
            {
                p.SqlWhere.Add(f => f.AddTime >= STime);
            }
            if (ETime.HasValue)
            {

                if (ETime < new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day))
                {
                    ETime = new DateTime(ETime.Value.Year, ETime.Value.Month, ETime.Value.Day, 23, 59, 59, 999);

                }
                //else
                //{
                //    ETime = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second, 999);
                //}
                p.SqlWhere.Add(f => f.AddTime <= ETime);
            }
           
            p.OrderByList.Add("Id", "DESC");
            IPageOfItems<UserLog> UserLogList = Entity.Selects<UserLog>(p);
            var UsersLogList = UserLogList.ToList();
            if (UsersLogList == null || UsersLogList.Count <= 0)
            {
                Response.Write("暂无符合条件数据");
                return;
            }
            string tempname = "UserLog.xlsx";
            string file = Server.MapPath("/template") + "\\" + tempname;
            ExcelPackage package = new ExcelPackage(new FileInfo(file), true);
            var sheet = package.Workbook.Worksheets[1];
            var cells = sheet.Cells;
            int i = 3;
            var typename="";
            //设置数据开始行
            int Befor = 3;
            foreach (var model in UsersLogList)
            {
                sheet.Row(i).Height = 20;//设置行高
                
                cells["A" + i].Value = model.OId;
                switch (model.OType)
                {
                    case 1:
                        typename = "收款";
                        break;
                    case 2:
                        typename = "付款";
                        break;
                    case 3:
                        typename = "申请提现";
                        break;
                    case 4:
                        typename = "提现";
                        break;
                    case 5:
                        typename = "提现失败";
                        break;
                    case 6:
                        typename = "退款冲正";
                        break;
                    case 7:
                        typename = "奖金";
                        break;
                    case 8:
                        typename = "分润";
                        break;
                    case 9:
                        typename = "冻结";
                        break;
                    case 10:
                        typename = "解冻";
                        break;
                    case 11:
                        typename = "退款";
                        break;
                    case 12:
                        typename = "扣款";
                        break;
                  }
                        cells["B" + i].Value = typename;
                        cells["C" + i].Value = model.Amount;
                        cells["D" + i].Value = model.BeforAmount;
                        cells["E" + i].Value = model.BeforFrozen;
                        cells["F" + i].Value = model.AfterAmount;
                        cells["G" + i].Value = model.AfterFrozen;
                        cells["H" + i].Value = model.AddTime.ToString("yyyy-MM-dd HH:mm");
                        i++;
                       
                
            }

            sheet.Cells[Befor, 3, i, 3].Style.Numberformat.Format = "#,##0.00";
            sheet.Cells[Befor, 4, i, 4].Style.Numberformat.Format = "#,##0.00";
            sheet.Cells[Befor, 5, i, 5].Style.Numberformat.Format = "#,##0.00";
            sheet.Cells[Befor, 6, i, 6].Style.Numberformat.Format = "#,##0.00";
            sheet.Cells[Befor, 7, i, 7].Style.Numberformat.Format = "#,##0.00";
            Response.BinaryWrite(package.GetAsByteArray());//输出
            Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            Response.AddHeader("content-disposition", "attachment;  filename=余额变动明细表"+DateTime.Now.ToString("yyyyMMddHHmm")+".xlsx"); 
        }
    }
}
