using System.Linq;
using System.Web.Mvc;
using LokFu.Repositories;
using Newtonsoft.Json.Linq;
using System;
using Newtonsoft.Json;
using System.Web;
using LokFu.Infrastructure;
using System.Text;
using System.Collections.Generic;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Crypto.Parameters;
using LokFu.HFJS.HFJSModels;
using LokFu.HFJS.HFJSResults;
using LokFu.HFJS;
namespace LokFu.Areas.Pay.Controllers
{
    public class PayController : BaseController
    {
        public ActionResult GoPay(string tnum, string sign)
        {

            //FastUserPay FastUserPay = Entity.FastUserPay.FirstOrDefault(o => o.UId == 95 && o.BusiState == 1 && o.MerState == 1 && o.CardState == 1);
            //if (FastUserPay != null)
            //{
            //    FastPayWay FastPayWay = Entity.FastPayWay.FirstOrDefault(o => o.State == 1);
            //    FastConfig FastConfig = Entity.FastConfig.FirstOrDefault();
            //    decimal Cost = 0.005M;//费率
            //    decimal Cash = FastConfig.UserCash;//手续费
            //    string[] PayConfigArr = FastPayWay.QueryArray.Split(',');
            //    string Code = PayConfigArr[0];
            //    string CodeKey = PayConfigArr[1];
            //    string PayWayCode = PayConfigArr[2];
            //    userspayopenbModel userspayopenbModel = new userspayopenbModel()
            //    {
            //        merid = FastUserPay.MerId,
            //        paywaycode = PayWayCode,
            //        code = Code,
            //        bankcost = Cost,
            //        //surcharge = 0,
            //        cash = Cash,
            //        bankcostmin = 1.2M,
            //        bankcostmax = 9999999
            //    };
            //    fastuserResult fastuserResult = HFJSTools.userspayedit(userspayopenbModel, CodeKey);
            //    if (fastuserResult.respcode == "00")
            //    {
            //        if (fastuserResult.state == 1)
            //        {
            //            FastUserPay.BusiState = 1;
            //            FastUserPay.BusiMsg = fastuserResult.respcode + "升级Vip[" + fastuserResult.respmsg + "]";
            //            FastUserPay.UserCost3 = 0.005M;
            //        }
            //        else
            //        {
            //            FastUserPay.BusiState = 4;
            //            FastUserPay.BusiMsg = fastuserResult.respcode + "升级Vip[" + fastuserResult.respmsg + "]";
            //        }
            //    }
            //    else
            //    {
            //        FastUserPay.BusiState = 4;
            //        FastUserPay.BusiMsg = fastuserResult.respcode + "升级Vip[" + fastuserResult.respmsg + "]";
            //    }
            //}
            //JobItem JobItem = this.Entity.JobItem.FirstOrDefault(o => o.RunNum == tnum);
            //JobItem.PaySuccess(Entity);
            //string publickey = "MIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEAymjEgsUDB0s2LssEYUxu8eLygKSwMTErb3xlSdjmGP99P9j29+Yjw3fpfGJy383dIKVMdMr6muKQe73vPJ4pwIAXP6iWuSA6MXMycebw10n/J6uv6fQxkVnvdG4eh9+DqO/F9NVIyHHvbkL+pw/3TcwLNKzMlrbxspxl6JHZjJuOW6DET/QESWG9GYjZGxeX+hgEihdbKFeDumvuOJKxAtGK99OOYT0LUdFA+FmC09Lb5wecR8e4Pmr0r+6fEbwArX5tBDxAwkFiPml7aFA/X07Mw7vVxd1fEIptbL45sTbMDY/1OQMnFJFClQ9XQnMMTC1FP+jFQ+J6Cag9SCQ0+wIDAQAB";

            //publickey =RSAPublicKeyJava2DotNet(publickey);

            //string privetkey = "-----BEGIN RSA PRIVATE KEY-----MIIEpAIBAAKCAQEAymjEgsUDB0s2LssEYUxu8eLygKSwMTErb3xlSdjmGP99P9j29+Yjw3fpfGJy383dIKVMdMr6muKQe73vPJ4pwIAXP6iWuSA6MXMycebw10n/J6uv6fQxkVnvdG4eh9+DqO/F9NVIyHHvbkL+pw/3TcwLNKzMlrbxspxl6JHZjJuOW6DET/QESWG9GYjZGxeX+hgEihdbKFeDumvuOJKxAtGK99OOYT0LUdFA+FmC09Lb5wecR8e4Pmr0r+6fEbwArX5tBDxAwkFiPml7aFA/X07Mw7vVxd1fEIptbL45sTbMDY/1OQMnFJFClQ9XQnMMTC1FP+jFQ+J6Cag9SCQ0+wIDAQABAoIBAANtntZwK8r11ScZvStAtihWpbDUGT/cC1ZKDxUfHVaTDTYFoLk2Jqjq4QOAT13HCNXCtvbLcU8qQzQ1VVX7f82I3dvwKQ1dpF6uCs6vEKk00aOEXuhhgG1rzoh0TQQUHxC/buTkr+TlOR7u940w0gl5ST9NJfHvi0xxJbC4t6yP9jGptY8xH7S0OZHlz7I/MLOjr9fGpGk50FKYCPNOlDwl5/CDphe5JQGNHwyGXZxDJk7qjzU6hP7eLEbtUSVL9IzfG8Tk8gy/mHUYNh5L/wRTY2eVC1qmCVJk59Ve2fNgX4+Xt4l3QIKtM8UafYp1VYghxrWYscUWSPr1gT0IrZECgYEA8dqsJMOLg5LmqH+7BurrObNkdR2yoxCTmdXPawSgKtAqs4+/TaoiGRNvkMn3NwX26+4luR3kjOPndI0mG6qiHJsp5XVrmSxxJkMkr7wT2QTafmRqlVOqxXZqEUSZBi+7v9L9DT/QhYH9FGobyORqsrdn6nmKbDkOYo8Y9ohTVTUCgYEA1j96kYC4GAvUgIH2C18EeyTwkO5bc2U0kMaMAx/CqJ/wKWoE5CWFymosCyGaie343YGtRmI5X54MsTy1H6D3VOPSTlxfbYlw77bZCr+VHsiaMRbLuzZfz3hm4WXL34Dx1kTjXciVWLWzcXEAyLqLWbRL2U4d6tnsD8/H90nAl28CgYB5YRDCBq2vI/vFFw3LQ/034TOKn5P2yKv7KdZGVHT55612nq8ZEEi6owl79hCvObwZ3lRqWnlIyFxyHyDAA9wGVU0qjsqofihvVoVD+TVQ1mG9yzG+rdXn8iz7a50NAMnOVNqRWrSRPJLCOxqksRZniSqK/+Kbu+rfRgf2Oqbv/QKBgQC7++CwkE93EdI3m1Bmc817u1K0598n5hH0QbGSACkNSaAwxkxaXi4BB51zet2czkfBG0oM6pxKsGOUk56cjODXBDp/9P7qHmEQg8/2ZVkTNJJOJiFGN5RjdtJWTLpQdlQ7XCBfJmMb/fnfvHYiqcMhR1gR6D/J08ITsY1UBctNUwKBgQCigyL5orWcWIf/opoCD7R25fj0cpAHYuCyOjPqEuaBPmx0BqBJwpCpaVUCnciqXWlBHWBUIoN7DpKlRePxlZ/ACd+WO4kb1nRmrkXSJEaQMnHpWTF76vtN0IiqZcx7aLj9MpW/j54LXw5JbsIWR7vLFONIrD7nU6so1lq6WTcvuw==-----END RSA PRIVATE KEY-----";
            //privetkey = ConvertFromPemPrivateKey(privetkey);
            if (tnum.IsNullOrEmpty() || sign.IsNullOrEmpty())
            {
                ViewBag.ErrorMsg = "参数错误[01]！";
                return View("Error");
            }
            if ((tnum + "NewPay").GetMD5().Substring(8, 8) != sign)
            {
                ViewBag.ErrorMsg = "参数错误[00]！";
                return View("Error");
            }
            Orders Orders = Entity.Orders.FirstOrDefault(n => n.TNum == tnum);
            if (Orders == null)
            {
                ViewBag.ErrorMsg = "交易不存在！";
                return View("Error");
            }
            Users baseUsers = Entity.Users.FirstOrDefault(n => n.Id == Orders.UId);
            if (baseUsers == null)//用户不存在
            {
                ViewBag.ErrorMsg = "用户不存在或信息有误！";
                return View("Error");
            }
            if (baseUsers.State != 1)//用户被锁定
            {
                ViewBag.ErrorMsg = "用户被锁定！";
                return View("Error");
            }
            ViewBag.Orders = Orders;
            if (Orders.TType != 1 && Orders.TType != 3 && Orders.TType != 5 && Orders.TType != 6 && Orders.TType != 10)
            {
                ViewBag.ErrorMsg = "交易类型有误！";
                return View("Error");
            }
            if (Orders.PayWay == 4)
            {
                ViewBag.ErrorMsg = "请使用余额支付！";
                return View("Error");
            }
            if (Orders.TState != 1)
            {
                ViewBag.ErrorMsg = "交易成功或已取消！";
                return View("Error");
            }
            if (Orders.PayState == 1)
            {//已支付
                return View("Success");
            }
            bool ok = SetPayConfig(Orders);
            if (!ok)
            {
                ViewBag.ErrorMsg = "支付通道错误！";
                return View("Error");
            }
            //Orders Orders = Entity.Orders.FirstOrDefault(n => n.TNum == tnum);
            //Orders.PaySuccess(Entity);
            //VIPOrder VIPOrder = Entity.VIPOrder.FirstOrDefault(n => n.TNum == tnum);
            //if (VIPOrder != null)
            //{
            //    VIPOrder = VIPOrder.PayAgent(Entity, 1);
            //    //O.AgentPayGet = (decimal)VIPOrder.SplitMoney;
            //}
            //FastOrder FastOrder = Entity.FastOrder.FirstOrDefault(n => n.TNum == tnum);
            //FastOrder.PaySuccess(Entity);
            //FastOrder.PayAgent(Entity, 1);

            //Orders Orders = Entity.Orders.FirstOrDefault(n => n.TNum == tnum);
            //Orders.PaySuccess(Entity);
            //DaiLiOrder DaiLiOrder = Entity.DaiLiOrder.FirstOrDefault(n => n.OId == tnum);
            // DaiLiOrder = DaiLiOrder.PayAgent(Entity, 1);
            //JobOrders job = Entity.JobOrders.FirstOrDefault(o => o.TNum == tnum);
            //job.PayAgent(Entity);
            return View("Index");
        }
        /// <summary>
        /// 将pem格式私钥(1024 or 2048)转换为RSAParameters
        /// </summary>
        /// <param name="pemFileConent">pem私钥内容</param>
        /// <returns>转换得到的RSAParamenters</returns>
        public string ConvertFromPemPrivateKey(string pemFileConent)
        {
            if (string.IsNullOrEmpty(pemFileConent))
            {
                throw new ArgumentNullException("pemFileConent", "This arg cann't be empty.");
            }
            pemFileConent = pemFileConent.Replace("-----BEGIN RSA PRIVATE KEY-----", "").Replace("-----END RSA PRIVATE KEY-----", "").Replace("\n", "").Replace("\r", "");
            byte[] keyData = Convert.FromBase64String(pemFileConent);

            bool keySize1024 = (keyData.Length == 608 || keyData.Length == 610);
            bool keySize2048 = (keyData.Length == 1190 || keyData.Length == 1192);

            if (!(keySize1024 || keySize2048))
            {
                throw new ArgumentException("pem file content is incorrect, Only support the key size is 1024 or 2048");
            }

            int index = (keySize1024 ? 11 : 12);
            byte[] pemModulus = (keySize1024 ? new byte[128] : new byte[256]);
            Array.Copy(keyData, index, pemModulus, 0, pemModulus.Length);

            index += pemModulus.Length;
            index += 2;
            byte[] pemPublicExponent = new byte[3];
            Array.Copy(keyData, index, pemPublicExponent, 0, 3);

            index += 3;
            index += 4;
            if ((int)keyData[index] == 0)
            {
                index++;
            }
            byte[] pemPrivateExponent = (keySize1024 ? new byte[128] : new byte[256]);
            Array.Copy(keyData, index, pemPrivateExponent, 0, pemPrivateExponent.Length);

            index += pemPrivateExponent.Length;
            index += (keySize1024 ? ((int)keyData[index + 1] == 64 ? 2 : 3) : ((int)keyData[index + 2] == 128 ? 3 : 4));
            byte[] pemPrime1 = (keySize1024 ? new byte[64] : new byte[128]);
            Array.Copy(keyData, index, pemPrime1, 0, pemPrime1.Length);

            index += pemPrime1.Length;
            index += (keySize1024 ? ((int)keyData[index + 1] == 64 ? 2 : 3) : ((int)keyData[index + 2] == 128 ? 3 : 4));
            byte[] pemPrime2 = (keySize1024 ? new byte[64] : new byte[128]);
            Array.Copy(keyData, index, pemPrime2, 0, pemPrime2.Length);

            index += pemPrime2.Length;
            index += (keySize1024 ? ((int)keyData[index + 1] == 64 ? 2 : 3) : ((int)keyData[index + 2] == 128 ? 3 : 4));
            byte[] pemExponent1 = (keySize1024 ? new byte[64] : new byte[128]);
            Array.Copy(keyData, index, pemExponent1, 0, pemExponent1.Length);

            index += pemExponent1.Length;
            index += (keySize1024 ? ((int)keyData[index + 1] == 64 ? 2 : 3) : ((int)keyData[index + 2] == 128 ? 3 : 4));
            byte[] pemExponent2 = (keySize1024 ? new byte[64] : new byte[128]);
            Array.Copy(keyData, index, pemExponent2, 0, pemExponent2.Length);

            index += pemExponent2.Length;
            index += (keySize1024 ? ((int)keyData[index + 1] == 64 ? 2 : 3) : ((int)keyData[index + 2] == 128 ? 3 : 4));
            byte[] pemCoefficient = (keySize1024 ? new byte[64] : new byte[128]);
            Array.Copy(keyData, index, pemCoefficient, 0, pemCoefficient.Length);

            return string.Format("<RSAKeyValue><Modulus>{0}</Modulus><Exponent>{1}</Exponent><P>{2}</P><Q>{3}</Q><DP>{4}</DP><DQ>{5}</DQ><InverseQ>{6}</InverseQ><D>{7}</D></RSAKeyValue>",
                Convert.ToBase64String(pemModulus),
                Convert.ToBase64String(pemPublicExponent),
                Convert.ToBase64String(pemPrime1),
                Convert.ToBase64String(pemPrime2),
                Convert.ToBase64String(pemExponent1),
                Convert.ToBase64String(pemExponent2),
                Convert.ToBase64String(pemCoefficient),
                Convert.ToBase64String(pemPrivateExponent));
        }

        /// <summary>
        /// RSA公钥格式转换，java->.net
        /// </summary>
        /// <param name="publicKey">java生成的公钥</param>
        /// <returns></returns>
        public string RSAPublicKeyJava2DotNet(string publicKey)
        {

            RsaKeyParameters publicKeyParam = (RsaKeyParameters)PublicKeyFactory.CreateKey(Convert.FromBase64String(publicKey));

            return string.Format("<RSAKeyValue><Modulus>{0}</Modulus><Exponent>{1}</Exponent></RSAKeyValue>",
                Convert.ToBase64String(publicKeyParam.Modulus.ToByteArrayUnsigned()),
                Convert.ToBase64String(publicKeyParam.Exponent.ToByteArrayUnsigned()));
        }


        public ActionResult Index(string data, string eno)
        {
            if (data.IsNullOrEmpty())
            {
                ViewBag.ErrorMsg = "data参数错误！";
                return View("Error");
            }
            if (eno.IsNullOrEmpty())
            {
                ViewBag.ErrorMsg = "eno参数错误！";
                return View("Error");
            }
            Equipment Equipment = Entity.Equipment.FirstOrDefault(n => n.No == eno);
            if (Equipment == null)
            {
                ViewBag.ErrorMsg = "设备故障！";
                return View("Error");
            }
            string Key = Equipment.Keys;
            string Json = LokFuEncode.LokFuAPIDecode(data, Key);
            JObject json = new JObject();
            try
            {
                json = (JObject)JsonConvert.DeserializeObject(Json);
            }
            catch (Exception)
            {
                ViewBag.ErrorMsg = "json参数错误！";
                return View("Error");
            }
            if (json == null)
            {
                ViewBag.ErrorMsg = "json数据错误！";
                return View("Error");
            }
            Orders order = new Orders();
            order = JsonToObject.ConvertJsonToModel(order, json);
            if (order.TNum.IsNullOrEmpty() || order.Token.IsNullOrEmpty())
            {
                ViewBag.ErrorMsg = "json参数错误[02]！";
                return View("Error");
            }
            Users baseUsers = Entity.Users.FirstOrDefault(n => n.Token == order.Token);
            if (baseUsers == null)//用户令牌不存在
            {
                ViewBag.ErrorMsg = "用户不存在或登录信息有误！";
                return View("Error");
            }
            if (baseUsers.State != 1)//用户被锁定
            {
                ViewBag.ErrorMsg = "用户被锁定！";
                return View("Error");
            }
            Orders Orders = Entity.Orders.FirstOrDefault(n => n.TNum == order.TNum);
            if (Orders == null)
            {
                ViewBag.ErrorMsg = "交易不存在！";
                return View("Error");
            }
            if (baseUsers.Id != Orders.UId)//禁止代付
            {
                ViewBag.ErrorMsg = "禁止代付！";
                return View("Error");
            }
            ViewBag.Orders = Orders;
            if (Orders.TType != 1 && Orders.TType != 3 && Orders.TType != 5 && Orders.TType != 6 && Orders.TType != 10)
            {
                ViewBag.ErrorMsg = "交易类型有误！";
                return View("Error");
            }
            if (Orders.PayWay == 4)
            {
                ViewBag.ErrorMsg = "请使用余额支付！";
                return View("Error");
            }
            if (Orders.TState != 1)
            {
                ViewBag.ErrorMsg = "交易成功或已取消！";
                return View("Error");
            }
            if (Orders.PayState == 1)
            {//已支付
                return View("Success");
            }
            bool ok = SetPayConfig(Orders);
            if (!ok)
            {
                ViewBag.ErrorMsg = "支付通道错误！";
                return View("Error");
            }
            return View();
        }
        private bool SetPayConfig(Orders Orders)
        {
            if (Orders.PayWay != 0)
            {
                PayConfig PayConfig = Entity.PayConfig.FirstOrDefault(n => n.Id == Orders.PayWay && n.State == 1);
                if (PayConfig == null)
                {
                    return false;
                }
                if (PayConfig.GroupType != "Bank") {
                    return false;
                }
                ViewBag.PayConfig = PayConfig;
            }
            else {
                //获取最佳支付通道
                IList<PayConfig> PayConfigList = Entity.PayConfig.Where(n => n.State == 1 && n.GroupType == "Bank").ToList();
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
                        if (Orders.Amoney >= (decimal)T.SNum && Orders.Amoney <= (decimal)T.ENum)
                        {
                            PCList.Add(p);
                        }
                    }
                }
                PayConfig PayConfig = PCList.OrderBy(n => n.Cost).FirstOrDefault();
                if (PayConfig == null)
                {
                    return false;
                }
                ViewBag.PayConfig = PayConfig;
                if (Orders.TType == 6)
                {
                    //PayConfigOrder PayConfigOrder = Entity.PayConfigOrder.FirstOrDefault(n => n.OId == Orders.TNum);
                    //PayConfigOrder.SysRate = 0;
                    //PayConfigOrder.Poundage = PayConfigOrder.Amoney * (decimal)PayConfigOrder.SysRate;
                    //Orders.Poundage = PayConfigOrder.Poundage;
                    Orders.PayWay = PayConfig.Id;
                }
                else if (Orders.TType == 10)
                {
                    Orders.PayWay = PayConfig.Id;
                }
                else {
                    return false;
                }
                Entity.SaveChanges();
            }
            return true;
        }
    }
}
