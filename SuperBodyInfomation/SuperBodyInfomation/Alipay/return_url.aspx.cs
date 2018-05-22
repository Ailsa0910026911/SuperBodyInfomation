
using Com.Alipay;
using SBIModel;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace SuperBodyInfomation.Alipay
{
    public partial class return_url : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            using (SBIContext sc = new SBIContext())
            {

                SortedDictionary<string, string> sPara = GetRequestGet();
              
                if (sPara.Count > 0)//判断是否有带返回参数
                {
                    
                    Notify aliNotify = new Notify();
                    bool verifyResult = aliNotify.Verify(sPara, Request.QueryString["notify_id"], Request.QueryString["sign"]);
                   
                    if (verifyResult)//验证成功
                    {
                        /////////////////////////////////////////////////////////////////////////////////////////////////////////////
                        //请在这里加上商户的业务逻辑程序代码


                        //——请根据您的业务逻辑来编写程序（以下代码仅作参考）——
                        //获取支付宝的通知返回参数，可参考技术文档中页面跳转同步通知参数列表

                        //商户订单号

                        string out_trade_no = Request.QueryString["out_trade_no"];

                        //支付宝交易号

                        string trade_no = Request.QueryString["trade_no"];
                        Core.LogResult("return:商户订单号：" + out_trade_no);
                        //交易状态
                        string trade_status = Request.QueryString["trade_status"];
                        string ID = out_trade_no;
                        ordersinfo os = sc.ordersinfo.Where(o => o.ID == ID && o.PayStatus == 0).FirstOrDefault();

                        if (Request.QueryString["trade_status"] == "TRADE_FINISHED" || Request.QueryString["trade_status"] == "TRADE_SUCCESS")
                        {
                            //判断该笔订单是否在商户网站中已经做过处理
                            //如果没有做过处理，根据订单号（out_trade_no）在商户网站的订单系统中查到该笔订单的详细，并执行商户的业务程序
                            //如果有做过处理，不执行商户的业务程序
                            if (os != null)
                            {
                                DateTime endtime = Convert.ToDateTime(os.DateTime).AddMinutes(10);
                                DateTime now = DateTime.Now;
                                if (now > endtime)
                                {
                                    os.Remark = "支付超时";
                                    sc.SaveChanges();
                                    Core.LogResult("return:支付宝异步支付超时->支付失败->跳转到支付失败页面！");
                                    Response.Write("fail");
                                }
                                else
                                {
                                    os.PayStatus = 1;
                                    sc.SaveChanges();
                                    Core.LogResult("return:支付宝异步回调数据成功->支付成功->跳转到支付成功页面！");
                                    Response.Write("success");
                                }
                            }
                        }
                        else
                        {
                            Core.LogResult("return:支付宝同步回调数据验证失败->支付状态trade_status=" + Request.QueryString["trade_status"]);
                            Response.Write("trade_status=" + Request.QueryString["trade_status"]);
                        }

                        //打印页面
                        Response.Write("验证成功<br />");

                        //——请根据您的业务逻辑来编写程序（以上代码仅作参考）——

                        /////////////////////////////////////////////////////////////////////////////////////////////////////////////
                    }
                    else//验证失败
                    {
                        Core.LogResult("return:支付宝同步回调数据验证失败->验证失败");
                        Response.Write("验证失败");
                    }
                }
                else
                {
                    Core.LogResult("return:支付宝同步回调数据验证失败->无返回参数");
                    Response.Write("无返回参数");
                }
            }
        }

        /// <summary>
        /// 获取支付宝GET过来通知消息，并以“参数名=参数值”的形式组成数组
        /// </summary>
        /// <returns>request回来的信息组成的数组</returns>
        public SortedDictionary<string, string> GetRequestGet()
        {
            int i = 0;
            SortedDictionary<string, string> sArray = new SortedDictionary<string, string>();
            NameValueCollection coll;
            //Load Form variables into NameValueCollection variable.
            coll = Request.QueryString;

            // Get names of all forms into a string array.
            String[] requestItem = coll.AllKeys;

            for (i = 0; i < requestItem.Length; i++)
            {
                sArray.Add(requestItem[i], Request.QueryString[requestItem[i]]);
            }

            return sArray;
        }
    }
}