

using Com.Alipay;
using Newtonsoft.Json;
using SBIModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace SuperBodyInfomation.Alipay
{
    public partial class _default : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            var obj = Session["OrderInfo"];
            if(obj!=null)
            {
                ordersinfo os = JsonConvert.DeserializeObject<ordersinfo>(obj.ToString());
                WIDout_trade_no.Text = os.ID;
                WIDsubject.Text = os.OExtension;
                WIDtotal_fee.Text = os.Money.ToString();
                WIDshow_url.Text = "http://mp.weixin.qq.com/s/latm6ZXn2Hi6yCy8oWKa5Q";
                WIDbody.Text = "回馈众多Vip用户特权礼品，每人仅能领取一次！";
            }
            
        }

        protected void BtnAlipay_Click(object sender, EventArgs e)
        {
            ////////////////////////////////////////////请求参数////////////////////////////////////////////


            //商户订单号，商户网站订单系统中唯一订单号，必填
            string out_trade_no = WIDout_trade_no.Text.Trim();

            //订单名称，必填
            string subject = WIDsubject.Text.Trim();

            //付款金额，必填
            string total_fee = WIDtotal_fee.Text.Trim();

            //收银台页面上，商品展示的超链接，必填
            string show_url = WIDshow_url.Text.Trim();

            //商品描述，可空
            string body = WIDbody.Text.Trim();



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
            //sParaTemp.Add("app_pay","Y");//启用此参数可唤起钱包APP支付。
            sParaTemp.Add("body", body);
            //其他业务参数根据在线开发文档，添加参数.文档地址:https://doc.open.alipay.com/doc2/detail.htm?spm=a219a.7629140.0.0.2Z6TSk&treeId=60&articleId=103693&docType=1
            //如sParaTemp.Add("参数名","参数值");

            //建立请求
            string sHtmlText = Submit.BuildRequest(sParaTemp, "get", "确认");
            Response.Write(sHtmlText);

        }
    }
}