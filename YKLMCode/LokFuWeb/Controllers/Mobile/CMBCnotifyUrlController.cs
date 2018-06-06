using LokFu.Extensions;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace LokFu.Areas.Mobile.Controllers
{
    public class CMBCnotifyUrlController : Controller
    {
        //
        // GET: /CMBCnotifyUrl/

        public void Index()
        {
            Dictionary<string, string> resData = new Dictionary<string, string>();
            NameValueCollection coll = Request.Form;
            string[] requestItem = coll.AllKeys;
            string str = "";
            for (int i = 0; i < requestItem.Length; i++)
            {
                resData.Add(requestItem[i], Request.Form[requestItem[i]]);
                str += "【" + requestItem[i] + "】：【" + Request.Form[requestItem[i]] + "】" + System.Environment.NewLine;
            }

            Response.Write("支付成功:" + str);
        }
        public void Back()
        {
            Dictionary<string, string> resData = new Dictionary<string, string>();
            NameValueCollection coll = Request.Form;
            string str = "";
            Stream s = System.Web.HttpContext.Current.Request.InputStream;
            byte[] b = new byte[s.Length];
            s.Read(b, 0, (int)s.Length);
            str += System.Text.Encoding.UTF8.GetString(b) + System.Environment.NewLine;

            string[] requestItem = coll.AllKeys;

            for (int i = 0; i < requestItem.Length; i++)
            {
                resData.Add(requestItem[i], Request.Form[requestItem[i]]);
                str += "【" + requestItem[i] + "】：【" + Request.Form[requestItem[i]] + "】" + System.Environment.NewLine;
            }
            Utils.WriteLog(str, "BackInfo");
            Response.Write("SUCCESS");
        }

    }
}
