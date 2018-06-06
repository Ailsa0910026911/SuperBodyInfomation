using LokFu.Infrastructure;
using LokFu.Models;
using LokFu.Repositories;
using LokFu.Extensions;
using LokFu.Repositories.SqlServer;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Script.Serialization;


namespace LokFu.Controllers
{
    public class OrdersInfo_2_0Controller : InitController
    {
        public OrdersInfo_2_0Controller()
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
                Log.Write("[Orders]:", "【Data】" + Data, Ex);
            }
            if (json == null)
            {
                DataObj.OutError("1000");
                return;
            }
            //System.Threading.Thread.Sleep(1000 * 150);
            Orders Orders = new Orders();
            Orders = JsonToObject.ConvertJsonToModel(Orders, json);

            Users baseUsers = Entity.Users.FirstOrDefault(n => n.Token == Orders.Token);
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
                //DataObj.OutError("2006");
                //return;
            }
            //if (baseUsers.MiBao != 1)//未设置支付密码
            //{
            //    DataObj.OutError("2008");
            //    return;
            //}

            Orders = Entity.Orders.FirstOrDefault(n => n.TNum == Orders.TNum && (n.UId == baseUsers.Id || (n.RUId == baseUsers.Id && n.PayState == 1)));
            if (Orders == null)//不存在
            {
                DataObj.OutError("1001");
                return;
            }
            if (Orders.RUId == baseUsers.Id) {
                Orders.TType = 4;
            }
            Orders.DoRemark(Entity);

            Orders.StateTxt = Orders.GetState();
            Orders.Cols += ",Json,PicList,UserCardName,CardUpdateTime";
            Orders.DoRemark(Entity);
            Orders.Colour = Orders.GeStateColour();
            IList<OrdersModel> OML = Utils.GetOrdersModel();
            OrdersModel OrdersModel = OML.FirstOrNew(n => n.Id == Orders.TType);
            Orders.Otypename = OrdersModel.Name;
            Orders.LagEntryDay = (byte)(Orders.TrunType.HasValue && Orders.TrunType.Value != 0 ? Orders.TrunType.Value : Orders.LagEntryDay);

            if (Orders.IdCardState > 1)
            {
                Orders.Remark = Orders.DDAuditRemark;
            }
            if (!Orders.UserCardPic.IsNullOrEmpty())
            {
                var UserCardPicList = Orders.UserCardPic.Split(',').ToList();
                for (int i = 0; i < UserCardPicList.Count; i++)
                {
                    var a = Utils.ImageUrl("Orders", UserCardPicList[i], AppImgPath);
                    UserCardPicList[i] = a;
                }
                JavaScriptSerializer JSS = new JavaScriptSerializer();
                string data = JSS.Serialize(UserCardPicList);
                JArray JO = (JArray)JsonConvert.DeserializeObject(data);
                Orders.PicList = JO;
            }

            JObject JS = new JObject();
            if (Orders.TType == 1)
            { //银联卡支付
                OrderRecharge OrderRecharge = Entity.OrderRecharge.FirstOrNew(n => n.OId == Orders.TNum);
                OrderRecharge.Cols = "Amoney,PayMoney,Poundage,PayType";
                string JsStr = OrderRecharge.OutJson();
                try
                {
                    JS = (JObject)JsonConvert.DeserializeObject(JsStr);
                }
                catch (Exception Ex)
                {
                    Log.Write("[Order]:", "【JsStr】" + JsStr, Ex);
                }
                Orders.Json = JS;
            }
            if (Orders.TType == 2)//提现不能取消
            {
                OrderCash OrderCash = Entity.OrderCash.FirstOrNew(n => n.OId == Orders.TNum);
                OrderCash.Cols = "Owner,Bank,CardNum,Deposit,Mobile,Province,City,District,Amoney,UserRate,TrunType,PayMoney";
                OrderCash.PayMoney = OrderCash.Amoney - (decimal)OrderCash.UserRate;
                string JsStr = OrderCash.OutJson();
                try
                {
                    JS = (JObject)JsonConvert.DeserializeObject(JsStr);
                }
                catch (Exception Ex)
                {
                    Log.Write("[Order]:", "【JsStr】" + JsStr, Ex);
                }
                Orders.Json = JS;
            }
            if (Orders.TType == 3 || Orders.TType == 4)//付款
            {
                OrderTransfer OrderTransfer = Entity.OrderTransfer.FirstOrNew(n => n.OId == Orders.TNum);
                Users Users = new Users();
                if (Orders.TType == 4)
                {
                    Users = Entity.Users.FirstOrNew(n => n.Id == OrderTransfer.UId);
                }
                if (Orders.TType == 3)
                {
                    Users = Entity.Users.FirstOrNew(n => n.Id == OrderTransfer.RUId);
                }
                OrderTransfer.Mobile = Users.UserName;
                OrderTransfer.ToUserName = Users.TrueName;
                OrderTransfer.Cols = "UId,RUId,PayMoney,Poundage,Amoney,Mobile,ToUserName";
                string JsStr = OrderTransfer.OutJson();
                try
                {
                    JS = (JObject)JsonConvert.DeserializeObject(JsStr);
                }
                catch (Exception Ex)
                {
                    Log.Write("[Order]:", "【JsStr】" + JsStr, Ex);
                }
                Orders.Json = JS;
            }
            if (Orders.TType == 5)//防租
            {
                OrderHouse OrderHouse = Entity.OrderHouse.FirstOrNew(n => n.OId == Orders.TNum);
                OrderHouse.Cols = "HouseOwner,Bank,CardNum,Deposit,Mobile,MonthRent,SecurityMoney,PayMonth,Poundage,Amoney";
                string JsStr = OrderHouse.OutJson();
                try
                {
                    JS = (JObject)JsonConvert.DeserializeObject(JsStr);
                }
                catch (Exception Ex)
                {
                    Log.Write("[Order]:", "【JsStr】" + JsStr, Ex);
                }
                Orders.Json = JS;
            }
            if (Orders.TType == 6)
            { //升级
                //PayConfigOrder PayConfigOrder = Entity.PayConfigOrder.FirstOrNew(n => n.OId == Orders.TNum);
                //Out = Out + "," + PayConfigOrder.ToStr();
                Orders.Poundage = 0;
            }
            if (Orders.TType == 7 || Orders.TType == 8 || Orders.TType == 9)
            { //扫码
                OrderF2F OrderF2F = Entity.OrderF2F.FirstOrNew(n => n.OId == Orders.TNum);
                OrderF2F.Cols = "Amoney,PayMoney,Poundage";
                string JsStr = OrderF2F.OutJson();
                try
                {
                    JS = (JObject)JsonConvert.DeserializeObject(JsStr);
                }
                catch (Exception Ex)
                {
                    Log.Write("[Order]:", "【JsStr】" + JsStr, Ex);
                }
                Orders.Json = JS;
            }
            if (Orders.TType == 10)
            { //代理
                
            }

            String Out = Orders.ToStr();
            Out = "{" + Out + "}";
            DataObj.Data = Out;
            DataObj.Code = "0000";
            DataObj.OutString();
        }
    }
}
