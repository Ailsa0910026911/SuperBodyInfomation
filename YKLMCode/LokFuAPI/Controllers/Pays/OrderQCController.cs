using Aop.Api;
using Aop.Api.Request;
using Aop.Api.Response;
using LokFu.Extensions;
using LokFu.Infrastructure;
using LokFu.PayMent.ALF2FPAY;
using LokFu.PayMent.WxPayAPI;
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
    public class OrderQCController : InitController
    {
        public string AlipayVer = "2.0";
        public OrderQCController()
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
                Log.Write("[OrderQC]:", "【Data】" + Data, Ex);
                json = null;
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
            #region 创建交易
            if (OrderF2F.Action == "Create")
            {
                #region 处理选择通道
                int PayWay = OrderF2F.PayWay;

                string Tag = "";
                byte PayType = 1;
                string PayName = "";
                string OpenId = "";//微信JSAPI专用
                if (OrderF2F.OType == 7)
                {//支付宝
                    Tag = "AliPay";
                }
                if (OrderF2F.OType == 8)
                {//微信
                    Tag = "WeiXin";
                }

                if (PayWay.IsNullOrEmpty())
                {
                    if (Tag == "AliPay")
                    {
                        PayWay = 5;//兼容老版本
                    }
                    if (Tag == "WeiXin")
                    {
                        PayWay = 6;//兼容老版本
                    }
                }

                PayConfig PayConfig = Entity.PayConfig.FirstOrDefault(n => n.Id == PayWay && n.State == 1);
                //获取系统支付配置
                if (PayConfig == null)
                {
                    DataObj.OutError("1000");
                    return;
                }
                if (PayConfig.GroupType != Tag)
                {
                    DataObj.OutError("1000");
                    return;
                }
                PayName = "扫码付-" + PayConfig.Name;
                byte ComeWay = 1;
                if (!OrderF2F.PayId.IsNullOrEmpty())
                {
                    if (OrderF2F.PayId == "shop")
                    {
                        ComeWay = 2;
                    }
                    PayName = "收银台-" + PayConfig.Name;
                    if (OrderF2F.OType == 7)
                    {
                        if (OrderF2F.PayId == "shop")
                        {
                            OrderF2F.PayId = string.Empty;
                            PayType = 2;
                        }
                    }
                    if (OrderF2F.OType == 8)
                    {
                        OpenId = OrderF2F.PayId;
                        OrderF2F.PayId = string.Empty;
                        PayType = 2;
                    }
                }
                int InType = 0;
                if (!OrderF2F.InType.IsNullOrEmpty())
                {
                    InType = 1;
                }
                SysControl SysControl = new SysControl();
                bool IsNew = true;
                if (OrderF2F.Id.IsNullOrEmpty())
                {
                    //这是旧逻辑,有一些旧版还在用
                    #region 版本比较 升级之后比较长时间后可以考滤删除版本判断代码
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
                            SysControl = Entity.SysControl.FirstOrDefault(n => n.Tag == Tag && n.PayWay == PayWay && n.LagEntryDay > 0);
                        }
                        else
                        {
                            SysControl = Entity.SysControl.FirstOrDefault(n => n.Tag == Tag && n.PayWay == PayWay && n.LagEntryDay == 0);
                        }
                    }
                    else//旧版
                    {
                        SysControl = Entity.SysControl.FirstOrDefault(n => n.Tag == Tag && n.PayWay == PayWay);
                    }
                }
                else
                {
                    SysControl = Entity.SysControl.FirstOrDefault(n => n.Id == OrderF2F.Id);
                    if (SysControl.LagEntryDay > 0)
                    {
                        InType = 1;
                    }
                    else {
                        InType = 0;
                    }
                }
                //获取系统支付配置
                if (SysControl == null) {
                    DataObj.OutError("1005");
                    return;
                }
                if (InType == 1 && (SysControl.LagEntryDay.IsNullOrEmpty() || SysControl.LagEntryNum.IsNullOrEmpty()))
                {
                    DataObj.Msg = "请升级到最新版再发起Tn到帐交易！";
                    DataObj.OutError("1005");
                    return;
                }
                SysControl syscontrol = SysControl.ChkState();
                if (syscontrol.State != 1)
                {
                    DataObj.OutError("1005");
                    return;
                }
                #endregion

                #region 订单生成
                //处理交易地点
                if (OrderF2F.X.IsNullOrEmpty() || OrderF2F.Y.IsNullOrEmpty())
                {
                    DataObj.OutError("1000");
                    return;
                }
                //开始处理参数 OType通道，7支付宝，8微信
                if (OrderF2F.Amoney.IsNullOrEmpty() || OrderF2F.OType.IsNullOrEmpty())
                {
                    DataObj.OutError("1000");
                    return;
                }
                OrderF2F.Amoney = OrderF2F.Amoney.FormatMoney();

                //获取用户支付配置
                UserPay UserPay = Entity.UserPay.FirstOrDefault(n => n.UId == baseUsers.Id && n.PId == PayWay);
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

                OrderF2F.PayWay = PayWay;

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
                else {
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
                OrderF2F.PayId = string.Empty;
                OrderF2F.PayType = PayType;

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

                Orders.PayType = PayType;
                Orders.PayName = PayName;

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
                Orders.ComeWay = ComeWay;
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
                if (!OrderF2F.IP.IsNullOrEmpty())
                {
                    UserTrack.IP = OrderF2F.IP;
                }
                Orders.SeavGPSLog(UserTrack, Entity);
                //=======================================

                #endregion
                
                if (PayConfig.DllName == "AliPay")
                {
                        #region 提交支付宝
                        string[] PayConfigArr = PayConfig.QueryArray.Split(new char[] { ',' });//接口信息 商户号,密钥,支付宝号
                        if (PayConfigArr.Length != 3)
                        {
                            DataObj.OutError("9999");
                            return;
                        }
                        if (AlipayVer=="1.0")
                        {
                            #region 1.0老接口
                            Submit Submit = new Submit();
                            Submit.pid = PayConfigArr[0];
                            Submit.key = PayConfigArr[1];
                            //卖家支付宝帐户
                            string seller_email = PayConfigArr[2];

                            //订单业务类型
                            string product_code = "QR_CODE_OFFLINE";
                            //SOUNDWAVE_PAY_OFFLINE：声波支付，FINGERPRINT_FAST_PAY：指纹支付，BARCODE_PAY_OFFLINE：条码支付

                            ////////////////////////////////////////////////////////////////////////////////////////////////
                            //把请求参数打包成数组
                            SortedDictionary<string, string> sParaTemp = new SortedDictionary<string, string>();
                            sParaTemp.Add("partner", Submit.pid);
                            sParaTemp.Add("_input_charset", "utf-8");
                            sParaTemp.Add("service", "alipay.acquire.precreate");

                            sParaTemp.Add("out_trade_no", OrderF2F.OId);
                            sParaTemp.Add("subject", Orders.TName + "：" + OrderF2F.OId);
                            sParaTemp.Add("product_code", product_code);
                            sParaTemp.Add("total_fee", OrderF2F.Amoney.ToString("F2"));
                            sParaTemp.Add("seller_email", seller_email);

                            sParaTemp.Add("notify_url", NoticePath + "/PayCenter/AliPay/Notice.html");
                            //建立请求
                            string sHtmlText = Submit.BuildRequest(sParaTemp);
                            Utils.WriteLog(sHtmlText, "orderface");
                            try
                            {
                                XmlDocument xmlDoc = new XmlDocument();
                                xmlDoc.LoadXml(sHtmlText);
                                string is_success = xmlDoc.SelectSingleNode("/alipay/is_success").InnerText;
                                if (is_success == "T")
                                {
                                    string result_code = xmlDoc.SelectSingleNode("/alipay/response/alipay/result_code").InnerText;
                                    if (result_code == "SUCCESS")
                                    {
                                        //成功
                                        string qr_code = xmlDoc.SelectSingleNode("/alipay/response/alipay/qr_code").InnerText;
                                        /*
                                         * voucher_type 凭证类型
                                         * qr_code 二维码码串
                                         * pic_url 二维码图片地址
                                         * small_pic_ur 二维码小图地址
                                        */
                                        OrderF2F.PayId = qr_code;
                                        Entity.SaveChanges();
                                        Orders.PayId = qr_code;
                                    }
                                    else
                                    {
                                        Orders.TState = 0;
                                        OrderF2F.OrderState = 0;
                                        Entity.SaveChanges();
                                    }
                                }
                                else
                                {
                                    Orders.TState = 0;
                                    OrderF2F.OrderState = 0;
                                    Entity.SaveChanges();
                                }
                            }
                            catch (Exception)
                            {
                                //Utils.WriteLog(Ex.ToString());
                                Orders.TState = 0;
                                OrderF2F.OrderState = 0;
                                Entity.SaveChanges();
                            }
                            #endregion
                        }
                        if (AlipayVer == "2.0")
                        {
                            #region 2.0新接口

                            ALF2FPAY ALF2FPAY = new ALF2FPAY();
                            ALF2FPAY.pid = PayConfigArr[0];
                            ALF2FPAY.appId = PayConfigArr[2];
                            IAopClient client = new DefaultAopClient(ALF2FPAY.serverUrl, ALF2FPAY.appId, ALF2FPAY.merchant_private_key, "json", ALF2FPAY.version, ALF2FPAY.sign_type, ALF2FPAY.alipay_public_key, ALF2FPAY.charset);
                            StringBuilder sb = new StringBuilder();
                            sb.Append("{");
                            sb.Append("\"out_trade_no\":\"" + OrderF2F.OId + "\",");
                            sb.Append("\"total_amount\":\"" + OrderF2F.Amoney.ToMoney() + "\",");
                            sb.Append("\"timeout_express\":\"30m\",");
                            sb.Append("\"subject\":\"" + OrderF2F.OId + "\"");
                            sb.Append("}");
                            AlipayTradePrecreateRequest payRequst = new AlipayTradePrecreateRequest();

                            string notify_url = NoticePath + "/PayCenter/AliPay/Notice.html";
                            payRequst.SetNotifyUrl(notify_url);

                            payRequst.BizContent = sb.ToString();

                            Dictionary<string, string> paramsDict = (Dictionary<string, string>)payRequst.GetParameters();
                            AlipayTradePrecreateResponse payResponse = client.Execute(payRequst);

                            if (payResponse != null)
                            {
                                payResponse.SaveLog(Entity);//保存记录
                                if (payResponse.Code == "10000")
                                {
                                    OrderF2F.PayId = payResponse.QrCode;
                                    Entity.SaveChanges();
                                    Orders.PayId = payResponse.QrCode;
                                }
                                else
                                {
                                    Orders.TState = 0;
                                    OrderF2F.OrderState = 0;
                                    Entity.SaveChanges();
                                }
                            }
                            else { 
                                Orders.TState = 0;
                                OrderF2F.OrderState = 0;
                                Entity.SaveChanges();
                            }
                            #endregion
                        }
                        #endregion
                    
                }
                if (PayConfig.DllName == "WeiXin")
                {
                        #region 提交微信
                        //初始化支付配置
                        WxPayConfig WxPayConfig = new WxPayConfig();
                        string[] PayConfigArr = PayConfig.QueryArray.Split(new char[] { ',' });//接口信息 appid,mchid,key,appsecret
                        if (PayConfigArr.Length != 4 && PayConfigArr.Length != 5)
                        {
                            DataObj.OutError("9999");
                            return;
                        }
                        string ServerIp = ConfigurationManager.AppSettings["ServerIp"].ToString();
                        string Wx_Cert_Path = ConfigurationManager.AppSettings["Wx_Cert_Path"].ToString();
                        string Wx_Cert_PWD = ConfigurationManager.AppSettings["Wx_Cert_PWD"].ToString();
                        WxPayConfig.IP = ServerIp;
                        WxPayConfig.APPID = PayConfigArr[0];
                        WxPayConfig.MCHID = PayConfigArr[1];
                        WxPayConfig.KEY = PayConfigArr[2];
                        WxPayConfig.APPSECRET = PayConfigArr[3];
                        if (PayConfigArr.Length == 5)
                        {
                            WxPayConfig.SubMCHID = PayConfigArr[4];
                        }
                        WxPayConfig.SSLCERT_PATH = Wx_Cert_Path;
                        WxPayConfig.SSLCERT_PASSWORD = Wx_Cert_PWD;
                        //支付配置结束

                        WxPayData data = new WxPayData();
                        //data.SetValue("auth_code", OrderF2F.PayId);//授权码
                        data.SetValue("body", Orders.TName + "：" + OrderF2F.OId);//商品描述
                        string total_fee = (OrderF2F.Amoney * 100).ToString("F0");
                        data.SetValue("total_fee", total_fee);//总金额
                        data.SetValue("out_trade_no", OrderF2F.OId);//产生随机的商户订单号
                        if (PayType == 2)
                        {
                            data.SetValue("trade_type", "JSAPI");//交易类型
                            if (PayConfigArr.Length == 5)
                            {//子商户模式
                                data.SetValue("sub_openid", OpenId);//用户OpenId
                            }
                            else
                            {
                                data.SetValue("openid", OpenId);//用户OpenId
                            }
                        }
                        else
                        {
                            data.SetValue("trade_type", "NATIVE");//交易类型
                        }

                        data.SetValue("notify_url", NoticePath + "/PayCenter/WeiXinPay/Notice.html");//异步通知地址
                        WxPayData result = WxPayApi.UnifiedOrder(data, WxPayConfig, 10);//调用统一下单接口
                        result.SaveLog(Entity);

                        if (!result.IsSet("return_code") || result.GetValue("return_code").ToString() == "FAIL")
                        {//支付失败
                            Orders.TState = 0;
                            OrderF2F.OrderState = 0;
                            Entity.SaveChanges();
                        }
                        else
                        {
                            if (!result.CheckSign(WxPayConfig.KEY))
                            {//签名失败，取消订单
                                Orders.TState = 0;
                                OrderF2F.OrderState = 0;
                                Entity.SaveChanges();
                            }
                            else
                            {
                                if (result.GetValue("return_code").ToString() == "SUCCESS" && result.GetValue("result_code").ToString() == "SUCCESS")
                                {
                                    //获取支付字符串
                                    if (PayType == 2)//JSAPI
                                    {
                                        string PayId = result.GetValue("prepay_id").ToString();
                                        OrderF2F.PayId = PayId;
                                    }
                                    else
                                    {
                                        string PayId = result.GetValue("code_url").ToString();
                                        OrderF2F.PayId = PayId;
                                    }
                                    Entity.SaveChanges();
                                    Orders.PayId = OrderF2F.PayId;
                                }
                                else
                                {
                                    //业务明确失败
                                    Orders.TState = 0;
                                    OrderF2F.OrderState = 0;
                                    Entity.SaveChanges();
                                }
                            }
                        }
                        #endregion
                }
                    

                #region 提交好付
                if (PayConfig.DllName == "HFAliPay" || PayConfig.DllName == "HFWeiXin")
                {
                    string NoticeUrl = "";
                    string Action = "";
                    if (PayConfig.DllName == "HFAliPay")
                    {
                        NoticeUrl = NoticePath + "/PayCenter/HFAliPay/Notice.html";
                        Action = "AliSao";
                    }
                    if (PayConfig.DllName == "HFWeiXin")
                    {
                        NoticeUrl = NoticePath + "/PayCenter/HFWeiXin/Notice.html";
                        if (PayType == 2)
                        {
                            Action = "WxJsApi";
                        }
                        else
                        {
                            Action = "WxSao";
                        }
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

                    string orderId = Orders.TNum;//商户流水号
                    decimal money = Orders.Amoney * 100;
                    long intmoney = Int64.Parse(money.ToString("F0"));
                    string OrderMoney = intmoney.ToString();//金额，以分为单

                    string OpenIdStr = "";
                    if (PayConfig.Id == 13)
                    {
                        //特殊处理通道
                        Action = "WxJsApi";
                        OpenId = "OpenId";
                    }
                    if (Action == "WxJsApi")
                    {
                        OpenIdStr = ",\"openid\":\"" + OpenId + "\"";
                    }
                    
                    string PostJson = "{\"action\":\"" + Action + "\",\"txnamt\":\"" + OrderMoney + "\",\"merid\":\"" + merId + "\",\"payway\":\"" + JsPayWay + "\",\"orderid\":\"" + orderId + "\",\"backurl\":\"" + NoticeUrl + "\"" + OpenIdStr + "}";
                    
                    string DataBase64 = LokFuEncode.Base64Encode(PostJson, "utf-8");
                    string Sign = (DataBase64 + merKey).GetMD5();

                    DataBase64 = HttpUtility.UrlEncode(DataBase64);
                    string PostData = string.Format("req={0}&sign={1}", DataBase64, Sign);

                    string HF_Url = "https://api.zhifujiekou.com/api/mpgateway";

                    string Ret = Utils.PostRequest(HF_Url, PostData, "utf-8");

                    JObject JS = new JObject();
                    try
                    {
                        JS = (JObject)JsonConvert.DeserializeObject(Ret);
                    }
                    catch (Exception Ex)
                    {
                        Log.Write("[Order_HF]:", "【PostData】" + PostData + "\n【Ret】" + Ret, Ex);
                        json = null;
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
                        Log.Write("[Order_HF]:", "【Ret2】" + Ret, Ex);
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
                    if (JS["formaction"] == null) {
                        Utils.WriteLog("[Order_HF_Err]:【formaction NULL】" + Ret, "orderface");
                        DataObj.OutError("2096");
                        return;
                    }
                    Orders.PayId = JS["formaction"].ToString();
                    if (PayConfig.Id == 13)
                    {
                        string myData = "{\"merid\":\"" + merId + "\",\"orderid\":\"" + orderId + "\",\"code\":\"" + Orders.PayId + "\"}";
                        DataBase64 = LokFuEncode.Base64Encode(myData, "utf-8");
                        Sign = (DataBase64 + merKey).GetMD5();
                        DataBase64 = HttpUtility.UrlEncode(DataBase64);
                        string myUrl = string.Format("req={0}&sign={1}", DataBase64, Sign);
                        string Url = "https://api.zhifujiekou.com/wxjsapi/gopay.html?" + myUrl;

                        OrderF2F.PayId = Url;
                        Entity.SaveChanges();
                        Orders.PayId = "http://api.kkapay.com/home/jump?tnum=" + Orders.TNum;
                    }
                }
                #endregion
                Orders.Cols = "Id,TNum,PayId";
                DataObj.Data = Orders.OutJson();
                DataObj.Code = "0000";
                DataObj.OutString();
            }
            #endregion
            if (OrderF2F.Action == "GET")//获取订交易信息
            {
                //开始处理参数
                if (OrderF2F.OId.IsNullOrEmpty())
                {
                    DataObj.OutError("1000");
                    return;
                }
                Orders Orders = Entity.Orders.FirstOrDefault(n => n.TNum == OrderF2F.OId && n.UId == baseUsers.Id);
                if (Orders == null)
                {
                    DataObj.OutError("1000");
                    return;
                }
                OrderF2F Orderf2f = Entity.OrderF2F.FirstOrDefault(n => n.OId == OrderF2F.OId && n.UId == baseUsers.Id);
                if (Orders.TState == 1 && Orders.PayState == 0)
                {
                    PayConfig PayConfig = Entity.PayConfig.FirstOrNew(n => n.Id == Orders.PayWay);
                    if (PayConfig.DllName == "AliPay")
                    {
                            #region 支付宝处理
                            string[] PayConfigArr = PayConfig.QueryArray.Split(new char[] { ',' });//接口信息 商户号,密钥，支付宝号
                            if (PayConfigArr.Length != 3)
                            {
                                DataObj.OutError("9999");
                                return;
                            }
                            if (AlipayVer == "1.0")
                            {
                                #region 1.0老接口
                                string pid = PayConfigArr[0];
                                string key = PayConfigArr[1];

                                int queryTimes = 10;//查询次数计数器
                                int Run = 0;
                                while (queryTimes > 0 && Run != 1)
                                {
                                    AliPayApi Api = new AliPayApi();
                                    int succResult = Api.AliPayQuery(Orderf2f.OId, pid, key);
                                    //如果需要继续查询，则等待2s后继续
                                    if (succResult == 2)
                                    {
                                        //Thread.Sleep(5000);
                                        //continue;
                                        Run = 1;//跳出循环，20160420
                                    }
                                    //查询成功,返回订单查询接口返回的数据
                                    else if (succResult == 1)
                                    {
                                        //Orders.PayState = 1;//此处不保存支付状态，由通知返回再操作
                                        Orders = Orders.PaySuccess(Entity);
                                        Run = 1;
                                    }
                                    //订单交易失败
                                    else
                                    {
                                        Orders.TState = 0;
                                        Orderf2f.OrderState = 0;
                                        Entity.SaveChanges();
                                    }
                                    queryTimes--;
                                }
                                #endregion
                            }
                            if (AlipayVer == "2.0")
                            {
                                #region 2.0新接口
                                ALF2FPAY ALF2FPAY = new ALF2FPAY();
                                ALF2FPAY.pid = PayConfigArr[0];
                                ALF2FPAY.appId = PayConfigArr[2];

                                IAopClient client = new DefaultAopClient(ALF2FPAY.serverUrl, ALF2FPAY.appId, ALF2FPAY.merchant_private_key, "json", ALF2FPAY.version, ALF2FPAY.sign_type, ALF2FPAY.alipay_public_key, ALF2FPAY.charset);

                                string QueryStr = "{\"out_trade_no\":\"" + Orders.TNum + "\"}";
                                ALF2FPAYObj ObjQuery = new ALF2FPAYObj();
                                ObjQuery.BizCode = QueryStr;
                                ObjQuery.Client = client;
                                ObjQuery.Entity = Entity;
                                AliPayApi Api = new AliPayApi();
                                AlipayTradeQueryResponse queryResponse = Api.LoopAlipayQuery(ObjQuery, 1);
                                if (queryResponse != null)
                                {
                                    if (queryResponse.Code == "10000")
                                    {
                                        if (queryResponse.TradeStatus == "TRADE_FINISHED" || queryResponse.TradeStatus == "TRADE_SUCCESS")
                                        {
                                            //Orders.PayState = 1;//此处不保存支付状态，由通知返回再操作
                                            Orders = Orders.PaySuccess(Entity);
                                        }
                                    }
                                }
                                #endregion
                            }
                            #endregion
                    }
                    if (PayConfig.DllName == "WeiXin")
                    {
                            #region 微信处理
                            //初始化支付配置
                            WxPayConfig WxPayConfig = new WxPayConfig();
                            string[] PayConfigArr = PayConfig.QueryArray.Split(new char[] { ',' });//接口信息 appid,mchid,key,appsecret
                            if (PayConfigArr.Length != 4 && PayConfigArr.Length != 5)
                            {
                                DataObj.OutError("9999");
                                return;
                            }
                            string ServerIp = ConfigurationManager.AppSettings["ServerIp"].ToString();
                            string Wx_Cert_Path = ConfigurationManager.AppSettings["Wx_Cert_Path"].ToString();
                            string Wx_Cert_PWD = ConfigurationManager.AppSettings["Wx_Cert_PWD"].ToString();
                            WxPayConfig.IP = ServerIp;
                            WxPayConfig.APPID = PayConfigArr[0];
                            WxPayConfig.MCHID = PayConfigArr[1];
                            WxPayConfig.KEY = PayConfigArr[2];
                            WxPayConfig.APPSECRET = PayConfigArr[3];
                            if (PayConfigArr.Length == 5)
                            {
                                WxPayConfig.SubMCHID = PayConfigArr[4];
                            }
                            WxPayConfig.SSLCERT_PATH = Wx_Cert_Path;
                            WxPayConfig.SSLCERT_PASSWORD = Wx_Cert_PWD;
                            //支付配置结束
                            int queryTimes = 10;//查询次数计数器
                            int Run = 0;
                            while (queryTimes > 0 && Run != 1)
                            {
                                int succResult = 0;//查询结果
                                MicroPay MicroPay = new MicroPay();
                                WxPayData queryResult = MicroPay.WXQuery(Orderf2f.OId, WxPayConfig, Entity, out succResult);
                                //如果需要继续查询，则等待2s后继续
                                if (succResult == 2)
                                {
                                    //Thread.Sleep(5000);
                                    //continue;
                                    Run = 1;//跳出循环，20160420
                                }
                                //查询成功,返回订单查询接口返回的数据
                                else if (succResult == 1)
                                {
                                    Orderf2f.Trade_no = queryResult.GetValue("transaction_id").ToString();
                                    int ret = Entity.SaveChanges();
                                    //Orders.PayState = 1;//此处不保存支付状态，由通知返回再操作
                                    Orders = Orders.PaySuccess(Entity);
                                    Run = 1;
                                }
                                //订单交易失败
                                else
                                {
                                    Orders.TState = 0;
                                    Orderf2f.OrderState = 0;
                                    Entity.SaveChanges();
                                }
                                queryTimes--;
                            }
                            #endregion
                    }
                    if (PayConfig.DllName == "HFAliPay" || PayConfig.DllName == "HFWeiXin")
                    {
                        #region 好付处理
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
                            string HF_Url = "https://api.zhifujiekou.com/api/query";
                            string Ret = Utils.PostRequest(HF_Url, PostData, "utf-8");

                            JObject JS = new JObject();
                            try
                            {
                                JS = (JObject)JsonConvert.DeserializeObject(Ret);
                            }
                            catch (Exception Ex)
                            {
                                Log.Write("[Order_HFQuery]:", "【PostData】" + PostData + "\n【Ret】" + Ret, Ex);
                                JS = null;
                            }
                            if (JS == null)
                            {
                                DataObj.OutError("1000");
                                return;
                            }
                            if (JS["resp"] == null) {
                                Utils.WriteLog("【Ret】" + Ret, "OrderQC_HFQuery");
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
                                Log.Write("[Order_HFQuery]:", "【Ret2】" + Ret, Ex);
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
                            if (resultcode == "0000" || resultcode == "1002" || resultcode == "1004")
                            {
                                string txnamt = JS["txnamt"].ToString();
                                int factmoney = int.Parse(txnamt);
                                if (((int)(Orders.Amoney * 100)) == factmoney)
                                {
                                    //Orders.PayState = 1;//此处不保存支付状态，由通知返回再操作
                                    Orders = Orders.PaySuccess(Entity);
                                }
                            }
                        }
                        #endregion
                    }
                }
                DataObj.Data = Orders.OutJson(); 
                DataObj.Code = "0000";
                DataObj.OutString();
            }
        }

    }
}