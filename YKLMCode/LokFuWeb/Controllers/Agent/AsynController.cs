using LokFu.Infrastructure;
using LokFu.Models;
using LokFu.Repositories;
using LokFu.Repositories.SqlServer;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System;
namespace LokFu.Areas.Agent.Controllers
{
    public class AsynController : BaseController
    {
        public AsynController()
        {
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
        public void CheckAdminUserName(string fieldId, string fieldValue)
        {
            SysAdmin SysAdmin = Entity.SysAdmin.FirstOrDefault(n => n.UserName == fieldValue);
            if (SysAdmin == null)
            {
                Response.Write("[\"" + fieldId + "\",true]");
            }
            else
            {
                Response.Write("[\"" + fieldId + "\",false]");
            }
        }
        [HttpPost]
        public JsonResult UpLoadCompanyLogo()
        {
            var CompanyLogoFile = HttpContext.Request.Files.Get("CompanyLogoFile");
            JsonResult json = new JsonResult() { ContentType = "text/html" };
            if (CompanyLogoFile != null)
            {
                string[] allowType = new string[] { "image/gif", "image/png", "image/jpeg" };
                if (!allowType.Contains(CompanyLogoFile.ContentType))
                {
                    json.Data = new
                    {
                        Status = false,
                        Message = "请上传gif,png,jpg格式图片"
                    };
                    return json;
                }
                if (CompanyLogoFile.ContentLength > (1024 * 1024 * 2))
                {
                    json.Data = new
                    {
                        Status = false,
                        Message = "文件不能超过2M"
                    };
                    return json;
                }
                var CompanyLogoPath = "/UpLoadFiles/SysAgentCompanyLogo/" + this.BasicAgent.Id + "_" + CompanyLogoFile.FileName;
                string ThumbnailPath = string.Format("/UpLoadFiles/{0}/{1}", "SysAgentCompanyLogo", "Thu_" + this.BasicAgent.Id + "_" + CompanyLogoFile.FileName);
                var savePath = HttpContext.Server.MapPath(CompanyLogoPath);
                var ThumbnailSavePath = HttpContext.Server.MapPath(ThumbnailPath); // 服务器端缩略图路径
                CompanyLogoFile.SaveAs(savePath);
                ImageUpload.MakeThumbnail(savePath, ThumbnailSavePath, 210, 71, "HW");
                json.Data = new
                {
                    Status = true,
                    Message = "",
                    Result = ThumbnailPath,
                };
                return json;
            }
            else
            {
                json.Data = new
                {
                    Status = false,
                    Message = "请选择上传文件"
                };
                return json;
            }
        }
        [HttpPost]
        public JsonResult UpLoadUserPayChangePic()
        {
            var logoFile = HttpContext.Request.Files.Get("UpLoad");
            JsonResult json = new JsonResult() { ContentType = "text/html" };
            if (logoFile != null)
            {
                string[] allowType = new string[] { "image/bmp", "image/gif", "image/png", "image/jpeg" };
                if (!allowType.Contains(logoFile.ContentType))
                {
                    json.Data = new
                    {
                        Status = false,
                        Message = "请上传bmp,gif,png,jpg格式图片"
                    };
                    return json;
                }
                if (logoFile.ContentLength > (1024 * 1024 * 2))
                {
                    json.Data = new
                    {
                        Status = false,
                        Message = "文件不能超过2M"
                    };
                    return json;
                }
                var path = "/UpLoadFiles/UserPayChange/" + this.BasicAgent.Id + "_" + logoFile.FileName;
                var savePath = HttpContext.Server.MapPath(path);
                logoFile.SaveAs(savePath);
                json.Data = new
                {
                    Status = true,
                    Message = "",
                    Result = this.BasicAgent.Id + "_" + logoFile.FileName,
                };
                return json;
            }
            else
            {
                json.Data = new
                {
                    Status = false,
                    Message = "请选择上传文件"
                };
                return json;
            }
        }
        [HttpPost]
        public void GetUsers(string UserName, bool? IsShowSupAgent)
        {
            if (UserName.IsNullOrEmpty())
            {
                Response.Write("[]");
                return;
            }
            IList<Users> List = null;
            if (!(bool)IsShowSupAgent)
            {
                List = Entity.Users.Where(n => (n.TrueName.Contains(UserName) || n.NeekName.Contains(UserName) || n.UserName == UserName) && n.Agent == BasicAgent.Id).ToList();
                //List = List.Where(n => n.Agent == BasicAgent.Id).ToList();
            }
            else
            {
                SysAgent LowerLevelAgent = Entity.SysAgent.FirstOrDefault(s => s.Id == BasicAgent.Id);
                IList<SysAgent> SysAgentList = LowerLevelAgent.GetSupAgent(Entity);
                IList<int> UID = new List<int>();
                foreach (var s in SysAgentList)
                {
                    UID.Add(s.Id);
                }
                List = Entity.Users.Where(n => (n.TrueName.Contains(UserName) || n.NeekName.Contains(UserName) || n.UserName == UserName)&&UID.Contains(n.Agent)).ToList();
               // List = List.Where(n => UID.Contains(n.Agent)).ToList();
            }
            foreach (var p in List)
            {
                p.Cols = "Id,UserName,TrueName,NeekName";
            }
            string Json = List.EntityToJson();
            Response.Write(Json);
        }

        [HttpPost]
        public void GetCount(string start, string  end)
        {
            string sql = "select COUNT(Code) as Count from Card where Code>='" + start + "' and Code<='" + end + "'  and State=1 and AId=" + BasicAgent.Id + " and AdminId=0";
            IList<CardNum> sqlList = Entity.ExecuteStoreQuery<CardNum>(sql, null).ToList();
           Response.Write(sqlList[0].Count);
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
            var param = new UpLoadFileParam(file, savePath, false)
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
                if (param.MaxWidth > 0 || param.MaxHigh > 0 || param.MinWidth > 0 || param.MinHigh > 0)
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

        /// <summary>
        /// 上传文件
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public JsonResult UpLoadFilePic(UpLoadFileHelp UpLoadFileHelp)
        {
            UpLoadFileHelp.Ini();
            JsonResult json = new JsonResult() { ContentType = "text/html" };
            json.Data = UpLoadFileHelp.UpLoadFile();
            return json;
        }
    }
    public class CardNum
    {
        public Int32 Count { get; set; }
    }
}
