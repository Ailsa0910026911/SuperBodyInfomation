using LokFu;
using LokFu.Extensions;
using LokFu.HFJS;
using LokFu.HFJS.HFJSModels;
using LokFu.HFJS.HFJSResults;
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
    /// 收付直通车——出款查询
    /// </summary>
    public class JobFastQuery : IJob
    {
        public static bool IsRun = false;
        public void Execute(IJobExecutionContext context)
        {
            string JobName = "FastQuery";
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
                        DateTime STime = DateTime.Now.AddDays(-1);
                        DateTime ETime = DateTime.Now.AddSeconds(-30);
                        IList<FastOrder> List = Entity.FastOrder.Where(n => n.State == 1 && n.PayState == 1 && n.UserState == 3 && n.UserTime < ETime && n.UserTime > STime).ToList();
                        //UserState 0未付 1已付 2失败 3结果未明 4付起中
                        foreach (var p in List)
                        {
                            FastPayWay FastPayWay = Entity.FastPayWay.FirstOrDefault(n => n.Id == p.PayWay && n.State == 1);
                            FastUserPay FastUserPay = Entity.FastUserPay.FirstOrDefault(n => n.PayWay == p.PayWay && n.UId == p.UId);
                            if (FastPayWay != null)
                            {
                                string[] PayConfigArr = FastPayWay.QueryArray.Split(',');
                                if (FastPayWay.DllName == "HFPay")
                                {
                                    #region 查代付结果
                                    if (PayConfigArr.Length == 3)
                                    {
                                        string HF_Url = "https://api.zhifujiekou.com/api/qcashquery";
                                        string MerId = PayConfigArr[0];
                                        string MerKey = PayConfigArr[1];
                                        string orderId = p.TNum;//商户流水号
                                        string PostJson = "{\"merid\":\"" + MerId + "\",\"orderid\":\"" + orderId + "\"}";
                                        string DataBase64 = LokFuEncode.Base64Encode(PostJson, "utf-8");
                                        string Sign = (DataBase64 + MerKey).GetMD5();
                                        DataBase64 = HttpUtility.UrlEncode(DataBase64);
                                        string PostData = string.Format("req={0}&sign={1}", DataBase64, Sign);
                                        string Ret = Utils.PostRequest(HF_Url, PostData, "utf-8");
                                        JObject JS = new JObject();
                                        try
                                        {
                                            JS = (JObject)JsonConvert.DeserializeObject(Ret);
                                        }
                                        catch (Exception)
                                        {
                                            JS = null;
                                        }
                                        if (JS != null)
                                        {
                                            if (JS["resp"] != null)
                                            {
                                                string resp = JS["resp"].ToString();
                                                Ret = LokFuEncode.Base64Decode(resp, "utf-8");
                                                try
                                                {
                                                    JS = (JObject)JsonConvert.DeserializeObject(Ret);
                                                }
                                                catch (Exception)
                                                {
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

                                                        }
                                                        Entity.SaveChanges();
                                                    }
                                                }
                                            }
                                        }
                                        //================================================
                                        PayLog PayLog = new PayLog();
                                        PayLog.PId = FastPayWay.Id;
                                        PayLog.OId = p.TNum;
                                        PayLog.TId = "";
                                        PayLog.Amount = 0;
                                        PayLog.Way = "FASTDFQ";
                                        PayLog.AddTime = DateTime.Now;
                                        PayLog.Data = Ret;
                                        PayLog.State = 1;
                                        Entity.PayLog.AddObject(PayLog);
                                        //================================================
                                        Entity.SaveChanges();
                                    }
                                    #endregion
                                }
                                if (FastPayWay.DllName == "HFJSPay")
                                {
                                    #region 结算系统
                                    //不需要
                                    fastordersqueryModel fastordersqueryModel = new fastordersqueryModel()
                                    {
                                        merid = FastUserPay.MerId,
                                        orderid = "",
                                        queryid = p.TNum
                                    };
                                    fastordersqueryResult fastordersqueryResult = HFJSTools.fastordersquery(fastordersqueryModel, FastUserPay.MerKey);
                                    //================================================
                                    //记录通知信息
                                    PayLog PayLog = new PayLog();
                                    PayLog.PId = p.PayWay.Value;
                                    PayLog.OId = p.TNum;
                                    PayLog.TId = fastordersqueryResult.queryid;
                                    PayLog.Amount = p.Amoney;
                                    PayLog.Way = "FASTDFQ";
                                    PayLog.AddTime = DateTime.Now;
                                    PayLog.Data = HFJSTools.MyObjectToJson(fastordersqueryResult);
                                    PayLog.State = 1;
                                    Entity.PayLog.AddObject(PayLog);
                                    Entity.SaveChanges();
                                    //================================================
                                    if (fastordersqueryResult.respcode == "00")
                                    {
                                        if (fastordersqueryResult.resultcode == "0000")
                                        {
                                            p.UserState = 1;
                                            Entity.SaveChanges();
                                        }
                                        if (fastordersqueryResult.resultcode == "1003")
                                        {
                                            p.UserState = 2;
                                            Entity.SaveChanges();
                                        }
                                    }
                                    else
                                    {
                                        string resp_desc = fastordersqueryResult.respmsg;
                                        Utils.WriteLog("HFJS[Query][" + p.TNum + "]:" + resp_desc, "JobHFJS");
                                    }
                                    #endregion
                                }
                            }
                            Log.WriteLog("查询代付[" + p.TNum + "]！", JobName);
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
