using System;
using System.Collections.Generic;
using System.Linq;
using LokFu.Repositories;
using LokFu.Infrastructure;
using LokFu.Extensions;
using System.Web.Mvc;
using System.Collections;
using System.Data;
namespace LokFu.Areas.Agent.Controllers
{
    public class BaseController : InitController
    {
        public SysAdmin AdminUser;
        public SysAgent BasicAgent;
        public SysSet BasicSet;
        public BaseController() {
            AdminUser = GetAdmin();
            if (AdminUser == null) {
                System.Web.HttpContext.Current.Response.Redirect("/Agent/Login.html");
                System.Web.HttpContext.Current.Response.End();
                //ViewBag.View("blank");
                return;
            }
            if (IsLowerIe)
            { //ID
                System.Web.HttpContext.Current.Response.Redirect("/Agent/Login.html");
                System.Web.HttpContext.Current.Response.End();
                ViewBag.View("blank");
                return;
            }
            string[] Sub = AdminUser.PowerID.Split(',');
            List<int> ID = new List<int>();
            foreach (var s in Sub)
            {
                if (!s.IsNullOrEmpty())
                {
                    ID.Add(Int32.Parse(s));
                }
            }
            ViewBag.SysPowerList_ = Entity.SysPower.Where(n => n.State == 1 && n.PType == 2 && ID.Contains(n.Id)).OrderBy(n => n.Sort).ToList();
            ViewBag.MenuList_ = Entity.SysMenu.Where(n => n.MType == 2).OrderBy(n => n.Sort).ToList();
            ViewBag.BaseMenu = GetBaseMenu(AdminUser);//登录用户
            ViewBag.AdminUser = AdminUser;//登录用户
            //ViewBag.Authorization = true;//权限验证开关
            BasicAgent = Entity.SysAgent.FirstOrNew(n => n.Id == AdminUser.AgentId);
            BasicAgent.AgentType = Entity.AgentType.FirstOrDefault(n => n.Id == BasicAgent.AgentTypeID);
            ViewBag.BasicAgent = BasicAgent;
            BasicSet = Entity.SysSet.FirstOrNew();
            ViewBag.BasicSet = BasicSet;

        }
        public SysAdmin GetAdmin()
        {
            SysAdmin user = null;
            string Str=System.Web.HttpContext.Current.Request.Cookies.GetAgent();
            if (Str.IsNullOrEmpty()){
                return user;
            }
            string neiw = System.Configuration.ConfigurationManager.AppSettings["key"];
            string[] UArr = LokFuEncode.LokFuAuthcodeDecode(Str, neiw).Split('|');
            if (UArr.Length == 3)
            {
                int Id = Int32.Parse(UArr[0]);
                string UName = UArr[1];
                string DTStr = UArr[2];
                user = Entity.SysAdmin.Where(n => n.UserName == UName && n.Id == Id).FirstOrDefault();
                if (user != null)
                {
                    DateTime now = (DateTime)user.LastTime;
                    if (DTStr == now.ToString("yyyy-MM-dd HH:mm"))
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
            //if (AdminUser.UserName == "admin")
            //{
            //    return Temp;
            //}
            IList<SysMenu> List = new List<SysMenu>();
            IList<SysPower> SysPowerAdminList = GetSysPowerList();
            foreach (var p in Temp)
            {
                string Url = p.Url;
                if (!Url.IsNullOrEmpty())
                {
                    if (p.PId == 0)
                    {
                        int PowerId = Int32.Parse(p.Url);//当顶级菜单 Url存放权限表对应Id
                        SysPower P0List = SysPowerAdminList.FirstOrDefault(n => n.Id == PowerId);
                        if (P0List != null || PowerId == 0)
                        {
                            List.Add(p);
                        }
                    }
                    else if (p.Sort > 10000) {
                        List.Add(p);
                    }
                    else
                    {
                        string[] Arr = Url.Split('?');
                        Url = Arr[0];
                        Url = Url.Replace(".html", "").Replace("/Agent/", "");
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
                    Response.Redirect("/Agent");
                }
            }
        }

        /// <summary>
        /// 是否有权限
        /// </summary>
        /// <param name="Power">权限</param>
        /// <returns></returns>
        public bool checkPower(string Power) {
            bool Ret = false;
            string Ctrl = ViewBag.ControllerName;
            IList<SysPower> SysPowerList = ViewBag.SysPowerList_;
            SysPower SysPower = SysPowerList.FirstOrDefault(n => n.Ctrl == Ctrl && n.Method == Power);
            if (SysPower != null) { //权限存在
                string PId = string.Format(",{0},", SysPower.Id);
                if (AdminUser.PowerID.IndexOf(PId) != -1) {
                    Ret = true;
                }
            }
            return Ret;
        }

        /// <summary>
        /// 是否有权限
        /// </summary>
        /// <param name="Ctrl">控制器</param>
        /// <param name="Power">权限</param>
        /// <returns></returns>
        public bool checkPower(string Ctrl, string Power)
        {
            bool Ret = false;
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
            return this.checkPower(Ctrl,Power);
        }

        public bool checkSignPower(string Power)
        {
            string Ctrl = ViewBag.ControllerName;
            Power = this.ActionNameToSign(Power);
            return this.checkPower(Ctrl,Power);
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

        [NonAction]
        /// <summary>
        /// 是否所属代理
        /// </summary>
        /// <param name="id">Agentid</param>
        /// <param name="SysAgentList">所属路径</param>
        /// <returns></returns>
        public bool IsBelongToAgent(int id,IList<SysAgent> SysAgentList)
        {
            bool result = false;
            if (SysAgentList != null && SysAgentList.Count > 0)
            {
                if (SysAgentList.Select(o => o.Id).Contains(id))
                {
                    result = true;
                }
            }
            return result;
        }

        /// <summary>
        /// 是否所属代理
        /// </summary>
        /// <param name="id">Agentid</param>
        /// <returns></returns>
        [NonAction]
        public bool IsBelongToAgent(int id)
        {
            bool result = false;
            SysAgent SysAgent = Entity.SysAgent.FirstOrNew(o => o.Id == id);
            IList<SysAgent> SysAgentList = SysAgent.GetAgentsById(Entity);
            if (SysAgentList != null && SysAgentList.Count > 0)
            {
                if (SysAgentList.Select(o => o.Id).Contains(BasicAgent.Id))
                {
                    result = true;
                }
            }
            return result;
        }
        /// <summary>
        /// 所有下属代理的id
        /// </summary>
        /// <param name="id">Agentid</param>
        /// <returns></returns>
        [NonAction]
        public List<int> BelongToAgentForIds(int id)
        {
            var result = new List<int>();
            var Current = Entity.SysAgent.Where(o => o.Id == id).Select(o => new { o.Id,o.Tier }).FirstOrDefault();
            var oid = new List<int>() { Current.Id };
            if (oid.Count > 0)
            {
                for (int i = 1; i <= BasicAgent.AgentLevelMax - Current.Tier; i++)
                {
                    var temp = Entity.SysAgent.Where(o => oid.Contains(o.AgentID)).Select(o => o.Id).ToList();
                    oid = temp;
                    result.AddRange(temp);
                }
            }
            return result;
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
    }

    public class AgentLanguage
    {
        /// <summary>
        /// 越权
        /// </summary>
        public static readonly string Surmount = "越权操作,系统已记录你的行为";

        /// <summary>
        /// 数据为空
        /// </summary>
        public static readonly string Empty = "无法查询到数据"; 
    }
}