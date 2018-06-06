using LokFu.Extensions;
using LokFu.Repositories;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
namespace LokFu.Areas.Pay.Controllers
{
    public class HLBPayController : BaseController
    {
        public void Notice()
        {
            #region 接收与验签
            Dictionary<string, string> resData = new Dictionary<string, string>();
            NameValueCollection coll = Request.Form;
            string[] requestItem = coll.AllKeys;
            for (int i = 0; i < requestItem.Length; i++)
            {
                string formvalue = Request.Form[requestItem[i]];
                if (formvalue.IndexOf("%") != -1)
                {
                    formvalue = HttpUtility.UrlDecode(formvalue, Encoding.UTF8);
                }
                resData.Add(requestItem[i], formvalue);
            }
            if (resData["sign"] == null || resData["rt5_orderId"] == null || resData["rt6_serialNumber"] == null || resData["rt8_orderAmount"] == null || resData["rt9_orderStatus"] == null)
            {
                Response.Write("NULL");
                return;
            }
            string rt5_orderId = resData["rt5_orderId"]; //返回状态
            JobItem JobItem = this.Entity.JobItem.FirstOrDefault(o => o.RunNum == rt5_orderId);
            if (JobItem == null) {
                Response.Write("E1");
                return;
            }
            if (JobItem.State != 2) {
                Response.Write("S1");
                return;
            }
            if (JobItem.RunState != 0 && JobItem.RunState != 2)
            {
                Response.Write("S2");
                return;
            }
            JobPayWay JobPayWay = Entity.JobPayWay.FirstOrDefault(n => n.Id == JobItem.PayWay);
            if (JobPayWay == null)
            {
                Response.Write("E2");
                return;
            }
            string[] JobPayWayArr = JobPayWay.QueryArray.Split(',');
            string MerId = JobPayWayArr[0];
            string MerKey = JobPayWayArr[1];
            #endregion
            string rt6_serialNumber = resData["rt6_serialNumber"];
            string rt8_orderAmount = resData["rt8_orderAmount"];
            decimal Amount= decimal.Parse(rt8_orderAmount) / 100;
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
            //验签
            //Dictionary<string, string> map = Utils.FilterPara(resData);
            string data = Utils.CreateLinkString(resData);
            if (!veritySign(data, MerKey))
            {
                Response.Write("E0");
            }

            string status = resData["rt9_orderStatus"]; //返回状态
            string sign = resData["sign"];
            if (status != "SUCCESS")
            {
                Response.Write("E3");
                return;
            }
            else
            {
                if (Amount == JobItem.RunMoney)
                {
                    JobItem.PaySuccess(Entity);
                    Response.Write("success");
                }
                else {
                    Response.Write("E99");
                }
            }
            
        }

        /// <summary>
        /// 验签
        /// </summary>
        /// <returns></returns>
        private bool veritySign(string data, string key)
        {
            bool success = false;
            if (data == null)
            {
                return success;
            }
            string[] arrray = data.Split(new char[] { '&' });
            Dictionary<string, string> res = new Dictionary<string, string>();
            string[] arr = new string[arrray.Length - 1];
            int i = 0;
            foreach (string s in arrray)
            {
                int n = s.IndexOf("=");
                string keys = s.Substring(0, n);
                string value = s.Substring(n + 1);
                if (keys != "sign")
                {
                    arr[i] = "\"" + keys + '：' + HttpUtility.UrlDecode(value) + "\"";
                    if (i < arrray.Length)
                    {
                        i++;
                    }
                }
                res.Add(keys, HttpUtility.UrlDecode(value));
            }

            StringBuilder builder = new StringBuilder();
            var query = arr.OrderBy(x => int.Parse(Regex.Match(x, "\\d+").Value));
            //拼接签名短信
            foreach (var s in query)
            {
                string signStr = s.Split('：')[1];
                builder.Append("&" + signStr);
            }

            builder.Append("&" + key);
            string signData = builder.ToString().Replace("\"", "");
            string signTrue = signData.GetMD5();
            string sign = res["sign"];
            if (sign.ToLower() == signTrue.ToLower())
            {
                success = true;
            }
            return success;
        }
    }
}
