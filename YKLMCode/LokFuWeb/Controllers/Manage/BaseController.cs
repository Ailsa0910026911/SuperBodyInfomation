using System;
using System.Collections.Generic;
using System.Linq;
using LokFu.Repositories;
using LokFu.Extensions;
using LokFu.Infrastructure;
using System.Web.Mvc;
using LokFu.Models;
using System.Data;
using System.Web;
using System.Web.Caching;
namespace LokFu.Areas.Manage.Controllers
{
    public class BaseController : InitController
    {
        public SysAdmin AdminUser;
        public SysSet BasicSet;
        /// <summary>
        /// 锁屏状态(1为锁屏,0为不锁屏)
        /// </summary>
        public String IsLockScreen = "0";
        public BaseController()
        {
            AdminUser = GetAdmin();
            if (AdminUser == null)
            {
                System.Web.HttpContext.Current.Response.Redirect("/Manage/Login.html");
                System.Web.HttpContext.Current.Response.End();
                //ViewBag.View("blank");
                return;
            }
            if (IsLowerIe)
            { //ID
                System.Web.HttpContext.Current.Response.Redirect("/Manage/Login.html");
                System.Web.HttpContext.Current.Response.End();
                //ViewBag.View("blank");
                return;
            }
            ViewBag.SysPowerList_ = Entity.SysPower.Where(n => n.State == 1 && n.PType == 1).OrderBy(n => n.Sort).ToList();//加载管理系统权限
            ViewBag.MenuList_ = Entity.SysMenu.Where(n => n.MType == 1).OrderBy(n => n.PId).OrderBy(n => n.Sort).ToList();
            ViewBag.BaseMenu = GetBaseMenu(AdminUser);//登录用户
            ViewBag.AdminUser = AdminUser;//登录用户
            BasicSet = Entity.SysSet.FirstOrNew();
            ViewBag.BasicSet = BasicSet;

            //读取配置文件判断是否需要锁屏
            ViewBag.IsLock = System.Configuration.ConfigurationManager.AppSettings["IsLock"];
            //读取配置文件获取锁屏间隔时间
            ViewBag.LockTime = System.Configuration.ConfigurationManager.AppSettings["LockTime"];

            //验证是否是锁屏状态中 如果是就显示锁屏页面(1为锁屏,0为不锁屏)
            string controllername = System.Web.HttpContext.Current.Request.RequestContext.RouteData.Values["controller"].ToString().ToLower();
            if (IsLockScreen == "1" && controllername != "lockscreen")
            {
                //跳转到锁屏页
                System.Web.HttpContext.Current.Response.Redirect("/Manage/LockScreen/Index.html");
                System.Web.HttpContext.Current.Response.End();
                return;
            }
        }
        /// <summary>
        /// NPOI.dll方法导出Excel
        /// </summary>
        /// <param name="table">datatable</param>
        /// <param name="fileName">文件名称</param>
        /// <param name="strHeaderText">表头文本</param>
        /// <returns></returns>
        [NonAction]
        protected FileContentResult ExportExcelBase(DataTable table, string fileName, string strHeaderText = null)
        {
            string dt = DateTime.Now.ToString("yyyyMMddHHmmss");
            string headerName = string.IsNullOrEmpty(fileName) ? dt : fileName;
            if (string.IsNullOrEmpty(fileName))
            {
                fileName = dt + ".xls";
            }
            else
            {
                fileName = fileName + ".xls";
            }

            //ie 需要编码
            if (string.Compare(Request.Browser.Browser, "IE", true) == 0 || string.Compare(Request.Browser.Browser, "InternetExplorer", true) == 0)
            {
                fileName = System.Web.HttpUtility.UrlEncode(fileName, System.Text.Encoding.UTF8);
            }

            var export = new ExcelExport();
            return File(export.ExportExcelFromDataTable(table, headerName).GetBuffer(),
                "application/vnd.ms-excel", fileName);
        }
        /// <summary>
        /// NPOI导出Excel模板
        /// </summary>
        /// <param name="table"></param>
        /// <param name="fileName"></param>
        /// <param name="strHeaderText"></param>
        /// <returns></returns>
        [NonAction]
        protected FileContentResult ExportExcelBase(NPOI.HSSF.UserModel.HSSFWorkbook HSSFWorkbook, string fileName, string strHeaderText = null)
        {
            string dt = DateTime.Now.ToString("yyyyMMddHHmmss");
            string headerName = string.IsNullOrEmpty(fileName) ? dt : fileName;
            if (string.IsNullOrEmpty(fileName))
            {
                fileName = dt + ".xls";
            }
            else
            {
                fileName = fileName + "-" + dt + ".xls";
            }

            //ie 需要编码
            if (string.Compare(Request.Browser.Browser, "IE", true) == 0 || string.Compare(Request.Browser.Browser, "InternetExplorer", true) == 0)
            {
                fileName = System.Web.HttpUtility.UrlEncode(fileName, System.Text.Encoding.UTF8);
            }

            var export = new ExcelExport();
            return File(export.ToMemoryStream(HSSFWorkbook).GetBuffer(), "application/vnd.ms-excel", fileName);
        }
        public SysAdmin GetAdmin()
        {
            SysAdmin user = null;
            string neiw = System.Configuration.ConfigurationManager.AppSettings["key"];
            string Str = System.Web.HttpContext.Current.Request.Cookies.GetAdmin();
            if (Str.IsNullOrEmpty())
            {
                return user;
            }
            string[] UArr = LokFuEncode.LokFuAuthcodeDecode(Str, neiw).Split('|');
            if (UArr.Length == 4)
            {
                IsLockScreen = UArr[3];
                int Id = Int32.Parse(UArr[0]);
                string UName = UArr[1];
                string DTStr = UArr[2];
                user = Entity.SysAdmin.Where(n => n.UserName == UName && n.Id == Id).FirstOrDefault();
                if (user != null)
                {
                    DateTime now = (DateTime)user.LastTime;
                    if (DTStr == now.ToString("yyyy-MM-dd HH:mm:ss"))
                    {
                        return user;
                    }
                    else
                    {
                        user = null;
                    }
                }
            }
            return user;
        }
        public IList<SysMenu> GetBaseMenu(SysAdmin User)
        {
            IList<SysMenu> Temp = GetMenuList();
            User = User == null ? new SysAdmin() : User;
            IList<SysMenu> List = new List<SysMenu>();
            IList<SysPower> SysPowerList = GetSysPowerList();
            string[] Sub = User.PowerID.Split(',');
            List<int> ID = new List<int>();
            foreach (var s in Sub)
            {
                if (!s.IsNullOrEmpty())
                {
                    ID.Add(Int32.Parse(s));
                }
            }
            IList<SysPower> SysPowerAdminList = Entity.SysPower.Where(n => n.State == 1 && n.PType == 1 && ID.Contains(n.Id)).ToList();
            foreach (var p in Temp)
            {
                string Url = p.Url;
                if (!Url.IsNullOrEmpty())
                {
                    if (p.PId == 0)
                    {
                        int PowerId = Int32.Parse(p.Url);//当顶级菜单 Url存放权限表对应Id
                        SysPower P0List = SysPowerAdminList.FirstOrDefault(n => n.Id == PowerId);
                        if (P0List != null)
                        {
                            List.Add(p);
                        }
                    }
                    else
                    {
                        string[] Arr = Url.Split('?');
                        Url = Arr[0];
                        Url = Url.Replace(".html", "").Replace("/Manage/", "");
                        Arr = Url.Split('/');
                        string Ctrl = "", Method = "";
                        if (Arr.Length == 2)
                        {
                            Ctrl = Arr[0];
                            Method = Arr[1];
                        }
                        else if (Arr.Length == 1)
                        {
                            Ctrl = Arr[0];
                            Method = "index";
                        }
                        //使用严格验证，不存在权限的不能访问
                        SysPower SysPowerAdmin = SysPowerAdminList.FirstOrDefault(n => n.Ctrl == Ctrl && n.Method == Method);
                        if (SysPowerAdmin != null)
                        {
                            List.Add(p);
                        }
                    }
                }
            }
            //处理二级菜单
            foreach (var p in Temp.Where(n => n.Url == "###"))
            {
                if (List.Count(n => n.PId == p.Id) > 0)
                {
                    List.Add(p);
                }
            }
            return List;
        }
        public IList<SysMenu> GetMenuList()
        {
            return ViewBag.MenuList_;
        }
        public IList<SysPower> GetSysPowerList()
        {
            return ViewBag.SysPowerList_;
        }
        /// <summary>
        /// 根据分类名称查询分类短语
        /// </summary>
        /// <param name="enumCode"></param>
        /// <returns></returns>
        public IList<BasicDesc> GetBasicDescList(BasicCodeEnum enumCode)
        {
            EFPagingInfo<BasicDesc> p_BasicDesc = new EFPagingInfo<BasicDesc>();
            p_BasicDesc.SqlWhere.Add(x => x.CharCode.Contains(enumCode.ToString()));
            p_BasicDesc.OrderByList.Add("Sort", "DESC");
            string charCode = enumCode.ToString();
            return Entity.BasicDesc.Where(s => s.CharCode == charCode).ToList();
        }
        public void BaseRedirect()
        {
            bool IsAjax = Request.Form["IsAjax"] != null ? true : false;
            if (IsAjax)
            {
                string iframeId = Request.QueryString["iframeId"] == null ? string.Empty : Request.QueryString["iframeId"].ToString();
                if (iframeId != string.Empty)
                {
                    Response.Write("<script>parent.Open" + iframeId + ".window.location.href=parent.Open" + iframeId + ".document.URL;</script>");
                }
                else
                {
                    Response.Write("<script>parent.window.location.href=parent.document.URL;</script>");
                }
            }
            else
            {
                if (Session["Url"] != null)
                {
                    Response.Redirect(Session["Url"].ToString());
                }
                else
                {
                    Response.Redirect("/Manage");
                }
            }
        }

        public bool checkPower(string Power)
        {
            string Ctrl = ViewBag.ControllerName;
            return this.checkPower(Ctrl, Power);
        }

        public bool checkPower(string ControllerName, string Power)
        {
            bool Ret = false;
            string Ctrl = ControllerName;
            IList<SysPower> SysPowerList = ViewBag.SysPowerList_;
            SysPower SysPower = SysPowerList.FirstOrDefault(n => n.Ctrl == Ctrl && n.Method == Power);
            if (SysPower != null)
            { //权限存在
                string PId = string.Format(",{0},", SysPower.Id);
                if (AdminUser.PowerID.IndexOf(PId) != -1)
                {
                    Ret = true;
                }
            }
            return Ret;
        }

        public bool checkSignPower(string Ctrl, string Power)
        {
            Power = this.ActionNameToSign(Power);
            return this.checkPower(Ctrl, Power);
        }

        public bool checkSignPower(string Power)
        {
            string Ctrl = ViewBag.ControllerName;
            Power = this.ActionNameToSign(Power);
            return this.checkPower(Ctrl, Power);
        }

        private string ActionNameToSign(string tempActionName)
        {
            if (tempActionName == "ChangeStatus")//批量更新当修改权限
            {
                tempActionName = "Edit";
            }
            if (tempActionName == "XlsDo")//导出权限
            {
                tempActionName = "Xls";
            }
            if (tempActionName == "CancelSave")//导出权限
            {
                tempActionName = "Cancel";
            }
            if (tempActionName == "Info")//查看单个权限相关于查看列表权限
            {
                tempActionName = "Index";
            }
            if (tempActionName.IndexOf("Index") != -1)
            {
                if (tempActionName.Substring(0, 5) == "Index")//扩展方法
                {
                    tempActionName = "Index";
                }
            }
            if (tempActionName.IndexOf("Edit") != -1)
            {
                if (tempActionName.Substring(0, 4) == "Edit")//扩展方法
                {
                    tempActionName = "Edit";
                }
            }
            if (tempActionName.IndexOf("Info") != -1)
            {
                if (tempActionName.Substring(0, 4) == "Info")//扩展方法
                {
                    tempActionName = "Index";
                }
            }
            if (tempActionName.IndexOf("Delete") != -1)
            {
                if (tempActionName.Substring(0, 6) == "Delete")//扩展方法
                {
                    tempActionName = "Delete";
                }
            }
            if (tempActionName.IndexOf("Save") != -1)
            {
                if (tempActionName != "SaveAssureImg")
                {
                    if (tempActionName.Substring(0, 4) == "Save")//扩展方法
                    {
                        tempActionName = "Save";
                    }
                }

            }
            if (tempActionName == "PayCashDo" || tempActionName == "PayCashSetTask" || tempActionName == "PayCashDelTask")
            {
                tempActionName = "PayCash";
            }
            return tempActionName;
        }

        #region 防止订单重复支付
        protected bool GetRuningState(string tnum)
        {
            string CacheKey = tnum;
            Cache Cache = HttpRuntime.Cache;
            bool Ret = false;
            if (Cache[CacheKey] != null)
            {
                if (Cache[CacheKey].ToString() == "Runing")
                {
                    Ret = true;
                }
            }
            return Ret;
        }
        protected void SetRuningState(string tnum)
        {
            string CacheKey = tnum;
            Cache Cache = HttpRuntime.Cache;
            string Value = "Runing";
            Cache.Insert(CacheKey, Value, null, DateTime.Now.AddSeconds(120), TimeSpan.Zero);
        }
        protected void ClearRuningState(string tnum)
        {
            string CacheKey = tnum;
            Cache Cache = HttpRuntime.Cache;
            Cache.Remove(CacheKey);
        }
        #endregion
    }
}