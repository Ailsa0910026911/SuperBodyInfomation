using LokFu.Extensions;
using LokFu.Infrastructure;
using LokFu.Models;
using LokFu.Repositories;
using LokFu.Repositories.SqlServer;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace LokFu.Controllers
{
    public class BanKaTypeController : InitController
    {
        public BanKaTypeController()
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
                Log.Write("[BanKaType]:", "【Data】" + Data, Ex);
            }
            if (json == null)
            {
                DataObj.OutError("1000");
                return;
            }
            BanKaType BanKaType = new BanKaType();
            BanKaType = JsonToObject.ConvertJsonToModel(BanKaType, json);

            Users baseUsers = Entity.Users.FirstOrDefault(n => n.Token == BanKaType.Token);
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
            if (baseUsers.CardStae != 2)//未实名认证
            {
                DataObj.OutError("2006");
                return;
            }
            if (baseUsers.MiBao != 1)//未设置支付密码
            {
                DataObj.OutError("2008");
                return;
            }


            IList<BanKaType> BanKaTypeList = Entity.BanKaType.Where(n => n.State == 1).OrderBy(n => n.Sort).ToList();

            IList<BanKaOrder> BanKaOrderList = Entity.BanKaOrder.Where(n => n.OrderState == 2 && n.PayState == 1 && n.UId == baseUsers.Id).ToList();

            foreach (var P in BanKaTypeList)
            {
                //处理已读
                BanKaOrder BanKaOrder = BanKaOrderList.FirstOrDefault(n => n.BKTId == P.Id);
                if (BanKaOrder != null)
                {
                    P.PayState = 1;
                }
                else {
                    P.PayState = 0;
                }
                P.Pic = Utils.ImageUrl("BanKaType", P.Pic, SysImgPath);
            }

            DataObj.Data = BanKaTypeList.EntityToJson();
            DataObj.Code = "0000";
            DataObj.OutString();
        }
    }
}
