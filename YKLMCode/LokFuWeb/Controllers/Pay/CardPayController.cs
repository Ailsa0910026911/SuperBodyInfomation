using LokFu.Extensions;
using LokFu.Infrastructure;
using LokFu.Repositories;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace LokFu.Areas.Pay.Controllers
{
    public class CardPayController : BaseController
    {
        private string DllName = "CardPay";
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
            ViewBag.FastOrder = FastOrder;

            Users Users = Entity.Users.FirstOrDefault(n => n.Id == FastOrder.UId);
            
            ViewBag.Users = Users;

            IList<UsersPayCard> UsersPayCardList = Entity.UsersPayCard.Where(n => n.UId == Users.Id && n.State == 1).ToList();
            ViewBag.UsersPayCardList = UsersPayCardList;

            ViewBag.etnum = etnum;

            return View();
        }

        public ActionResult AddCard(string etnum, string Card = "", string Mobile = "", string code = "")
        {
            string tnum = LokFuEncode.LokFuAPIDecode(etnum, DllName);
            FastOrder FastOrder = Entity.FastOrder.FirstOrDefault(n => n.TNum == tnum);
            if (FastOrder == null)
            {
                ViewBag.ErrorMsg = "订单有误！";
                return View("Error");
            }
            Users Users = Entity.Users.FirstOrDefault(n => n.Id == FastOrder.UId);
            if (Users == null)
            {
                ViewBag.ErrorMsg = "商户信息有误！";
                return View("Error");
            }

            ViewBag.FastOrder = FastOrder;
            ViewBag.Users = Users;
            ViewBag.etnum = etnum;

            if (!Card.IsNullOrEmpty() && !Mobile.IsNullOrEmpty() && !code.IsNullOrEmpty())
            {
                if (Card.Length > 20 )
                {
                    ViewBag.ErrorMsg = "卡号长度超限！";
                    return View("Error");
                }
                if (Mobile.Length > 20)
                {
                    ViewBag.ErrorMsg = "手机号码长度超限！";
                    return View("Error");
                }
                if (code.ToUpper() != Session.GetCheckCode())
                {
                    ViewBag.ErrorMsg = "验证码错误！";
                    return View("Error");
                }
                Session.ClearCheckCode();
                //绑卡前验证是否有鉴权权限
                UsersPayCard UsersPayCard = Entity.UsersPayCard.FirstOrDefault(n => n.Card == Card && n.Mobile == Mobile && n.UId == FastOrder.UId);
                if (UsersPayCard == null)//不存在则绑卡
                {
                    UserBlackList UserBlackList = Entity.UserBlackList.FirstOrDefault(UBL => UBL.CardNumber == Card && UBL.State == 3);
                    if (UserBlackList != null)
                    {
                        ViewBag.ErrorMsg = "暂不支持该银行卡绑卡";
                        return View("Error");
                    }

                    string HaoFu_Auth_MerId = ConfigurationManager.AppSettings["HaoFu_Auth_MerId"].ToString();
                    string HaoFu_Auth_MerKey = ConfigurationManager.AppSettings["HaoFu_Auth_MerKey"].ToString();
                    string HaoFu_Auth_Url = ConfigurationManager.AppSettings["HaoFu_Auth_Url"].ToString();

                    string data = "{\"action\":\"authuser\",\"merid\":\"" + HaoFu_Auth_MerId + "\",\"orderid\":\"" + FastOrder.TNum + "\",\"bankaccount\":\"" + Card + "\",\"accountname\":\"" + Users.TrueName + "\",\"identitycode\":\"" + Users.CardId + "\",\"mobile\":\"" + Mobile + "\"}";
                    string DataBase64 = LokFuEncode.Base64Encode(data, "utf-8");
                    string Sign = (DataBase64 + HaoFu_Auth_MerKey).GetMD5();

                    DataBase64 = HttpUtility.UrlEncode(DataBase64, Encoding.UTF8);
                    string postdata = "req=" + DataBase64 + "&sign=" + Sign;

                    string CONTENT = Utils.PostRequest(HaoFu_Auth_Url, postdata, "utf-8");

                    JObject JS = new JObject();
                    try
                    {
                        JS = (JObject)JsonConvert.DeserializeObject(CONTENT);
                    }
                    catch (Exception)
                    {
                        ViewBag.ErrorMsg = "请求银联鉴权失败！【00】";
                        return View("Error");
                    }
                    string resp = JS["resp"].ToString();
                    CONTENT = LokFuEncode.Base64Decode(resp, "utf-8");
                    try
                    {
                        JS = (JObject)JsonConvert.DeserializeObject(CONTENT);
                    }
                    catch (Exception)
                    {
                        ViewBag.ErrorMsg = "请求银联鉴权失败！【01】";
                        return View("Error");
                    }
                    string ret_code = JS["respcode"].ToString();
                    
                    if (ret_code == "0000")
                    {
                        string BankNumLeft = Card.Substring(0, 6);
                        //获取卡Bin
                        BasicCardBin BasicCardBin = Entity.BasicCardBin.FirstOrDefault(n => n.BIN == BankNumLeft);
                        string bankName = "银联卡";
                        byte CardType = 0;
                        if (BasicCardBin != null)
                        {
                            bankName = BasicCardBin.BankName;
                            CardType = (byte)BasicCardBin.CardType;
                        }
                        else { 
                            
                        }
                        UsersPayCard = new UsersPayCard();
                        UsersPayCard.UId = FastOrder.UId;
                        UsersPayCard.Type = CardType;
                        UsersPayCard.Bank = bankName;
                        UsersPayCard.Name = Users.TrueName;
                        UsersPayCard.Card = Card;
                        UsersPayCard.Mobile = Mobile;
                        UsersPayCard.State = 1;
                        UsersPayCard.AddTime = DateTime.Now;
                        Entity.UsersPayCard.AddObject(UsersPayCard);
                        Entity.SaveChanges();
                        Response.Redirect("GoPay.html?BankId=" + UsersPayCard.Id + "&etnum=" + HttpUtility.UrlEncode(etnum));
                    }
                    else
                    {
                        string ret_msg = JS["respmsg"].ToString();
                        ViewBag.ErrorMsg = "银行卡认证失败！";
                        return View("Error");
                    }
                }
                else {
                    if (UsersPayCard.State == 0)
                    {
                        UsersPayCard.State = 1;
                        Entity.SaveChanges();
                    }
                    Response.Redirect("GoPay.html?BankId=" + UsersPayCard.Id + "&etnum=" + HttpUtility.UrlEncode(etnum));
                }
            }
            return View();
        }

        public void GoPay(string etnum, int BankId)
        {
            if (etnum.IsNullOrEmpty() || BankId.IsNullOrEmpty())
            {
                Response.Write("Some Error[00]");
                return;
            }
            string tnum = LokFuEncode.LokFuAPIDecode(etnum, DllName);
            FastOrder FastOrder = Entity.FastOrder.FirstOrDefault(n => n.TNum == tnum);
            if (FastOrder == null)
            {
                Response.Write("Some Error[02]");
                return;
            }
            FastPayWay FastPayWay = Entity.FastPayWay.FirstOrDefault(n => n.Id == FastOrder.PayWay);
            if (FastOrder == null)
            {
                Response.Write("Some Error[03]");
                return;
            }
            Response.Redirect("/paycenter/" + FastPayWay.DllName.ToLower() + "/gopay.html?etnum=" + HttpUtility.UrlEncode(LokFuEncode.LokFuAPIEncode(FastOrder.TNum + "|" + BankId, FastPayWay.DllName)));
        }

        public string DelCard(string etnum, int id = 0)
        {
            string tnum = LokFuEncode.LokFuAPIDecode(etnum, DllName);
            FastOrder FastOrder = Entity.FastOrder.FirstOrDefault(n => n.TNum == tnum);
            if (FastOrder == null)
            {
                return "0";
            }
            Users Users = Entity.Users.FirstOrDefault(n => n.Id == FastOrder.UId);
            if (Users == null)
            {
                return "0";
            }
            UsersPayCard UsersPayCard = Entity.UsersPayCard.FirstOrDefault(n => n.UId == FastOrder.UId && n.Id == id);
            if (UsersPayCard == null)
            {
                return "0";
            }
            UsersPayCard.State = 0;
            Entity.SaveChanges();
            return "1";
        }

    }
}
