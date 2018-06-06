using LokFu.Extensions;
using LokFu.Infrastructure;
using LokFu.Repositories;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Configuration;
using System.Data.Objects;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Script.Serialization;

namespace LokFu.Controllers
{
    public class OrderNFCController : InitController
    {
        public OrderNFCController()
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
                Log.Write("[OrderNFC]:", "【Data】" + Data, Ex);
            }
            if (json == null)
            {
                DataObj.OutError("1000");
                return;
            }

            OrderF2F OrderF2F = new OrderF2F();
            OrderF2F = JsonToObject.ConvertJsonToModel(OrderF2F, json);

            UserTrack UserTrack = new UserTrack();
            UserTrack = JsonToObject.ConvertJsonToModel(UserTrack, json);

            #region 获取并处理用户信息
            Users baseUsers = Entity.Users.FirstOrDefault(n => n.Token == OrderF2F.Token);
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
                //DataObj.OutError("2006");
                //return;
            }
            if (baseUsers.MiBao != 1)//未设置支付密码
            {
                //DataObj.OutError("2008");
                //return;
            }
            string NeekName = baseUsers.NeekName;
            if (NeekName.IsNullOrEmpty())
            {
                NeekName = baseUsers.TrueName;
            }
            if (NeekName.IsNullOrEmpty())
            {
                NeekName = "货款";
            }
            #endregion

            if (OrderF2F.Action == "Create")
            {
                //处理交易地点
                if (OrderF2F.X.IsNullOrEmpty() || OrderF2F.Y.IsNullOrEmpty())
                {
                    DataObj.OutError("1000");
                    return;
                }
                int PayWay = OrderF2F.PayWay;
                if (PayWay.IsNullOrEmpty())
                {
                    PayWay = 7;//兼容老版本
                }

                //获取系统支付配置
                PayConfig PayConfig = Entity.PayConfig.FirstOrDefault(n => n.Id == PayWay && n.State == 1);
                if (PayConfig == null)
                {
                    DataObj.OutError("1000");
                    return;
                }
                if (PayConfig.GroupType != "NFC") {
                    DataObj.OutError("1000");
                    return;
                }
                int InType = 0;
                if (!OrderF2F.InType.IsNullOrEmpty())
                {
                    InType = 1;
                }
                SysControl SysControl = new SysControl();
                #region 版本比较 升级之后比较长时间后可以考滤删除版本判断代码
                bool IsNew = true;
                var vSysAgent = this.Entity.SysAgent.FirstOrDefault(o => o.Id == baseUsers.Agent);
                if (vSysAgent == null)
                {
                    DataObj.OutError("1000");
                    return;
                }
                IsNew = BizExt.NewOrOldVersion(vSysAgent, Equipment, this.Entity);
                #endregion
                if (IsNew)//新版
                {
                    if (InType == 1)
                    {
                        SysControl = Entity.SysControl.FirstOrDefault(n => n.Tag == "NFC" && n.PayWay == PayWay && n.LagEntryDay > 0);
                    }
                    else
                    {
                        SysControl = Entity.SysControl.FirstOrDefault(n => n.Tag == "NFC" && n.PayWay == PayWay && n.LagEntryDay == 0);
                    }
                }
                else//旧版
                {
                    SysControl = Entity.SysControl.FirstOrDefault(n => n.Tag == "NFC" && n.PayWay == PayWay);
                }

                //获取系统支付配置
                if (SysControl == null)
                {
                    DataObj.OutError("1005");
                    return;
                }
                SysControl syscontrol = SysControl.ChkState();
                if (syscontrol.State != 1)
                {
                    DataObj.OutError("1005");
                    return;
                }

                #region 创建交易
                OrderF2F.PayWay = PayWay;
                OrderF2F.PayType = 0;
                OrderF2F.PayId = PayConfig.DllName;
                OrderF2F.OType = 9; //NFC

                //开始处理参数 PayId扣款码，OType通道，7支付宝，8微信
                if (OrderF2F.Amoney.IsNullOrEmpty())
                {
                    DataObj.OutError("1000");
                    return;
                }
                OrderF2F.Amoney = OrderF2F.Amoney.FormatMoney();

                //获取用户支付配置
                UserPay UserPay = Entity.UserPay.FirstOrDefault(n => n.UId == baseUsers.Id && n.PId == PayConfig.Id);
                if (UserPay == null)
                {
                    DataObj.OutError("1000");
                    return;
                }
                
                //获取分支机构信息
                SysAgent SysAgent = new SysAgent();
                if (!baseUsers.Agent.IsNullOrEmpty())
                {
                    SysAgent = Entity.SysAgent.FirstOrNew(n => n.Id == baseUsers.Agent);
                }


                //获取系统配置
                SysSet SysSet = Entity.SysSet.FirstOrDefault();
                if (InType == 1)
                {//客户端传来T+N但是系统没开启时无效
                    if (SysSet.LagEntry == 0)
                    {
                        InType = 0;
                    }
                }
                if (InType == 1)
                {
                    //手续费
                    OrderF2F.Poundage = 0;
                    //商户费率
                    OrderF2F.UserRate = 0;
                    //T+n时，代理佣金为 交易金额*费率
                    decimal AgentPayGet = OrderF2F.Amoney * SysSet.AgentGet;
                    OrderF2F.AgentPayGet = (Double)AgentPayGet;
                    //佣金舍位
                    OrderF2F.AgentPayGet = OrderF2F.AgentPayGet.Floor();
                }
                else
                {
                    //手续费
                    OrderF2F.Poundage = OrderF2F.Amoney * (decimal)UserPay.Cost;
                    //手续费取进
                    OrderF2F.Poundage = OrderF2F.Poundage.Ceiling();
                    //商户费率
                    OrderF2F.UserRate = UserPay.Cost;
                    //分支机构佣金设置为0，待分润计算后再写入
                    OrderF2F.AgentPayGet = 0;
                }

                OrderF2F.UId = baseUsers.Id;

                //到帐金额=支付金额-手续费
                OrderF2F.PayMoney = OrderF2F.Amoney - OrderF2F.Poundage;
                //第三方支付通道率
                OrderF2F.SysRate = (double)PayConfig.Cost;
                //这里是利润计算==========
                //利润=总金额-到帐-支付手续费
                decimal GetAll = OrderF2F.Amoney - OrderF2F.PayMoney - OrderF2F.Amoney * (decimal)OrderF2F.SysRate;
                //利润舍位
                GetAll = GetAll.Floor();
                //总利润
                OrderF2F.AIdPayGet = (double)GetAll;

                OrderF2F.Agent = SysAgent.Id;//分支机构Id
                OrderF2F.AId = baseUsers.AId;
                OrderF2F.FId = 0;
                OrderF2F.OrderState = 1;
                OrderF2F.PayState = 0;
                OrderF2F.AgentState = 0;
                OrderF2F.AddTime = DateTime.Now;

                //写入前，判断交易金额限制
                if (OrderF2F.Amoney < syscontrol.SNum || OrderF2F.Amoney > syscontrol.ENum)
                {
                    DataObj.OutError("1006");
                    return;
                }

                //写入订单总表
                Orders Orders = new Orders();
                Orders.UId = OrderF2F.UId;
                Orders.TName = NeekName;

                Orders.PayType = OrderF2F.PayType;
                Orders.PayName = "NFC";

                Orders.RUId = 0;
                Orders.RName = string.Empty;
                Orders.TType = OrderF2F.OType;
                Orders.TState = 1;
                Orders.Amoney = OrderF2F.Amoney;
                Orders.Poundage = OrderF2F.Poundage;
                Orders.AddTime = DateTime.Now;
                Orders.PayState = 0;
                Orders.PayWay = PayConfig.Id;

                Orders.Agent = OrderF2F.Agent;
                Orders.AgentState = 0;
                Orders.AId = OrderF2F.AId;
                Orders.FId = 0;

                string OrderAddress = OrderF2F.OrderAddress;
                if (OrderAddress.IsNullOrEmpty())
                {
                    OrderAddress = Utils.GetAddressByGPS(OrderF2F.X, OrderF2F.Y);
                }
                Orders.OrderAddress = OrderAddress;
                Orders.X = OrderF2F.X;
                Orders.Y = OrderF2F.Y;

                if (InType == 1)
                {
                    if (IsNew)//新版
                    {
                        Orders.LagEntryDay = SysControl.LagEntryDay;
                        Orders.LagEntryNum = SysControl.LagEntryNum;
                    }
                    else
                    {
                        Orders.LagEntryDay = SysSet.LagEntryDay;
                        Orders.LagEntryNum = SysSet.LagEntryNum;
                    }
                }
                else
                {
                    Orders.LagEntryDay = 0;
                    Orders.LagEntryNum = 0;
                }

                Entity.Orders.AddObject(Orders);
                Entity.SaveChanges();
                Entity.Refresh(RefreshMode.StoreWins, Orders);

                OrderF2F.OId = Orders.TNum;
                Entity.OrderF2F.AddObject(OrderF2F);
                Entity.SaveChanges();

                //=======================================
                UserTrack.ENo = DataObj.ENo;
                UserTrack.OPType = Orders.PayName;
                UserTrack.UserName = Orders.TNum;
                UserTrack.GPSAddress = Orders.OrderAddress;
                UserTrack.GPSX = Orders.X;
                UserTrack.GPSY = Orders.Y;
                Orders.SeavGPSLog(UserTrack, Entity);
                //=======================================

                if (PayConfig.DllName == "NFC")
                {
                    string[] PayConfigArr = PayConfig.QueryArray.Split(',');
                    if (PayConfigArr.Length != 2)
                    {
                        DataObj.OutError("1000");
                        return;
                    }

                    string NoticeUrl = NoticePath + "/PayCenter/NFC/Notice.html";

                    NFCObj NFCObj = new NFCObj();
                    NFCObj.merchantCode = PayConfigArr[0];// 		商户编码
                    NFCObj.outOrderId = Orders.TNum;//		订单号
                    NFCObj.outUserId = Orders.UId.ToString();//		用户编号
                    NFCObj.orderCreateTime = Orders.AddTime.ToString("yyyyMMddHHmmss");//		订单生成时间20140423112324
                    NFCObj.nonceStr = Utils.RandomSMSCode(10);//		随机字符串
                    NFCObj.goodName = Orders.TName;//		商品名称
                    NFCObj.goodsExplain = Orders.TNum; ;//		商品详情
                    NFCObj.totalAmount = (Orders.Amoney * 100).ToString("F0");//		订单金额	100
                    NFCObj.payNotifyUrl = NoticeUrl;//		支付通知地址
                    NFCObj.sign = PayConfigArr[1];
                    NFCObj.lastPayTime = "";
                    //===================================
                    JavaScriptSerializer jss = new JavaScriptSerializer();
                    string jsonString = jss.Serialize(NFCObj);
                    try
                    {
                        json = (JObject)JsonConvert.DeserializeObject(jsonString);
                    }
                    catch (Exception Ex)
                    {
                        Log.Write("[OrderNFC_My]:", "【jsonString】" + jsonString, Ex);
                    }
                    Orders.Json = json;
                }
                else if (PayConfig.DllName == "HFNFC")
                {
                    string NoticeUrl = NoticePath + "/PayCenter/HFNFC/Notice.html";

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

                    string orderId = Orders.TNum;//商户流水号
                    decimal money = Orders.Amoney * 100;
                    long intmoney = Int64.Parse(money.ToString("F0"));
                    string OrderMoney = intmoney.ToString();//金额，以分为单

                    string PostJson = "{\"action\":\"getOrder\",\"txnamt\":\"" + OrderMoney + "\",\"merid\":\"" + merId + "\",\"payway\":\"" + JsPayWay + "\",\"orderid\":\"" + orderId + "\",\"backurl\":\"" + NoticeUrl + "\"}";

                    string DataBase64 = LokFuEncode.Base64Encode(PostJson, "utf-8");
                    string Sign = (DataBase64 + merKey).GetMD5();

                    DataBase64 = HttpUtility.UrlEncode(DataBase64);
                    string PostData = string.Format("req={0}&sign={1}", DataBase64, Sign);

                    string HFNFC_Url = "https://api.zhifujiekou.com/api/nfcgateway";

                    string Ret = Utils.PostRequest(HFNFC_Url, PostData, "utf-8");

                    JObject JS = new JObject();
                    try
                    {
                        JS = (JObject)JsonConvert.DeserializeObject(Ret);
                    }
                    catch (Exception Ex)
                    {
                        Log.Write("[OrderNFC_HF]:", "【Ret】" + Ret, Ex);
                    }
                    if (JS == null)
                    {
                        DataObj.OutError("1000");
                        return;
                    }
                    string resp = JS["resp"].ToString();
                    Ret = LokFuEncode.Base64Decode(resp, "utf-8");
                    try
                    {
                        JS = (JObject)JsonConvert.DeserializeObject(Ret);
                    }
                    catch (Exception Ex)
                    {
                        Log.Write("[OrderNFC_HF]:", "【Ret2】" + Ret, Ex);
                    }
                    if (JS == null)
                    {
                        DataObj.OutError("1000");
                        return;
                    }
                    string respcode = JS["respcode"].ToString();
                    if (respcode != "00") {
                        string respmsg = JS["respmsg"].ToString();
                        DataObj.OutError("1000");
                        Utils.WriteLog("[OrderNFC_HF_Err]:【" + respcode + "】" + respmsg, "orderface");
                        return;
                    }
                    JObject Json = (JObject)JS["webform"];
                    Orders.Json = Json;
                }
                Orders.Cols = Orders.Cols + ",Json";
                DataObj.Data = Orders.OutJson();
                DataObj.Code = "0000";
                DataObj.OutString();
                #endregion
            }
            if (OrderF2F.Action == "GET")//获取订交易信息
            {
                //开始处理参数
                if (OrderF2F.OId.IsNullOrEmpty())
                {
                    DataObj.OutError("1000");
                    return;
                }
                Orders Orders = Entity.Orders.FirstOrDefault(n => n.TNum == OrderF2F.OId && n.UId == baseUsers.Id);
                if (Orders == null) {
                    DataObj.OutError("1000");
                    return;
                }
                OrderF2F Orderf2f = Entity.OrderF2F.FirstOrDefault(n => n.OId == OrderF2F.OId && n.UId == baseUsers.Id);
                PayConfig PayConfig = Entity.PayConfig.FirstOrNew(n => n.Id == Orders.PayWay);
                if (Orders.TState == 1 && Orders.PayState == 0)
                {
                    if (PayConfig.DllName == "NFC")
                    {
                        //1000000951,f38e989d-900f-4768-8a01-f6667a21f7d3
                        string[] QueryArr = PayConfig.QueryArray.Split(',');
                        if (QueryArr.Length == 2)
                        {
                            string merchantCode = QueryArr[0];
                            string Key = QueryArr[1];
                            string outOrderId = Orders.TNum;
                            string dataStr = "merchantCode=" + merchantCode + "&outOrderId=" + outOrderId;
                            string signStr = dataStr + "&KEY=" + Key;
                            string sign = signStr.GetMD5().ToUpper();
                            string dataJson = "{\"merchantCode\":\"" + merchantCode + "\",\"outOrderId\":\"" + outOrderId + "\",\"sign\":\"" + sign + "\"}";
                            string postData = "{\"param\":" + dataJson + ",\"project_id\":\"WEPAYPLUGIN_PAY\"}";
                            string postUrl = "https://payment.kklpay.com/query/queryOrder.do";
                            string Ret = Utils.GetPostJson(postUrl, postData);
                            //"{"code":"00","data":{"amount":1,"instructCode":"11001998044","merchantCode":"1000000951","outOrderId":"201511170900077","replyCode":"00","sign":"EA778C87B5ACDCBC7735BB78C15CAC72","transTime":"20151117174726","transType":"00200"},"msg":"成功"}"
                            JObject JS = new JObject();
                            try
                            {
                                JS = (JObject)JsonConvert.DeserializeObject(Ret);
                            }
                            catch (Exception Ex)
                            {
                                Utils.WriteLog("[OrderNFC]:JSON[" + Ret + "]" + Ex.ToString(), "orderface");
                            }
                            if (JS != null)
                            {

                                string code = JS["code"].ToString();//返回状态--
                                if (code == "00")
                                {
                                    JObject JSD = (JObject)JS["data"];
                                    if (JSD != null)
                                    {
                                        string amount = JSD["amount"].ToString();//交易金额 单位分
                                        string instructCode = JSD["instructCode"].ToString();//交易单号
                                        string merchantCodeR = JSD["merchantCode"].ToString();//商户号
                                        string outOrderIdR = JSD["outOrderId"].ToString();//订单号
                                        string replyCode = JSD["replyCode"].ToString();//交易状态
                                        string transTime = JSD["transTime"].ToString();//交易时间
                                        string transType = JSD["transType"].ToString();//交易类型
                                        string signR = JSD["sign"].ToString();
                                        //================================================
                                        PayLog PayLog = new PayLog();
                                        PayLog.PId = PayConfig.Id;
                                        PayLog.OId = outOrderId;
                                        PayLog.TId = instructCode;
                                        PayLog.Amount = decimal.Parse(amount) / 100;
                                        PayLog.Way = "GET";
                                        PayLog.AddTime = DateTime.Now;
                                        PayLog.Data = Ret;
                                        PayLog.State = 1;
                                        Entity.PayLog.AddObject(PayLog);
                                        Entity.SaveChanges();
                                        //================================================
                                        if (replyCode == "00")
                                        {
                                            if (merchantCodeR == merchantCode)
                                            {
                                                int factmoney = int.Parse(amount);
                                                if (((int)(Orders.Amoney * 100)) == factmoney)
                                                {
                                                    //Orders.PayState = 1;//此处不保存支付状态，由通知返回再操作
                                                    Orders = Orders.PaySuccess(Entity);
                                                }
                                            }
                                        }
                                    }
                                }

                            }
                        }
                        //PaySuccess
                    }else if (PayConfig.DllName == "HFNFC"){
                        string[] QueryArr = PayConfig.QueryArray.Split(',');
                        if (QueryArr.Length == 3)
                        {
                            //提交结算中心
                            string merId = QueryArr[0];//商户号
                            string merKey = QueryArr[1];//商户密钥
                            string orderId = Orders.TNum;//商户流水号
                            string PostJson = "{\"merid\":\"" + merId + "\",\"orderid\":\"" + orderId + "\"}";
                            string DataBase64 = LokFuEncode.Base64Encode(PostJson, "utf-8");
                            string Sign = (DataBase64 + merKey).GetMD5();
                            DataBase64 = HttpUtility.UrlEncode(DataBase64);
                            string PostData = string.Format("req={0}&sign={1}", DataBase64, Sign);
                            string HFNFC_Url = "https://api.zhifujiekou.com/api/query";
                            string Ret = Utils.PostRequest(HFNFC_Url, PostData, "utf-8");

                            JObject JS = new JObject();
                            try
                            {
                                JS = (JObject)JsonConvert.DeserializeObject(Ret);
                            }
                            catch (Exception Ex)
                            {
                                Log.Write("[OrderNFC_HFQuery]:", "【Ret】" + Ret, Ex);
                            }
                            if (JS == null)
                            {
                                DataObj.OutError("1000");
                                return;
                            }
                            string resp = JS["resp"].ToString();
                            Ret = LokFuEncode.Base64Decode(resp, "utf-8");
                            try
                            {
                                JS = (JObject)JsonConvert.DeserializeObject(Ret);
                            }
                            catch (Exception Ex)
                            {
                                Log.Write("[OrderNFC_HFQuery]:", "【Ret2】" + Ret, Ex);
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
                                Utils.WriteLog("[OrderNFC_HFQuery_Err]:【" + respcode + "】" + respmsg, "orderface");
                                return;
                            }
                            string resultcode = JS["resultcode"].ToString();
                            if (resultcode == "0000" || resultcode == "1002" || resultcode == "1004")
                            {
                                string txnamt = JS["txnamt"].ToString();
                                int factmoney = int.Parse(txnamt);
                                if (((int)(Orders.Amoney * 100)) == factmoney){
                                    //Orders.PayState = 1;//此处不保存支付状态，由通知返回再操作
                                    Orders = Orders.PaySuccess(Entity);
                                }
                            }
                        }
                        //PaySuccess
                    }
                }
                DataObj.Data = Orders.OutJson();
                DataObj.Code = "0000";
                DataObj.OutString();
            }
        }
    }
}
