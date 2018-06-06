using LokFu.Extensions;
using LokFu.FastPay;
using LokFu.HFJS;
using LokFu.HFJS.HFJSModels;
using LokFu.HFJS.HFJSResults;
using LokFu.Infrastructure;
using LokFu.Repositories;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.Script.Serialization;

namespace LokFu.Controllers
{
    public class FastOrderQueryController : InitController
    {
        public FastOrderQueryController()
        {
            if (!InitState)
            {
                DataObj.OutError("8080");
                return;
            }
            if (DataObj == null)
            {
                DataObj.OutError("1000");
                return;
            }
            if (!DataObj.IsReg)
            {
                DataObj.OutError("3002");
                return;
            }
        }
        public void Post()
        {
            string Data = DataObj.GetData();
            if (Data.IsNullOrEmpty())
            {
                DataObj.OutError("1000");
                return;
            }
            JObject json = new JObject();
            try
            {
                json = (JObject)JsonConvert.DeserializeObject(Data);
            }
            catch (Exception Ex)
            {
                Log.Write("[FastOrderQueryController]:", "【Data】" + Data, Ex);
                json = null;
            }
            if (json == null)
            {
                DataObj.OutError("1000");
                return;
            }

            FastOrder InFastOrder = new FastOrder();
            InFastOrder = JsonToObject.ConvertJsonToModel(InFastOrder, json);
            if (InFastOrder.TNum.IsNullOrEmpty() || InFastOrder.Token.IsNullOrEmpty())
            {
                DataObj.OutError("1000");
                return;
            }

            Users baseUsers = Entity.Users.FirstOrDefault(n => n.Token == InFastOrder.Token);
            if (baseUsers == null)//用户令牌不存在
            {
                DataObj.OutError("2004");
                return;
            }
            if (baseUsers.State != 1)//用户被锁定
            {
                DataObj.OutError("2003");
                return;
            }
            if (baseUsers.CardStae != 2)//未实名认证
            {
                DataObj.OutError("2006");
                return;
            }

            FastOrder FastOrder = Entity.FastOrder.FirstOrDefault(n => n.TNum == InFastOrder.TNum && n.UId == baseUsers.Id);
            if (FastOrder == null)
            {
                DataObj.OutError("1000");
                return;
            }
            if (FastOrder.State == 1 && FastOrder.PayState == 0)
            {
                FastPayWay FastPayWay = Entity.FastPayWay.FirstOrNew(n => n.Id == FastOrder.PayWay);
                if (FastPayWay == null)
                {
                    DataObj.OutError("2079");
                    return;
                }
                string[] PayConfigArr = FastPayWay.QueryArray.Split(new char[] { ',' });//接口信息
                if (FastPayWay.DllName == "HFPay")
                {
                    #region 结算中心
                    if (PayConfigArr.Length == 3)
                    {
                        string HF_Url = "https://api.zhifujiekou.com/api/query";
                        string MerId = PayConfigArr[0];
                        string MerKey = PayConfigArr[1];
                        string orderId = FastOrder.TNum;//商户流水号
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
                                        if (resultcode == "0000" || resultcode == "1002" || resultcode == "1004")
                                        {
                                            string queryid = JS["queryid"].ToString();
                                            FastOrder.Trade = queryid;
                                            Entity.SaveChanges();
                                            string txnamt = JS["txnamt"].ToString();
                                            int factmoney = int.Parse(txnamt);
                                            if (((int)(FastOrder.Amoney * 100)) == factmoney)
                                            {
                                                FastOrder = FastOrder.PaySuccess(Entity);
                                            }
                                        }
                                    }
                                    //================================================
                                    //这里记录日志
                                    PayLog PayLog = new PayLog();
                                    PayLog.PId = (int)FastOrder.PayWay;
                                    PayLog.OId = FastOrder.TNum;
                                    PayLog.TId = FastOrder.Trade;
                                    PayLog.Amount = FastOrder.Amoney;
                                    PayLog.Way = "Query";
                                    PayLog.AddTime = DateTime.Now;
                                    PayLog.Data = Ret;
                                    PayLog.State = 1;
                                    Entity.PayLog.AddObject(PayLog);
                                    Entity.SaveChanges();
                                    //================================================
                                }
                            }
                        }
                    }
                    #endregion
                }
                if (FastPayWay.DllName == "HFJSPay")
                {
                    #region 结算中心
                    if (PayConfigArr.Length == 3)
                    {
                        FastUserPay FastUserPay = Entity.FastUserPay.FirstOrDefault(n => n.PayWay == FastOrder.PayWay && n.UId == baseUsers.Id && n.MerState == 1);
                        fastordersqueryModel fastordersqueryModel = new fastordersqueryModel()
                        {
                            merid = FastUserPay.MerId,
                            orderid = "",
                            queryid = FastOrder.TNum
                        };
                        fastordersqueryResult fastordersqueryResult = HFJSTools.fastordersquery(fastordersqueryModel, FastUserPay.MerKey);
                        //================================================
                        //记录通知信息
                        PayLog PayLog = new PayLog();
                        PayLog.PId = FastOrder.PayWay.Value;
                        PayLog.OId = FastOrder.TNum;
                        PayLog.TId = fastordersqueryResult.queryid;
                        PayLog.Amount = FastOrder.Amoney;
                        PayLog.Way = "Query";
                        PayLog.AddTime = DateTime.Now;
                        PayLog.Data = HFJSTools.MyObjectToJson(fastordersqueryResult);
                        PayLog.State = 1;
                        Entity.PayLog.AddObject(PayLog);
                        Entity.SaveChanges();
                        //================================================
                        if (fastordersqueryResult.respcode == "00")
                        {
                            if (fastordersqueryResult.resultcode == "0000" || fastordersqueryResult.resultcode == "1002")
                            {
                                FastOrder.Trade = fastordersqueryResult.queryid;
                                Entity.SaveChanges();
                                FastOrder = FastOrder.PaySuccess(Entity);
                                if (FastOrder.PayState == 1)
                                {
                                    if (fastordersqueryResult.resultcode == "0000") {
                                        FastOrder.UserState = 1;
                                    }
                                    if (fastordersqueryResult.resultcode == "1002")
                                    {
                                        FastOrder.UserState = 3;
                                    }
                                    FastOrder.UserTime = DateTime.Now;
                                    Entity.SaveChanges();
                                }
                            }
                            else if (fastordersqueryResult.resultcode == "1004")
                            {
                                FastOrder.State = 0;
                                Entity.SaveChanges();
                            }
                            else
                            {

                            }
                            Entity.SaveChanges();
                        }
                        else
                        {
                            string resp_desc = fastordersqueryResult.respmsg;
                            Utils.WriteLog("HFJS[Query][" + FastOrder.TNum + "]:" + resp_desc, "JobHFJS");
                        }
                    }
                    #endregion
                }
            }
            if (FastOrder.State == 1)
            {
                if (FastOrder.PayState == 1)
                {
                    if (FastOrder.UserState == 1)
                    {
                        FastOrder.State = 3;
                    }
                    else
                    {
                        FastOrder.State = 2;
                    }
                }
                else
                {
                    FastOrder.State = 1;
                }
            }
            else
            {
                FastOrder.State = 0;
            }
            FastOrder.Cols = "TNum,PayId,Amoney,Poundage,State";
            DataObj.Data = FastOrder.OutJson();
            DataObj.Code = "0000";
            DataObj.OutString();
        }
    }
}