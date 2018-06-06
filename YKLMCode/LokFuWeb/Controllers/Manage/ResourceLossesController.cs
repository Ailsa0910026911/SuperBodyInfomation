using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using LokFu.Extensions;
using LokFu.Infrastructure;
using LokFu.Models;
using LokFu.Repositories;
using LokFu.Repositories.SqlServer;
namespace LokFu.Areas.Manage.Controllers
{
    public class ResourceLossesController : BaseController
    {
        //
        // GET: /ResourceLosses/

        public ActionResult Index(SMSLog SMSLog, string Year, string Month, int State = 0, int IsFirst = 0)
        {
            
            ViewBag.SMSLog = SMSLog;
            ViewBag.Year = Year;
            ViewBag.Month = Month;
            ViewBag.State = State;
            if (IsFirst == 0)
            {
                ViewBag.SMSReportList = new List<Report>();
                ViewBag.AuthReportList = new List<Report>();
                return View();
            }
          
            string SMSReprotSql = "";
            string AuthReprotSql = "";
            switch (State)
            {
                case 1://年报表
                    SMSReprotSql = @"SELECT   CONVERT(varchar(4), DATEPART(YY,ADDTIME)) 'DateTime',COUNT(1) 'Sum' 
  FROM [Lokfu].[dbo].[SMSLog] 
  GROUP BY DATEPART(YY,ADDTIME)";
                    AuthReprotSql = @"SELECT   CONVERT(varchar(4), DATEPART(YY,ADDTIME)) 'DateTime',COUNT(1) 'Sum' 
  FROM [Lokfu].[dbo].[UserAuth] 
  GROUP BY DATEPART(YY,ADDTIME)";
                    break;
                case 2://月报表
                    SMSReprotSql = @"  SELECT   CONVERT(VARCHAR(6),addtime,112) 'DateTime',COUNT(1) 'Sum'
  FROM [Lokfu].[dbo].[SMSLog] 
  GROUP BY CONVERT(VARCHAR(6),addtime,112)
  HAVING left (CONVERT(VARCHAR(6),addtime,112),4)='" + Year + @"'
  order by 1";
                    AuthReprotSql = @"  SELECT   CONVERT(VARCHAR(6),addtime,112) 'DateTime',COUNT(1) 'Sum'
  FROM [Lokfu].[dbo].[UserAuth] 
  GROUP BY CONVERT(VARCHAR(6),addtime,112)
  HAVING left (CONVERT(VARCHAR(6),addtime,112),4)='" + Year + @"'
  order by 1";
                    break;
                case 3://日报表
                    SMSReprotSql = @"SELECT   CONVERT(VARCHAR(24),addtime,112) 'DateTime',COUNT(1) 'Sum'
  FROM [Lokfu].[dbo].[SMSLog] 
  GROUP BY CONVERT(VARCHAR(24),addtime,112)  
    HAVING left (CONVERT(VARCHAR(24),addtime,112),6)='" + Year + Month + @"'
   order by 1";
                    AuthReprotSql = @"SELECT   CONVERT(VARCHAR(24),addtime,112) 'DateTime',COUNT(1) 'Sum'
  FROM [Lokfu].[dbo].[UserAuth] 
  GROUP BY CONVERT(VARCHAR(24),addtime,112)  
    HAVING left (CONVERT(VARCHAR(24),addtime,112),6)='" + Year + Month + @"'
   order by 1";
                    break;
                default:
                    SMSReprotSql = @"SELECT   CONVERT(varchar(4), DATEPART(YY,ADDTIME)) 'DateTime',COUNT(1) 'Sum' 
  FROM [Lokfu].[dbo].[SMSLog] 
  GROUP BY DATEPART(YY,ADDTIME)";
                    AuthReprotSql = @"SELECT   CONVERT(varchar(4), DATEPART(YY,ADDTIME)) 'DateTime',COUNT(1) 'Sum' 
  FROM [Lokfu].[dbo].[UserAuth] 
  GROUP BY DATEPART(YY,ADDTIME)";
                    break;
            }
            IList<Report> SMSReportList = Entity.ExecuteStoreQuery<Report>(SMSReprotSql, null).ToList();
            IList<Report> AuthReportList = Entity.ExecuteStoreQuery<Report>(AuthReprotSql, null).ToList();
            ViewBag.AuthReportList = AuthReportList;
            ViewBag.SMSReportList = SMSReportList;
            return View();
        }
    }
    public class Report
    {
        public String DateTime { get; set; }
        public Int32 Sum { get; set; }
    }
}
