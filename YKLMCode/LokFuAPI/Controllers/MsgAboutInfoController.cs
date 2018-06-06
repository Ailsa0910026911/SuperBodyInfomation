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

namespace LokFu.Controllers
{
    public class MsgAboutInfoController : InitController
    {
        public MsgAboutInfoController()
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
                Log.Write("[MsgAboutInfo]:", "【Data】" + Data, Ex);
            }
            if (json == null)
            {
                DataObj.OutError("1000");
                return;
            }
            MsgAbout MsgAbout = new MsgAbout();
            MsgAbout = JsonToObject.ConvertJsonToModel(MsgAbout, json);
            //获取信息
            MsgAbout = Entity.MsgAbout.FirstOrDefault(n => n.State == 1 && n.Id == MsgAbout.Id);
            if (MsgAbout == null)//不存在
            {
                DataObj.OutError("1001");
                return;
            }
            DataObj.Data = MsgAbout.OutJson();
            DataObj.Code = "0000";
            DataObj.OutString();
        }
    }
}
