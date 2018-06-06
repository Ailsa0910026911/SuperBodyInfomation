using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CMSManage.Log
{
    public class LoggerHelper
    {
        private static readonly log4net.ILog LogInfo = log4net.LogManager.GetLogger("LogInfo");
        private static readonly log4net.ILog LogError = log4net.LogManager.GetLogger("LogError");
        private static readonly log4net.ILog LogMonitor = log4net.LogManager.GetLogger("LogMonitor");

        /// <summary>
        /// 记录Error日志
        /// </summary>
        /// <param name="errorMsg"></param>
        /// <param name="ex"></param>
        public static void Error(string errorMsg, Exception ex = null)
        {
            if (ex != null)
            {
                LogError.Error(errorMsg, ex);
            }
            else
            {
                LogError.Error(errorMsg);
            }
        }

        /// <summary>
        /// 记录Info日志
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="ex"></param>
        public static void Info(string msg, Exception ex = null)
        {
            if (ex != null)
            {
                LogInfo.Info(msg, ex);
            }
            else
            {
                LogInfo.Info(msg);
            }
        }

        /// <summary>
        /// 记录Monitor日志
        /// </summary>
        /// <param name="msg"></param>
        public static void Monitor(string msg)
        {
            LogMonitor.Info(msg);
        }
    }
}