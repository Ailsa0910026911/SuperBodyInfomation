
using CTModel;
using Newtonsoft.Json;
using SBIModel;
using SuperBodyInfomation.Extended;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using Webdiyer.WebControls.Mvc;
using System.Globalization;
using Com.Alipay;

namespace SuperBodyInfomation.Control
{
    public class DefaultController : Controller
    {

        // GET: Default
        public ActionResult Index()
        {
            return View();
        }
        //public ActionResult Orders(int id = 1)
        //{
        //    using (SBIContext sc = new SBIContext())
        //    {
        //        //查询已经付款成功的订单
        //        var model = sc.ordersinfo.Where(o => o.PayStatus == 1).ToList();
        //        ViewBag.OrderNum = model.Count();
        //        model = model.OrderByDescending(o => o.DateTime).ToPagedList(id, 5);
        //        if (Request.IsAjaxRequest())
        //            return PartialView("_OrdersTable", model);
        //        return View(model);
        //    }
        //}
        public ActionResult Orders(string phone = "", DateTime? startTime = null, DateTime? endTime = null, int id = 1)
        {
            //根据不同的条件查询相关订单
            using (SBIContext sc = new SBIContext())
            {
                //查询已经付款成功的订单
                var model = sc.ordersinfo.Where(o => o.PayStatus == 1).ToList();
                if (phone != "")
                    model = model.Where(o => o.Phone == phone).ToList();
                if (startTime != null)
                    model = model.Where(o => o.DateTime >= startTime).ToList();
                if (endTime != null)
                    model = model.Where(o => o.DateTime <= endTime).ToList();
                ViewBag.OrderNum = model.Count();
                model = model.OrderByDescending(o => o.DateTime).ToPagedList(id, 20);
                if (Request.IsAjaxRequest())
                    return PartialView("_OrdersTable", model);
                return View(model);
            }
        }


        public ActionResult Gift()
        {
            return View();
        }
        public ActionResult Gifts()
        {
            return View();
        }
        public ActionResult Goods()
        {
            return View();
        }
        public ActionResult OrderInfo()
        {
            return View();
        }
        [HttpPost]
        public ActionResult OrderInfo(string UserName, string Phone, string Adress)
        {

            using (CTContext cc = new CTContext())
            {
                var result = cc.Users.Where(o => o.UserName == Phone && o.IsVip == 1).ToList();
                if (result.Count() > 0)
                {
                    //生成订单ID
                    var id = MD5Helper.getMd5Hash(Phone + DateTime.Now.ToString());
                    if (string.IsNullOrWhiteSpace(UserName))
                    {
                        Response.Write("<script languge='javascript'>alert('名字不能为空！');</script>");
                        return View();
                    }
                    if (string.IsNullOrWhiteSpace(Phone))
                    {
                        Response.Write("<script languge='javascript'>alert('手机号码不能为空！');</script>");
                        return View();
                    }
                    //电信手机号码正则        
                    string dianxin = @"^1[3578][01379]\d{8}$";
                    Regex dReg = new Regex(dianxin);
                    //联通手机号正则        
                    string liantong = @"^1[34578][01256]\d{8}$";
                    Regex tReg = new Regex(liantong);
                    //移动手机号正则        
                    string yidong = @"^(134[012345678]\d{7}|1[34578][012356789]\d{8})$";
                    Regex yReg = new Regex(yidong);

                    if (!dReg.IsMatch(Phone) && !tReg.IsMatch(Phone) && !yReg.IsMatch(Phone))
                    {
                        Response.Write("<script languge='javascript'>alert('手机号码错误，请填写正确号码！');</script>");
                        return View();
                    }
                    if (string.IsNullOrWhiteSpace(Adress))
                    {
                        Response.Write("<script languge='javascript'>alert('手机号码不能为空alert('地址不能为空！');</script>");
                        return View();
                    }
                    using (SBIContext sc = new SBIContext())
                    {
                        try
                        {
                            var model = sc.ordersinfo.Where(o => o.Phone == Phone && o.PayStatus == 1).FirstOrDefault();
                            if (model == null)
                            {
                                ordersinfo os = new ordersinfo();
                                os.ID = id;
                                os.Name = UserName;
                                os.Phone = Phone;
                                os.Adress = Adress;
                                os.DateTime = DateTime.Now;
                                os.Money = 20.00;
                                os.OExtension = "茶叶运送邮费";
                                var obj = JsonConvert.SerializeObject(os);
                                Session["OrderInfo"] = obj;
                                sc.ordersinfo.Add(os);
                                sc.SaveChanges();
                                AliPlay(os);
                            }
                            else
                            {
                                Response.Write("<script languge='javascript'>alert('该用户已申请过，不可重复申请！');</script>");
                                return View();
                            }
                        }
                        catch (Exception e)
                        {
                            Session["Error"] = e.Message;
                            Response.Redirect("default/Error");
                        }
                    }
                    return View();
                }
                else
                {
                    Response.Write("<script languge='javascript'>alert('手机号码与注册手机号不符，请用开通VIP的账号来领取礼品！');</script>");
                    return View();
                }
            }
        }
        public void AliPlay(ordersinfo os)
        {
            ////////////////////////////////////////////请求参数////////////////////////////////////////////


            //商户订单号，商户网站订单系统中唯一订单号，必填
            string out_trade_no = os.ID;

            //订单名称，必填
            string subject = os.OExtension;

            //付款金额，必填
            string total_fee = os.Money.ToString();

            //收银台页面上，商品展示的超链接，必填
            string show_url = "http://mp.weixin.qq.com/s/latm6ZXn2Hi6yCy8oWKa5Q";

            //商品描述，可空
            string body = "回馈众多Vip用户特权礼品，每人仅能领取一次！";



            ////////////////////////////////////////////////////////////////////////////////////////////////

            //把请求参数打包成数组
            SortedDictionary<string, string> sParaTemp = new SortedDictionary<string, string>();
            sParaTemp.Add("partner", Config.partner);
            sParaTemp.Add("seller_id", Config.seller_id);
            sParaTemp.Add("_input_charset", Config.input_charset.ToLower());
            sParaTemp.Add("service", Config.service);
            sParaTemp.Add("payment_type", Config.payment_type);
            sParaTemp.Add("notify_url", Config.notify_url);
            sParaTemp.Add("return_url", Config.return_url);
            sParaTemp.Add("out_trade_no", out_trade_no);
            sParaTemp.Add("subject", subject);
            sParaTemp.Add("total_fee", total_fee);
            sParaTemp.Add("show_url", show_url);
            sParaTemp.Add("app_pay", "Y");//启用此参数可唤起钱包APP支付。
            sParaTemp.Add("body", body);
            //其他业务参数根据在线开发文档，添加参数.文档地址:https://doc.open.alipay.com/doc2/detail.htm?spm=a219a.7629140.0.0.2Z6TSk&treeId=60&articleId=103693&docType=1
            //如sParaTemp.Add("参数名","参数值");

            //建立请求
            string sHtmlText = Submit.BuildRequest(sParaTemp, "get", "确认");
            Response.Write(sHtmlText);
        }
        public ActionResult Error()
        {
            return View();
        }
        public ActionResult GoodInfo()
        {
            return View();
        }
    }
}