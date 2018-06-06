using LokFu.Repositories;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;


namespace LokFu.Controllers
{
    public class FastOrdersInfoController : InitController
    {
        public FastOrdersInfoController()
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
                Log.Write("[FastOrdersInfo]:", "【Data】" + Data, Ex);
            }
            if (json == null)
            {
                DataObj.OutError("1000");
                return;
            }
            FastOrder FastOrder = new FastOrder();
            FastOrder = JsonToObject.ConvertJsonToModel(FastOrder, json);

            Users baseUsers = Entity.Users.FirstOrDefault(n => n.Token == FastOrder.Token);
            if (baseUsers == null)//用户令牌不存在
            {
                DataObj.OutError("2004");
                return;
            }
            if (baseUsers.State != 1)//用户被锁定
            {
                DataObj.OutError("2003");
                return;
            }

            FastOrder FO = Entity.FastOrder.FirstOrDefault(n => n.TNum == FastOrder.TNum && n.UId == baseUsers.Id);
            if (FO == null) { 
                DataObj.OutError("1000");
                return;
            }
            FO.StateName = FO.GeStateName();
            FO.Colour = FO.GeStateColour();
            #region 旧版本要使用来控制颜色
            if (FO.State == 1)
            {
                if (FO.PayState == 1)
                {
                    if (FO.UserState == 1)
                    {
                        FO.State = 3;
                    }
                    else
                    {
                        FO.State = 2;
                    }
                }
                else
                {
                    FO.State = 1;
                }
            }
            else
            {
                FO.State = 0;
            }
            #endregion
            DataObj.Data = FO.OutJson();
            DataObj.Code = "0000";
            DataObj.OutString();
        }
    }
}
