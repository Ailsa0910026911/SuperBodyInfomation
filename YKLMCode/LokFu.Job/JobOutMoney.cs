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
using System.Threading;

namespace GoodPayJobs
{
    /// <summary>
    /// 批量出款任务
    /// </summary>
    public class JobOutMoney : IJob
    {
        public static bool IsRun = false;
        public void Execute(IJobExecutionContext context)
        {
            string JobName = "OutMoney";
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
                        #region 全局变量
                        SysSet BasicSet = Entity.SysSet.FirstOrDefault();

                        string NoticePath = ConfigurationManager.AppSettings["NoticePath"].ToString();
                        string NoticeUrl = NoticePath + "/PayCenter/HFCash/Notice.html";
                        //提交结算中心
                        string merId = ConfigurationManager.AppSettings["Cash_merId"].ToString();//商户号
                        string merKey = ConfigurationManager.AppSettings["Cash_merKey"].ToString();//商户密钥
                        #endregion
                        //0=无效,1=待执行,2=执行中,3=完成
                        IList<TaskCash> TaskCashList = Entity.TaskCash.Where(n => n.State == 1 || n.State == 2).OrderBy(n => n.Id).ToList();
                        //读取待执行
                        foreach (var P in TaskCashList)
                        {
                            //任务状态设置为执行中
                            if (P.State == 1)
                            {
                                P.State = 2;
                                P.STime = DateTime.Now;
                                Entity.SaveChanges();
                            }
                            IList<TaskCashInfo> TaskCashInfoList = Entity.TaskCashInfo.Where(n => (n.State == 1 || n.State == 2) && n.TId == P.Id).OrderBy(n => n.Id).ToList();
                            foreach (var p in TaskCashInfoList)
                            {
                                if (p.State == 1)
                                {
                                    p.State = 2;
                                    p.STime = DateTime.Now;
                                    Entity.SaveChanges();
                                }
                                OrderCash OC = Entity.OrderCash.FirstOrDefault(n => n.OId == p.OId);
                                bool CanPay = true;
                                if (OC == null)
                                {
                                    CanPay = false;
                                }
                                if (OC.FState == 1)//已付过款
                                {
                                    CanPay = false;
                                }
                                if (OC.OrderState != 2 || OC.PayState != 1)
                                {
                                    CanPay = false;
                                }
                                if (CanPay)//开启自动结算时执行
                                {
                                    if (BasicSet.CashPayWay == 0)
                                    {
                                        p.State = 3;//标识成功
                                        p.OState = 2;
                                        p.NState = 1;
                                        p.Remark = "批量人工结算";
                                        p.ETime = DateTime.Now;

                                        OC.PayState = 2;
                                        OC.FState = 1;
                                        OC.FTime = DateTime.Now;
                                        Orders O = Entity.Orders.FirstOrDefault(n => n.TNum == p.OId);
                                        if (O != null)
                                        {
                                            O.PayState = 2;
                                            O.InternalRm = "批量人工结算";
                                        }
                                        Entity.SaveChanges();
                                        //======分润======
                                        OC = OC.PayAgent(Entity, 1);
                                        if (O != null)
                                        {
                                            O.AgentPayGet = (decimal)OC.AgentCashGet;
                                        }
                                        Entity.SaveChanges();
                                    }
                                    else if (BasicSet.CashPayWay == 1)
                                    {
                                        #region 提交接口
                                        string orderId = OC.OId;//商户流水号
                                        decimal PayMoney = OC.Amoney - (decimal)OC.UserRate;
                                        decimal money = PayMoney * 100;
                                        long intmoney = Int64.Parse(money.ToString("F0"));
                                        string OrderMoney = intmoney.ToString();//金额，以分为单


                                        string UserCardId = Entity.Users.FirstOrNew(n => n.Id == OC.UId).CardId;
                                        string PostJson = "{\"action\":\"QCash\",\"txnamt\":\"" + OrderMoney + "\",\"merid\":\"" + merId + "\",\"orderid\":\"" + orderId + "\",\"backurl\":\"" + NoticeUrl + "\",\"bin\":\"" + OC.Bin + "\",\"accno\":\"" + OC.CardNum + "\",\"accname\":\"" + OC.Owner + "\",\"cardno\":\"" + UserCardId + "\"}";

                                        //传送数据Base64
                                        string DataBase64 = LokFuEncode.Base64Encode(PostJson, "utf-8");

                                        //获得签名
                                        string Sign = (DataBase64 + merKey).GetMD5();

                                        //传送数据UrlEnCode
                                        DataBase64 = System.Web.HttpUtility.UrlEncode(DataBase64);

                                        //组装Post数据
                                        string PostData = string.Format("req={0}&sign={1}", DataBase64, Sign);

                                        string HFNFC_Url = "https://api.zhifujiekou.com/api/qcashgateway";

                                        //Post数据，获得结果
                                        string Ret = Utils.PostRequest(HFNFC_Url, PostData, "utf-8");
                                        string runType = "PayOk";
                                        JObject JS = new JObject();
                                        try
                                        {
                                            JS = (JObject)JsonConvert.DeserializeObject(Ret);
                                        }
                                        catch (Exception)
                                        {
                                            Utils.WriteLog("[" + OC.OId + "]" + Ret, "PayCashCenterErr");
                                            runType = "PayIng";
                                        }
                                        string Remark = "";
                                        if (runType == "PayOk")
                                        {
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
                                                    runType = "PayIng";
                                                }
                                                if (runType == "PayOk")
                                                {
                                                    if (JS != null)
                                                    {
                                                        string respcode = JS["respcode"].ToString();
                                                        if (respcode != "00")
                                                        {
                                                            if (respcode == "45")
                                                            {
                                                                //限额了，需要特别处理
                                                                runType = "PayErr";
                                                            }
                                                            else
                                                            {
                                                                runType = "PayIng";
                                                            }
                                                            Remark = JS["respmsg"].ToString();
                                                        }
                                                        else
                                                        {
                                                            string resultcode = JS["resultcode"].ToString();
                                                            if (resultcode == "0000")
                                                            {
                                                                runType = "PayOk";
                                                            }
                                                            else if (resultcode == "2002" || resultcode == "2003")
                                                            {
                                                                runType = "PayErr";
                                                                Remark = JS["resultmsg"].ToString();
                                                            }
                                                            else
                                                            {
                                                                runType = "PayIng";
                                                                Remark = JS["resultmsg"].ToString();
                                                            }
                                                        }
                                                    }
                                                    else
                                                    {
                                                        runType = "PayIng";
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                runType = "PayIng";
                                            }
                                        }

                                        if (runType == "PayIng")
                                        {
                                            //处理中
                                            p.ETime = DateTime.Now;
                                            p.State = 5;//标识 未知状态
                                            Entity.SaveChanges();
                                        }
                                        if (runType == "PayErr")
                                        {
                                            OC.PayState = 3;
                                            OC.Remark = Remark;
                                            p.State = 4;//标识失败
                                            p.OState = 2;
                                            p.NState = 1;
                                            p.Remark = Remark;
                                            p.ETime = DateTime.Now;
                                            Orders O = Entity.Orders.FirstOrDefault(n => n.TNum == p.OId);
                                            if (O != null)
                                            {
                                                O.PayState = 3;
                                                O.Remark = Remark;
                                            }
                                            Entity.SaveChanges();
                                        }
                                        if (runType == "PayOk")
                                        {
                                            OC.PayState = 2;
                                            OC.FState = 1;
                                            OC.FTime = DateTime.Now;
                                            p.State = 3;//标识成功
                                            p.OState = 2;
                                            p.NState = 1;
                                            p.ETime = DateTime.Now;
                                            Entity.SaveChanges();
                                            OC = OC.PayAgent(Entity, 1);//======分润======
                                            Orders O = Entity.Orders.FirstOrDefault(n => n.TNum == p.OId);
                                            if (O != null)
                                            {
                                                O.PayState = 2;
                                                O.AgentPayGet = (decimal)OC.AgentCashGet;
                                                Entity.SaveChanges();
                                            }
                                        }
                                        Log.WriteLog("执行提现:" + p.OId, JobName);
                                        Thread.Sleep(1000);
                                        #endregion
                                    }
                                }
                                else
                                {
                                    p.Remark = "订单状态不符，需查单！";
                                    p.ETime = DateTime.Now;
                                    p.State = 5;//标识 未知状态
                                    Entity.SaveChanges();
                                }
                            }
                            //当前任务所有子项执行完成
                            int state1 = Entity.TaskCashInfo.Count(n => n.State == 1 && n.TId == P.Id);
                            int state2 = Entity.TaskCashInfo.Count(n => n.State == 2 && n.TId == P.Id);
                            int state3 = Entity.TaskCashInfo.Count(n => n.State == 3 && n.TId == P.Id);
                            int state4 = Entity.TaskCashInfo.Count(n => n.State == 4 && n.TId == P.Id);
                            int state5 = Entity.TaskCashInfo.Count(n => n.State == 5 && n.TId == P.Id);
                            if (state1 == 0 && state2 == 0)
                            {
                                P.State = 3;
                                P.ETime = DateTime.Now;
                            }
                            P.Success = state3 + state5;
                            P.Fail = state4;
                            Entity.SaveChanges();
                            Log.WriteLog("TaskCashInfo任务执行完毕！共计" + TaskCashInfoList.Count + "笔交易", JobName);
                        }
                        Log.WriteLog("TaskCash任务执行完毕！共计" + TaskCashList.Count + "个任务", JobName);
                        #endregion
                        //-------------------------------------------------------
                        Log.Write(JobName + "任务执行结算！");
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
