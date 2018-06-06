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
    public class BanKaListController : InitController
    {
        public BanKaListController()
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
                Log.Write("[BanKaList]:", "【Data】" + Data, Ex);
            }
            if (json == null)
            {
                DataObj.OutError("1000");
                return;
            }
            BanKaList BanKaList = new BanKaList();
            BanKaList = JsonToObject.ConvertJsonToModel(BanKaList, json);

            Users baseUsers = Entity.Users.FirstOrDefault(n => n.Token == BanKaList.Token);
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

            BanKaType BanKaType = Entity.BanKaType.FirstOrDefault(n => n.Id == BanKaList.BKTId && n.State == 1);
            if (BanKaType == null) {
                DataObj.OutError("1001");
                return;
            }
            BanKaOrder BanKaOrder = Entity.BanKaOrder.FirstOrDefault(n => n.OrderState == 2 && n.PayState == 1 && n.UId == baseUsers.Id && n.BKTId == BanKaType.Id);
            if (BanKaOrder == null)
            {
                DataObj.OutError("6052");
                return;
            }
            IList<BanKaList> BanKaListList = Entity.BanKaList.Where(n => n.State == 1 && n.BKTId == BanKaType.Id).OrderBy(n => n.Sort).ToList();
            foreach (var p in BanKaListList)
            {
                p.Pic = Utils.ImageUrl("BanKaList", p.Pic, SysImgPath);
            }
            DataObj.Data = BanKaListList.EntityToJson();
            DataObj.Code = "0000";
            DataObj.OutString();
        }
    }
}
