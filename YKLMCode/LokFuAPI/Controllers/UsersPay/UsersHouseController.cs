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
    public class UsersHouseController : InitController
    {
        public UsersHouseController()
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
                Log.Write("[UsersHouse]:", "【Data】" + Data, Ex);
            }
            if (json == null)
            {
                DataObj.OutError("1000");
                return;
            }
            //JObject pp = (JObject)p;
            Users Users = new Users();
            Users = JsonToObject.ConvertJsonToModel(Users, json);
            if (Users.Token.IsNullOrEmpty())
            {
                //
                DataObj.OutError("1000");
                return;
            }

            Users = Entity.Users.FirstOrDefault(n => n.Token == Users.Token);
            if (Users == null)//用户令牌不存在
            {
                DataObj.OutError("2004");
                return;
            }
            if (Users.State != 1)
            {
                DataObj.OutError("2003");
                return;
            }
            if (Users.CardStae != 2)//未实名认证
            {
                DataObj.OutError("2006");
                return;
            }
            if (Users.MiBao != 1)//未设置支付密码
            {
                DataObj.OutError("2008");
                return;
            }
            int Day = (DateTime.Now - Users.AddTime).Days;
            int OrderCount = Entity.Orders.Count(n => (n.TType == 2 || n.TType == 5) && n.PayState == 2 && n.UId == Users.Id);
            decimal OrderMoney = 0;
            if (OrderCount > 0)
            {
                OrderMoney = Entity.Orders.Where(n => (n.TType == 2 || n.TType == 5) && n.PayState == 2 && n.UId == Users.Id).Sum(n => n.Amoney);
            }
            int isT0 = 0;

            SysSet SysSet = Entity.SysSet.FirstOrDefault();
            SysSet.Cols = "House,Cash0,ECash0,Cash1,ECash1,isT0";
            SysSet.Cash0 = Users.Cash0;
            SysSet.Cash1 = Users.Cash1;
            SysSet.ECash0 = Users.ECash0;
            SysSet.ECash1 = Users.ECash1;
            if (Day >= SysSet.CashDay && OrderCount >= SysSet.CashNum && OrderMoney >= SysSet.CashMoney)
            {
                isT0 = 1;
            }

            //验证是否在快速提现时段内
            TaskTimeSet TaskTimeSet = Entity.TaskTimeSet.FirstOrDefault(n => n.STime <= DateTime.Now && n.ETime >= DateTime.Now && n.TId == 1);
            if (TaskTimeSet == null)
            {
                isT0 = 2;
            }
            else
            {
                //验证配额是否满足
                decimal Peier = TaskTimeSet.AllMoney - TaskTimeSet.UsedMoney;
                decimal minMoney = 0.01M;
                SysControl SysControl = Entity.SysControl.FirstOrDefault(n => n.Tag == "Cash" && n.State == 1);
                if (SysControl != null)
                {
                    minMoney = SysControl.SNum;
                }
                if (Peier < minMoney)
                {
                    isT0 = 3;
                }
            }

            SysSet.isT0 = isT0;

            DataObj.Data = SysSet.OutJson();
            DataObj.Code = "0000";
            DataObj.OutString();
        }
    }
}
