using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace LokFu.Areas.Base.Controllers
{
    public class ExceptionController : Controller
    {
        //
        // GET: /Exception/

        public ActionResult Messing(string statusCode)
        {
            List<String> Content = new List<String>();
            String Title = "";
            if (statusCode == null)
            {
                Title = "您竟然看到这个页面了";
                Content.Add("谁开小差了,工程师已经被罚去面壁了……");
            }
            else
            {
                if (statusCode == "404")
                {
                    Title = "抱歉!您访问的页面不存在……";
                    Content.Add("请检查您输入的网址是否正确.");
                    Content.Add("该网页已删除或者移动.");
                }
                if (statusCode == "502"||statusCode=="504")
                {
                    Title = "服务器无响应……";
                    Content.Add("请您稍后再试，或者刷新下试试。");
                }
                if (statusCode == "500")
                {
                    Title = "您竟然看到这个页面了";
                    Content.Add("谁开小差了，工程师已经被罚去面壁了……");
                    Content.Add("请您稍后再试！");
                }
                if (statusCode == "503")
                {
                    Title = "服务器临时维护或者过载";
                    Content.Add("请您稍后再试，或者刷新下试试。");
                }
            }
            Content.Add("时间：【" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "】");
            ViewBag.Title = Title;
            ViewBag.Content = Content;
            return View();

        }

    }
}
