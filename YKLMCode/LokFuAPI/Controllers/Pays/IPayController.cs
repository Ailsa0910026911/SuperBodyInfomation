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
    public class IPayController : InitController
    {
        public IPayController()
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
                Log.Write("[IPay]:", "【Data】" + Data, Ex);
                json = null;
            }
            if (json == null)
            {
                DataObj.OutError("1000");
                return;
            }
            Orders Orders = new Orders();
            Orders = JsonToObject.ConvertJsonToModel(Orders, json);
            if (Orders.TNum.IsNullOrEmpty() || Orders.Token.IsNullOrEmpty()) {
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
            if (BaseOrders.TState != 1)
            {
                DataObj.OutError("6024");
                return;
            }
            if (BaseOrders.PayState != 0)
            {
                //已支付
                DataObj.OutError("6025");
                return;
            }
            #region 处理选择通道
            PayConfig PayConfig = null;
            if (!BaseOrders.PayWay.IsNullOrEmpty())
            {
                PayConfig = Entity.PayConfig.FirstOrDefault(n => n.State == 1 && n.Id == BaseOrders.PayWay);
                if (Orders.PayName != PayConfig.GroupType)
                {
                    DataObj.OutError("1008");
                    return;
                }
            }
            else
            {
                if (Orders.PayName.IsNullOrEmpty()) {
                    DataObj.OutError("1000");
                    return;
                }
                //获取最佳支付通道
                IList<PayConfig> PayConfigList = Entity.PayConfig.Where(n => n.State == 1 && n.GroupType == Orders.PayName).ToList();
                IList<SysControl> SysControlList = Entity.SysControl.OrderBy(n => n.Sort).ToList();//SysControl
                IList<SysControl> SCList = new List<SysControl>();
                IList<PayConfig> PCList = new List<PayConfig>();
                foreach (var p in SysControlList)
                {
                    SysControl T = p.ChkState();
                    if (T.State == 1)
                    {
                        SCList.Add(p);
                    }
                }
                foreach (var p in PayConfigList)
                {
                    SysControl T = SCList.FirstOrDefault(n => n.PayWay == p.Id);
                    if (T != null)
                    {
                        if (BaseOrders.Amoney >= (decimal)T.SNum && BaseOrders.Amoney <= (decimal)T.ENum)
                        {
                            PCList.Add(p);
                        }
                    }
                }
                PayConfig = PCList.OrderBy(n => n.Cost).FirstOrDefault();
                if (PayConfig == null)
                {
                    DataObj.OutError("1000");
                    return; ;
                }
                if (BaseOrders.TType == 6)
                {
                    //PayConfigOrder PayConfigOrder = Entity.PayConfigOrder.FirstOrDefault(n => n.OId == BaseOrders.TNum);
                    //PayConfigOrder.SysRate = 0;
                    //PayConfigOrder.Poundage = PayConfigOrder.Amoney * (decimal)PayConfigOrder.SysRate;
                    //BaseOrders.Poundage = PayConfigOrder.Poundage;
                    BaseOrders.PayWay = PayConfig.Id;
                }
                else if (BaseOrders.TType == 10)
                {
                    BaseOrders.PayWay = PayConfig.Id;
                }
                Entity.SaveChanges();
            }
            if (PayConfig == null)
            {
                DataObj.OutError("1000");
                return;
            }
            if (PayConfig.GroupType != "AliPay" && PayConfig.GroupType != "WeiXin")
            {
                DataObj.OutError("1007");
                return;
            }
            #endregion
            string DataRet = "", Trade = "";
            #region 提交好付
            if (PayConfig.DllName == "HFAliPay" || PayConfig.DllName == "HFWeiXin")
            {
                string NoticeUrl = NoticePath + "/PayCenter/HFPay/Notice.html"; ;
                string Action = "";
                if (PayConfig.DllName == "HFAliPay")
                {
                    Action = "AliSao";
                }
                if (PayConfig.DllName == "HFWeiXin")
                {
                    Action = "WxSao";
                }
                string[] PayConfigArr = PayConfig.QueryArray.Split(',');
                if (PayConfigArr.Length != 3)
                {
                    DataObj.OutError("1000");
                    return;
                }
                //提交结算中心
                string merId = PayConfigArr[0];//商户号
                string merKey = PayConfigArr[1];//商户密钥
                string JsPayWay = PayConfigArr[2];//绑定通道

                string orderId = BaseOrders.TNum;//商户流水号
                decimal money = BaseOrders.Amoney * 100;
                long intmoney = Int64.Parse(money.ToString("F0"));
                string OrderMoney = intmoney.ToString();//金额，以分为单

                string PostJson = "{\"action\":\"" + Action + "\",\"txnamt\":\"" + OrderMoney + "\",\"merid\":\"" + merId + "\",\"payway\":\"" + JsPayWay + "\",\"orderid\":\"" + orderId + "\",\"backurl\":\"" + NoticeUrl + "\"}";

                string DataBase64 = LokFuEncode.Base64Encode(PostJson, "utf-8");
                string Sign = (DataBase64 + merKey).GetMD5();

                DataBase64 = HttpUtility.UrlEncode(DataBase64);
                string PostData = string.Format("req={0}&sign={1}", DataBase64, Sign);

                string HF_Url = "https://api.zhifujiekou.com/api/mpgateway";

                DataRet = Utils.PostRequest(HF_Url, PostData, "utf-8");

                JObject JS = new JObject();
                try
                {
                    JS = (JObject)JsonConvert.DeserializeObject(DataRet);
                }
                catch (Exception Ex)
                {
                    Log.Write("[Order_HF]:", "【PostData】" + PostData + "\n【DataRet】" + DataRet, Ex);
                    json = null;
                }
                if (JS == null)
                {
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
                    Log.Write("[Order_HF]:", "【DataRet2】" + DataRet, Ex);
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
                    Utils.WriteLog("[Order_HF_Err]:【" + respcode + "】" + respmsg, "orderface");
                    return;
                }
                if (JS["formaction"] == null)
                {
                    Utils.WriteLog("[Order_HF_Err]:【formaction NULL】" + DataRet, "orderface");
                    DataObj.OutError("2096");
                    return;
                }
                BaseOrders.PayId = JS["formaction"].ToString();
                Trade = JS["queryid"].ToString();
            }
            #endregion
            //================================================
            //这里记录日志
            PayLog PayLog = new PayLog();
            PayLog.PId = PayConfig.Id;
            PayLog.OId = BaseOrders.TNum;
            PayLog.TId = Trade;
            PayLog.Amount = BaseOrders.Amoney;
            PayLog.Way = "GET";
            PayLog.AddTime = DateTime.Now;
            PayLog.Data = Data;
            PayLog.State = 1;
            Entity.PayLog.AddObject(PayLog);
            Entity.SaveChanges();
            //================================================
            BaseOrders.Cols = "TNum,PayId,Amount,PayState";
            DataObj.Data = BaseOrders.OutJson();
            DataObj.Code = "0000";
            DataObj.OutString();
        }
    }
}