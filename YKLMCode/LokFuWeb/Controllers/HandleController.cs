using LokFu.Extensions;
using LokFu.Repositories;
using System;
using System.Configuration;
using System.IO;
using System.Web.Mvc;
namespace LokFu
{
    public partial class InitController : Controller
    {
        public LokFuEntity Entity;

        public string ApiPath;
        public string ApkPath;
        public string AppPath;
        public string SysPath;
        public string PayPath;
        public string NoticePath;


        public string ApiImgPath;
        public string ApkImgPath;
        public string AppImgPath;
        public string SysImgPath;

        public bool IsLowerIe = false;
        //public string WebPath;
        public InitController()
        {
            ApiPath = ConfigurationManager.AppSettings["ApiPath"].ToString();
            ApkPath = ConfigurationManager.AppSettings["ApkPath"].ToString();
            SysPath = ConfigurationManager.AppSettings["SysPath"].ToString();
            PayPath = ConfigurationManager.AppSettings["PayPath"].ToString();
            NoticePath = ConfigurationManager.AppSettings["NoticePath"].ToString();

            ApiImgPath = ConfigurationManager.AppSettings["ApiImgPath"].ToString();
            ApkImgPath = ConfigurationManager.AppSettings["ApkImgPath"].ToString();
            SysImgPath = ConfigurationManager.AppSettings["SysImgPath"].ToString();

            AppPath = ApiPath;
            AppImgPath = ApiImgPath;

            ViewBag.ApiPath = ApiPath;
            ViewBag.ApkPath = ApkPath;
            ViewBag.SysPath = SysPath;
            ViewBag.PayPath = PayPath;
            ViewBag.NoticePath = NoticePath;

            ViewBag.ApiImgPath = ApiImgPath;
            ViewBag.ApkImgPath = ApkImgPath;
            ViewBag.AppImgPath = AppImgPath;
            ViewBag.SysImgPath = SysImgPath;

            Entity = new LokFuEntity(ConfigurationManager.ConnectionStrings["LokFuEntity"].ToString());
            ViewBag.Entity = Entity;
            
            System.Web.HttpBrowserCapabilities brObject = System.Web.HttpContext.Current.Request.Browser;
            if (brObject.Type.IndexOf("IE") != -1)
            { //ID
                string[] VersionArr = brObject.Version.Split('.');
                int Version = Int32.Parse(VersionArr[0]);
                if (Version <= 8)
                {
                    IsLowerIe = true;
                }
            }
            ViewBag.IsLowerIe = IsLowerIe;
        }
        protected override void HandleUnknownAction(string actionName)
        {
            string Language = System.Configuration.ConfigurationManager.AppSettings["Language"] == null ? string.Empty : System.Configuration.ConfigurationManager.AppSettings["Language"].ToString();
            if ((this.ControllerContext.HttpContext.Response.ContentType.IndexOf("text/html") != -1 || this.ControllerContext.HttpContext.Response.ContentType.IndexOf("javascript") != -1) && Language.ToUpper() == "BIG5")
            {
                this.ControllerContext.RequestContext.HttpContext.Response.Filter = new LokFu.Filters.StaticFileWriteFilterAttribute.CapitalFilter(Response.Filter);
            }
            this.View(actionName).ExecuteResult(this.ControllerContext);
        }
        protected override void OnException(ExceptionContext filterContext)
        {
            string errinfo = filterContext.Exception.ToString();
            if (errinfo.IndexOf("IX_OrdersPayOnly") != -1) {
                errinfo = "IX_OrdersPayOnly";
            }
            Utils.WriteLog(errinfo, "errlog");
            base.OnException(filterContext);
        }
    }
}