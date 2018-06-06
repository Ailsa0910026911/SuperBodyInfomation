using LokFu.Base;
using LokFu.Infrastructure;
using LokFu.Models;
using LokFu.Repositories;
using LokFu.Repositories.SqlServer;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
namespace LokFu.Areas.Manage.Controllers
{
    public class AsynController : BaseController
    {
        public AsynController(){
            ViewBag.Authorization = true;//允许权限
        }
        public void Check(string fieldId, string fieldValue, string extraData)
        {
            switch (extraData)
            {
                case "SysAdmin"://管理员登录帐户
                    SysAdmin SysAdmin = Entity.SysAdmin.FirstOrDefault(n => n.UserName == fieldValue);
                    if (SysAdmin == null)
                    {
                        Response.Write("[\"UserName\",true]");
                    }
                    else
                    {
                        Response.Write("[\"UserName\",false]");
                    }
                    break;
                case "Users"://会员登录帐户
                    Users Users = Entity.Users.FirstOrDefault(n => n.UserName == fieldValue);
                    if (Users == null)
                    {
                        Response.Write("[\"UName\",true]");
                    }
                    else
                    {
                        Response.Write("[\"UName\",false]");
                    }
                    break;
            }
        }
        public void GetUsers(string UserName)
        {
            if (UserName.IsNullOrEmpty()) {
                Response.Write("[]");
                return;
            }
            IList<Users> List = Entity.Users.Where(n => n.TrueName.Contains(UserName) || n.NeekName.Contains(UserName) || n.UserName == UserName).ToList();
            foreach (var p in List) {
                p.Cols = "Id,UserName,TrueName,NeekName";
            }
            string Json = List.EntityToJson();
            Response.Write(Json);
        }
        /// <summary>
        /// 上传APP图标
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public JsonResult AppIco(int MaxHigh, int MaxWidth, int MinHigh, int MinWidth)
        {
            var file = HttpContext.Request.Files.Get("upLoadFile");
            JsonResult json = new JsonResult() { ContentType = "text/html" };
            string allowType = "image/png";
            string ExtensionName = Path.GetExtension(file.FileName);
            var savePath = "/UpLoadFiles/APPModule/";
            var param = new UpLoadFileParam(file, savePath,false)
            {
                AllowType = allowType,
                MaxHigh = MaxHigh,
                MaxWidth = MaxWidth,
                MinHigh = MinHigh,
                MinWidth = MinWidth,
            };
            json.Data = UpLoadFile(param);
            return json;
        }
        /// <summary>
        /// 上传广告图
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public JsonResult ADPicture(int MaxHigh, int MaxWidth, int MinHigh, int MinWidth)
        {
            var file = HttpContext.Request.Files.Get("upLoadFile");
            JsonResult json = new JsonResult() { ContentType = "text/html" };
            string allowType = "image/png";
            var savePath = "/UpLoadFiles/AdInfo/";
            var param = new UpLoadFileParam(file, savePath,false)
            {
                AllowType = allowType,
                MaxHigh = MaxHigh,
                MaxWidth = MaxWidth,
                MinHigh = MinHigh,
                MinWidth = MinWidth,
            };
            json.Data = UpLoadFile(param);
            return json;
        }

        /// <summary>
        /// 上传文件
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public JsonResult UpLoadFile(UpLoadFileHelp UpLoadFileHelp)
        {
            UpLoadFileHelp.Ini();
            JsonResult json = new JsonResult() { ContentType = "text/html" };
            json.Data = UpLoadFileHelp.UpLoadFile();
            return json;
        }

        /// <summary>
        /// 上传单个文件
        /// </summary>
        /// <returns></returns>
        private dynamic UpLoadFile(UpLoadFileParam param)
        {
            dynamic data;
            var types = param.AllowType.Split(',');
            if (param.File != null)
            {
                #region 校验
                if (!types.Contains(param.File.ContentType))
                {
                    data = new
                    {
                        Status = false,
                        Message = "请上传" + param.AllowType + "格式"
                    };
                    return data;
                }
                if (param.File.ContentLength > (1024 * 1024 * param.Size))
                {
                    data = new
                    {
                        Status = false,
                        Message = "文件不能超过" + param.Size + "M"
                    };
                    return data;
                }
                //图片校验
                if(param.MaxWidth > 0 || param.MaxHigh > 0 || param.MinWidth > 0 || param.MinHigh > 0)
                {
                    var image = System.Drawing.Image.FromStream(param.File.InputStream);
                    if (image.Width > param.MaxWidth)
                    {
                        data = new
                        {
                            Status = false,
                            Message = "图片超过最大宽度" + param.MaxWidth + ",请重新上传"
                        };
                        return data;
                    }
                    if (image.Height > param.MaxHigh)
                    {
                        data = new
                        {
                            Status = false,
                            Message = "图片超过最大高度" + param.MaxHigh + ",请重新上传"
                        };
                        return data;
                    }
                    if (image.Width < param.MinWidth)
                    {
                        data = new
                        {
                            Status = false,
                            Message = "图片小于最小宽度" + param.MinWidth + ",请重新上传"
                        };
                        return data;
                    }
                    if (image.Height < param.MinHigh)
                    {
                        data = new
                        {
                            Status = false,
                            Message = "图片小于最小高度" + param.MinHigh + ",请重新上传"
                        };
                        return data;
                    }
                }
                #endregion
                #region 保存
                var physicsPath = HttpContext.Server.MapPath(param.SavePath + "/" + param.SaveFileName);
                param.File.SaveAs(physicsPath);
                data = new
                {
                    Status = true,
                    Message = "",
                    Result = param.SaveFileName,
                };
                #endregion
            }
            else
            {
                data = new
                {
                    Status = false,
                    Message = "请选择上传文件"
                };
            }
            return data;
        }

        public ActionResult QQ()
        {
            IList<SysAdmin> SysAdminList = Entity.SysAdmin.Where(n => n.State == 1 && n.QQNum.Length > 0).ToList();
            ViewBag.SysAdminList = SysAdminList;
            return View();
        }

        public string ClearCacheAll()
        {
            LokFu.Extensions.APIExtensions.ClearCacheAll();
            return "true";
        }
        
        public void Agents(int tier,int AgengtId)
        {
            if (tier.IsNullOrEmpty())
            {
                Response.Write("[]");
                return;
            }
            List<SysAgent> SysAgentList = Entity.SysAgent.Where(o => o.Tier == tier && o.State == 1 && o.Id != AgengtId).ToList();
            string Json = SysAgentList.EntityToJson();
            Response.Write(Json);
        }
        public void GetAgent(int AgengtId)
        {
            SysAgent SysAgent = Entity.SysAgent.FirstOrDefault(o=>o.Id==AgengtId);
            if (SysAgent == null)
            {
                Response.Write("[]");
                return;
            }
            List<SysAgent> SysAgentList = Entity.SysAgent.Where(o => o.Tier <= SysAgent.Tier && o.State == 1 && o.Id != AgengtId).ToList();
            string Json = SysAgentList.EntityToJson();
            Response.Write(Json);
        }
    }
}
