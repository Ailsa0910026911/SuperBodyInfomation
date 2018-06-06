using LokFu.Extensions;
using System;
using System.Net;
using System.Net.Http;
using System.Web.Http.Filters;

namespace LokFu
{
    public class WebApiExceptionFilterAttribute : ExceptionFilterAttribute
    {
        //重写基类的异常处理方法
        public override void OnException(HttpActionExecutedContext actionExecutedContext)
        {
            string errinfo = actionExecutedContext.Exception.ToString();
            if (errinfo.IndexOf("IX_OrdersPayOnly") != -1)
            {
                errinfo = "IX_OrdersPayOnly";
            }
            Utils.WriteLog(errinfo, "errlog");
            //2.返回调用方具体的异常信息
            if (actionExecutedContext.Exception is NotImplementedException)
            {
                actionExecutedContext.Response = new HttpResponseMessage(HttpStatusCode.NotImplemented);
            }
            else if (actionExecutedContext.Exception is TimeoutException)
            {
                actionExecutedContext.Response = new HttpResponseMessage(HttpStatusCode.RequestTimeout);
            }
            //.....这里可以根据项目需要返回到客户端特定的状态码。如果找不到相应的异常，统一返回服务端错误500
            else
            {
                actionExecutedContext.Response = new HttpResponseMessage(HttpStatusCode.InternalServerError);
            }
        }
    }
}