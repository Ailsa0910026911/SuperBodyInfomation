using LokFu.Extensions;
using LokFu.Infrastructure;
using LokFu.Models;
using LokFu.Repositories;
using LokFu.Repositories.Extensions;
using LokFu.Repositories.SqlServer;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web.Mvc;
namespace LokFu.Areas.Manage.Controllers
{
    public class ProfitLossController : BaseController
    {
        /// <summary>
        /// 盈亏报表
        /// </summary>
        /// <param name="WTimes">需要查询的时间</param>
        /// <param name="EndDT">需要查询的时间</param>
        /// <param name="IsFirst">是否查询</param>
        /// <returns></returns>
        public ActionResult Index(DateTime? WTimes = null, DateTime? EndDT = null, int IsFirst = 0)
        {
            this.Session["ListModel"] = null;
            this.Session["WTimes"] = null;
            this.Session["EndDT"] = null;

            ViewBag.Xls = this.checkPower("Xls");
            IList<ProfitLossModel> ProfitLossList1 = null;
            IList<ProfitLossModel> ProfitLossList2 = null;
            IList<ProfitLossModel> ProfitLossList3 = null;
            IList<ProfitLossModel> ProfitLossList4 = null;
            IList<ProfitLossModel> ProfitLossList6 = null;
            IList<ProfitLossModel> ProfitLossList7 = null;
            Dictionary<string, string> dicChar = new Dictionary<string, string>();
            WTimes = WTimes == null ? DateTime.Parse(DateTime.Now.ToString("yyyy-MM-dd") + " 00:00:00") : WTimes;
            EndDT = EndDT == null ? DateTime.Now : EndDT;

            TimeSpan TS = EndDT.Value.Subtract(WTimes.Value);
            int Days = TS.Days;
            if (Days > 31)
            {
                ViewBag.ErrorMsg = "统计时间间隔不能超过31天！";
                return View("Error");
            }
            EndDT = EndDT.Value.AddMilliseconds(999);
            dicChar.Add("S_Time", WTimes.Value.ToString("yyyy-MM-dd HH:mm:ss.fff"));
            dicChar.Add("E_Time", EndDT.Value.ToString("yyyy-MM-dd HH:mm:ss.fff"));
            if (IsFirst > 0)
            {
                dicChar.Add("TYPE", "1");
                ProfitLossList1 = Entity.GetSPExtensions<ProfitLossModel>("SP_Statistics_Profit", dicChar);
                dicChar.Remove("TYPE");
                dicChar.Add("TYPE", "2");
                ProfitLossList2 = null;//Entity.GetSPExtensions<ProfitLossModel>("SP_Statistics_Profit", dicChar);
                dicChar.Remove("TYPE");
                dicChar.Add("TYPE", "3");
                ProfitLossList3 = Entity.GetSPExtensions<ProfitLossModel>("SP_Statistics_Profit", dicChar);
                dicChar.Remove("TYPE");
                dicChar.Add("TYPE", "5");
                ProfitLossList4 = Entity.GetSPExtensions<ProfitLossModel>("SP_Statistics_Profit", dicChar);
                dicChar.Remove("TYPE");
                dicChar.Add("TYPE", "6");
                ProfitLossList6 = Entity.GetSPExtensions<ProfitLossModel>("SP_Statistics_Profit", dicChar);
                dicChar.Remove("TYPE");
                dicChar.Add("TYPE", "7");
                ProfitLossList7 = Entity.GetSPExtensions<ProfitLossModel>("SP_Statistics_Profit", dicChar);
            }
            ViewBag.WTimes = WTimes;
            ViewBag.EndDT = EndDT.Value;
            ProfitLossList1 = ProfitLossList1 ?? new List<ProfitLossModel>() { new ProfitLossModel() };
            ProfitLossList2 = ProfitLossList2 ?? new List<ProfitLossModel>() { new ProfitLossModel() };
            ProfitLossList3 = ProfitLossList3 ?? new List<ProfitLossModel>() { new ProfitLossModel() };
            ProfitLossList4 = ProfitLossList4 ?? new List<ProfitLossModel>() { new ProfitLossModel() };
            ProfitLossList6 = ProfitLossList6 ?? new List<ProfitLossModel>() { new ProfitLossModel() };
            ProfitLossList7 = ProfitLossList7 ?? new List<ProfitLossModel>() { new ProfitLossModel() };

            ProfitLossList1 = ProfitLossList1.Count == 0 ? new List<ProfitLossModel>() { new ProfitLossModel() } : ProfitLossList1;
            ProfitLossList2 = ProfitLossList2.Count == 0 ? new List<ProfitLossModel>() { new ProfitLossModel() } : ProfitLossList2;
            ProfitLossList3 = ProfitLossList3.Count == 0 ? new List<ProfitLossModel>() { new ProfitLossModel() } : ProfitLossList3;
            ProfitLossList4 = ProfitLossList4.Count == 0 ? new List<ProfitLossModel>() { new ProfitLossModel() } : ProfitLossList4;
            ProfitLossList6 = ProfitLossList6.Count == 0 ? new List<ProfitLossModel>() { new ProfitLossModel() } : ProfitLossList6;
            ProfitLossList7 = ProfitLossList7.Count == 0 ? new List<ProfitLossModel>() { new ProfitLossModel() } : ProfitLossList7;
            
            ViewBag.ProfitLossList1 = ProfitLossList1; //资金流入
            ViewBag.ProfitLossList2 = ProfitLossList2; //理财转入
            ViewBag.ProfitLossList3 = ProfitLossList3; //资金流出
            ViewBag.ProfitLossList4 = ProfitLossList4;  //理财流出
            ViewBag.ProfitLossList6 = ProfitLossList6;  //订单退款
            ViewBag.ProfitLossList7 = ProfitLossList7;  //提现退款

            Dictionary<string, IList<ProfitLossModel>> ListModel = new Dictionary<string, IList<ProfitLossModel>>();
            ListModel.Add("1",ProfitLossList1);
            ListModel.Add("2",ProfitLossList2);
            ListModel.Add("3",ProfitLossList3);
            ListModel.Add("4",ProfitLossList4);
            ListModel.Add("6", ProfitLossList6);
            ListModel.Add("7",ProfitLossList7);

            this.Session["ListModel"] = ListModel;
            this.Session["WTimes"] = WTimes;
            this.Session["EndDT"] = EndDT;
            return View();
        }
        /// <summary>
        /// 导出
        /// </summary>
        /// <param name="WTimes">开始时间</param>
        /// <param name="EndDT">结束时间</param>
        /// <returns></returns>
        public FileResult XlsDo()
        {
            var ListModel = this.Session["ListModel"] as Dictionary<string, IList<ProfitLossModel>>;
            var WTimes = this.Session["WTimes"] as DateTime?;
            var EndDT = this.Session["EndDT"] as DateTime?;
            DataTable table = new DataTable();
            table.Columns.Add(new DataColumn("项目", typeof(string)));
            table.Columns.Add(new DataColumn("支付方式代码", typeof(string)));
            table.Columns.Add(new DataColumn("支付方式", typeof(string)));
            table.Columns.Add(new DataColumn("交易金额", typeof(string)));
            table.Columns.Add(new DataColumn("（商户）到账金额", typeof(string)));
            table.Columns.Add(new DataColumn("用户手续费", typeof(string)));
            table.Columns.Add(new DataColumn("支出手续费", typeof(string)));
            table.Columns.Add(new DataColumn("结算金额（分润）", typeof(string)));
            table.Columns.Add(new DataColumn("利润", typeof(string)));
            DataRow row = null;
            string fileName = "好付盈亏报表";
            decimal countAmoney1 = 0M;
            decimal countPayMoney1 = 0M;
            decimal countPoundage1 = 0M;
            decimal countSysRate1 = 0M;
            decimal countAgentPayGet1 = 0M;
            decimal countProfie1 = 0M;

            decimal countAmoney2 = 0M;
            decimal countPayMoney2 = 0M;
            decimal countPoundage2 = 0M;
            decimal countSysRate2 = 0M;
            decimal countAgentPayGet2 = 0M;
            decimal countProfie2 = 0M;

            decimal countAmoney3 = 0M;
            decimal countPayMoney3 = 0M;
            decimal countPoundage3 = 0M;
            decimal countSysRate3 = 0M;
            decimal countAgentPayGet3 = 0M;
            decimal countProfie3 = 0M;

            if (ListModel != null && WTimes != null && EndDT != null)
            {
                fileName += WTimes.Value.ToString("yyyy-MM-dd HH:mm:ss") + "至" + EndDT.Value.ToString("yyyy-MM-dd HH:mm:ss") + "";

                #region 资金流入
                if (ListModel.ContainsKey("1"))
                {
                    var model = ListModel["1"];
                    bool Istrue = true;
                    if (model != null && model.Count > 0)
                    {
                        foreach (var item in model)
                        {
                            row = table.NewRow();
                            if (Istrue)
                            {
                                row[0] = "资金流入";
                                Istrue = false;
                            }
                            else
                            {
                                row[0] = "";
                            }
                            row[1] = item.PayType;
                            row[2] = item.PayWay;
                            row[3] = item.Amoney.ToString("F2");
                            row[4] = item.PayMoney.ToString("F2");
                            row[5] = item.Poundage.ToString("F2");
                            row[6] = item.SysRate.ToString("F2");
                            row[7] = item.AgentPayGet.ToString("F2");
                            row[8] = item.Profie.ToString("F2");
                            table.Rows.Add(row);
                        }
                        countAmoney1 = model.Sum(x => x.Amoney);// +ListModel[1].Sum(x => x.Amoney);
                        countPayMoney1 = model.Sum(x => x.PayMoney);// +ListModel[1].Sum(x => x.PayMoney);
                        countPoundage1 = model.Sum(x => x.Poundage);// +ListModel[1].Sum(x => x.Poundage);
                        countSysRate1 = model.Sum(x => x.SysRate);// +ListModel[1].Sum(x => x.SysRate);
                        countAgentPayGet1 = model.Sum(x => x.AgentPayGet);// +ListModel[1].Sum(x => x.AgentPayGet);
                        countProfie1 = model.Sum(x => x.Profie);// +ListModel[1].Sum(x => x.Profie);
                        //小计
                        row = table.NewRow();
                        row[0] = "小计";
                        row[1] = "";
                        row[2] = "";
                        row[3] = countAmoney1.ToString("F2");
                        row[4] = countPayMoney1.ToString("F2");
                        row[5] = countPoundage1.ToString("F2");
                        row[6] = countSysRate1.ToString("F2");
                        row[7] = countAgentPayGet1.ToString("F2");
                        row[8] = countProfie1.ToString("F2");
                        table.Rows.Add(row);
                    }
                }
                #endregion

                #region 理财转入
                    //else if (i == 2)
                    //{
                    //    if (model != null && model.Count > 0)
                    //    {
                    //        foreach (var item in model)
                    //        {
                    //            row = table.NewRow();
                    //            row[0] = "理财转入";
                    //            row[1] = "?";
                    //            row[2] = item.PayWay;
                    //            row[3] = item.Amoney.ToString("F2");
                    //            row[4] = item.PayMoney.ToString("F2");
                    //            row[5] = item.Poundage.ToString("F2");
                    //            row[6] = item.SysRate.ToString("F2");
                    //            row[7] = item.AgentPayGet.ToString("F2");
                    //            row[8] = item.Profie.ToString("F2");
                    //            table.Rows.Add(row);
                    //        }
                    //    }
                    //    else
                    //    {
                    //        row = table.NewRow();
                    //        row[0] = "理财转入";
                    //        row[1] = "";
                    //        row[2] = "";
                    //        row[3] = "0";
                    //        row[4] = "0";
                    //        row[5] = "0";
                    //        row[6] = "0";
                    //        row[7] = "0";
                    //        row[8] = "0";
                    //        table.Rows.Add(row);
                    //    }

                    //    //空一行
                    //    row = table.NewRow();
                    //    row[0] = "";
                    //    row[1] = "";
                    //    row[2] = "";
                    //    row[3] = "";
                    //    row[4] = "";
                    //    row[5] = "";
                    //    row[6] = "";
                    //    row[7] = "";
                    //    row[8] = "";
                    //    table.Rows.Add(row);

                    //    //空一行
                    //    row = table.NewRow();
                    //    row[0] = "";
                    //    row[1] = "";
                    //    row[2] = "";
                    //    row[3] = "";
                    //    row[4] = "";
                    //    row[5] = "";
                    //    row[6] = "";
                    //    row[7] = "";
                    //    row[8] = "";
                    //    table.Rows.Add(row);
                    //}
                    #endregion

                #region 资金流出 & 理财利息支出
                if (ListModel.ContainsKey("3"))
                {
                    var model = ListModel["3"];
                    bool Istrue = true;
                    if (model != null && model.Count > 0)
                    {
                        foreach (var item in model)
                        {
                            row = table.NewRow();
                            if (Istrue)
                            {
                                row[0] = "资金流出";
                                Istrue = false;
                            }
                            else
                            {
                                row[0] = "";
                            }
                            row[1] = item.PayType;
                            row[2] = item.PayWay;
                            row[3] = item.Amoney.ToString("F2");
                            row[4] = item.PayMoney.ToString("F2");
                            row[5] = item.Poundage.ToString("F2");
                            row[6] = item.SysRate.ToString("F2");
                            row[7] = item.AgentPayGet.ToString("F2");
                            row[8] = item.Profie.ToString("F2");
                            table.Rows.Add(row);
                        }
                    }
                }
               
                if (ListModel.ContainsKey("3"))
                {
                    var model3 = ListModel["3"];
                    //小计
                    countAmoney2 = model3.Sum(x => x.Amoney);
                    countPayMoney2 = model3.Sum(x => x.PayMoney);// +ListModel[3].Sum(x => x.PayMoney);
                    countPoundage2 = model3.Sum(x => x.Poundage);// +ListModel[3].Sum(x => x.Poundage);
                    countSysRate2 = model3.Sum(x => x.SysRate);// +ListModel[3].Sum(x => x.SysRate);
                    countAgentPayGet2 = model3.Sum(x => x.AgentPayGet);// + ListModel[3].Sum(x => x.AgentPayGet);
                    countProfie2 = model3.Sum(x => x.Profie);// +ListModel[3].Sum(x => x.Profie);
                    row = table.NewRow();
                    row[0] = "小计";
                    row[1] = "";
                    row[2] = "";
                    row[3] = countAmoney2.ToString("F2");
                    row[4] = countPayMoney2.ToString("F2");
                    row[5] = countPoundage2.ToString("F2");
                    row[6] = countSysRate2.ToString("F2");
                    row[7] = countAgentPayGet2.ToString("F2");
                    row[8] = countProfie2.ToString("F2");
                    table.Rows.Add(row);
                }
                #endregion

                #region 订单退款 & 提现退款
                if (ListModel.ContainsKey("6"))
                {
                    var model = ListModel["6"];
                    bool Istrue = true;
                    if (model != null && model.Count > 0)
                    {
                        foreach (var item in model)
                        {
                            row = table.NewRow();
                            if (Istrue)
                            {
                                row[0] = "订单退款";
                                Istrue = false;
                            }
                            else
                            {
                                row[0] = "";
                            }
                            row[1] = item.PayType;
                            row[2] = item.PayWay;
                            row[3] = item.Amoney.ToString("F2");
                            row[4] = item.PayMoney.ToString("F2");
                            row[5] = item.Poundage.ToString("F2");
                            row[6] = item.SysRate.ToString("F2");
                            row[7] = item.AgentPayGet.ToString("F2");
                            row[8] = item.Profie.ToString("F2");
                            table.Rows.Add(row);
                        }
                    }
                }
                if (ListModel.ContainsKey("7"))
                {
                    var model = ListModel["7"];
                    if (model != null && model.Count > 0)
                    {
                        foreach (var item in model)
                        {
                            row = table.NewRow();
                            row[0] = "提现退款";
                            row[1] = item.PayType;
                            row[2] = item.PayWay;
                            row[3] = item.Amoney.ToString("F2");
                            row[4] = item.PayMoney.ToString("F2");
                            row[5] = item.Poundage.ToString("F2");
                            row[6] = item.SysRate.ToString("F2");
                            row[7] = item.AgentPayGet.ToString("F2");
                            row[8] = item.Profie.ToString("F2");
                            table.Rows.Add(row);
                        }
                    }
                }
                if (ListModel.ContainsKey("7") || ListModel.ContainsKey("6"))
                {
                    var model6 = ListModel["6"];
                    var model7 = ListModel["7"];
                    //小计
                    countAmoney3 = model7.Sum(x => x.Amoney) + model6.Sum(x => x.Amoney);
                    countPayMoney3 = model7.Sum(x => x.PayMoney);// +ListModel[3].Sum(x => x.PayMoney);
                    countPoundage3 = model7.Sum(x => x.Poundage);// +ListModel[3].Sum(x => x.Poundage);
                    countSysRate3 = model7.Sum(x => x.SysRate);// +ListModel[3].Sum(x => x.SysRate);
                    countAgentPayGet3 = model7.Sum(x => x.AgentPayGet);// + ListModel[3].Sum(x => x.AgentPayGet);
                    countProfie3 = model7.Sum(x => x.Profie);// +ListModel[3].Sum(x => x.Profie);
                    row = table.NewRow();
                    row[0] = "小计";
                    row[1] = "";
                    row[2] = "";
                    row[3] = countAmoney3.ToString("F2");
                    row[4] = countPayMoney3.ToString("F2");
                    row[5] = countPoundage3.ToString("F2");
                    row[6] = countSysRate3.ToString("F2");
                    row[7] = countAgentPayGet3.ToString("F2");
                    row[8] = countProfie3.ToString("F2");
                    table.Rows.Add(row);
                }
                #endregion
            }
            
            //总计
            row = table.NewRow();
            row[0] = "总计";
            row[1] = "";
            row[2] = "";
            row[3] = (countAmoney1 - countAmoney2 - countAmoney3).ToString("F2");
            row[4] = (countPayMoney1 - countPayMoney2 - countPayMoney3).ToString("F2");
            row[5] = (countPoundage1 + countPoundage2 + countPoundage3).ToString("F2");
            row[6] = (countSysRate1 + countSysRate2 + countSysRate3).ToString("F2");
            row[7] = (countAgentPayGet1 + countAgentPayGet2 + countAgentPayGet3).ToString("F2");
            row[8] = (countProfie1 + countProfie2 + countProfie3).ToString("F2");
            table.Rows.Add(row);
            //理财余额利息	 余额奖励金	
            if (ListModel.ContainsKey("4"))
            {
                var model = ListModel["4"];
                if (model != null && model.Count > 0)
                {
                    foreach (var item in model)
                    {
                        row = table.NewRow();
                        row[0] = "理财利息支出";
                        row[1] = "";
                        row[2] = "";
                        row[3] = item.Amount.ToString("F2");
                        row[4] = "";
                        row[5] = "";
                        row[6] = "";
                        row[7] = "";
                        row[8] = "";
                        table.Rows.Add(row);
                    }
                }
            }
            return ExportExcelBase(table, fileName);
        }
    }
}
