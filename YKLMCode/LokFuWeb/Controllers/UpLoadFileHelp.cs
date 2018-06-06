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
    /// 上传文件参数
    /// </summary>
    public class UpLoadFileHelp
    {
        /// <summary>
        /// 上传字段
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 保存的路径
        /// </summary>
        public string SavePath { get; set; }

        /// <summary>
        /// 是否使用原来的名称
        /// </summary>
        public bool IsOriginalName { get; set; }

        /// <summary>
        /// 允许上传Mime格式,以","号分开
        /// 如"image/gif,image/png,image/jpeg"
        /// </summary>
        public string AllowType { get; set; } 

        /// <summary>
        /// 大小限制,单/M
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
        private string ExtendName { get; set; }

        /// <summary>
        /// 文件
        /// </summary>
        private HttpPostedFile File { get; set; }

        /// <summary>
        /// 保存的文件名
        /// </summary>
        private string SaveFileName { get; set; }


        public void Ini()
        {
            this.File = HttpContext.Current.Request.Files.Get(this.Name);
            if (this.File == null)
            {
                throw new Exception("参数错误");
            }
            this.ExtendName = Path.GetExtension(this.File.FileName);
            this.Size = this.Size != 0 ? this.Size : 2.0;
            this.AllowType = string.Empty;
            if (!this.IsOriginalName)
            {
                //按时间创建一个保存的文件名
                this.SaveFileName = DateTime.Now.ToString("yyyyMMddHHmmss") + LokFu.Extensions.Utils.GetCode(6) + this.ExtendName;
            }
            else
            {
                this.SaveFileName = File.FileName;
            }
        }

        /// <summary>
        /// 上传单个文件
        /// </summary>
        /// <returns></returns>
        public UpLoadResult UpLoadFile()
        {
            UpLoadResult result = new UpLoadResult();
            
            #region 校验
            if (!string.IsNullOrWhiteSpace(this.AllowType))
            {
                var types = this.AllowType.Split(',');
                if (!types.Contains(this.File.ContentType))
                {
                    result.Message = "格式不正确,请重新上传";
                    return result;
                }
            }

            if (this.File.ContentLength > (1024 * 1024 * this.Size))
            {
                result.Message = "最大只能上传" + this.Size + "M,请重新上传";
                return result;
            }

            //图片校验
            if (this.MaxWidth > 0 || this.MaxHigh > 0 || this.MinWidth > 0 || this.MinHigh > 0)
            {
                var image = System.Drawing.Image.FromStream(this.File.InputStream);
                if (image.Width > this.MaxWidth)
                {
                    result.Message = "图片超过最大宽度" + this.MaxWidth + ",请重新上传";
                    return result;
                }

                if (image.Height > this.MaxHigh)
                {
                    result.Message = "图片超过最大高度" + this.MaxHigh + ",请重新上传";
                    return result;
                }

                if (image.Width < this.MinWidth)
                {
                    result.Message = "图片小于最小宽度" + this.MinWidth + ",请重新上传";
                    return result;
                }

                if (image.Height < this.MinHigh)
                {
                    result.Message = "图片小于最小高度" + this.MinHigh + ",请重新上传";
                    return result;
                }
            }

            #endregion

            #region 保存
            var physicsSavePath = HttpContext.Current.Server.MapPath("/UpLoadFiles/" + this.SavePath);
            if (!System.IO.Directory.Exists(physicsSavePath))
            {
                System.IO.Directory.CreateDirectory(physicsSavePath);
            }
            var physicsSaveFileName = HttpContext.Current.Server.MapPath("/UpLoadFiles/" + this.SavePath + "/" + this.SaveFileName);
            this.File.SaveAs(physicsSaveFileName);
            result.Status = true;
            result.Result = new {
                UpLoadFileName = this.IsOriginalName ? this.File.FileName : this.SaveFileName, 
                SaveFileName = this.SaveFileName 
            };
            #endregion

            return result;
        }
    }

    /// <summary>
    /// 上传结果
    /// </summary>
    public class UpLoadResult
    {
        /// <summary>
        /// 上传结果
        /// </summary>
        public bool Status {get;set;}

        /// <summary>
        /// 信息
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// 返回结果
        /// </summary>
        public dynamic Result { get; set; }

        public UpLoadResult()
        {
            this.Status = false;
        }
    }

}