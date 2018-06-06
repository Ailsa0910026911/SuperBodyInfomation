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
    public class BasicDistrictController : InitController
    {
        public BasicDistrictController()
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
                Log.Write("[BasicDistrict]:", "【Data】" + Data, Ex);
            }
            if (json == null)
            {
                DataObj.OutError("1000");
                return;
            }
            BasicDistrict BasicDistrict = new BasicDistrict();
            BasicDistrict = JsonToObject.ConvertJsonToModel(BasicDistrict, json);

            IList<BasicDistrict> BasicDistrictList = Entity.BasicDistrict.Where(n => n.State == 1 && n.CId == BasicDistrict.CId).ToList();
            DataObj.Data = BasicDistrictList.EntityToJson();
            DataObj.Code = "0000";
            DataObj.OutString();
        }
    }
}
