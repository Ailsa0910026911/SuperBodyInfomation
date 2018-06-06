using LokFu.Extensions;
using LokFu.FastPay;
using LokFu.Infrastructure;
using LokFu.Models;
using LokFu.Repositories;
using LokFu.Repositories.SqlServer;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OfficeOpenXml;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Security.Cryptography;
using System.Text;
using System.Web.Script.Serialization;
namespace LokFu.Areas.Manage.Controllers
{
    public class FastOrderController : BaseController
    {
        /// <summary>
        /// 人工处理支持的通道
        /// </summary>
        public string[] AllowFastPayWay = new string[] { "HFPay" };

        public ActionResult Index(FastOrder FastOrder, EFPagingInfo<FastOrder> p, bool? IsShowSupAgent, int IsFirst = 0)
        {
            if (IsShowSupAgent == null)
            {
                IsShowSupAgent = false;
            }
            p = this.Condition(FastOrder, p, IsShowSupAgent.Value);
            IPageOfItems<FastOrder> FastOrderList = null;
            if (IsFirst == 0)
            {
                FastOrderList = new PageOfItems<FastOrder>(new List<FastOrder>(), 0, 10, 0, new Hashtable());
            }
            else
            {
                FastOrderList = Entity.Selects<FastOrder>(p);
            }
            ViewBag.FastOrderList = FastOrderList;
            ViewBag.FastOrder = FastOrder;
            //副表信息
            List<int> UId = FastOrderList.Select(o => o.UId).Distinct().ToList();
            ViewBag.UsersList = Entity.Users.Where(n => n.State == 1 && UId.Contains(n.Id)).ToList();

            //其他信息
            var FastPayWayList = Entity.FastPayWay.ToList();
            var AllowFastPayWayIdList = FastPayWayList.Where(o => AllowFastPayWay.Contains(o.DllName)).Select(o => o.Id).ToList();
            ViewBag.FastPayWayList = FastPayWayList;
            ViewBag.AllowFastPayWayIdList = AllowFastPayWayIdList;
            ViewBag.SysAgentList = Entity.SysAgent.Where(n => n.State == 1).ToList();
            ViewBag.IsShowSupAgent = IsShowSupAgent;//是否显示下级

            //权限相关
            ViewBag.Edit = this.checkPower("Edit");
            ViewBag.IndexStats = this.checkPower("IndexStats");
            ViewBag.ExcelExport = this.checkPower("ExcelExport");
            ViewBag.SysAgentClearing = this.checkPower("SysAgentClearing");
            ViewBag.StateChange = this.checkPower("StateChange");
            ViewBag.Resubmit = this.checkPower("Resubmit");
            ViewBag.ExcelInport = this.checkPower("ExcelInport");
            ViewBag.Download = this.checkPower("Download");
            ViewBag.OrdersRepair = this.checkPower("OrdersRepair");
            return View();
        }

        /// <summary>
        /// 统计
        /// </summary>
        /// <returns></returns>
        public ActionResult IndexStats(FastOrder FastOrder, EFPagingInfo<FastOrder> p, bool? IsShowSupAgent)
        {
            //条件：已支付,正常
            //总金额
            //笔数
            //手续费
            //分润
            if (IsShowSupAgent == null)
            {
                IsShowSupAgent = false;
            }
            p = this.Condition(FastOrder, p, IsShowSupAgent.Value);
            var Iquery = this.Entity.FastOrder.AsQueryable();
            foreach (var item in p.SqlWhere)
            {
                Iquery = Iquery.Where(item);
            }
            //var Iquery = this.Entity.FastOrder.Where(o => o.State == 1 && o.PayState == 1);
            ViewBag.SumAmoney = Iquery.Sum(o => (decimal?)o.Amoney) ?? 0m;
            ViewBag.SumPoundage = Iquery.Sum(o => (decimal?)o.Poundage) ?? 0m;
            ViewBag.SumAgentPayGet = Iquery.Sum(o => (decimal?)o.AgentPayGet) ?? 0m;
            ViewBag.Count = Iquery.Count();
            return this.View();
        }

        public ActionResult Info(FastOrder FastOrder)
        {
            if (!FastOrder.Id.IsNullOrEmpty())
            {
                FastOrder = Entity.FastOrder.FirstOrDefault(n => n.Id == FastOrder.Id);
            }
            else if (!FastOrder.TNum.IsNullOrEmpty())
            {
                FastOrder = Entity.FastOrder.FirstOrDefault(n => n.TNum == FastOrder.TNum);
            }
            if (FastOrder == null)
            {
                ViewBag.ErrorMsg = "数据不存在";
                return View("Error");
            }
            ViewBag.FastOrder = FastOrder;
            ViewBag.Users = Entity.Users.FirstOrNew(n => n.Id == FastOrder.UId);
            ViewBag.SysAgent = Entity.SysAgent.FirstOrNew(n => n.Id == FastOrder.Agent);
            ViewBag.FastPayWay = Entity.FastPayWay.FirstOrNew(o => o.Id == FastOrder.PayWay);
            return View("Edit");
        }

        /// <summary>
        /// 代理商代理商结算
        /// </summary>
        /// <returns></returns>
        public string SysAgentClearing(string List)
        {
            string sql = "UPDATE FastOrder SET FastOrder.AgentState = 1,FastOrder.AgentTime =getdate() Where FastOrder.AgentState != 1 AND FastOrder.Id In(" + List + ");";
            this.Entity.ExecuteStoreCommand(sql);
            return "1";
        }

        private EFPagingInfo<FastOrder> Condition(FastOrder FastOrder, EFPagingInfo<FastOrder> p, bool IsShowSupAgent)
        {
            #region 筛选条件
            if (!FastOrder.PayWay.IsNullOrEmpty())
            {
                p.SqlWhere.Add(o => o.PayWay == FastOrder.PayWay);
            }

            if (!FastOrder.TNum.IsNullOrEmpty())
            {
                if (FastOrder.UId == 1)
                {
                    p.SqlWhere.Add(f => f.TNum == FastOrder.TNum);
                }
                else
                {
                    IList<Users> UList = Entity.Users.Where(n => n.TrueName.Contains(FastOrder.TNum) || n.NeekName.Contains(FastOrder.TNum) || n.UserName == FastOrder.TNum).ToList();
                    List<int> UIds = new List<int>();
                    foreach (var pp in UList)
                    {
                        UIds.Add(pp.Id);
                    }
                    p.SqlWhere.Add(f => UIds.Contains(f.UId));
                }
            }
            
            if (!FastOrder.UserState.IsNullOrEmpty())
            {
                var value = FastOrder.UserState == 99 ? 0 : FastOrder.UserState;
                p.SqlWhere.Add(o => o.UserState == value);
            }
            if (!FastOrder.AgentState.IsNullOrEmpty())
            {
                var value = FastOrder.AgentState == 99 ? 0 : FastOrder.AgentState;
                p.SqlWhere.Add(o => o.AgentState == value);
            }
            //是否选择了分支机构
            if (!FastOrder.Agent.IsNullOrEmpty())
            {
                //是否要显示下级
                if ((bool)IsShowSupAgent)
                {
                    SysAgent LowerLevelAgent = Entity.SysAgent.Where(s => s.Id == FastOrder.Agent).FirstOrNew();
                    IList<SysAgent> SysAgentList = LowerLevelAgent.GetSupAgent(Entity,true);
                    IList<int> UID = SysAgentList.Select(o => o.Id).ToList();
                    p.SqlWhere.Add(f => UID.Contains(f.Agent));
                }
                else
                {
                    p.SqlWhere.Add(f => f.Agent == FastOrder.Agent);
                }
            }

            if (!FastOrder.STime.IsNullOrEmpty() && !FastOrder.ETime.IsNullOrEmpty())
            {
                DateTime ETime = FastOrder.ETime;
                if (FastOrder.Bin == "1")
                {
                    p.SqlWhere.Add(f => f.AddTime > FastOrder.STime && f.AddTime < ETime);
                }
                else
                {
                    p.SqlWhere.Add(f => f.PayTime > FastOrder.STime && f.PayTime < ETime);
                }
            }

            #region 交易类型条件判断
            if (!FastOrder.OType.IsNullOrEmpty())
            {
                p.SqlWhere.Add(f => f.OType == FastOrder.OType);//读取对应的类型
            }

            if (!FastOrder.State.IsNullOrEmpty())
            {
                switch (FastOrder.State)
                {
                    case 1://未付
                        p.SqlWhere.Add(f => f.State == 1 && f.PayState == 0);
                        break;
                    case 2://已付
                        p.SqlWhere.Add(f => f.State == 1 && f.PayState == 1);
                        break;
                    case 99://交易关闭
                        p.SqlWhere.Add(f => f.State == 0);
                        break;
                }
            }
            #endregion

            #endregion
            p.OrderByList.Add("Id", "DESC");

            return p;
        }

        public FileResult ExcelExport(FastOrder FastOrder, EFPagingInfo<FastOrder> p, bool? IsShowSupAgent)
        {
            p.PageSize = 9999999;
            if (IsShowSupAgent == null)
            {
                IsShowSupAgent = false;
            }
            p = this.Condition(FastOrder, p, IsShowSupAgent.Value);
            IPageOfItems<FastOrder> FastOrderList = Entity.Selects<FastOrder>(p);
            List<int> UId = FastOrderList.Select(o => o.UId).Distinct().ToList();
            //List<int> AgentIds = FastOrderList.Select(o => o.Agent).Distinct().ToList();
            var UsersList = Entity.Users.Where(n => n.State == 1 && UId.Contains(n.Id)).ToList();
            //var ResideSysAgentList = Entity.SysAgent.Where(n => AgentIds.Contains(n.Id)).ToList();
            var FastPayWayList = Entity.FastPayWay.ToList();
            var SysAgentList = Entity.SysAgent.Where(n => n.Tier == 1).ToList();

            DataTable table = new DataTable();
            DataRow row = null;

            // 创建 datatable
            table.Columns.Add(new DataColumn("交易号", typeof(string)));
            table.Columns.Add(new DataColumn("交易商户", typeof(string)));
            table.Columns.Add(new DataColumn("总金额", typeof(decimal)));
            table.Columns.Add(new DataColumn("手续费", typeof(decimal)));
            table.Columns.Add(new DataColumn("分润", typeof(decimal)));
            table.Columns.Add(new DataColumn("成本费率‰", typeof(decimal)));
            table.Columns.Add(new DataColumn("成本费用(交易)", typeof(decimal)));
            table.Columns.Add(new DataColumn("成本费用(代付)", typeof(decimal)));
            table.Columns.Add(new DataColumn("用户费率‰", typeof(decimal)));
            table.Columns.Add(new DataColumn("用户费用", typeof(decimal)));
            table.Columns.Add(new DataColumn("利润", typeof(decimal)));
            table.Columns.Add(new DataColumn("交易类型", typeof(string)));
            table.Columns.Add(new DataColumn("交易时间", typeof(string)));
            table.Columns.Add(new DataColumn("交易状态", typeof(string)));
            table.Columns.Add(new DataColumn("用户结算", typeof(string)));
            table.Columns.Add(new DataColumn("代理结算", typeof(string)));
            table.Columns.Add(new DataColumn("支付通道", typeof(string)));
            table.Columns.Add(new DataColumn("支付时间", typeof(string)));
            table.Columns.Add(new DataColumn("分支机构", typeof(string)));
            table.Columns.Add(new DataColumn("同级分润", typeof(decimal)));

            // 填充数据
            foreach (var item in FastOrderList)
            {
                var AgentPathArr = item.AgentPath.Split('|').ToList();
                AgentPathArr.RemoveAll(o => o == string.Empty);
                string TopAgentName = "";
                if (AgentPathArr.Count > 0)
                {
                    int AgentID = 0;
                    if (int.TryParse(AgentPathArr[0], out AgentID))
                    {
                        TopAgentName = SysAgentList.FirstOrNew(o => o.Id == AgentID).Name;
                    }
                }

                row = table.NewRow();
                row[0] = item.TNum;
                row[1] = UsersList.FirstOrNew(o=>o.Id == item.UId).TrueName;
                row[2] = item.Amoney.ToString("F2");
                row[3] = item.Poundage.ToString("F2");
                row[4] = item.AgentPayGet.ToString("F2");
                row[5] = (item.SysRate * 1000).ToString("F2");
                row[6] = (item.SysRate * item.Amoney).Floor().ToString("F2");
                row[7] = item.SysCash.ToString("F2");
                row[8] = (item.UserRate * 1000).ToString("F2");
                row[9] = item.UserCash.ToString("F2");
                row[10] = item.HFGet.ToString("F2");
                row[11] = Utils.GetFastOrderModel().FirstOrNew(n => n.Id == item.OType).Name;
                row[12] = item.AddTime.ToString("yyyy-MM-dd HH:mm");
                row[13] = item.GetState();
                row[14] = item.GetUserClearingState();
                row[15] = item.GetAgentClearingState();
                row[16] = FastPayWayList.FirstOrNew(o => o.Id == item.PayWay).Title;
                row[17] = item.PayTime.HasValue ? item.PayTime.Value.ToString("yyyy-MM-dd HH:mm") : "";
                row[18] = TopAgentName;
                row[19] = item.SameGet.ToString("F2");
                table.Rows.Add(row);
            }

            return this.ExportExcelBase(table, "收付直通车");
        }

        /// <summary>
        /// 模板下载
        /// </summary>
        public void Download()
        {
            string tempname = "FastOrder.xlsx";
            string file = Server.MapPath("/template") + "\\" + tempname;
            ExcelPackage package = new ExcelPackage(new FileInfo(file), true);
            var sheet = package.Workbook.Worksheets[1];
            Response.BinaryWrite(package.GetAsByteArray());//输出
            Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            Response.AddHeader("content-disposition", "attachment;  filename=收付直通车结算导入模板.xlsx"); 
        }

        [HttpGet]
        public ActionResult StateChange(string TNum)
        {
            var FastOrder = this.Entity.FastOrder.FirstOrDefault(o => o.TNum == TNum);
            this.ViewBag.FastOrder = FastOrder;
            return View();
        }

        /// <summary>
        /// 人工处理-设置用户结算状态
        /// </summary>
        [HttpPost]
        public ActionResult StateChange(FastOrder FastOrder)
        {
            if (!(FastOrder.UserState == 1 || FastOrder.UserState == 2))
            {
                ViewBag.ErrorMsg = "请输入正确的状态";
                return View("Error");
            }
            var baseFastOrder = this.Entity.FastOrder.FirstOrDefault(o => o.TNum == FastOrder.TNum);
            if (!baseFastOrder.IsStateChange())
            {
                ViewBag.ErrorMsg = "用户结算状态未符合规则";
                return View("Error");
            }
            baseFastOrder.UserState = FastOrder.UserState;
            baseFastOrder.Remark = FastOrder.Remark;
            var FastOrderOP = new FastOrderOP()
            {
                AddTime = DateTime.Now,
                AdminId = this.AdminUser.Id,
                AdminName = this.AdminUser.TrueName,
                State = baseFastOrder.UserState,
                TNum = baseFastOrder.TNum,
                Remark = baseFastOrder.Remark,
            };
            this.Entity.FastOrderOP.AddObject(FastOrderOP);
            this.Entity.SaveChanges();
            ViewBag.Msg = "设置成功";
            return View("Succeed");
        }

        /// <summary>
        /// 米刷隔天直接通过接口
        /// </summary>
        /// <param name="TNum">订单号</param>
        /// <param name="Confirm">确认</param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult Resubmit(string TNum, bool Confirm)
        { 
            var p = this.Entity.FastOrder.FirstOrDefault(o => o.TNum == TNum);
            if (p.UserState != 2)
            {
                ViewBag.ErrorMsg = "只有用户结算失败才能使用该功能";
                return View("Error");
            }
            var FastPayWay = this.Entity.FastPayWay.FirstOrDefault(o => o.Id == p.PayWay);
            if (FastPayWay == null)
            {
                ViewBag.ErrorMsg = "未能找到相关通道";
                return View("Error");
            }
            var Today = new DateTime(DateTime.Now.Year,DateTime.Now.Month,DateTime.Now.Day,0,0,0);
            var OrderDay = new DateTime(p.PayTime.Value.Year,p.PayTime.Value.Month,p.PayTime.Value.Day,0,0,0);
            if ((FastPayWay.DllName == "MiPay" || FastPayWay.DllName == "MiShua") && Today != OrderDay)
            {
                if (Confirm)
                {
                    p.UserState = 1;
                    p.Remark = p.Remark + "|" + DateTime.Now.ToString() + "被" + AdminUser.TrueName + "修改为成功";
                    this.Entity.SaveChanges();
                    ViewBag.Msg = "用户结算已修改为'已结算'";
                    return View("Succeed");
                }
                else
                {
                    ViewBag.ErrorMsg = "参数错误";
                    return View("Error");
                }
            }
            else
            {
                ViewBag.ErrorMsg = "不支持相关通道";
                return View("Error");
            }
        }

        /// <summary>
        /// 重新出款
        /// </summary>
        [HttpGet]
        public ActionResult Resubmit(FastOrder FastOrder)
        {
            //1. 接口配置校验与规则校验
            //2. 米刷当天校验与处理
            //3. 检查之前重出情况
            //4. 创建重出单
            //5. 重出

            //1. 接口配置校验与规则校验
            var p = this.Entity.FastOrder.FirstOrDefault(o => o.TNum == FastOrder.TNum);
            if (p.UserState != 2)
            {
                ViewBag.ErrorMsg = "只有用户结算失败才能使用该功能";
                return View("Error");
            }
            var FastUser = Entity.FastUser.FirstOrDefault(o => o.UId == p.UId);
            var FastPayWay = this.Entity.FastPayWay.FirstOrDefault(o => o.Id == p.PayWay);
            var FastUserPay = Entity.FastUserPay.FirstOrDefault(o => o.PayWay == p.PayWay && o.CardState == 1 && o.UId == p.UId);
            if (FastPayWay == null)
            {
                ViewBag.ErrorMsg = "未能找到相关通道";
                return View("Error");
            }
            if(FastUserPay == null)
            {
                ViewBag.ErrorMsg = "用户银行卡未绑定或未通过第三方验证";
                return View("Error");
            }
            if (!AllowFastPayWay.Contains(FastPayWay.DllName))
            {
                ViewBag.ErrorMsg = "不支持相关通道";
                return View("Error");
            }
           

            //2. 米刷当天校验与处理
            var Today = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 0, 0, 0);
            var OrderDay = new DateTime(p.PayTime.Value.Year, p.PayTime.Value.Month, p.PayTime.Value.Day, 0, 0, 0);
            if ((FastPayWay.DllName == "MiPay" || FastPayWay.DllName == "MiShua") && Today != OrderDay)
            {
                ViewBag.Msg = "请确认接口方是否出款成功？";
                ViewBag.ActionUrl = this.Url.Action("Resubmit", new { TNum = p.TNum, IsAjax = 1 });
                return View("Confirm");
            }

            //3. 检查之前重出情况
            var LastFastOrderChange = Entity.FastOrderChange.OrderByDescending(o => o.Id).FirstOrDefault(o => o.TNum == FastOrder.TNum);//最后一次重出
            if(LastFastOrderChange!=null)
            {
                if (LastFastOrderChange.State == 1)
                {
                    ViewBag.ErrorMsg = "重新出款已成功";
                    return View("Error");
                }
                else if (LastFastOrderChange.State != 2)
                {
                    ViewBag.ErrorMsg = "已重新出款,请同步用户结算状态";
                    return View("Error");
                }      
            }

            //4. 创建重出单
            var count = Entity.FastOrderChange.Count(o=>o.TNum == p.TNum);//重出次数
            var FastOrderChange = new FastOrderChange()//新重出单 
            {
                TNum = p.TNum,
                STNum = p.TNum + "_" + (count + 1).ToString(),
                AddTime = DateTime.Now,
                AdminId = AdminUser.Id,
                AdminName = AdminUser.TrueName,
                State = 0,
            };
            //4.1更新成最新的绑卡信息
            p.Card = FastUserPay.Card;
            p.Bin = FastUserPay.Bin;
            p.Bank = FastUserPay.Bank;
            p.CardName = FastUserPay.CardName;
            
            string[] PayConfigArr = FastPayWay.QueryArray.Split(',');
            if (FastPayWay.DllName == "HFPay")
            {
                #region
                var TNum = FastOrderChange.STNum;
                string HFCash_Url = "https://api.zhifujiekou.com/api/qcashgateway";

                //不需要
                string PayPath = ConfigurationManager.AppSettings["PayPath"].ToString();
                string NoticeUrl = PayPath + "/PayCenter/HFCash/FastNotice.html";

                //提交结算中心
                string merId = PayConfigArr[0];//商户号
                string merKey = PayConfigArr[1];//商户密钥

                string orderId = TNum;//商户流水号

                string OrderMoney = (p.PayMoney * 100).ToString("F0");//金额，以分为单

                string UserCardId = FastUser.CardId;
                string PostJson = "{\"action\":\"QCash\",\"txnamt\":\"" + OrderMoney + "\",\"merid\":\"" + merId + "\",\"orderid\":\"" + orderId + "\",\"backurl\":\"" + NoticeUrl + "\",\"bin\":\"" + p.Bin + "\",\"accno\":\"" + p.Card + "\",\"accname\":\"" + p.CardName + "\",\"cardno\":\"" + UserCardId + "\"}";

                //传送数据Base64
                string DataBase64 = LokFuEncode.Base64Encode(PostJson, "utf-8");
                //获得签名
                string Sign = (DataBase64 + merKey).GetMD5();
                //传送数据UrlEnCode
                DataBase64 = HttpUtility.UrlEncode(DataBase64);
                //组装Post数据
                string PostData = string.Format("req={0}&sign={1}", DataBase64, Sign);
                //Post数据，获得结果
                string Ret = Utils.PostRequest(HFCash_Url, PostData, "utf-8");
              
                JObject JS = new JObject();
                try
                {
                    JS = (JObject)JsonConvert.DeserializeObject(Ret);
                }
                catch (Exception)
                {
                    Utils.WriteLog("处理代付[" + p.TNum + "]！" + Ret, "CashPay");
                    JS = null;
                }
                if (JS != null)
                {
                    string resp = JS["resp"].ToString();
                    Ret = LokFuEncode.Base64Decode(resp, "utf-8");
                 
                    try
                    {
                        JS = (JObject)JsonConvert.DeserializeObject(Ret);
                    }
                    catch (Exception)
                    {
                        Utils.WriteLog("处理代付[" + p.TNum + "]！解密出错", "CashPay");
                        JS = null;
                    }
                    if (JS != null)
                    {
                        string respcode = JS["respcode"].ToString();
                        if (respcode == "00")
                        {
                            string resultcode = JS["resultcode"].ToString();
                            if (resultcode == "0000")
                            {
                                p.UserState = 1;
                                FastOrderChange.State = 1;
                            }
                            else if (resultcode == "2002" || resultcode == "2003")
                            {
                                p.UserState = 2;
                                FastOrderChange.State = 2;
                            }
                            else
                            {
                                p.UserState = 3;
                                FastOrderChange.State = 3;
                            }
                            //======================================
                            PayLog PayLog = new PayLog();
                            PayLog.PId = FastPayWay.Id;
                            PayLog.OId = TNum;
                            PayLog.TId = "";
                            PayLog.Amount = 0;
                            PayLog.Way = "FASTDF";
                            PayLog.AddTime = DateTime.Now;
                            PayLog.Data = Ret;
                            PayLog.State = 1;
                            Entity.PayLog.AddObject(PayLog);
                            //======================================
                            FastOrderChange.Data = Ret;
                            Entity.FastOrderChange.AddObject(FastOrderChange);
                            Entity.SaveChanges();
                        }
                        else
                        {
                            string message = JS["respmsg"].ToString();
                            //p.UserState = 0;//提交的时候报错，可重新提交
                            //p.Remark = message;
                            ViewBag.ErrorMsg = message;
                            return View("Error");
                        }
                    }
                    else
                    {
                        ViewBag.ErrorMsg = "接口服务器无响应";
                        return View("Error");
                    }
                }
                #endregion
            }
            if (p.UserState == 1)
            {
                ViewBag.Msg = "订单已成功重新出款";
                return View("Succeed");
            }
            else
            {
                ViewBag.ErrorMsg = "订单重新出款暂未成功,请稍后同步用户结算状态";
                return View("Error");
            }
            
        }

        public ActionResult IndexFastOrderChange(string TNum)
        {
            var FastOrderChangeList = this.Entity.FastOrderChange.Where(o => o.TNum == TNum).OrderByDescending(o => o.Id).ToList();
            ViewBag.FastOrderChangeList = FastOrderChangeList;
            ViewBag.ResubmitSyncUserState = this.checkPower("ResubmitSyncUserState");
            return View();
        }

        /// <summary>
        /// 同步重新出款用户结算状态
        /// </summary>
        public ActionResult ResubmitSyncUserState(string STNum)
        {
            var FastOrderChange = Entity.FastOrderChange.FirstOrDefault(o => o.STNum == STNum);
            if (FastOrderChange==null)
            {
                ViewBag.ErrorMsg = "重新出款单不存在";
                return View("Error");
            }
            if (FastOrderChange.State != 2)
            {
                ViewBag.ErrorMsg = "只有用户结算失败才能使用该功能";
                return View("Error");
            }
            FastOrder FastOrder = Entity.FastOrder.FirstOrDefault(o => o.TNum == FastOrderChange.TNum);
            if (FastOrder == null)
            {
                ViewBag.ErrorMsg = "订单不存在";
                return View("Error");
            }
            string OldUserState = FastOrder.GetUserClearingState();

            FastPayWay FastPayWay = Entity.FastPayWay.FirstOrDefault(n => n.Id == FastOrder.PayWay);
            FastUserPay FastUserPay = Entity.FastUserPay.FirstOrDefault(n => n.PayWay == FastOrder.PayWay && n.UId == FastOrder.UId && n.CardState == 1);
            if (FastPayWay != null)
            {
                string[] PayConfigArr = FastPayWay.QueryArray.Split(',');
                if (FastPayWay.DllName == "HFPay")
                {
                    string TNum = FastOrderChange.STNum;
                    #region
                    string HFCash_Url = "https://api.zhifujiekou.com/api/qcashgateway";

                    //不需要
                    string PayPath = ConfigurationManager.AppSettings["PayPath"].ToString();
                    string NoticeUrl = PayPath + "/PayCenter/HFCash/FastNotice.html";

                    //提交结算中心
                    string merId = PayConfigArr[0];//商户号
                    string merKey = PayConfigArr[1];//商户密钥

                    string orderId = TNum;//商户流水号

                    string OrderMoney = (FastOrder.PayMoney * 100).ToString("F0");//金额，以分为单

                    FastUser FastUser = Entity.FastUser.FirstOrNew(n => n.UId == FastOrder.UId);
                    string UserCardId = FastUser.CardId;
                    string PostJson = "{\"action\":\"QCash\",\"txnamt\":\"" + OrderMoney + "\",\"merid\":\"" + merId + "\",\"orderid\":\"" + orderId + "\",\"backurl\":\"" + NoticeUrl + "\",\"bin\":\"" + FastOrder.Bin + "\",\"accno\":\"" + FastOrder.Card + "\",\"accname\":\"" + FastOrder.CardName + "\",\"cardno\":\"" + UserCardId + "\"}";

                    //传送数据Base64
                    string DataBase64 = LokFuEncode.Base64Encode(PostJson, "utf-8");
                    //获得签名
                    string Sign = (DataBase64 + merKey).GetMD5();
                    //传送数据UrlEnCode
                    DataBase64 = HttpUtility.UrlEncode(DataBase64);
                    //组装Post数据
                    string PostData = string.Format("req={0}&sign={1}", DataBase64, Sign);
                    //Post数据，获得结果
                    string Ret = Utils.PostRequest(HFCash_Url, PostData, "utf-8");

                    JObject JS = new JObject();
                    try
                    {
                        JS = (JObject)JsonConvert.DeserializeObject(Ret);
                    }
                    catch (Exception)
                    {
                        Utils.WriteLog("处理代付[" + FastOrder.TNum + "]！" + Ret, "CashPay");
                        JS = null;
                    }
                    if (JS != null)
                    {
                        string resp = JS["resp"].ToString();
                        Ret = LokFuEncode.Base64Decode(resp, "utf-8");

                        try
                        {
                            JS = (JObject)JsonConvert.DeserializeObject(Ret);
                        }
                        catch (Exception Ex)
                        {
                            FastOrder.UserState = 0;//提交的时候报错，可重新提交
                            JS = null;
                            Utils.WriteLog("[CashPay]:JSON[" + Ret + "]" + Ex.ToString(), "FastOrder");
                            ViewBag.ErrorMsg = "返回数据错误";
                            return View("Error");
                        }
                        if (JS != null)
                        {
                            string respcode = JS["respcode"].ToString();
                            if (respcode == "00")
                            {
                                string resultcode = JS["resultcode"].ToString();
                                if (resultcode == "0000")
                                {
                                    FastOrder.UserState = 1;
                                    FastOrderChange.State = 1;
                                }
                                else if (resultcode == "2002" || resultcode == "2003")
                                {
                                    FastOrder.UserState = 2;
                                    FastOrderChange.State = 2;
                                }
                                else
                                {
                                    FastOrder.UserState = 3;
                                    FastOrderChange.State = 3;
                                }
                                //======================================
                                PayLog PayLog = new PayLog();
                                PayLog.PId = FastPayWay.Id;
                                PayLog.OId = TNum;
                                PayLog.TId = "";
                                PayLog.Amount = 0;
                                PayLog.Way = "FASTDF";
                                PayLog.AddTime = DateTime.Now;
                                PayLog.Data = Ret;
                                PayLog.State = 1;
                                Entity.PayLog.AddObject(PayLog);
                                //======================================
                                FastOrderChange.Data = Ret;
                                Entity.FastOrderChange.AddObject(FastOrderChange);
                                Entity.SaveChanges();
                            }
                           
                        }
                        
                    }
                    #endregion
                }
            }
            if (FastOrder.UserState == 1)
            {
                ViewBag.Msg = "订单已成功重新出款";
                return View("Succeed");
            }
            else
            {
                string NewUserState = FastOrder.GetUserClearingState();
                ViewBag.ErrorMsg = "用户结算状态从" + OldUserState + "->" + NewUserState;
                return View("Error");
            }
            
        }

        /// <summary>
        /// 上传
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public JsonResult EditUpLoadFastOrder()
        {
            JsonResult json = new JsonResult();
            try
            {
                //上传
                var file = HttpContext.Request.Files.Get("UpLoadFile");
                var savePath = "/UpLoadFiles/FastOrder/";
                var param = new UpLoadFileParam(file, savePath, false);
                param.AllowType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                json.Data = UpLoadFileHelpOld.UpLoadFileExcel(param);
                string filename = param.SaveFileName;
                //获取数据
                string path = Server.MapPath("//UpLoadFiles/FastOrder") + "\\" + filename;
                ExcelPackage package = new ExcelPackage(new FileInfo(path), true);
                var sheet = package.Workbook.Worksheets[1];
                int rows = sheet.Dimension.End.Row;
                string TNums = "";
                for (int i = 2; i < rows+1; i++)
                {
                    if (TNums.IsNullOrEmpty())
                    {
                        if (sheet.Cells[i, 1].Value != null)
                        {
                            TNums = "'" + sheet.Cells[i, 1].Value.ToString().Replace(" ", "") + "'";
                        }
                    }
                    else
                    {
                        if (sheet.Cells[i, 1].Value != null)
                        {
                            TNums = TNums + "," + "'" + sheet.Cells[i, 1].Value.ToString().Replace(" ", "") + "'";
                        }
                    }
                }

                string sql = "UPDATE FastOrder SET FastOrder.AgentState = 1,FastOrder.AgentTime = getdate() Where FastOrder.AgentState != 1 AND FastOrder.TNum In(" + TNums + ");";
                this.Entity.ExecuteStoreCommand(sql);
                return json;
            }
            catch {
                json = new JsonResult();
                return json;
            }
        }

        /// <summary>
        /// 补单
        /// </summary>
        /// <returns></returns>
        public ActionResult OrdersRepair(FastOrder FastOrder)
        {
            if (!FastOrder.Id.IsNullOrEmpty())
            {
                FastOrder = Entity.FastOrder.FirstOrDefault(n => n.Id == FastOrder.Id);
            }
            if (FastOrder == null)
            {
                ViewBag.ErrorMsg = "订单不存在";
                return View("Error");
            }
            if (FastOrder.PayState != 0)
            {
                ViewBag.ErrorMsg = "不符合补单规则";
                return View("Error");
            }
            if (FastOrder.State == 0) {
                FastOrder.State = 1;
                Entity.SaveChanges();
            }
            if (this.Request.QueryString["Confirm"] == "true")
            {
                OrdersPayOnly ConfirmOrdersPayOnly = Entity.OrdersPayOnly.Where(o => o.TNum == FastOrder.TNum).FirstOrDefault();
                Entity.OrdersPayOnly.DeleteObject(ConfirmOrdersPayOnly);
                Entity.SaveChanges();
            }
            OrdersPayOnly OrdersPayOnly = Entity.OrdersPayOnly.Where(o => o.TNum == FastOrder.TNum).FirstOrDefault();
            if (OrdersPayOnly != null)
            {
                ViewBag.Msg = "入账流程未完成，是否先结束流程？";
                return View("Confirm");
            }
            FastPayWay FastPayWay = Entity.FastPayWay.FirstOrNew(n => n.Id == FastOrder.PayWay);
            if (FastPayWay.State.IsNullOrEmpty())
            {
                ViewBag.ErrorMsg = "支付通道已关闭";
                return View("Error");
            }
            //补单查询
            Users Users = Entity.Users.FirstOrDefault(o => o.Id == FastOrder.UId);
            Equipment Equipment = Entity.Equipment.FirstOrDefault(e => e.No == Users.ENo);
            if (Equipment == null)
            {
                ViewBag.ErrorMsg = "支付通道已关闭";
                return View("Error");
            }
            string keys = Equipment.Keys;
            string dataJosn = "{\"token\":\"" + Users.Token + "\",\"tnum\":\"" + FastOrder.TNum + "\"}";
            string data_data = HttpUtility.UrlEncode(LokFuEncode.LokFuAPIEncode(dataJosn, keys));
            string Url = ConfigurationManager.AppSettings["ApiPath"].ToString() + "/API/FastOrderQuery/";
            string PostData = string.Format("data={0}&eno={1}&code=0000", data_data, Users.ENo);
            string result = Utils.PostRequest(Url, PostData, "utf-8");
            JObject JS = new JObject();
            try
            {
                JS = (JObject)JsonConvert.DeserializeObject(result);
                string statename = "";
                //返回成功
                if (JS["code"].ToString() == "0000")
                {
                    string ret_data = JS["data"].ToString();
                    result = LokFuEncode.LokFuAPIDecode(ret_data, keys);
                    JS = (JObject)JsonConvert.DeserializeObject(result);

                    if (JS["state"].ToString() == "0" || JS["state"].ToString() == "1")
                    {

                        if (JS["state"].ToString() == "0")
                        {
                            statename = "取消交易";
                        }
                        else
                        {
                            statename = "待支付";
                        }
                        ViewBag.ErrorMsg = "补单失败!状态:" + statename;
                        return View("Error");
                    }
                    else
                    {
                        if (JS["state"].ToString() == "2")
                        {
                            statename = "待结算";
                        }
                        else
                        {
                            statename = "已结算";
                        }

                        ViewBag.Msg = "补单成功!状态:" + statename;
                        return View("Succeed");
                    }
                }
                else
                {
                    ViewBag.ErrorMsg = JS["msg"].ToString();
                    return View("Error");
                }
            }
            catch (Exception)
            {
                ViewBag.ErrorMsg = "补单失败!";
                return View("Error");
            }


        }

        public ActionResult IndexOrderProfitLog(string tnum)
        {
            var OrderProfitLog = this.Entity.OrderProfitLog.Where(o => o.TNum == tnum).OrderByDescending(o => o.Id).ToList();
            ViewBag.OrderProfitLog = OrderProfitLog;

            var UIds = OrderProfitLog.Select(o => o.UId).ToList();
            var UserNameList = this.Entity.Users.Where(o => UIds.Contains(o.Id)).ToDictionary(o => o.Id, o => o.TrueName);
            ViewBag.UserNameList = UserNameList;

            var Agents = OrderProfitLog.Select(o => o.Agent).ToList();
            var AgentsList = this.Entity.SysAgent.Where(o => Agents.Contains(o.Id)).ToDictionary(o => o.Id, o => o.Name);
            ViewBag.AgentsList = AgentsList;

            return View();
        }

        
    }
}


