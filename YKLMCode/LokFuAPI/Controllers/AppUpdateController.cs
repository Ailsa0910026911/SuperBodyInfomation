using System.Collections.Generic;
using System.Linq;
using System.Text;
using System;
using System.Collections;
using LokFu;
using LokFu.Repositories;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using LokFu.Extensions;
using System.Configuration;

namespace LokFu.Controllers
{
    public class AppUpdateController : InitController
    {
        public AppUpdateController()
        {
            if (!InitState)
            {
                DataObj.OutError("8080");
                return;
            }
            if (DataObj == null)
            {
                DataObj.OutError("1000");
                return;
            }
        }
        public void Post()
        {
            string Data = DataObj.GetData();
            if (!Data.IsNullOrEmpty())
            {
                JObject json = new JObject();
                try
                {
                    json = (JObject)JsonConvert.DeserializeObject(Data);
                }
                catch (Exception Ex)
                {
                    Log.Write("[AppUpdate]:", "【Data】" + Data, Ex);
                }
                if (json == null)
                {
                    DataObj.OutError("1000");
                    return;
                }

                //处理贴牌相关
                AppUpdate AppUpdate = new AppUpdate();
                AppUpdate = JsonToObject.ConvertJsonToModel(AppUpdate, json);

                AppUpdate = Entity.AppUpdate.FirstOrNew(n => n.Tag == AppUpdate.Tag);
                if (AppUpdate.Id.IsNullOrEmpty())
                {
                    DataObj.OutError("1000");
                    return;
                }
                if (Equipment.RqType == "Apple")
                {
                    AppUpdate.Cols = "Name,Tag,IosVer,IosInt,IosUrl,IosYYB,IosInfo,IOSState,IosColor";
                }
                if (Equipment.RqType == "Android")
                {
                    AppUpdate.Cols = "Name,Tag,ApkVer,ApkInt,ApkUrl,ApkYYB,ApkInfo,ApkColor,APKState";
                }
                DataObj.Data = AppUpdate.ToJson();
                DataObj.Code = "0000";
                DataObj.OutString();
            }
        }
    }
}
