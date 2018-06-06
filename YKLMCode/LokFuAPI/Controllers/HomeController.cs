using Aop.Api;
using Aop.Api.Request;
using Aop.Api.Response;
using LokFu.Extensions;
using LokFu.FastPay;
using LokFu.PayMent.ALF2FPAY;
using LokFu.Repositories;
using LokFu.WeiXin.Repositories;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
using System.Text;
using System.Web.Mvc;
using System.Xml;

namespace LokFu.Controllers
{
    public class HomeController : Controller
    {
        private LokFuEntity Entity = new LokFuEntity();
        public HomeController() {
            
        }
        public ActionResult Index()
        {
            return View();
        }
        public void Jump(string TNum)
        {
            OrderF2F OrderF2F = Entity.OrderF2F.FirstOrNew(n => n.OId == TNum);
            Response.Redirect(OrderF2F.PayId);
        }
        public void FastTest()
        {

            //AliPayCom Com = new AliPayCom();
            //Com.app_id = "2018012402051240";
            //Com.method = "alipay.trade.precreate";
            //Com.notify_url = "http://test.12fen.com.cn/home/post";

            //Dictionary<string, string> Dic = new Dictionary<string, string>();
            //Dic.Add("out_trade_no", "20180125001");
            //Dic.Add("total_amount ", "11.8");
            ////Dic.Add("timeout_express ", "30m");
            //Dic.Add("subject", "IphoneX 512G");
            //string Ret = Com.Send(Dic);
            //Response.Write(Ret);

            //FastUser FastUser = Entity.FastUser.FirstOrDefault(n => n.UId == 25);
            //FastPayWay FastPayWay = Entity.FastPayWay.FirstOrDefault(n => n.Id == 17);
            //FastConfig FastConfig = Entity.FastConfig.FirstOrDefault();
            ////BusFastPay.AddMer(FastUser, FastPayWay, FastConfig, Entity);
            //FastUserPay FastUserPay = Entity.FastUserPay.FirstOrDefault(n => n.UId == 25 && n.PayWay == 17);
            //BusFastPay.AddCard(FastUser, FastUserPay, FastPayWay, Entity);

            #region 2.0新接口



            //ALF2FPAY ALF2FPAY = new ALF2FPAY();
            //ALF2FPAY.pid = "2088221932373826";
            //ALF2FPAY.appId = "2018012402051240";


            //IAopClient client = new DefaultAopClient(ALF2FPAY.serverUrl, ALF2FPAY.appId, ALF2FPAY.merchant_private_key, "json", ALF2FPAY.version, ALF2FPAY.sign_type, ALF2FPAY.alipay_public_key, ALF2FPAY.charset);
            //StringBuilder sb = new StringBuilder();
            //sb.Append("{");
            //sb.Append("\"out_trade_no\":\"20180123001\",");
            //sb.Append("\"total_amount\":\"20\",");
            //sb.Append("\"timeout_express\":\"30m\",");
            //sb.Append("\"subject\":\"20180101\"");
            //sb.Append("}");
            //AlipayTradePrecreateRequest payRequst = new AlipayTradePrecreateRequest();

            //string notify_url =  "http://test.12fen.com.cn/home/post";
            //payRequst.SetNotifyUrl(notify_url);

            //payRequst.BizContent = sb.ToString();

            //Dictionary<string, string> paramsDict = (Dictionary<string, string>)payRequst.GetParameters();
            //AlipayTradePrecreateResponse payResponse = client.Execute(payRequst);

            //if (payResponse != null)
            //{
            //    payResponse.SaveLog(Entity);//保存记录
            //    if (payResponse.Code == "10000")
            //    {
            //        Response.Write(payResponse.QrCode);
            //    }
            //    else
            //    {

            //    }
            //}
            //else
            //{

            //}
            #endregion


            /*
            #region 1.0老接口
            Submit Submit = new Submit();
            Submit.pid = "2088502608527038";
            Submit.key = "zzj4yc03ctni2s89zgeapy6hoodo8ee6";
            //Submit.pid = "2088221932373826";
            //Submit.key = "uwpzkmu9f2ofygu9azeeexoslbcvmvvk";
            //卖家支付宝帐户
            string seller_email = "";
//合作伙伴好PID：2088221932373826
//安全校验码KEY：uwpzkmu9f2ofygu9azeeexoslbcvmvvk
//支付宝账号：hainan@kingsundo.com

            //订单业务类型
            string product_code = "QR_CODE_OFFLINE";
            //SOUNDWAVE_PAY_OFFLINE：声波支付，FINGERPRINT_FAST_PAY：指纹支付，BARCODE_PAY_OFFLINE：条码支付

            ////////////////////////////////////////////////////////////////////////////////////////////////
            //把请求参数打包成数组
            SortedDictionary<string, string> sParaTemp = new SortedDictionary<string, string>();
            sParaTemp.Add("partner", Submit.pid);
            sParaTemp.Add("_input_charset", "utf-8");
            sParaTemp.Add("service", "alipay.acquire.precreate");

            sParaTemp.Add("out_trade_no", "20180123001");
            sParaTemp.Add("subject", "订单20180123001");
            sParaTemp.Add("product_code", product_code);
            sParaTemp.Add("total_fee", "20");
            sParaTemp.Add("seller_email", seller_email);

            sParaTemp.Add("notify_url","http://test.12fen.com.cn");
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
                         //* voucher_type 凭证类型
                         //* qr_code 二维码码串
                         //* pic_url 二维码图片地址
                         //* small_pic_ur 二维码小图地址
                        
                    }
                    else
                    {
                        
                    }
                }
                else
                {
                   
                }
            }
            catch (Exception)
            {
               
            }
            #endregion
             */
        }
        public void GetQRCode(string mobile) {
            Response.AddHeader("content-type", "application/json");
            if (mobile.IsNullOrEmpty()) {
                Response.Write("{\"error\":\"1\",\"info\":\"手机号为空\"}");
                return;
            }
            Users Users = Entity.Users.FirstOrDefault(n => n.UserName == mobile);
            if (Users == null)
            {
                Response.Write("{\"error\":\"1\",\"info\":\"用户不存在\"}");
                return;
            }
            string Url = string.Format("http://sys.goodpay.net.cn/mobile/shop/index-{0}.html#hf{1}", Users.Id, Users.UserName);
            Response.Write("{\"error\":\"0\",\"info\":\"" + Url + "\"}");
        }
        public void GetShareCode(string mobile)
        {
            Response.AddHeader("content-type", "application/json");
            if (mobile.IsNullOrEmpty())
            {
                Response.Write("{\"error\":\"1\",\"info\":\"手机号为空\"}");
                return;
            }
            Users Users = Entity.Users.FirstOrDefault(n => n.UserName == mobile);
            if (Users == null)
            {
                Response.Write("{\"error\":\"1\",\"info\":\"用户不存在\"}");
                return;
            }
            string Url = string.Format("http://sys.goodpay.net.cn/mobile/reg/money-{0}.html", Users.Id);
            Response.Write("{\"error\":\"0\",\"info\":\"" + Url + "\"}");
        }
        public void ClearToken() {
            string AccToken = MemoryCacheBuilder.EntityCache.Get("access_token", null) as string;
            Response.Write("<br />==============================<br />");
            Response.Write(AccToken);
            Response.Write("<br />==============================<br />");
            MemoryCacheBuilder.EntityCache.Remove("access_token", null);
            Response.Write("OK");
        }
        public void ClearCache(string Name,string key) {
            if (!Check(key))
            {
                Response.Write("请不要从外部提交数据");
                return;
            }
            ObjectCache cache = MemoryCache.Default;
            IEnumerable<KeyValuePair<string, object>> items = cache.AsEnumerable().OrderBy(n => n.Key);
            if (!Name.IsNullOrEmpty())
            {
                items = items.Where(n => n.Key.Contains(Name));
            }
            foreach (KeyValuePair<string, object> item in items) { //item.Key为缓存名称, item.Value为缓存值
                CacheBuilder.EntityCache.Remove(item.Key, null);
            }
            Response.Write("OK");
        }
        public void GetCache(string Name,string key)
        {
            if (!Check(key)) {
                Response.Write("请不要从外部提交数据");
                return;
            }
            Response.Write("<div style='width:800px; margin:0 auto;'>");
            ObjectCache cache = MemoryCache.Default;
            IEnumerable<KeyValuePair<string, object>> items = cache.AsEnumerable().OrderBy(n => n.Key);
            if (!Name.IsNullOrEmpty()) {
                items = items.Where(n => n.Key.Contains(Name));
            }
            foreach (KeyValuePair<string, object> item in items)
            {
                Response.Write("<p>缓存名称:" + item.Key + "　　<a href='/home/clearcache?name=" + item.Key + "&key=" + key + "' target='_blank'>清除</a></p>");
            }
            Response.Write("</div>");
        }
        private bool Check(string key) {
            if (key.IndexOf("|") == -1) {
                return false;
            }
            string[] Key = key.Split('|');
            if (Key.Length != 2) {
                return false;
            }
            int Id = Int32.Parse(Key[0]);
            string Token = Key[1];
            SysAdmin SysAdmin = Entity.SysAdmin.FirstOrNew(n => n.Id == Id);
            string LastTime = SysAdmin.LastTime.GetValueOrDefault().ToString("yyyy-MM-dd HH:mm:ss");
            if (Token != LastTime.GetMD5()) {
                return false;
            }
            return true;
        }
    }
}
