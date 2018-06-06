using CMSManage.Extended;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CMSManage.App_Start
{
    public class FilterConfig
    {
        //这个方法是用于注册全局过滤器（在Global中被调用）
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new LoginCheckFilterAttribute() { IsCheck = true });
            filters.Add(new HandleErrorAttribute());
            //监控日志
            filters.Add(new Log.TrackerFilter());
        }
    }
}