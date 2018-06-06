using LokFu.Extensions;
using LokFu.Infrastructure;
using LokFu.Repositories;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Configuration;
using System.Data.Objects;
using System.Linq;
using System.Text;
using System.Web;

namespace LokFu.Controllers
{
    public class BankCardScanController : InitController
    {
        public BankCardScanController()
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
                Log.Write("[BankCardScanController]:", "【Data】" + Data, Ex);
            }
            if (json == null)
            {
                DataObj.OutError("1000");
                return;
            }
            Users Users = new Users();
            Users = JsonToObject.ConvertJsonToModel(Users, json);

            if (Users.Token.IsNullOrEmpty())
            {
                DataObj.OutError("1000");
                return;
            }

            if (Users.CardPic.IsNullOrEmpty())
            {
                DataObj.OutError("4002");
                return;
            }

            if (Users.CardNum.IsNullOrEmpty())
            {
                Users.CardNum = string.Empty;
            }

            Users baseUsers = Entity.Users.FirstOrDefault(n => n.Token == Users.Token);
            if (baseUsers == null)//用户令牌不存在
            {
                DataObj.OutError("2004");
                return;
            }
            if (baseUsers.State != 1)
            {
                DataObj.OutError("2003");
                return;
            }

            Users.CardPic = Utils.Base64StringToImage(Users.CardPic, "UserCard");
            if (Users.CardPic.IsNullOrEmpty() || Users.CardPic == "Err")
            {
                DataObj.OutError("4001");
                return;
            }
            Users.CardNum = Users.CardNum.Replace(" ", "");
            string CashName = baseUsers.Id.ToString() + "CardPicTemp";
            CacheBuilder.EntityCache.Remove(CashName, null);
            CacheBuilder.EntityCache.Add(CashName, Users, DateTime.Now.AddHours(1));
            
            Entity.SaveChanges();

            DataObj.Data = "";
            DataObj.Code = "0000";
            DataObj.OutString();
        }
    }
}
