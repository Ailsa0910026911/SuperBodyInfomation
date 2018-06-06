using System.Security.Cryptography;
using System.Text;

namespace LokFu.Repositories
{
    public class NFCObj
    {
        private string merchantcode;// 		商户编码
        private string outorderid;//		订单号
        private string outuserid;//		用户编号
        private string ordercreatetime;//		订单生成时间20140423112324
        private string noncestr;//		随机字符串
        private string goodname;//		商品名称
        private string goodsexplain;//		商品详情
        private string totalamount;//		订单金额	100
        private string paynotifyurl;//		支付通知地址
        private string lastpaytime;//		最晚支付时间

        private string Sign;//			签名
        /// <summary>
        /// 商户编码
        /// </summary>
        public string merchantCode
        {
            get { return merchantcode; }
            set { merchantcode = value; }
        }
        /// <summary>
        /// 商户订单号
        /// </summary>
        public string outOrderId
        {
            get { return outorderid; }
            set { outorderid = value; }
        }
        /// <summary>
        /// 交易金额
        /// </summary>
        public string totalAmount
        {
            get { return totalamount; }
            set { totalamount = value; }
        }
        /// <summary>
        /// 订单生成时间
        /// </summary>
        public string orderCreateTime
        {
            get { return ordercreatetime; }
            set { ordercreatetime = value; }
        }
        /// <summary>
        /// 通知地址
        /// </summary>
        public string payNotifyUrl
        {
            get { return paynotifyurl; }
            set { paynotifyurl = value; }
        }
        /// <summary>
        /// 商品名称
        /// </summary>
        public string goodName
        {
            get { return goodname; }
            set { goodname = value; }
        }
        /// <summary>
        /// 商品详情
        /// </summary>
        public string goodsExplain
        {
            get { return goodsexplain; }
            set { goodsexplain = value; }
        }
        /// <summary>
        /// 用户编号
        /// </summary>
        public string outUserId
        {
            get { return outuserid; }
            set { outuserid = value; }
        }
        /// <summary>
        /// 随机字符串
        /// </summary>
        public string nonceStr
        {
            get { return noncestr; }
            set { noncestr = value; }
        }
        /// <summary>
        /// 最晚支付时间
        /// </summary>
        public string lastPayTime
        {
            get { return lastpaytime; }
            set { lastpaytime = value; }
        }

        /// <summary>
        /// 签名
        /// </summary>
        public string sign
        {
            get
            {
                string Md5Str = "lastPayTime=" + lastpaytime + "&merchantCode=" + merchantcode + "&nonceStr=" + noncestr + "&orderCreateTime=" + ordercreatetime + "&outOrderId=" + outorderid + "&outUserId=" + outuserid + "&totalAmount=" + totalamount;
                Md5Str += "&KEY=" + Sign;
                return GetMD5(Md5Str);
            }
            set { Sign = value; }
        }
        /// <summary>
        /// 标准MD5算法
        /// </summary>
        /// <param name="value">原始字符串</param>
        /// <returns>加密后的字符串</returns>
        public string GetMD5(string value)
        {
            MD5 md5Hasher = MD5.Create();
            byte[] data = md5Hasher.ComputeHash(Encoding.UTF8.GetBytes(value));
            StringBuilder sBuilder = new StringBuilder();
            for (int i = 0; i < data.Length; i++)
            {
                sBuilder.Append(data[i].ToString("X2"));
            }
            return sBuilder.ToString();
        }
    }
}
