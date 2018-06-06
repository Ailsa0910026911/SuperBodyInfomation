using System.Collections.Generic;
using System.Linq;
using System.Text;
using System;
using System.Collections;
using LokFu;
using LokFu.Repositories;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Reflection;
using LokFu.Extensions;
using System.Web;
using System.IO;

namespace LokFu.Controllers
{
    public class UpFileController : InitController
    {
        //private string AllowType = "image/bmp,image/gif,image/jpeg,image/png";
        public UpFileController()
        {
            if (!InitState)
            {
                DataObj.OutError("8080");
                return;
            }
            if (!DataObj.IsReg)
            {
                DataObj.OutError("3002");
                return;
            }
        }
        public void Post()
        {
            string Data = DataObj.GetData();
            if (Data.IsNullOrEmpty())
            {
                DataObj.OutError("1000");
                return;
            }
            JObject json = new JObject();
            try
            {
                json = (JObject)JsonConvert.DeserializeObject(Data);
            }
            catch (Exception Ex)
            {
                Log.Write("[OrdersPicController]:", "【Data】" + Data, Ex);
            }
            if (json == null)
            {
                DataObj.OutError("1000");
                return;
            }

            if (!DataObj.IsReg)
            {
                return;
            }

            UpFileSubmitModel model = new UpFileSubmitModel();
            model = JsonToObject.ConvertJsonToModel(model, json);

            if (model.filepath.IsNullOrEmpty())
            {
                DataObj.OutError("1002");
                return;
            }

            if (model.filename.IsNullOrEmpty())
            {
                DataObj.OutError("1002");
                return;
            }

            Users baseUsers = Entity.Users.FirstOrDefault(n => n.Token == model.token);
            if (baseUsers == null || model.token.IsNullOrEmpty())//用户令牌不存在
            {
                DataObj.OutError("2004");
                return;
            }

            var fielname = Utils.Base64StringToImage(model.filename, model.filepath) ?? string.Empty;

            ////图片格式
            //var types = this.AllowType.Split(',');
            //if (!types.Contains(File.ContentType))
            //{
            //    DataObj.OutError("4001");
            //    return;
            //}
            ////图片大小
            //if (File.ContentLength > 2097152)
            //{
            //    DataObj.OutError("4001");
            //    return;
            //}
            //string[] fileNames = Utils.DateToFileName();
            //string fileName = fileNames[2] + ".jpg";
            //var LocalFilePath = HttpContext.Current.Server.MapPath(string.Format("~/UpLoadFiles/{0}/", FilePath));
            //if (!Directory.Exists(LocalFilePath))
            //    Directory.CreateDirectory(LocalFilePath);
            //var LocalFileName = HttpContext.Current.Server.MapPath(string.Format("~/UpLoadFiles/{0}/{1}", FilePath, fileName));

            ////var LocalFileName = HttpContext.Current.Server.MapPath(string.Format("~/UpLoadFiles/UserPic/{0}", fileName));
            //File.SaveAs(LocalFileName);

            DataObj.Data = JsonConvert.SerializeObject(new { filename = fielname });
            DataObj.Code = "0000";
            DataObj.OutString();
        }

        public class UpFileSubmitModel
        {
            public string filepath { get; set; }

            public string token { get; set; }

            public string filename { get; set; }
        }

    }
}
