using System.Configuration;
using System.Web;
using System.Web.Configuration;

namespace LokFu.Payment.WLBPay
{
    public class WLBPayConfig
    {
        private static string chanenlNmae = "深圳趣购电子商务有限公司";  //代理商名称(channelName);
        public static string ChanenlNmae
        {
            get { return WLBPayConfig.chanenlNmae; }
            set { WLBPayConfig.chanenlNmae = value; }
        }
      
    }
}
