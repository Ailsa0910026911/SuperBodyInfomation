using LokFu;
using LokFu.Extensions;
using LokFu.Infrastructure;
using LokFu.Repositories;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Quartz;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;

namespace GoodPayJobs
{
    /// <summary>
    /// 收付直通车——出款
    /// </summary>
    public class JobFastCash : IJob
    {
        public static bool IsRun = false;
        public void Execute(IJobExecutionContext context)
        {
            string JobName = "FastCash";
            string CanRun = ConfigurationManager.AppSettings["Run" + JobName].ToString();
            if (CanRun == "true")
            {
                if (!IsRun)
                {
                    LokFuEntity Entity = new LokFuEntity();
                    IsRun = true;
                    try
                    {
                        Log.Write(JobName + "任务开始执行！");
                        //-------------------------------------------------------
                        #region 任务主体
                        //查询所有已支付但未代付订单
                        DateTime CanPayTime = DateTime.Now.AddSeconds(-300);//20秒前支付订单可付
                        IList<FastOrder> List = Entity.FastOrder.Where(n => n.State == 1 && n.PayState == 1 && n.UserState == 0 && n.PayTime < CanPayTime).ToList();
                        //UserState 0未付 1已付 2失败 3结果未明 4付起中
                        //foreach (var p in List.Where(n=>n.PayWay==16 && n.))
                        //List = List.Where(n => n.PayWay != 16 || (n.PayWay == 16 && n.PayTime < CanPayTime2)).ToList();
                        foreach (var p in List)
                        {
                            p.UserTime = DateTime.Now;
                            p.UserState = 4;
                        }
                        Entity.SaveChanges();
                        foreach (var p in List)
                        {
                            Users Users = Entity.Users.FirstOrDefault(n => n.Id == p.UId);
                            if (Users.StopPayState == 0)
                            {
                                FastPayWay FastPayWay = Entity.FastPayWay.FirstOrDefault(n => n.Id == p.PayWay && n.State == 1);
                                if (FastPayWay != null)
                                {
                                    string[] PayConfigArr = FastPayWay.QueryArray.Split(',');
                                    if (FastPayWay.DllName == "HFPay")
                                    {
                                        #region 结算中心代付

                                        string HFCash_Url = "https://api.zhifujiekou.com/api/qcashgateway";

                                        //不需要
                                        string NoticePath = ConfigurationManager.AppSettings["NoticePath"].ToString();
                                        string NoticeUrl = NoticePath + "/PayCenter/HFCash/FastNotice.html";

                                        //提交结算中心
                                        string merId = PayConfigArr[0];//商户号
                                        string merKey = PayConfigArr[1];//商户密钥

                                        string orderId = p.TNum;//商户流水号

                                        string OrderMoney = (p.PayMoney * 100).ToString("F0");//金额，以分为单

                                        string UserCardId = Users.CardId;
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
                                                    }
                                                    else if (resultcode == "2002" || resultcode == "2003")
                                                    {
                                                        p.UserState = 2;
                                                    }
                                                    else
                                                    {
                                                        p.UserState = 3;
                                                    }
                                                }
                                                else
                                                {
                                                    string respmsg = JS["respmsg"].ToString();
                                                    Utils.WriteLog("处理代付[" + p.TNum + "]！" + respmsg, "CashPay");
                                                }
                                            }
                                        }
                                        //======================================
                                        PayLog PayLog = new PayLog();
                                        PayLog.PId = FastPayWay.Id;
                                        PayLog.OId = p.TNum;
                                        PayLog.TId = "";
                                        PayLog.Amount = 0;
                                        PayLog.Way = "FASTDF";
                                        PayLog.AddTime = DateTime.Now;
                                        PayLog.Data = Ret;
                                        PayLog.State = 1;
                                        Entity.PayLog.AddObject(PayLog);
                                        //======================================
                                        Entity.SaveChanges();
                                        #endregion
                                    }
                                    if (FastPayWay.DllName == "HFJSPay")
                                    {
                                        #region 结算系统
                                        //不需要
                                        #endregion
                                    }
                                }
                                Log.WriteLog("处理代付[" + p.TNum + "]！", JobName);
                            }
                            else
                            {
                                Log.WriteLog("处理代付[" + p.TNum + "]！商户止付", JobName);
                            }
                        }
                        #endregion
                        //-------------------------------------------------------
                        Log.Write(JobName + "任务执行结束！[共计" + List.Count + "条]");
                    }
                    catch (Exception Ex)
                    {
                        Log.Write(JobName + "任务执行过程出错！", Ex);
                    }
                    IsRun = false;
                }
                else {
                    Log.Write(JobName + "任务还在执行中！");
                }
            }
        }
    }
}
