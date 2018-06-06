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
using System.Collections;
using System.Data;
using System.IO;
using NPOI.SS.UserModel;
using NPOI.HSSF.UserModel;
using NPOI.XSSF.UserModel;

namespace LokFu.Areas.Manage.Controllers
{
    public class ApplyCreditCardController : BaseController
    {
        //
        // GET: /ApplyCreditCard/

        public ActionResult Index(ApplyCreditCard ApplyCreditCard, EFPagingInfo<ApplyCreditCard> p, DateTime? STime, DateTime? ETime, bool? IsShowSupAgent, int IsFirst = 0)
        {
            if (STime.IsNullOrEmpty())
            {
                STime = DateTime.Parse(DateTime.Now.ToString("yyyy-MM-dd"));
            }
            if (ETime.IsNullOrEmpty())
            {
               // ETime = DateTime.Parse(DateTime.Now.ToString("yyyy-MM-dd"));
                ETime = DateTime.Now;
            }
            if (IsShowSupAgent == null)
            {
                IsShowSupAgent = false;
            }
            ViewBag.Save = checkPower("Save");
            ViewBag.IsUpLoad = checkPower("UpLoad");
            ViewBag.ETime = ETime;
            ViewBag.STime = STime;
            ViewBag.BasicBankList = Entity.BasicBank.Where(n => n.CreditCardUrl != null).ToList();
            ViewBag.ApplyCreditCard = ApplyCreditCard;
            ViewBag.SysAgentList = Entity.SysAgent.Where(n => n.Tier == 1).ToList();
            ViewBag.IsShowSupAgent = IsShowSupAgent;
            
           // ETime = ETime.Value.AddDays(1);
            p.SqlWhere.Add(f => f.AddTime > STime && f.AddTime < ETime);
            if (!ApplyCreditCard.BankId.IsNullOrEmpty()) { p.SqlWhere.Add(f => f.BankId == ApplyCreditCard.BankId); }
            if (!ApplyCreditCard.UserMobile.IsNullOrEmpty()) { p.SqlWhere.Add(f => f.UserMobile == ApplyCreditCard.UserMobile); }
            if (!ApplyCreditCard.OrderNum.IsNullOrEmpty()) { p.SqlWhere.Add(f => f.OrderNum == ApplyCreditCard.OrderNum); }
            if (!ApplyCreditCard.UserName.IsNullOrEmpty()) { p.SqlWhere.Add(f => f.UserName == ApplyCreditCard.UserName); }
            if (!ApplyCreditCard.State.IsNullOrEmpty()) { p.SqlWhere.Add(f => f.State == ApplyCreditCard.State); }
            if (!ApplyCreditCard.FirstAgentAmount.IsNullOrEmpty()) { p.SqlWhere.Add(f => f.FirstAgentAmount == ApplyCreditCard.FirstAgentAmount); }
            if (!ApplyCreditCard.FirstAgentId.IsNullOrEmpty())
            {
                p.SqlWhere.Add(f => f.FirstAgentId == ApplyCreditCard.FirstAgentId);
            }
            p.OrderByList.Add("Id", "DESC");
            IPageOfItems<ApplyCreditCard> ApplyCreditCardList = null;
            if (IsFirst == 0)
            {

                ApplyCreditCardList = new PageOfItems<ApplyCreditCard>(new List<ApplyCreditCard>(), 0, 10, 0, new Hashtable());
            }
            else
            {
                ApplyCreditCardList = Entity.Selects<ApplyCreditCard>(p);
            }
            ViewBag.ApplyCreditCardList = ApplyCreditCardList;

            return View();
        }
        public ActionResult Edit(ApplyCreditCard ApplyCreditCard)
        {
            ViewBag.IsSave = checkPower("Save");
            if (ApplyCreditCard.Id != 0) ApplyCreditCard = Entity.ApplyCreditCard.FirstOrDefault(n => n.Id == ApplyCreditCard.Id);
            if (ApplyCreditCard == null)
            {
                ViewBag.ErrorMsg = "数据不存在";
                return View("Error");
            }
            ViewBag.ApplyCreditCard = ApplyCreditCard;
            if (Request.UrlReferrer != null)
            {
                Session["Url"] = Request.UrlReferrer.ToString();
            }
            ViewBag.ApplyCreditCard = ApplyCreditCard;
            return View();
        }
        public void Save(ApplyCreditCard ApplyCreditCard)
        {
            ApplyCreditCard baseApplyCredit = Entity.ApplyCreditCard.FirstOrDefault(n => n.Id == ApplyCreditCard.Id);

            baseApplyCredit.State = ApplyCreditCard.State;
            baseApplyCredit.SettlementAmount = ApplyCreditCard.SettlementAmount;
            if (ApplyCreditCard.SettlementAmount > 0)
            {
                baseApplyCredit.SettlementState = 1;
                baseApplyCredit.SettlementTime = DateTime.Now;
            }
            if (ApplyCreditCard.FirstAgentAmount > 0)
            {
                baseApplyCredit.FirstAgentAmountState = 1;
                baseApplyCredit.FirstAgentTime = DateTime.Now;
            }
            if (ApplyCreditCard.SettlementAmount>0&&ApplyCreditCard.FirstAgentAmount>0)
            {
                baseApplyCredit.State = 7;
            }
            baseApplyCredit.FirstAgentAmount = ApplyCreditCard.FirstAgentAmount;
            Entity.SaveChanges();
            BaseRedirect();
        }
        /// <summary>
        /// 上传报表
        /// </summary>
        public ActionResult UpLoad(ApplyCreditCard ApplyCreditCard, string BankId)
        {
            if (Request.UrlReferrer != null)
            {
                Session["Url"] = Request.UrlReferrer.ToString();
            }
            ApplyCreditCard = Request.ConvertRequestToModel<ApplyCreditCard>(ApplyCreditCard, ApplyCreditCard);
            if (ApplyCreditCard.UserName == "System.Web.HttpPostedFileWrapper")
            {
                ViewBag.ErrorMsg = "文件类型错误";
                //RedirectToAction("Error");
                //return;
                return View("Error");
            }
            bool Result = false;
            //处理报表数据
            switch (BankId)
            {
                case "1"://兴业银行
                    Result = XingYe(BankId, ApplyCreditCard.UserName);
                    break;
                case "2"://交通银行
                    Result = JiaoTong(BankId, ApplyCreditCard.UserName);
                    break;
                default:
                    ViewBag.ErrorMsg = "上传数据有误,请查看是否选择了对应银行.";
                    return View("Error");
            }
            if (Result == false)
            {
                ViewBag.ErrorMsg = "上传数据有误,请查看是否选择了对应银行.";
                return View("Error");
            }
            BaseRedirect();
            return View("Succeed");
        }
        private bool XingYe(string BankId, string FileName)
        {
            try
            {
                DataTable dt = new DataTable();
                dt = ExcelToDataTable(System.Web.HttpContext.Current.Server.MapPath("~/UpLoadFiles/ApplyCreditCard/" + FileName), "Sheet1", true);
                if (dt == null) return false;
                ApplyCreditCard ApplyCreditCard;
                DateTime Stime;
                DateTime Etime;
                string Name = string.Empty;
                string Birthday = string.Empty;
                foreach (DataRow row in dt.Rows)
                {
                    Stime =Convert.ToDateTime( row[0].ToString());
                    Etime = Stime.AddDays(1);
                    Name = row[3].ToString();
                    Birthday = row[5].ToString().Replace("-", "").Replace("/","");
                 
                    string state = row[8].ToString();
                    switch (state)
                    {
                        case "拒件":
                            ApplyCreditCard = Entity.ApplyCreditCard.FirstOrDefault(f => f.UserName == Name && f.UserIdCard.Contains(Birthday) && f.AddTime > Stime && f.AddTime < Etime);
                            if (ApplyCreditCard==null)continue;
                            ApplyCreditCard.State = 5;
                            Entity.SaveChanges();
                            break;
                        case "待转人工审核":
                            ApplyCreditCard = Entity.ApplyCreditCard.FirstOrDefault(f => f.UserName == Name && f.UserIdCard.Contains(Birthday) && f.AddTime > Stime && f.AddTime < Etime);
                            if (ApplyCreditCard == null) continue;
                            ApplyCreditCard.State = 6;
                            Entity.SaveChanges();
                            break;
                        case "过件未发卡":
                            ApplyCreditCard = Entity.ApplyCreditCard.FirstOrDefault(f => f.UserName == Name && f.UserIdCard.Contains(Birthday) && f.AddTime > Stime && f.AddTime < Etime);
                            if (ApplyCreditCard == null) continue;
                            ApplyCreditCard.State = 3;
                            Entity.SaveChanges();
                            break;
                        case "过件已发卡":
                            ApplyCreditCard = Entity.ApplyCreditCard.FirstOrDefault(f => f.UserName == Name && f.UserIdCard.Contains(Birthday) && f.AddTime > Stime && f.AddTime < Etime);
                            if (ApplyCreditCard == null) continue;
                            ApplyCreditCard.State = 4;
                            Entity.SaveChanges();
                            break;
                        case "转人工审核中":
                            ApplyCreditCard = Entity.ApplyCreditCard.FirstOrDefault(f => f.UserName == Name && f.UserIdCard.Contains(Birthday) && f.AddTime > Stime && f.AddTime < Etime);
                            if (ApplyCreditCard == null) continue;
                            ApplyCreditCard.State = 2;
                            Entity.SaveChanges();
                            break;
                        default:
                            break;
                    }
                }
                return true;
            }
            catch (Exception)
            {

                return false;
            }

        }
        private bool JiaoTong(string BankId, string FileName)
        {
            try
            {
                DataTable dt = new DataTable();
                dt = ExcelToDataTable(System.Web.HttpContext.Current.Server.MapPath("~/UpLoadFiles/ApplyCreditCard/" + FileName), "Sheet1", true);
                if (dt == null) return false;
                return true;
            }
            catch (Exception)
            {

                return false;
            }
        }

        /// <summary>
        /// 将excel中的数据导入到DataTable中
        /// </summary>
        /// <param name="sheetName">excel工作薄sheet的名称</param>
        /// <param name="isFirstRowColumn">第一行是否是DataTable的列名</param>
        /// <returns>返回的DataTable</returns>
        public DataTable ExcelToDataTable(string fileName, string sheetName, bool isFirstRowColumn)
        {
            ISheet sheet = null;
            DataTable data = new DataTable();
            IWorkbook workbook = null;
            FileStream fs = null;
            int startRow = 0;
            try
            {
                fs = new FileStream(fileName, FileMode.Open, FileAccess.Read);
                if (fileName.IndexOf(".xlsx") > 0) // 2007版本
                    workbook = new XSSFWorkbook(fs);
                else if (fileName.IndexOf(".xls") > 0) // 2003版本
                    workbook = new HSSFWorkbook(fs);

                if (sheetName != null)
                {
                    sheet = workbook.GetSheet(sheetName);
                    if (sheet == null) //如果没有找到指定的sheetName对应的sheet，则尝试获取第一个sheet
                    {
                        sheet = workbook.GetSheetAt(0);
                    }
                }
                else
                {
                    sheet = workbook.GetSheetAt(0);
                }
                if (sheet != null)
                {
                    IRow firstRow = sheet.GetRow(0);
                    int cellCount = firstRow.LastCellNum; //一行最后一个cell的编号 即总的列数

                    if (isFirstRowColumn)
                    {
                        for (int i = firstRow.FirstCellNum; i < cellCount; ++i)
                        {
                            ICell cell = firstRow.GetCell(i);
                            if (cell != null)
                            {
                                string cellValue = cell.StringCellValue;
                                if (cellValue != null)
                                {
                                    DataColumn column = new DataColumn(cellValue);
                                    data.Columns.Add(column);
                                }
                            }
                        }
                        startRow = sheet.FirstRowNum + 1;
                    }
                    else
                    {
                        startRow = sheet.FirstRowNum;
                    }

                    //最后一列的标号
                    int rowCount = sheet.LastRowNum;
                    for (int i = startRow; i <= rowCount; ++i)
                    {
                        IRow row = sheet.GetRow(i);
                        if (row == null) continue; //没有数据的行默认是null　　　　　　　

                        DataRow dataRow = data.NewRow();
                        for (int j = row.FirstCellNum; j < cellCount; ++j)
                        {
                            if (row.GetCell(j) != null) //同理，没有数据的单元格都默认是null
                                dataRow[j] = row.GetCell(j).ToString();
                        }
                        data.Rows.Add(dataRow);
                    }
                }

                return data;
            }
            catch (Exception ex)
            {
                // Console.WriteLine("Exception: " + ex.Message);
                return null;
            }
        }
    }
}
