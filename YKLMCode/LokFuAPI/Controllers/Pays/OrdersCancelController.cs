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


namespace LokFu.Controllers
{
    public class OrdersCancelController : InitController
    {
        public OrdersCancelController()
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
                DataObj.OutError("2006");
                return;
            }
            if (baseUsers.MiBao != 1)//未设置支付密码
            {
                DataObj.OutError("2008");
                return;
            }

            Orders = Entity.Orders.FirstOrDefault(n => n.TNum == Orders.TNum && n.UId == baseUsers.Id);
            if (Orders == null)//不存在
            {
                DataObj.OutError("1001");
                return;
            }
            if (Orders.TState != 1)
            {
                DataObj.OutError("6010");
                return;
            }
            if (Orders.PayState != 0)
            {
                DataObj.OutError("6010");
                return;
            }
            Orders.TState = 3;
            if (Orders.TType == 1)
            { //银联卡支付
                OrderRecharge OrderRecharge = Entity.OrderRecharge.FirstOrNew(n => n.OId == Orders.TNum);
                OrderRecharge.OrderState = 3;
            }
            if (Orders.TType == 2)//提现不能取消
            {
                DataObj.OutError("6010");
                return;
            }
            if (Orders.TType == 3)//付款
            {
                OrderTransfer OrderTransfer = Entity.OrderTransfer.FirstOrNew(n => n.OId == Orders.TNum);
                OrderTransfer.OrderState = 3;
            }
            if (Orders.TType == 5)//防租
            {
                OrderHouse OrderHouse = Entity.OrderHouse.FirstOrNew(n => n.OId == Orders.TNum);
                OrderHouse.OrderState = 3;
            }
            if (Orders.TType == 7)//不能取消
            {
                DataObj.OutError("6010");
                return;
            }
            if (Orders.TType == 8)//不能取消
            {
                DataObj.OutError("6010");
                return;
            }
            if (Orders.TType == 9)//不能取消
            {
                DataObj.OutError("6010");
                return;
            }
            Entity.SaveChanges();

            Orders.SendMsg(Entity);//发送消息类

            DataObj.Data = "";
            DataObj.Code = "0000";
            DataObj.OutString();
        }
    }
}
