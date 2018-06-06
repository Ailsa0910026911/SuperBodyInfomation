using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CMSManage.Log
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
    public class TrackerFilter : ActionFilterAttribute, IExceptionFilter
    {
        private readonly string key = "_thisOnActionMonitorLog_";

        #region Action时间监控
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            MonitorLog monLog = new MonitorLog();
            monLog.ExecuteStartTime = Convert.ToDateTime(DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss.ffff", DateTimeFormatInfo.InvariantInfo));
            monLog.ControllerName = filterContext.RouteData.Values["controller"] as string;
            monLog.ActionName = filterContext.RouteData.Values["action"] as string;
            filterContext.Controller.ViewData[this.key] = monLog;
        }

        public override void OnActionExecuted(ActionExecutedContext filterContext)
        {
            MonitorLog monLog = filterContext.Controller.ViewData[this.key] as MonitorLog;
            monLog.ExecuteEndTime = DateTime.Now;
            monLog.FormCollections = filterContext.HttpContext.Request.Form;//form表单提交的数据
            monLog.QueryCollections = filterContext.HttpContext.Request.QueryString;//Url 参数
            LoggerHelper.Monitor(monLog.GetLogInfo());
        }
        #endregion

        #region View 视图生成时间监控
        public override void OnResultExecuting(ResultExecutingContext filterContext)
        {
            MonitorLog monLog = filterContext.Controller.ViewData[this.key] as MonitorLog;
            monLog.ExecuteStartTime = DateTime.Now;
        }

        public override void OnResultExecuted(ResultExecutedContext filterContext)
        {
            MonitorLog monLog = filterContext.Controller.ViewData[this.key] as MonitorLog;
            monLog.ExecuteEndTime = DateTime.Now;
            LoggerHelper.Monitor(monLog.GetLogInfo(MonitorLog.MonitorType.View));
            filterContext.Controller.ViewData.Remove(this.key);
        }
        #endregion

        #region 错误日志
        public void OnException(ExceptionContext filterContext)
        {
            if (!filterContext.ExceptionHandled)
            {
                string controllerName = string.Format("{0}Controller", filterContext.RouteData.Values["controller"] as string);
                string actionName = filterContext.RouteData.Values["action"] as string;
                string errorMsg = string.Format("在执行 controller[{0}] 的 action[{1}] 时产生异常", controllerName, actionName);
                LoggerHelper.Error(errorMsg, filterContext.Exception);
            }
        }
        #endregion
    }
}