using LokFu.Infrastructure;
using LokFu.Repositories;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace LokFu.Areas.Pay.Controllers
{
    public class HFJSPayController : BaseController
    {
        private string DllName = "HFJSPay";

        public ActionResult Index(string etnum)
        {
            if (etnum.IsNullOrEmpty())
            {
                ViewBag.ErrorMsg = "Some Error[00]";
                return View("Error");
            }
            string tnum = LokFuEncode.LokFuAPIDecode(etnum, DllName);
            FastOrder FastOrder = Entity.FastOrder.FirstOrDefault(n => n.TNum == tnum);
            if (FastOrder == null)
            {
                ViewBag.ErrorMsg = "Some Error[02]";
                return View("Error");
            }
            FastPayWay FastPayWay = Entity.FastPayWay.FirstOrDefault(n => n.Id == FastOrder.PayWay);
            if (FastOrder == null)
            {
                ViewBag.ErrorMsg = "Some Error[03]";
                return View("Error");
            }
            if (FastPayWay.DllName != DllName)
            {
                ViewBag.ErrorMsg = "Some Error[04]";
                return View("Error");
            }
            string[] PayConfigArr = FastPayWay.QueryArray.Split(',');
            if (PayConfigArr.Length != 3)
            {
                ViewBag.ErrorMsg = "Some Error[05]";
                return View("Error");
            }
            FastUserPay FastUserPay = Entity.FastUserPay.FirstOrDefault(n => n.PayWay == FastOrder.PayWay && n.UId == FastOrder.UId && n.MerState == 1);
            if (FastUserPay == null)
            {
                ViewBag.ErrorMsg = "商户通道异常";
                return View("Error");
            }
            
            string paywaycode = PayConfigArr[2];//绑定通道
            string OrderMoney = (FastOrder.Amoney * 100).ToString("F0");//金额，以分为单
            string ReturnUrl = PayPath + "/PayCenter/HFJSPay/FastResult.html";//支付成功跳转页
            string BackUrl = NoticePath + "/PayCenter/HFJSPay/FastNotice.html";//后台通过地址

            string Data = "{\"txnamt\":\"" + OrderMoney + "\",\"merid\":\"" + FastUserPay.MerId + "\",\"paywaycode\":\"" + paywaycode + "\",\"orderid\":\"" + FastOrder.TNum + "\",\"backurl\":\"" + BackUrl + "\",\"fronturl\":\"" + ReturnUrl + "\"}";

            string DataBase64 = LokFuEncode.Base64Encode(Data, "utf-8");
            string Sign = (DataBase64 + FastUserPay.MerKey).GetMD5();

            string GoUrl = "https://api.zhifujiekou.com/apis/fastorderspay?";
            GoUrl += "req=" + HttpUtility.UrlEncode(DataBase64);
            GoUrl += "&sign=" + Sign;

            Response.Redirect(GoUrl);

            return View("Null");
        }

        public ActionResult FastResult()
        {
            string Resp = Request.QueryString["resp"];
            string Sign = Request.QueryString["sign"];
            string SignStr = Resp;
            Resp = LokFuEncode.Base64Decode(Resp, "utf-8");
            JObject json = new JObject();
            try
            {
                json = (JObject)JsonConvert.DeserializeObject(Resp);
            }
            catch (Exception Ex)
            {
                ViewBag.ErrorMsg = Ex.ToString();
                return View("Error");
            }
            if (json == null)
            {
                ViewBag.ErrorMsg = "数据处理出错";
                return View("Error");
            }

            string resultcode = json["resultcode"].ToString();//交易结果码
            string resultmsg = json["resultmsg"].ToString();//交易结果信息
            string queryid = json["queryid"].ToString();//交易流水号
            string txnamt = json["txnamt"].ToString();//交易金额
            string merid = json["merid"].ToString();//商户号
            string orderid = json["orderid"].ToString();//交易单号

            FastOrder FastOrder = Entity.FastOrder.FirstOrDefault(n => n.TNum == orderid);
            if (FastOrder == null)
            {
                ViewBag.ErrorMsg = "交易不存在！";
                return View("Error");
            }
            if (FastOrder.State != 1)
            {
                ViewBag.ErrorMsg = "交易异常！";
                return View("Error");
            }
            FastUserPay FastUserPay = Entity.FastUserPay.FirstOrDefault(o => o.UId == FastOrder.UId && o.PayWay == FastOrder.PayWay);
            string MerId = FastUserPay.MerId;
            string MerKey = FastUserPay.MerKey;
            if (MerId.IsNullOrEmpty() || MerKey.IsNullOrEmpty())
            {
                ViewBag.ErrorMsg = "支付通道配置异常！";
                return View("Error");
            }
            string MD5Str = SignStr + MerKey;
            string sign = MD5Str.GetMD5();
            //================================================
            //这里记录日志
            PayLog PayLog = new PayLog();
            PayLog.PId = (int)FastOrder.PayWay;
            PayLog.OId = FastOrder.TNum;
            PayLog.TId = FastOrder.Trade;
            PayLog.Amount = decimal.Parse(txnamt) / 100;
            PayLog.Way = "Result";
            PayLog.AddTime = DateTime.Now;
            PayLog.Data = Request.Form.ToString();
            PayLog.State = 1;
            Entity.PayLog.AddObject(PayLog);
            Entity.SaveChanges();
            //================================================
            if (Sign != sign)
            {
                ViewBag.ErrorMsg = "签名错误！";
                return View("Error");
            }
            if (MerId != merid)
            {
                ViewBag.ErrorMsg = "商户号不一置！";
                return View("Error");
            }
            if (resultcode != "0000" && resultcode != "1002" && resultcode != "1003")
            {
                ViewBag.ErrorMsg = "支付失败！[" + resultcode + "]" + resultmsg;
                return View("Error");
            }
            int factmoney = int.Parse(txnamt);
            if (((int)(FastOrder.Amoney * 100)) != factmoney)
            {
                ViewBag.ErrorMsg = "支付金额与交易金额不符！";
                return View("Error");
            }
            FastOrder.Trade = queryid;
            Entity.SaveChanges();
            FastOrder = FastOrder.PaySuccess(Entity);
            if (FastOrder.PayState == 1)
            {
                if (resultcode == "0000")
                {
                    FastOrder.UserState = 1;
                    FastOrder.UserTime = DateTime.Now;
                }
                if (resultcode == "1002")
                {
                    FastOrder.UserState = 3;
                    FastOrder.UserTime = DateTime.Now;
                }
                if (resultcode == "1003")
                {
                    FastOrder.UserState = 2;
                    FastOrder.UserTime = DateTime.Now;
                }
                Entity.SaveChanges();
            }
            ViewBag.FastOrder = FastOrder;
            return View("FastSuccess");
        }

        public void FastNotice()
        {
            string Resp = Request.Form["resp"];
            string Sign = Request.Form["sign"];
            string SignStr = Resp;
            Resp = LokFuEncode.Base64Decode(Resp, "utf-8");
            JObject json = new JObject();
            try
            {
                json = (JObject)JsonConvert.DeserializeObject(Resp);
            }
            catch (Exception Ex)
            {
                Response.Write(Ex.ToString());
                return;
            }
            if (json == null)
            {
                Response.Write("Json Null");
                return;
            }
            string resultcode = json["resultcode"].ToString();//交易结果码
            string resultmsg = json["resultmsg"].ToString();//交易结果信息
            string queryid = json["queryid"].ToString();//交易流水号
            string txnamt = json["txnamt"].ToString();//交易金额\
            string merid = json["merid"].ToString();//交易金额
            string orderid = json["orderid"].ToString();//交易金额

            FastOrder FastOrder = Entity.FastOrder.FirstOrDefault(n => n.TNum == orderid);
            if (FastOrder == null)
            {
                Response.Write("E1");
                return;
            }
            if (FastOrder.State != 1)
            {
                Response.Write("E2");
                return;
            }
            if (FastOrder.PayState == 1)
            {
                if (resultcode == "0000")
                {
                    FastOrder.UserState = 1;
                    FastOrder.UserTime = DateTime.Now;
                }
                if (resultcode == "1002")
                {
                    FastOrder.UserState = 3;
                    FastOrder.UserTime = DateTime.Now;
                }
                if (resultcode == "1003")
                {
                    FastOrder.UserState = 2;
                    FastOrder.UserTime = DateTime.Now;
                }
                Entity.SaveChanges();
                Response.Write("0000");
                return;
            }
            FastUserPay FastUserPay = Entity.FastUserPay.FirstOrDefault(o => o.UId == FastOrder.UId && o.PayWay == FastOrder.PayWay);
            string MerId = FastUserPay.MerId;
            string MerKey = FastUserPay.MerKey;
            if (MerId.IsNullOrEmpty() || MerKey.IsNullOrEmpty())
            {
                Response.Write("E53");
                return;
            }
            string MD5Str = SignStr + MerKey;
            string sign = MD5Str.GetMD5();
            //================================================
            //这里记录日志
            PayLog PayLog = new PayLog();
            PayLog.PId = (int)FastOrder.PayWay;
            PayLog.OId = FastOrder.TNum;
            PayLog.TId = FastOrder.Trade;
            PayLog.Amount = decimal.Parse(txnamt) / 100;
            PayLog.Way = "POST";
            PayLog.AddTime = DateTime.Now;
            PayLog.Data = Request.Form.ToString();
            PayLog.State = 1;
            Entity.PayLog.AddObject(PayLog);
            Entity.SaveChanges();
            //================================================
            if (Sign != sign)
            {
                Response.Write("E4");
                return;
            }
            if (MerId != merid)
            {
                Response.Write("E5");
                return;
            }
            //0000 交易成功【已支付，结算到子商户结算卡】
            //1001 未支付
            //1002 交易处理中【已支付，未结算到结算卡】
            //1003交易成功结算失败
            //1004 交易失败
            if (resultcode != "0000" && resultcode != "1002" && resultcode != "1003")
            {
                Response.Write("E6");
                return;
            }
            int factmoney = int.Parse(txnamt);
            if (((int)(FastOrder.Amoney * 100)) != factmoney)
            {
                Response.Write("E7");
                return;
            }
            FastOrder.Trade = queryid;
            Entity.SaveChanges();
            FastOrder = FastOrder.PaySuccess(Entity);
            if (FastOrder.PayState == 1)
            {
                if (resultcode == "0000")
                {
                    FastOrder.UserState = 1;
                    FastOrder.UserTime = DateTime.Now;
                }
                if (resultcode == "1002")
                {
                    FastOrder.UserState = 3;
                    FastOrder.UserTime = DateTime.Now;
                }
                if (resultcode == "1003")
                {
                    FastOrder.UserState = 2;
                    FastOrder.UserTime = DateTime.Now;
                }
                Entity.SaveChanges();
            }
            Response.Write("0000");
        }
        /// <summary>
        /// 刷还系统通知
        /// </summary>
        public void Notice()
        {
            string Resp = Request.Form["resp"];
            string Sign = Request.Form["sign"];
            string SignStr = Resp;
            Resp = LokFuEncode.Base64Decode(Resp, "utf-8");
            JObject json = new JObject();
            try
            {
                json = (JObject)JsonConvert.DeserializeObject(Resp);
            }
            catch (Exception Ex)
            {
                Response.Write(Ex.ToString());
                return;
            }
            if (json == null)
            {
                Response.Write("Json Null");
                return;
            }
            string resultcode = json["resultcode"].ToString();//交易结果码
            string resultmsg = json["resultmsg"].ToString();//交易结果信息
            string queryid = json["queryid"].ToString();//交易流水号
            string txnamt = json["txnamt"].ToString();//交易金额\
            string merid = json["merid"].ToString();//交易金额
            string orderid = json["orderid"].ToString();//交易金额

            JobItem JobItem = this.Entity.JobItem.FirstOrDefault(o => o.RunNum == orderid);
            if (JobItem == null)
            {
                Response.Write("E1");
                return;
            }
            if (JobItem.State != 2)
            {
                Response.Write("S1");
                return;
            }
            if (JobItem.RunState != 0 && JobItem.RunState != 2)
            {
                Response.Write("S2");
                return;
            }
            JobUserPay JobUserPay = Entity.JobUserPay.FirstOrDefault(n => n.PayWay == JobItem.PayWay && n.UId == JobItem.UId);
            if (JobUserPay == null)
            {
                Response.Write("E2");
                return;
            }
            string MD5Str = SignStr + JobUserPay.MerKey;
            string sign = MD5Str.GetMD5();
            //================================================
            //这里记录日志
            JobLog JobLog = new JobLog();
            JobLog.PayWay = JobItem.PayWay;
            JobLog.ReqNo = JobItem.RunNum;
            JobLog.TNum = JobItem.TNum;
            JobLog.Trade = "";
            JobLog.Amount = JobItem.RunMoney;
            JobLog.Way = "Notice";
            JobLog.AddTime = DateTime.Now;
            JobLog.Data = Request.Form.ToString();
            JobLog.State = 1;
            Entity.JobLog.AddObject(JobLog);
            Entity.SaveChanges();
            //================================================
            if (Sign != sign)
            {
                Response.Write("E2");
                return;
            }
            if (JobUserPay.MerId != merid)
            {
                Response.Write("E1");
                return;
            }
            if (resultcode == "0000")
            {
                JobItem = JobItem.PaySuccess(Entity);
            }
            if (resultcode == "1004")
            {
                JobItem = JobItem.PayFail(Entity);
            }
            Response.Write("0000");
        }
    }
}
