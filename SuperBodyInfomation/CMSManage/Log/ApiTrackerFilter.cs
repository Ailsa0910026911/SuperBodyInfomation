using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;

namespace CMSManage.Log
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
    public class ApiTrackerFilter : ActionFilterAttribute
    {
        private readonly string key = "_thisOnApiActionMonitorLog_";

        public override void OnActionExecuting(HttpActionContext actionContext)
        {
            var monLog = new MonitorLog();
            monLog.ExecuteStartTime = Convert.ToDateTime(DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss.ffff", DateTimeFormatInfo.InvariantInfo));

            monLog.ControllerName = actionContext.ActionDescriptor.ControllerDescriptor.ControllerName;
            monLog.ActionName = actionContext.ActionDescriptor.ActionName;
            actionContext.Request.Properties[this.key] = monLog;
        }

        public override void OnActionExecuted(HttpActionExecutedContext actionExecutedContext)
        {
            if (actionExecutedContext.Exception != null)
            {
                string controllerName = string.Format(
                    "{0}Controller",
                    actionExecutedContext.ActionContext.ActionDescriptor.ControllerDescriptor.ControllerName);
                string actionName = actionExecutedContext.ActionContext.ActionDescriptor.ActionName;
                string errorMsg = string.Format("在执行 controller[{0}] 的 action[{1}] 时产生异常", controllerName, actionName);
                if (actionExecutedContext.Exception is Exception)
                {
                    LoggerHelper.Info(errorMsg, actionExecutedContext.Exception);
                }
                else
                {
                    LoggerHelper.Error(errorMsg, actionExecutedContext.Exception);
                }
            }

            if (!actionExecutedContext.Request.Properties.ContainsKey(this.key))
            {
                return;
            }

            var monLog = actionExecutedContext.Request.Properties[this.key] as MonitorLog;
            if (monLog != null)
            {
                monLog.ExecuteEndTime = DateTime.Now;

                monLog.Raw = actionExecutedContext.Request.Content.ReadAsStringAsync().Result;
                LoggerHelper.Monitor(monLog.GetLogInfo());
            }
        }
    }
}