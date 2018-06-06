using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Text;
using System.Security.Cryptography;
namespace LokFu
{
    /// <summary>
    /// 上传帮助
    /// </summary>
    public class UpLoadFileHelpOld
    {
        /// <summary>
        /// 上传单个文件
        /// </summary>
        /// <returns></returns>
        public static UpLoadResult UpLoadFile(UpLoadFileParam param)
        {
            UpLoadResult result = new UpLoadResult();
            var types = param.AllowType.Split(',');
            if (param.File != null)
            {
                #region 校验
                if (!types.Contains(param.File.ContentType))
                {
                    result.Message = "请上传" + param.AllowType + "格式";
                    return result;
                }
                if (param.File.ContentLength > (1024 * 1024 * param.Size))
                {
                    result.Message = "文件不能超过" + param.Size + "M";
                    return result;
                }
                //图片校验
                if(param.MaxWidth > 0 || param.MaxHigh > 0 || param.MinWidth > 0 || param.MinHigh > 0)
                {
                    var image = System.Drawing.Image.FromStream(param.File.InputStream);
                    if (image.Width > param.MaxWidth)
                    {
                        result.Message = "图片超过最大宽度" + param.MaxWidth + ",请重新上传";
                        return result;
                    }
                    if (image.Height > param.MaxHigh)
                    {
                        result.Message = "图片超过最大高度" + param.MaxHigh + ",请重新上传";
                        return result;
                    }
                    if (image.Width < param.MinWidth)
                    {
                        result.Message = "图片小于最小宽度" + param.MinWidth + ",请重新上传";
                        return result;
                    }
                    if (image.Height < param.MinHigh)
                    {
                        result.Message = "图片小于最小高度" + param.MinHigh + ",请重新上传";
                        return result;
                    }
                }
                #endregion
                #region 保存
                var physicsSavePath = HttpContext.Current.Server.MapPath(param.SavePath);
                if (!System.IO.Directory.Exists(physicsSavePath))
                {
                    System.IO.Directory.CreateDirectory(physicsSavePath);
                }
                var physicsSaveFileName = HttpContext.Current.Server.MapPath(param.SavePath + "/" + param.SaveFileName);
                param.File.SaveAs(physicsSaveFileName);
                result.Status = true;
                if (param.UpLoadFileName != null)
                {
                    result.Result = new { UpLoadFileName = param.UpLoadFileName, SaveFileName = param.SaveFileName };
                }
                else
                {
                    result.Result = new { SaveFileName = param.SaveFileName };
                }
                #endregion
            }
            else
            {
                result.Message = "请选择上传文件";
            }
            return result;
        }

        /// <summary>
        /// 上传单个文件
        /// </summary>
        /// <returns></returns>
        public static UpLoadResult UpLoadFileExcel(UpLoadFileParam param)
        {
            UpLoadResult result = new UpLoadResult();
            var types = param.AllowType.Split(',');
            if (param.File != null)
            {
               
                #region 保存
                var physicsSavePath = HttpContext.Current.Server.MapPath(param.SavePath);
                if (!System.IO.Directory.Exists(physicsSavePath))
                {
                    System.IO.Directory.CreateDirectory(physicsSavePath);
                }
                var physicsSaveFileName = HttpContext.Current.Server.MapPath(param.SavePath + "/" + param.SaveFileName);
                param.File.SaveAs(physicsSaveFileName);
                result.Status = true;
                if (param.UpLoadFileName != null)
                {
                    result.Result = new { UpLoadFileName = param.UpLoadFileName, SaveFileName = param.SaveFileName };
                }
                else
                {
                    result.Result = new { SaveFileName = param.SaveFileName };
                }
                #endregion
            }
            else
            {
                result.Message = "请选择上传文件";
            }
            return result;
        }
    }
    /// <summary>
    /// 上传文件参数
    /// </summary>
    public class UpLoadFileParam
    { 
        /// <summary>
        /// 文件
        /// </summary>
        public HttpPostedFileBase File {get;set;}
        /// <summary>
        /// 保存的路经
        /// </summary>
        public string SavePath {get;set;}
        /// <summary>
        /// 上传的文件名
        /// </summary>
        public string UpLoadFileName { get; set; }
        /// <summary>
        /// 保存的文件名
        /// </summary>
        public string SaveFileName { get; set; } 
        /// <summary>
        /// 允许上传Mime格式,以","号分开
        /// 如"image/gif,image/png,image/jpeg"
        /// </summary>
        public string AllowType { get; set; } 
        /// <summary>
        /// 大小
        /// </summary>
        public double Size { get; set; } 
        /// <summary>
        /// 最大宽度
        /// </summary>
        public int MaxWidth { get; set; } 
        /// <summary>
        /// 最大高度
        /// </summary>
        public int MaxHigh { get; set; }
        /// <summary>
        /// 最小宽度
        /// </summary>
        public int MinWidth { get; set; }
        /// <summary>
        /// 最小高度
        /// </summary>
        public int MinHigh { get; set; }
        /// <summary>
        /// 扩展名
        /// </summary>
        public string ExtendName { get; private set; }
        /// <summary>
        /// 上传文件参数
        /// </summary>
        /// <param name="File">文件</param>
        /// <param name="SavePath">保存路径</param>
        /// <param name="IsUseOriginalName">是否使用原来的名称</param>
        public UpLoadFileParam(HttpPostedFileBase File, string SavePath, bool IsUseOriginalName = true)
        {
            this.File = File;
            this.SavePath = SavePath;
            this.AllowType = "image/gif,image/png,image/jpeg";
            this.Size = 2.0;
            this.ExtendName = Path.GetExtension(File.FileName);
            this.UpLoadFileName = File.FileName;
            if (!IsUseOriginalName)
            {
                //按时间创建一个保存的文件名
                this.SaveFileName = DateTime.Now.ToString("yyyyMMddHHmmss") + LokFu.Extensions.Utils.GetCode(6) + this.ExtendName;
            }
            else
            {
                this.SaveFileName = File.FileName;
            }
        }
    }
}