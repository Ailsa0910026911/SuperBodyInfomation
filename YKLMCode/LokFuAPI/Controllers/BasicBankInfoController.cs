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
    public class BasicBankInfoController : InitController
    {
        public BasicBankInfoController()
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
                Log.Write("[BasicCity]:", "【Data】" + Data, Ex);
            }
            if (json == null)
            {
                DataObj.OutError("1000");
                return;
            }
            BasicBankInfo BasicBankInfo = new BasicBankInfo();
            BasicBankInfo = JsonToObject.ConvertJsonToModel(BasicBankInfo, json);
            if (BasicBankInfo.BId.IsNullOrEmpty() || (BasicBankInfo.CId.IsNullOrEmpty() && BasicBankInfo.DId.IsNullOrEmpty()))
            {
                DataObj.OutError("1000");
                return;
            }
            if (BasicBankInfo.CId.IsNullOrEmpty())
            {
                IList<BasicBankInfo> BasicBankInfoList = Entity.BasicBankInfo.Where(n => n.State == 1 && n.DId == BasicBankInfo.DId && n.BId == BasicBankInfo.BId).ToList();
                DataObj.Data = BasicBankInfoList.EntityToJson();
            }
            else {
                IList<BasicBankInfo> BasicBankInfoList = Entity.BasicBankInfo.Where(n => n.State == 1 && n.CId == BasicBankInfo.CId && n.BId == BasicBankInfo.BId).ToList();
                DataObj.Data = BasicBankInfoList.EntityToJson();
            }
            DataObj.Code = "0000";
            DataObj.OutString();
        }
    }
}
