using LokFu.Extensions;
using LokFu.Infrastructure;
using LokFu.Repositories;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Web;
using ThoughtWorks.QRCode.Codec;
namespace LokFu.Areas.Shop.Controllers
{
    public class OrdersController : BaseController
    {
        public string Shop_ENO = "00000000-0000-0000-0000-00000001";
        public string Shop_Keys = "120df9d3b7cf977e9b5185d6a123f3fe120df9d3b7cf977e9b5185d6a123f3fe";
        public void QrCodePay(string paytype, decimal amount, int PayWay = 0)//生成二维码
        {
            Response.AddHeader("content-type", "application/json");
            if (BasicUsers.Token.IsNullOrEmpty())
            {
                BasicUsers.Token = DateTime.Now.ToString().GetMD5();
                Entity.SaveChanges();
            }
            string otype = "0";
            if (paytype == "WxSao")
            {
                otype = "8";
            }
            if (paytype == "AliSao")
            {
                otype = "7";
            }
            if (otype == "0")
            {
                Response.Write("{\"err\":\"1\",\"msg\":\"参数有误\"}");
                return;
            }
            if (amount <= 0) {
                Response.Write("{\"err\":\"1\",\"msg\":\"交易金额有误！\"}");
                return;
            }
            string PostJson = "{\"amoney\":" + amount.ToString("F2") + ",\"token\":\"" + BasicUsers.Token + "\",\"otype\":" + otype + ",\"payway\":" + PayWay + ",\"action\":\"Create\",\"x\":\"0\",\"y\":\"0\",\"orderaddress\":\"超级收银台:" + Utils.GetAddressAndIp() + "\",ip:\"" + Utils.GetIP() + "\"}";
            //提交数据
            string PostData = LokFuEncode.LokFuAPIEncode(PostJson, Shop_Keys);
            PostData = HttpUtility.UrlEncode(PostData);
            //Post参数
            string PostString = "eno=" + Shop_ENO + "&data=" + PostData + "&code=0000";
            string url = AppPath + "/API/OrderQC/";
            string RetString = Utils.PostRequest(url, PostString, "utf-8");
            JObject json = new JObject();
            try
            {
                json = (JObject)JsonConvert.DeserializeObject(RetString);
            }
            catch (Exception)
            {
                Response.Write("{\"err\":\"1\",\"msg\":\"接口数据有误！[01]\"}");
                return;
            }
            if (json == null)
            {
                Response.Write("{\"err\":\"1\",\"msg\":\"接口数据有误！[02]\"}");
                return;
            }
            string code = "";
            string data = "";
            try
            {
                code = json["code"].ToString();
                data = json["data"].ToString();
            }
            catch (Exception)
            {
                Response.Write("{\"err\":\"1\",\"msg\":\"接口数据有误！[03]\"}");
                return;
            }
            if (code != "0000")
            {
                Response.Write("{\"err\":\"1\",\"msg\":\"交易有误！[" + code + "]\"}");
                return;
            }
            //解密
            string RetData = LokFuEncode.LokFuAPIDecode(data, Shop_Keys);
            JObject Json = new JObject();
            try
            {
                Json = (JObject)JsonConvert.DeserializeObject(RetData);
            }
            catch (Exception)
            {
                Response.Write("{\"err\":\"1\",\"msg\":\"数据解析有误！[01]\"}");
                return;
            }
            if (Json == null)
            {
                Response.Write("{\"err\":\"1\",\"msg\":\"数据解析有误！[02]\"}");
                return;
            }
            Orders Orders = new Orders();
            Orders = JsonToObject.ConvertJsonToModel(Orders, Json);
            if (Orders.PayId.IsNullOrEmpty())
            {
                Response.Write("{\"err\":\"1\",\"msg\":\"订单信息有误！\"}");
                return;
            }
            string tnum = Orders.TNum;
            string payid = Orders.PayId;
            string FilePath = Server.MapPath("/UpLoadFiles/ShopOrders/" + tnum + ".gif");        //服务器端文件路径
            QRCodeEncoder Encoder = new QRCodeEncoder();
            //QRCode("Byte", 4, 7, "M", "A");
            Encoder.QRCodeEncodeMode = QRCodeEncoder.ENCODE_MODE.BYTE;//二维码编码方式
            Encoder.QRCodeErrorCorrect = QRCodeEncoder.ERROR_CORRECTION.M;//纠错码等级
            Encoder.QRCodeScale = 5;//每个小方格的宽度
            Encoder.QRCodeVersion = 5;//二维码版本号
            //动态调整二维码版本号,上限40，过长返回空白图片，编码后字符最大字节长度2953
            Bitmap image = Encoder.Encode(payid, Encoding.UTF8);
            image.Save(FilePath, ImageFormat.Gif);
            Response.Write("{\"err\":\"0\",\"msg\":\"/UpLoadFiles/ShopOrders/" + tnum + ".gif\",\"tnum\":\"" + tnum + "\"}");
            return;
        }
        public void CodePay(string payid, string paytype, decimal amount, int PayWay = 0)//条码支付
        {
            Response.AddHeader("content-type", "application/json");
            if (BasicUsers.Token.IsNullOrEmpty())
            {
                BasicUsers.Token = DateTime.Now.ToString().GetMD5();
                Entity.SaveChanges();
            }
            string otype = "0";
            if (paytype == "WxCode")
            {
                otype = "8";
            }
            if (paytype == "AliCode")
            {
                otype = "7";
            }
            if (otype == "0")
            {
                Response.Write("{\"err\":\"1\",\"msg\":\"参数有误\"}");
                return;
            }
            if (amount <= 0)
            {
                Response.Write("{\"err\":\"1\",\"msg\":\"交易金额有误！\"}");
                return;
            }
            if (payid.IsNullOrEmpty())
            {
                Response.Write("{\"err\":\"1\",\"msg\":\"请扫描条码！\"}");
                return;
            }
            int InType = 0;
            if (BasicSet.LagEntry == 1)
            {
                if (BasicUsers.InTypePC == 1)
                {
                    InType = 1;
                }
            }

            string PostJson = "{\"amoney\":" + amount.ToString("F2") + ",\"token\":\"" + BasicUsers.Token + "\",\"otype\":" + otype + ",\"payway\":" + PayWay + ",\"payid\":\"" + payid + "\",\"action\":\"Create\",\"x\":\"0\",\"y\":\"0\",\"intype\":\"" + InType + "\",\"orderaddress\":\"超级收银台:" + Utils.GetAddressAndIp() + "\",ip:\"" + Utils.GetIP() + "\"}";

            //提交数据
            string PostData = LokFuEncode.LokFuAPIEncode(PostJson, Shop_Keys);
            PostData = HttpUtility.UrlEncode(PostData);
            //Post参数
            string PostString = "eno=" + Shop_ENO + "&data=" + PostData + "&code=0000";
            string url = AppPath + "/API/OrderF2F/";
            string RetString = Utils.PostRequest(url, PostString, "utf-8");
            JObject json = new JObject();
            try
            {
                json = (JObject)JsonConvert.DeserializeObject(RetString);
            }
            catch (Exception)
            {
                Response.Write("{\"err\":\"1\",\"msg\":\"接口数据有误！[01]\"}");
                return;
            }
            if (json == null)
            {
                Response.Write("{\"err\":\"1\",\"msg\":\"接口数据有误！[02]\"}");
                return;
            }
            string code = "";
            string data = "";
            try
            {
                code = json["code"].ToString();
                data = json["data"].ToString();
            }
            catch (Exception)
            {
                Response.Write("{\"err\":\"1\",\"msg\":\"接口数据有误！[03]\"}");
                return;
            }
            if (code != "0000")
            {
                Response.Write("{\"err\":\"1\",\"msg\":\"交易有误！[" + code + "]\"}");
                return;
            }
            //解密
            string RetData = LokFuEncode.LokFuAPIDecode(data, Shop_Keys);
            JObject Json = new JObject();
            try
            {
                Json = (JObject)JsonConvert.DeserializeObject(RetData);
            }
            catch (Exception)
            {
                Response.Write("{\"err\":\"1\",\"msg\":\"数据解析有误！[01]\"}");
                return;
            }
            if (Json == null)
            {
                Response.Write("{\"err\":\"1\",\"msg\":\"数据解析有误！[02]\"}");
                return;
            }
            Orders Orders = new Orders();
            Orders = JsonToObject.ConvertJsonToModel(Orders, Json);
            byte paystate = Orders.PayState;
            string tnum = Orders.TNum;
            string msg = "Wait";
            if (paystate == 1) {
                msg = "OK";
            }
            Response.Write("{\"err\":\"0\",\"msg\":\"" + msg + "\",\"tnum\":\"" + tnum + "\"}");
            return;
        }
        public void Query(string tnum) {
            Response.AddHeader("content-type", "application/json");
            if (tnum.IsNullOrEmpty())
            {
                Response.Write("{\"err\":\"1\",\"msg\":\"参数有误\"}");
                return;
            }
            Orders Orders = Entity.Orders.FirstOrDefault(n => n.TNum == tnum && n.UId == BasicUsers.Id);
            if (Orders == null)
            {
                Response.Write("{\"err\":\"1\",\"msg\":\"参数有误\"}");
                return;
            }
            string PostJson = "{\"oid\":\"" + tnum + "\",\"token\":\"" + BasicUsers.Token + "\",\"action\":\"GET\"}";

            //提交数据
            string PostData = LokFuEncode.LokFuAPIEncode(PostJson, Shop_Keys);
            PostData = HttpUtility.UrlEncode(PostData);
            //Post参数
            string PostString = "eno=" + Shop_ENO + "&data=" + PostData + "&code=0000";
            string url = AppPath + "/API/OrderF2F/";
            string RetString = Utils.PostRequest(url, PostString, "utf-8");
            JObject json = new JObject();
            try
            {
                json = (JObject)JsonConvert.DeserializeObject(RetString);
            }
            catch (Exception)
            {
                Response.Write("{\"err\":\"1\",\"msg\":\"接口数据有误！[01]\"}");
                return;
            }
            if (json == null)
            {
                Response.Write("{\"err\":\"1\",\"msg\":\"接口数据有误！[02]\"}");
                return;
            }
            string code = "";
            string data = "";
            try
            {
                code = json["code"].ToString();
                data = json["data"].ToString();
            }
            catch (Exception)
            {
                Response.Write("{\"err\":\"1\",\"msg\":\"接口数据有误！[03]\"}");
                return;
            }
            if (code != "0000")
            {
                Response.Write("{\"err\":\"1\",\"msg\":\"交易有误！[" + code + "]\"}");
                return;
            }
            //解密
            string RetData = LokFuEncode.LokFuAPIDecode(data, Shop_Keys);
            JObject Json = new JObject();
            try
            {
                Json = (JObject)JsonConvert.DeserializeObject(RetData);
            }
            catch (Exception)
            {
                Response.Write("{\"err\":\"1\",\"msg\":\"数据解析有误！[01]\"}");
                return;
            }
            if (Json == null)
            {
                Response.Write("{\"err\":\"1\",\"msg\":\"数据解析有误！[02]\"}");
                return;
            }
            Orders = JsonToObject.ConvertJsonToModel(Orders, Json);
            byte paystate = Orders.PayState;
            byte tstate = Orders.TState;
            string msg = "Wait";
            if (paystate == 1)
            {
                msg = "OK";
            }
            else if (tstate == 0)
            {
                msg = "Fail";
            }
            Response.Write("{\"err\":\"0\",\"msg\":\"" + msg + "\",\"tnum\":\"" + tnum + "\"}");
        }
        public void Cancel(string tnum)
        {
            Response.AddHeader("content-type", "application/json");
            if (tnum.IsNullOrEmpty())
            {
                Response.Write("{\"err\":\"1\",\"msg\":\"参数有误\"}");
                return;
            }
            Orders Orders = Entity.Orders.FirstOrDefault(n => n.TNum == tnum && n.UId == BasicUsers.Id);
            if (Orders == null)
            {
                Response.Write("{\"err\":\"1\",\"msg\":\"参数有误\"}");
                return;
            }
            if (Orders.TState == 0) {
                Response.Write("{\"err\":\"0\",\"msg\":\"OK\",\"tnum\":\"" + tnum + "\"}");
                return;
            }
            string PostJson = "{\"oid\":\"" + tnum + "\",\"token\":\"" + BasicUsers.Token + "\",\"action\":\"Cancel\"}";
            //提交数据
            string PostData = LokFuEncode.LokFuAPIEncode(PostJson, Shop_Keys);
            PostData = HttpUtility.UrlEncode(PostData);
            //Post参数
            string PostString = "eno=" + Shop_ENO + "&data=" + PostData + "&code=0000";
            string url = AppPath + "/API/OrderF2F/";
            string RetString = Utils.PostRequest(url, PostString, "utf-8");
            JObject json = new JObject();
            try
            {
                json = (JObject)JsonConvert.DeserializeObject(RetString);
            }
            catch (Exception)
            {
                Response.Write("{\"err\":\"1\",\"msg\":\"接口数据有误！[01]\"}");
                return;
            }
            if (json == null)
            {
                Response.Write("{\"err\":\"1\",\"msg\":\"接口数据有误！[02]\"}");
                return;
            }
            string code = "";
            string data = "";
            try
            {
                code = json["code"].ToString();
                data = json["data"].ToString();
            }
            catch (Exception)
            {
                Response.Write("{\"err\":\"1\",\"msg\":\"接口数据有误！[03]\"}");
                return;
            }
            if (code != "0000")
            {
                Response.Write("{\"err\":\"1\",\"msg\":\"交易有误！[" + code + "]\"}");
                return;
            }
            //解密
            string RetData = LokFuEncode.LokFuAPIDecode(data, Shop_Keys);
            JObject Json = new JObject();
            try
            {
                Json = (JObject)JsonConvert.DeserializeObject(RetData);
            }
            catch (Exception)
            {
                Response.Write("{\"err\":\"1\",\"msg\":\"数据解析有误！[01]\"}");
                return;
            }
            if (Json == null)
            {
                Response.Write("{\"err\":\"1\",\"msg\":\"数据解析有误！[02]\"}");
                return;
            }
            Orders = JsonToObject.ConvertJsonToModel(Orders, Json);
            byte tstate = Orders.TType;
            byte paystate = Orders.PayState;
            string msg = "操作成功";
            if (tstate == 0)
            {
                msg = "取消交易成功！";
            }
            else {
                if (paystate == 1) {
                    msg = "交易已经支付！";
                }
            }
            Response.Write("{\"err\":\"0\",\"msg\":\"" + msg + "\",\"tnum\":\"" + tnum + "\"}");
            return;
        }
    }
}
 