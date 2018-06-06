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
    public class BasicBankController : InitController
    {
        public BasicBankController()
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
            BasicBank BasicBank = new BasicBank();
            BasicBank = JsonToObject.ConvertJsonToModel(BasicBank, json);

            var query = Entity.BasicBank.AsQueryable();

            if (BasicBank.CanCredit == 1)//支持信用卡
            {
                query = query.Where(o => o.State == 1 && o.CanCredit == 1);
            }
            else
            {
                query = query.Where(n => n.State == 1 && n.IsPayCard == 1);
            }
            IList<BasicBank> BasicBankList = query.ToList();
            DataObj.Data = BasicBankList.EntityToJson();
            DataObj.Code = "0000";
            DataObj.OutString();
        }
    }
}
