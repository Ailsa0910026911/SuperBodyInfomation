using LokFu.Extensions;
using LokFu.Infrastructure;
using LokFu.Repositories;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Objects;
using System.Linq;
using System.Text;
using System.Web;
using System.Xml;

namespace LokFu.Controllers
{
    public class IPayQueryController : InitController
    {
        public IPayQueryController()
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
                Log.Write("[IPayQuery]:", "【Data】" + Data, Ex);
                json = null;
            }
            if (json == null)
            {
                DataObj.OutError("1000");
                return;
            }
            Orders Orders = new Orders();
            Orders = JsonToObject.ConvertJsonToModel(Orders, json);
            if (Orders.TNum.IsNullOrEmpty() || Orders.Token.IsNullOrEmpty())
            {
                DataObj.OutError("1000");
                return;
            }
            #region 处理用户信息
            Users baseUsers = Entity.Users.FirstOrDefault(n => n.Token == Orders.Token);
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
            #endregion
            Orders BaseOrders = Entity.Orders.FirstOrDefault(n => n.TNum == Orders.TNum);
            if (BaseOrders == null)
            {
                DataObj.OutError("1001");
                return;
            }
            if (baseUsers.Id != BaseOrders.UId)//禁止代付
            {
                DataObj.OutError("6021");
                return;
            }
            if (BaseOrders.TType != 6 && BaseOrders.TType != 10)
            {
                DataObj.OutError("6022");
                return;
            }
            if (BaseOrders.PayState != 1)
            {
                PayConfig PayConfig = Entity.PayConfig.FirstOrNew(n => n.Id == BaseOrders.PayWay);
                //开始处理参数
                string DataRet = "", Trade = "";
                if (PayConfig.DllName == "HFAliPay" || PayConfig.DllName == "HFWeiXin")
                {
                    #region 好付处理
                    string[] QueryArr = PayConfig.QueryArray.Split(',');
                    if (QueryArr.Length == 3)
                    {
                        //提交结算中心
                        string merId = QueryArr[0];//商户号
                        string merKey = QueryArr[1];//商户密钥
                        string orderId = BaseOrders.TNum;//商户流水号
                        string PostJson = "{\"merid\":\"" + merId + "\",\"orderid\":\"" + orderId + "\"}";
                        string DataBase64 = LokFuEncode.Base64Encode(PostJson, "utf-8");
                        string Sign = (DataBase64 + merKey).GetMD5();
                        DataBase64 = HttpUtility.UrlEncode(DataBase64);
                        string PostData = string.Format("req={0}&sign={1}", DataBase64, Sign);
                        string HF_Url = "https://api.zhifujiekou.com/api/query";
                        DataRet = Utils.PostRequest(HF_Url, PostData, "utf-8");

                        JObject JS = new JObject();
                        try
                        {
                            JS = (JObject)JsonConvert.DeserializeObject(DataRet);
                        }
                        catch (Exception Ex)
                        {
                            Log.Write("[Order_HFQuery]:", "【PostData】" + PostData + "\n【DataRet】" + DataRet, Ex);
                            JS = null;
                        }
                        if (JS == null)
                        {
                            DataObj.OutError("1000");
                            return;
                        }
                        if (JS["resp"] == null)
                        {
                            Utils.WriteLog("【DataRet】" + DataRet, "OrderQC_HFQuery");
                            DataObj.OutError("1000");
                            return;
                        }
                        string resp = JS["resp"].ToString();
                        DataRet = LokFuEncode.Base64Decode(resp, "utf-8");
                        try
                        {
                            JS = (JObject)JsonConvert.DeserializeObject(DataRet);
                        }
                        catch (Exception Ex)
                        {
                            Log.Write("[Order_HFQuery]:", "【DataRet2】" + DataRet, Ex);
                            JS = null;
                        }
                        if (JS == null)
                        {
                            DataObj.OutError("1000");
                            return;
                        }
                        string respcode = JS["respcode"].ToString();
                        if (respcode != "00")
                        {
                            string respmsg = JS["respmsg"].ToString();
                            DataObj.OutError("1000");
                            Utils.WriteLog("[Order_HFQuery_Err]:【" + respcode + "】" + respmsg, "orderface");
                            return;
                        }
                        string resultcode = JS["resultcode"].ToString();
                        string queryid = JS["queryid"].ToString();
                        if (resultcode == "0000" || resultcode == "1002" || resultcode == "1004")
                        {
                            string txnamt = JS["txnamt"].ToString();
                            int factmoney = int.Parse(txnamt);
                            if (((int)(BaseOrders.Amoney * 100)) == factmoney)
                            {
                                BaseOrders = BaseOrders.PaySuccess(Entity);
                            }
                        }
                    }
                    #endregion
                }
                //================================================
                //这里记录日志
                PayLog PayLog = new PayLog();
                PayLog.PId = BaseOrders.PayWay;
                PayLog.OId = BaseOrders.TNum;
                PayLog.TId = Trade;
                PayLog.Amount = BaseOrders.Amoney;
                PayLog.Way = "GET";
                PayLog.AddTime = DateTime.Now;
                PayLog.Data = DataRet;
                PayLog.State = 1;
                Entity.PayLog.AddObject(PayLog);
                Entity.SaveChanges();
                //================================================
            }
            BaseOrders.Cols = "TNum,Amount,PayState";
            DataObj.Data = BaseOrders.OutJson(); 
            DataObj.Code = "0000";
            DataObj.OutString();
        }
    }
}